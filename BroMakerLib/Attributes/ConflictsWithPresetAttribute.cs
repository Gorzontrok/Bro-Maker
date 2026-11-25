using System;

namespace BroMakerLib.Attributes
{
    /// <summary>Which checks a passive conflict declaration takes part in.</summary>
    public enum ConflictScope
    {
        /// <summary>Checked between listed passives, and against the base bro's native abilities in both directions.</summary>
        All,

        /// <summary>Checked between listed passives only.</summary>
        ListedOnly,

        /// <summary>Only drops the declaring passive when the base bro natively has the named preset.</summary>
        NativeOnly
    }

    /// <summary>
    /// Declares this passive incompatible with each named preset: a bro listing both keeps the first and drops the later one with a warning, and either one is dropped when the
    /// base bro natively has the other, unless the dropped passive's JSON sets `"allowConflict": true`; `Scope` limits which checks apply.
    /// </summary>
    /// <example>
    /// <code>
    /// [PassivePreset("SnakeBroskin")]
    /// [ConflictsWithPreset("SethBrondle", Scope = ConflictScope.ListedOnly)]
    /// public class SnakeBroskinPassive : PassiveAbility { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ConflictsWithPresetAttribute : Attribute
    {
        public string[] PresetNames { get; }

        /// <summary>Which conflict checks this declaration takes part in.</summary>
        public ConflictScope Scope { get; set; }

        public ConflictsWithPresetAttribute(params string[] presetNames)
        {
            PresetNames = presetNames ?? new string[0];
        }
    }
}
