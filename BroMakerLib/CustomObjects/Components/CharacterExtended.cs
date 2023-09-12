using BroMakerLib.Infos;
using BroMakerLib.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using BroMakerLib.Abilities;
using UnityEngine;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;

namespace BroMakerLib.CustomObjects.Components
{
    public class CharacterExtended : MonoBehaviour
    {
        public TestVanDammeAnim character;
        public Dictionary<string, CharacterAbility[]> characterAbilities;

        protected virtual void Awake()
        {
            character = GetComponent<TestVanDammeAnim>();
        }

        public virtual void Initialize(Dictionary<string, string[]> infoAbilities)
        {
            foreach (KeyValuePair<string, string[]> pair in infoAbilities)
            {
                var abilityName = pair.Key;
                var storedAbility = MakerObjectStorage.GetAbiltyByName(abilityName);
                if(storedAbility.IsEmpty)
                {
                    BMLogger.Warning($"Ability {abilityName} not founded");
                }
                var info = storedAbility.GetInfo();
                if (info == null)
                {
                    BMLogger.Warning($"{abilityName} info is null");
                }
                var ability = info.GetAbility<TestVanDammeAnim>();
                if (ability as CharacterAbility == null)
                {
                    BMLogger.Warning($"{abilityName} was not made for character");
                    continue;
                }

                ability.AssignOwner(character);
                foreach (string method in pair.Value)
                {
                    if (method.IsNotNullOrEmpty())
                    {
                        if (!characterAbilities.ContainsKey(method))
                            characterAbilities.Add(method, new CharacterAbility[] { ability as CharacterAbility });
                        else
                            characterAbilities[method].Append(ability);
                    }
                }
            }
        }

        public virtual void InvokeAbility(string method, params object[] parameters)
        {
            if ( characterAbilities == null || !characterAbilities.ContainsKey(method))
                return;
            var abilities = characterAbilities[method];
            foreach (var ability in abilities)
            {
                ability.CallMethod(method, parameters);
            }
        }
    }
}
