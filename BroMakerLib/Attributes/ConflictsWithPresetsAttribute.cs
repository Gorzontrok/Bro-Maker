using System;

namespace BroMakerLib.Attributes
{
    /// <summary>
    /// Multi-preset variant of ConflictsWithPreset.
    ///
    /// <example>
    /// <code>
    /// [PassivePreset("Stormbro")]
    /// [ConflictsWithPresets("BrondleFly", "Tornado", "Balloon")]
    /// public class StormbroPassive : PassiveAbility { }
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ConflictsWithPresetsAttribute : Attribute
    {
        public string[] PresetNames { get; }

        public ConflictsWithPresetsAttribute(params string[] presetNames)
        {
            PresetNames = presetNames ?? new string[0];
        }
    }
}
