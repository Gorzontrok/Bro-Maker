using System.Collections.Generic;
using BroMakerLib.Abilities;
using UnityEngine;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using System;
using System.Linq;

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
                    try
                    {
                        ability.info.BeforeAwake(ability);
                    }
                    catch(Exception ex)
                    {
                        BMLogger.ExceptionLog($"{ability} failed Before Awake\n" + ex);
                    }
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
                try
                {
                    ability.info.AfterAwake(ability);
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog($"{ability} failed After Awake\n" + ex);
                }
            }
        }
        public virtual void BeforeStart()
        {
            foreach(CharacterAbility ability in _characterAbilitiesAlone)
            {
                try
                {
                    ability.info.BeforeStart(ability);
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog($"{ability} failed Before Start\n" + ex);
                }
            }
        }
        public virtual void AfterStart()
        {
            foreach(CharacterAbility ability in _characterAbilitiesAlone)
            {
                try
                {
                    ability.info.AfterStart(ability);
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog($"{ability} failed After Start\n" + ex);
                }
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
                else
                {
                    BMLogger.Error($"Ability or Preset {abilityName} not founded");
                }
            }

        }

        protected virtual CharacterAbility TryToGetAbilityFromFiles(string abilityName)
        {
            var storedAbility = MakerObjectStorage.GetAbilityByName(abilityName);
            if (storedAbility.IsEmpty)
            {
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
            ability.Initialize(info);
            return ability as CharacterAbility;
        }

        protected virtual CharacterAbility TryToGetAbilityFromAssemblies(string abilityName)
        {
            var type = PresetManager.GetAbilityPreset(abilityName);
            if (type == null)
            {
                return null;
            }
            if (type != typeof(CharacterAbility) && !type.IsSubclassOf(typeof(CharacterAbility)))
            {
                BMLogger.Warning($"Ability {type} is not type or subclass of {typeof(CharacterAbility)}");
                return null;
            }
            CharacterAbility ability = (CharacterAbility)Activator.CreateInstance(type);
            if (ability == null)
            {
                BMLogger.Error("Can't create instance of type " + type);
            }
            ability.Initialize(new Infos.AbilityInfo());
            return ability;
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
                        {
                            var temp = characterAbilities[method].ToList();
                            temp.Add(ability);
                            characterAbilities[method] = temp.ToArray();
                        }
                    }
                }
                if (!_characterAbilitiesAlone.Contains(ability))
                    _characterAbilitiesAlone.Add(ability);
            }
            catch(Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }

        public virtual bool InvokeAbility(string method, params object[] parameters)
        {
            if ( characterAbilities == null || !characterAbilities.ContainsKey(method))
                return false;

            bool anAbilityHasBeenCalled = false;
            var abilities = characterAbilities[method];
            foreach (var ability in abilities)
            {
                try
                {
                    ability.All(method, parameters);
                    ability.CallMethod(method, parameters);
                    anAbilityHasBeenCalled = true;
                }
                catch (Exception e)
                {
                    BMLogger.ExceptionLog(e);
                }
            }
            return anAbilityHasBeenCalled;
        }

        public virtual void InvokeAbilityToAll(string method, params object[] parameters)
        {
            foreach (var ability in _characterAbilitiesAlone)
            {
                try
                {
                    ability.CallMethod(method, parameters);
                }
                catch (Exception e)
                {
                    BMLogger.ExceptionLog(e);
                }
            }
        }
    }
}
