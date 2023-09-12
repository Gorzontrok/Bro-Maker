using BroMakerLib.Abilities;
using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFBGames.Switch.Friends;

namespace BroMakerLib.Infos
{
    public class AbilityInfo : CustomBroforceObjectInfo
    {
        public AbilityInfo() : base() { }
        public AbilityInfo(string name) : base(name) { }

        public string preset = "None";

        public Ability<T> GetAbility<T>() where T : class
        {
            var type = PresetManager.GetAbilityPreset(preset);
            if (type == null)
            {
                BMLogger.Log($"Preset {preset} not founded");
            }
            if (type != typeof(Ability<T>) && !type.IsSubclassOf(typeof(Ability<T>)))
            {
                BMLogger.Log($"Ability {type} is not type or subclass of {typeof(Ability <T>)}");
            }
            return (Ability<T>)Activator.CreateInstance(type);
        }
    }
}
