using System.Collections.Generic;
using BroMakerLib.Abilities;
using UnityEngine;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using System;

namespace BroMakerLib.CustomObjects.Components
{
    public class CharacterExtended : MonoBehaviour
    {
        public TestVanDammeAnim character;
        // Key: Method ; Value: Abilities
        public Dictionary<string, CharacterAbility[]> characterAbilities;
        protected bool _initialized = false;
        protected List<CharacterAbility> _characterAbilitiesAlone;
        protected virtual void Awake()
        { }

        protected virtual void Start()
        { }

        public virtual void Initialize(Dictionary<string, string[]> infoAbilities, TestVanDammeAnim character)
        {
            if (_initialized)
                return;

            this.character = character;
            characterAbilities = new Dictionary<string, CharacterAbility[]>();
            _characterAbilitiesAlone = new List<CharacterAbility>();

            GetAbilities(infoAbilities);

            _initialized = true;
        }

        #region Info Method to call
        public virtual void BeforeAwake()
        {
            try
            {
                foreach (CharacterAbility ability in _characterAbilitiesAlone)
                {
                    ability.info.BeforeAwake(ability);
                }
            }
            catch(Exception ex)
            {
                BMLogger.Error(ex);
            }
        }
        public virtual void AfterAwake()
        {
            foreach(CharacterAbility ability in _characterAbilitiesAlone)
            {
                ability.info.AfterAwake(ability);
            }
        }
        public virtual void BeforeStart()
        {
            foreach(CharacterAbility ability in _characterAbilitiesAlone)
            {
                ability.info.BeforeStart(ability);
            }
        }
        public virtual void AfterStart()
        {
            foreach(CharacterAbility ability in _characterAbilitiesAlone)
            {
                ability.info.AfterStart(ability);
            }
        }
        #endregion

        protected virtual void GetAbilities(Dictionary<string, string[]> infoAbilities)
        {
            foreach (KeyValuePair<string, string[]> pair in infoAbilities)
            {
                var abilityName = pair.Key;

                CharacterAbility ability = TryToGetAbilityFromFiles(abilityName);
                if (ability != null)
                {
                    StoreAbility(ability, pair.Value);
                    continue;
                }

                ability = TryToGetAbilityFromAssemblies(abilityName);
                if (ability != null)
                {
                    StoreAbility(ability, pair.Value);
                }
            }

        }

        protected virtual CharacterAbility TryToGetAbilityFromFiles(string abilityName)
        {
            var storedAbility = MakerObjectStorage.GetAbilityByName(abilityName);
            if (storedAbility.IsEmpty)
            {
                BMLogger.Warning($"Ability {abilityName} not founded");
                return null;
            }

            var info = storedAbility.GetInfo();
            if (info == null)
            {
                BMLogger.Warning($"{abilityName} info is null");
                return null;
            }

            var ability = info.GetAbility<TestVanDammeAnim>();
            if (ability == null)
            {
                BMLogger.Error($"{abilityName} is null.");
                return null;
            }

            if (ability as CharacterAbility == null)
            {
                BMLogger.Warning($"{abilityName} was not made for character");
                return null;
            }
            info.BeforeAwake(ability);
            return ability as CharacterAbility;
        }

        protected virtual CharacterAbility TryToGetAbilityFromAssemblies(string abilityName)
        {
            var type = PresetManager.GetAbilityPreset(abilityName);
            if (type == null)
            {
                BMLogger.Error($"Preset {abilityName} not founded");
                return null;
            }
            if (type != typeof(CharacterAbility) && !type.IsSubclassOf(typeof(CharacterAbility)))
            {
                BMLogger.Warning($"Ability {type} is not type or subclass of {typeof(CharacterAbility)}");
                return null;
            }
            return (CharacterAbility)Activator.CreateInstance(type);
        }

        protected virtual void StoreAbility(CharacterAbility ability, string[] methods)
        {

            try
            {
                if (ability == null)
                    return;

                if (characterAbilities == null)
                {
                    characterAbilities = new Dictionary<string, CharacterAbility[]>();
                }

                ability.AssignOwner(character);
                foreach (string method in methods)
                {
                    if (method.IsNotNullOrEmpty())
                    {
                        if (!characterAbilities.ContainsKey(method))
                            characterAbilities.Add(method, new CharacterAbility[] { ability });
                        else
                            characterAbilities[method].Append(ability);
                        _characterAbilitiesAlone.Add(ability);
                    }
                }
            }
            catch(Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }

        public virtual void InvokeAbility(string method, params object[] parameters)
        {
            if ( characterAbilities == null || !characterAbilities.ContainsKey(method))
                return;
            var abilities = characterAbilities[method];
            foreach (var ability in abilities)
            {
                ability.All();
                ability.CallMethod(method, parameters);
            }
        }
    }
}
