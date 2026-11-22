# Loggers

`BMLogger.Error` / `Warning` feed the Issues tab and notifications; `Log` and `Debug` (gated on `Settings.debugLogs`) reach only the UMM log.

- **Error**: the mod, the bro or a feature won't work and the player must act. **Warning**: something is off but the mod still runs.
- `mod` and `bro` drive grouping and the notification source. Inside `BMLogger.Scope(mod, bro)` (a `using` block; a null part inherits the enclosing scope's) a report needs neither. Code that runs later than the block (coroutines, `Start`, `Update`, ability hooks, Harmony patches on game methods) is outside every scope: pass them there, or use the hero extensions. Never hold a scope across a `yield`: the scope stack is per thread, so a suspended scope stays on it and misattributes other code's reports. A `mod` still null resolves from `bro`, then from the exception's stack, then from the first stack frame outside BroMakerLib and the framework assemblies (`mscorlib`, `System*`, `UnityEngine*`, Harmony, Unity Mod Manager), when it belongs to a registered mod. Pass `ex` whenever there is an exception.
- `Error` / `Warning` may be called from any thread and never throw; the issue is committed at the next main-thread flush.

## One failure, one report

- The outermost catch holding the mod and bro context reports, with a context-only message and `ex`: the collector appends the root cause (unwrapping `TargetInvocationException` and `TypeInitializationException`), since a notification shows only the message, so a call site never appends `ex.Message` itself, unless the composed text is also stored somewhere the collector never sees.
- A report whose `ex` is, or wraps, an exception still queued replaces the queued report, keeping its message and filling in its `mod` / `bro`; once the queued report is flushed, a new report of that exception is recorded on its own. So a leaf may report and rethrow when both run on one stack, and the boundary's report wins.
- A throw caught in the same method doesn't report first, and a caller given a failure result the layer below already reported at most logs.

## Catches around mod code

A catch around a call into mod code through reflection or a callback passes `ex` to `Error` or `Warning`. When `IsIncompatibleAssembly(ex)` finds a missing member, access or type-load exception anywhere in the inner chain, either call records an Error and appends `FormatIncompatibleAssemblyMessage(mod, ex)` (the mod needs rebuilding against the current BroMaker) as the cause unless the message already contains `MissingCodePhrase`, so a `Warning` catch still surfaces an incompatible mod as an Error.

## Collector

- Issues dedup on `mod`, `bro`, `message` and exception type; a recurrence only bumps the entry's `OccurrenceCount` and `Timestamp`.
- `OnNewIssue` fires once per new issue on the main thread during the flush, so subscribers must not block.
- The collector and the pending queue each cap at 200; past the cap a new issue is dropped and one limit marker is added.
