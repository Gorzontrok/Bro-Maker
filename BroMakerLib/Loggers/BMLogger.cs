using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using BroMakerLib.Storages;
using UnityEngine;
using BSett = BroMakerLib.Settings;

namespace BroMakerLib.Loggers
{
    /// <summary>Whether an issue is an Error or a Warning.</summary>
    public enum IssueSeverity
    {
        Error,
        Warning
    }

    /// <summary>An Error or Warning entry shown in the Issues panel and as a notification; repeats of it update the same entry.</summary>
    public sealed class IssueEntry
    {
        internal IssueEntry()
        {
        }

        /// <summary>Whether the entry is an Error or a Warning.</summary>
        public IssueSeverity Severity { get; internal set; }

        /// <summary>Name of the mod the issue belongs to, or null or empty when none was resolved.</summary>
        public string ModName { get; internal set; }

        /// <summary>Name of the bro the issue belongs to, or null or empty when none was resolved.</summary>
        public string BroName { get; internal set; }

        /// <summary>The message shown in the Issues panel and the notification, followed by the exception's cause when there is one.</summary>
        public string Message { get; internal set; }

        /// <summary>The full exception text, or null when the issue had no exception.</summary>
        public string ExceptionDetails { get; internal set; }

        /// <summary>When the issue last occurred.</summary>
        public DateTime Timestamp { get; internal set; }

        /// <summary>How many times the issue has occurred.</summary>
        public int OccurrenceCount { get; internal set; }
    }

    public static class BMLogger
    {
        public const string PREFIX = "[BroMakerLib]";

        private const int PendingIssueLimit = 200;
        private const int IssueLimit = 200;
        private const string PendingLimitMessage = "Issue queue limit reached, so some issues were not recorded";
        private const string IssueLimitMessage = "Issue limit reached, so new issues are no longer recorded";

        internal const string MissingCodePhrase = "a function or type that isn't there";
        private const int QueuedLineLimit = 200;
        private const int TracedSignatureLimit = 1000;

        private class PendingIssue
        {
            public IssueSeverity severity;
            public string modName;
            public string broName;
            public string message;
            public Exception exception;
            public DateTime timestamp;
        }

        private static readonly List<IssueEntry> _issues = new List<IssueEntry>();
        private static readonly ReadOnlyCollection<IssueEntry> _issuesReadOnly = _issues.AsReadOnly();
        private static readonly Dictionary<string, IssueEntry> _issuesBySignature = new Dictionary<string, IssueEntry>();
        private static bool _issueLimitReported;
        private static readonly List<PendingIssue> _pendingIssues = new List<PendingIssue>();
        private static readonly object _pendingIssuesLock = new object();
        private static bool _pendingLimitReported;
        private static readonly HashSet<string> _tracedSignatures = new HashSet<string>();
        private static readonly List<string> _queuedLines = new List<string>();
        private static readonly object _queuedLinesLock = new object();
        private static int _droppedLineCount;

        private class ScopeFrame
        {
            public string mod;
            public string bro;
        }

        [ThreadStatic] private static List<ScopeFrame> _scopes;

        /// <summary>Raised on the main thread in BroMaker's next update after a new issue is reported; repeats of an issue only update its entry. Once 200 issues are collected it is raised only for one entry marking the limit, and after the Issues panel is cleared it is raised again for any issue.</summary>
        public static event Action<IssueEntry> OnNewIssue;

        public static void Log(string message)
        {
            Write(Format(message, LogType.Log, debug: false));
        }

        public static void Log(object message)
        {
            Log(message == null ? "null" : message.ToString());
        }

        public static void Debug(object message)
        {
            if (!DebugEnabled)
            {
                return;
            }
            Write(Format(message == null ? "null" : message.ToString(), LogType.Log, debug: true));
        }

        internal static bool DebugEnabled
        {
            get
            {
                return BSett.instance != null && BSett.instance.debugLogs;
            }
        }

        /// <summary>Records an Error for the Issues panel and a notification, appending the exception's message and resolving `mod` when omitted; an exception already reported and still queued replaces that report.</summary>
        public static void Error(string message, Exception ex = null, string mod = null, string bro = null)
        {
            ReportIssue(IssueSeverity.Error, message, ex, mod, bro);
        }

        /// <summary>Same as `Error`, recorded as a Warning unless `ex` shows the mod needs rebuilding.</summary>
        public static void Warning(string message, Exception ex = null, string mod = null, string bro = null)
        {
            ReportIssue(IssueSeverity.Warning, message, ex, mod, bro);
        }

        /// <summary>Live read-only view of the collected issues in the order first reported. Read it on the main thread. Emptied when the user clears the Issues panel.</summary>
        public static ReadOnlyCollection<IssueEntry> GetIssues()
        {
            return _issuesReadOnly;
        }

        internal static IDisposable Scope(string mod, string bro)
        {
            if (_scopes == null)
            {
                _scopes = new List<ScopeFrame>();
            }
            ScopeFrame frame = new ScopeFrame
            {
                mod = string.IsNullOrEmpty(mod) ? null : mod,
                bro = string.IsNullOrEmpty(bro) ? null : bro
            };
            if (_scopes.Count > 0)
            {
                ScopeFrame outer = _scopes[_scopes.Count - 1];
                frame.mod = frame.mod ?? outer.mod;
                frame.bro = frame.bro ?? outer.bro;
            }
            _scopes.Add(frame);
            return new ScopeHandle(_scopes, _scopes.Count - 1);
        }

        private sealed class ScopeHandle : IDisposable
        {
            private readonly List<ScopeFrame> _stack;
            private readonly int _depth;
            private bool _disposed;

            public ScopeHandle(List<ScopeFrame> stack, int depth)
            {
                _stack = stack;
                _depth = depth;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                if (_stack.Count > _depth)
                {
                    _stack.RemoveRange(_depth, _stack.Count - _depth);
                }
            }
        }

        /// <summary>Resets the collector so the next occurrence of any signature fires `OnNewIssue` again.</summary>
        internal static void ClearIssues()
        {
            _issues.Clear();
            _issuesBySignature.Clear();
            _issueLimitReported = false;
            lock (_pendingIssuesLock)
            {
                _pendingLimitReported = false;
                _tracedSignatures.Clear();
            }
        }

        /// <summary>True if `ex` or any inner exception shows that a function, field or type the caller referenced is missing or inaccessible.</summary>
        public static bool IsIncompatibleAssembly(Exception ex)
        {
            for (Exception cur = ex; cur != null; cur = cur.InnerException)
            {
                if (cur is MissingMemberException || cur is MethodAccessException || cur is FieldAccessException || cur is TypeLoadException || cur is ReflectionTypeLoadException)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Builds the "rebuild required" message for a mod that hit an incompatible-assembly exception.</summary>
        internal static string FormatIncompatibleAssemblyMessage(string modName, Exception ex)
        {
            string label = string.IsNullOrEmpty(modName) ? "A mod" : "'" + modName + "'";
            return label + " tried to use " + MissingCodePhrase + ". This usually means it was built against an older " +
                   "version of BroMaker and needs to be rebuilt against BroMaker " + Info.VERSION + " or later, but it can also mean " +
                   "one of the mod's own dependencies is missing or out of date. " +
                   "(Missing: " + MissingMemberMessage(ex) + ")";
        }

        internal static string MissingMemberMessage(Exception ex)
        {
            string innerMsg = ex.Message;
            for (Exception cur = ex; cur != null; cur = cur.InnerException)
            {
                ReflectionTypeLoadException reflectionEx = cur as ReflectionTypeLoadException;
                if (reflectionEx != null && !reflectionEx.LoaderExceptions.IsNullOrEmpty() && reflectionEx.LoaderExceptions[0] != null)
                {
                    return reflectionEx.LoaderExceptions[0].Message;
                }
                innerMsg = cur.Message;
            }
            return innerMsg;
        }

        internal static string FormatSource(string modName, string broName)
        {
            if (string.IsNullOrEmpty(modName) && string.IsNullOrEmpty(broName))
            {
                return "BroMaker";
            }
            if (string.IsNullOrEmpty(broName))
            {
                return modName;
            }
            if (string.IsNullOrEmpty(modName) || IsSingleBroMod(modName))
            {
                return broName;
            }
            return modName + "/" + broName;
        }

        internal static bool IsSingleBroMod(string modName)
        {
            BroMakerMod mod = BroMakerStorage.GetModByName(modName);
            return mod != null && mod.BrosNames.Length == 1 && !string.IsNullOrEmpty(mod.BrosNames[0]);
        }

        private static Exception RootCause(Exception ex)
        {
            Exception cur = ex;
            while ((cur is TargetInvocationException || cur is TypeInitializationException) && cur.InnerException != null)
            {
                cur = cur.InnerException;
            }
            return cur;
        }

        private static string ComposeMessage(string message, Exception ex, string mod, bool incompatible)
        {
            string detail = incompatible ? FormatIncompatibleAssemblyMessage(mod, ex) : RootCause(ex).Message;
            if (string.IsNullOrEmpty(detail) || message.IndexOf(incompatible ? MissingCodePhrase : detail, StringComparison.Ordinal) >= 0)
            {
                return message;
            }
            return message.Length == 0 ? detail : message + ": " + detail;
        }

        private static void ReportIssue(IssueSeverity severity, string message, Exception ex, string mod, string bro)
        {
            string msg = message ?? "";

            ScopeFrame scope = _scopes != null && _scopes.Count > 0 ? _scopes[_scopes.Count - 1] : null;
            if (scope != null && (mod == null || scope.mod == null || mod == scope.mod) && (bro == null || scope.bro == null || bro == scope.bro))
            {
                mod = mod ?? scope.mod;
                bro = bro ?? scope.bro;
            }

            // Can run off the main thread, where the unsynchronized registry reads may throw.
            if (mod == null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(bro))
                    {
                        mod = BroMakerStorage.TryResolveModFromBro(bro);
                    }
                    if (mod == null && ex != null)
                    {
                        mod = TryResolveModFromException(ex);
                    }
                    if (mod == null)
                    {
                        mod = TryResolveModFromCaller();
                    }
                }
                catch (Exception)
                {
                    mod = null;
                }
            }

            if (ex != null)
            {
                try
                {
                    bool incompatible = IsIncompatibleAssembly(ex);
                    if (incompatible)
                    {
                        severity = IssueSeverity.Error;
                    }
                    msg = ComposeMessage(msg, ex, mod, incompatible);
                }
                catch (Exception)
                {
                }
            }

            PendingIssue pending = new PendingIssue
            {
                severity = severity,
                modName = mod,
                broName = bro,
                message = msg,
                exception = ex,
                timestamp = DateTime.Now
            };
            bool writeTrace = false;
            bool pendingLimitQueued = false;
            lock (_pendingIssuesLock)
            {
                int earlier = ex == null ? -1 : FindPendingReportOf(ex);
                if (earlier >= 0)
                {
                    PendingIssue replaced = _pendingIssues[earlier];
                    pending.modName = pending.modName ?? replaced.modName;
                    pending.broName = pending.broName ?? replaced.broName;
                    if (!string.IsNullOrEmpty(replaced.message) && pending.message.IndexOf(replaced.message, StringComparison.Ordinal) < 0)
                    {
                        pending.message = pending.message + ": " + replaced.message;
                    }
                    _pendingIssues[earlier] = pending;
                }
                else
                {
                    if (ex != null)
                    {
                        string signature = BuildSignature(mod, bro, msg, ex.GetType().FullName);
                        writeTrace = !_tracedSignatures.Contains(signature);
                        if (writeTrace && _tracedSignatures.Count < TracedSignatureLimit)
                        {
                            _tracedSignatures.Add(signature);
                        }
                    }

                    if (_pendingIssues.Count < PendingIssueLimit)
                    {
                        _pendingIssues.Add(pending);
                    }
                    else if (!_pendingLimitReported)
                    {
                        _pendingLimitReported = true;
                        pendingLimitQueued = true;
                        _pendingIssues.Add(new PendingIssue
                        {
                            severity = IssueSeverity.Warning,
                            modName = "BroMaker",
                            message = PendingLimitMessage,
                            timestamp = pending.timestamp
                        });
                    }
                }
            }

            try
            {
                Write(Format(pending.message, ToLogType(pending.severity), debug: false));
                if (writeTrace)
                {
                    Write(Format(ex.ToString(), LogType.Exception, debug: false));
                }
                if (pendingLimitQueued)
                {
                    Write(Format(PendingLimitMessage, LogType.Warning, debug: false));
                }
            }
            catch (Exception)
            {
            }
        }

        private static int FindPendingReportOf(Exception ex)
        {
            for (int i = _pendingIssues.Count - 1; i >= 0; --i)
            {
                if (IsInChain(ex, _pendingIssues[i].exception))
                {
                    return i;
                }
            }
            return -1;
        }

        private static bool IsInChain(Exception ex, Exception target)
        {
            if (target == null)
            {
                return false;
            }
            for (Exception cur = ex; cur != null; cur = cur.InnerException)
            {
                if (ReferenceEquals(cur, target))
                {
                    return true;
                }
            }
            return false;
        }

        internal static void Flush()
        {
            WriteQueuedLines();

            PendingIssue[] drained;
            lock (_pendingIssuesLock)
            {
                if (_pendingIssues.Count == 0)
                {
                    return;
                }
                drained = _pendingIssues.ToArray();
                _pendingIssues.Clear();
            }

            for (int i = 0; i < drained.Length; ++i)
            {
                CommitIssue(drained[i]);
            }
        }

        private static void CommitIssue(PendingIssue pending)
        {
            Exception ex = pending.exception;
            string exTypeName = ex == null ? "" : ex.GetType().FullName;
            string signature = BuildSignature(pending.modName, pending.broName, pending.message, exTypeName);

            IssueEntry existing;
            if (_issuesBySignature.TryGetValue(signature, out existing))
            {
                existing.OccurrenceCount++;
                existing.Timestamp = pending.timestamp;
                return;
            }

            if (_issues.Count >= IssueLimit)
            {
                if (!_issueLimitReported)
                {
                    _issueLimitReported = true;
                    Write(Format(IssueLimitMessage, LogType.Warning, debug: false));
                    AddEntry(IssueSeverity.Warning, "BroMaker", null, IssueLimitMessage, null, BuildSignature("BroMaker", null, IssueLimitMessage, ""), pending.timestamp);
                }
                return;
            }

            AddEntry(pending.severity, pending.modName, pending.broName, pending.message, ex == null ? null : ex.ToString(), signature, pending.timestamp);
        }

        private static void AddEntry(IssueSeverity severity, string mod, string bro, string message, string exceptionDetails, string signature, DateTime timestamp)
        {
            IssueEntry entry = new IssueEntry
            {
                Severity = severity,
                ModName = mod,
                BroName = bro,
                Message = message,
                ExceptionDetails = exceptionDetails,
                Timestamp = timestamp,
                OccurrenceCount = 1
            };
            _issues.Add(entry);
            _issuesBySignature[signature] = entry;

            Action<IssueEntry> handler = OnNewIssue;
            if (handler == null)
            {
                return;
            }
            Delegate[] subscribers = handler.GetInvocationList();
            for (int i = 0; i < subscribers.Length; ++i)
            {
                try
                {
                    ((Action<IssueEntry>)subscribers[i])(entry);
                }
                catch (Exception handlerEx)
                {
                    Write(Format("OnNewIssue handler threw: " + handlerEx, LogType.Error, debug: false));
                }
            }
        }

        private static string BuildSignature(string mod, string bro, string message, string exTypeName)
        {
            return SignaturePart(mod) + SignaturePart(bro) + SignaturePart(message) + SignaturePart(exTypeName);
        }

        private static string SignaturePart(string value)
        {
            string part = value ?? "";
            return part.Length + ":" + part;
        }

        private static string TryResolveModFromException(Exception ex)
        {
            if (BroMakerStorage.Mods.Count == 0)
            {
                return null;
            }

            List<Exception> chain = new List<Exception>();
            for (Exception cur = ex; cur != null; cur = cur.InnerException)
            {
                chain.Add(cur);
            }
            for (int c = chain.Count - 1; c >= 0; --c)
            {
                StackTrace stack = new StackTrace(chain[c], false);
                for (int i = 0; i < stack.FrameCount; i++)
                {
                    BroMakerMod mod = BroMakerStorage.GetModByAssembly(stack.GetFrame(i).GetMethod()?.DeclaringType?.Assembly);
                    if (mod != null)
                    {
                        return mod.Name;
                    }
                }
            }
            return null;
        }

        private static string TryResolveModFromCaller()
        {
            if (BroMakerStorage.Mods.Count == 0)
            {
                return null;
            }

            Assembly bmAsm = typeof(BMLogger).Assembly;
            StackTrace stack = new StackTrace(1, false);
            for (int i = 0; i < stack.FrameCount; i++)
            {
                Assembly asm = stack.GetFrame(i).GetMethod()?.DeclaringType?.Assembly;
                if (asm == null)
                {
                    return null;
                }
                if (ReferenceEquals(asm, bmAsm) || IsFrameworkAssembly(asm))
                {
                    continue;
                }
                BroMakerMod mod = BroMakerStorage.GetModByAssembly(asm);
                return mod == null ? null : mod.Name;
            }
            return null;
        }

        private static bool IsFrameworkAssembly(Assembly asm)
        {
            string name = asm.GetName().Name;
            return name == "mscorlib" || name == "System" || name.StartsWith("System.", StringComparison.Ordinal) ||
                   name.StartsWith("UnityEngine", StringComparison.Ordinal) || name == "0Harmony" || name == "UnityModManager";
        }

        private static LogType ToLogType(IssueSeverity severity)
        {
            return severity == IssueSeverity.Error ? LogType.Error : LogType.Warning;
        }

        private static void Write(string formatted)
        {
            if (Main.mainThreadId == 0 || Thread.CurrentThread.ManagedThreadId == Main.mainThreadId)
            {
                WriteQueuedLines();
                Main.Log(formatted, PREFIX);
                return;
            }

            // The UMM logger isn't thread-safe, so only the main thread writes to it.
            lock (_queuedLinesLock)
            {
                if (_queuedLines.Count < QueuedLineLimit)
                {
                    _queuedLines.Add(formatted);
                }
                else
                {
                    _droppedLineCount++;
                }
            }
        }

        private static void WriteQueuedLines()
        {
            lock (_queuedLinesLock)
            {
                for (int i = 0; i < _queuedLines.Count; ++i)
                {
                    Main.Log(_queuedLines[i], PREFIX);
                }
                _queuedLines.Clear();
                if (_droppedLineCount > 0)
                {
                    Main.Log(Format(_droppedLineCount + " log lines from other threads were dropped because the queue was full", LogType.Warning, debug: false), PREFIX);
                    _droppedLineCount = 0;
                }
            }
        }

        private static string Format(string message, LogType logType, bool debug)
        {
            string time = "[" + DateTime.Now.ToString("HH:mm:ss") + "]";
            string debugTag = debug ? "[DEBUG] " : "";
            return time + " " + debugTag + FormatLogType(logType) + FormatMessage(message, logType);
        }

        private static string FormatLogType(LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return "<color=red><b>[" + logType + "]</b></color> ";
                case LogType.Warning:
                    return "<color=yellow><b>[" + logType + "]</b></color> ";
                default:
                    return "";
            }
        }

        private static string FormatMessage(string message, LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return "<color=red>" + message + "</color>";
                case LogType.Warning:
                    return "<color=yellow>" + message + "</color>";
                default:
                    return message;
            }
        }
    }
}
