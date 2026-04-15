using System;

namespace BroMakerLib.Attributes
{
    /// <summary>
    /// Marks this passive as incompatible with the named preset. If a bro lists both in its
    /// passives array, the first listed attaches and the later is dropped with a warning.
    /// Set "allowConflict": true on the later passive's JSON to bypass and allow both.
    /// </summary>
    /// <example>
    /// <code>
    /// [PassivePreset("SnakeBroskin")]
    /// [ConflictsWithPreset("BrondleFly")]
    /// public class SnakeBroskinPassive : PassiveAbility { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ConflictsWithPresetAttribute : Attribute
    {
        public string PresetName { get; }

        public ConflictsWithPresetAttribute(string presetName)
        {
            PresetName = presetName;
        }
    }
}
