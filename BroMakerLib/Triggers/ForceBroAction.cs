using System;
using System.Collections.Generic;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using RocketLib.CustomTriggers;
using UnityEngine;

namespace BroMakerLib.Triggers
{
    public class ForceBroActionInfo : LevelStartTriggerActionInfo
    {
        public List<string> ForcedBros = new List<string>();
        public bool EnableForced = true;

        public override void ShowGUI(LevelEditorGUI gui)
        {
            base.ShowGUI(gui);

            EnableForced = GUILayout.Toggle(EnableForced, "Enable custom bro forced spawn");
            EnableForced = !GUILayout.Toggle(!EnableForced, "Disable custom bro forced spawn");
            GUILayout.Space(15);
            GUILayout.Label("Forced Bros:");
            foreach (StoredHero hero in BroMakerStorage.Bros)
            {
                if (ForcedBros.Contains(hero.name))
                {
                    if (!GUILayout.Toggle(true, hero.name))
                    {
                        this.ForcedBros.Remove(hero.name);
                    }
                }
                else if (GUILayout.Toggle(false, hero.name))
                {
                    this.ForcedBros.Add(hero.name);
                }
            }
        }
    }

    public class ForceBroAction : LevelStartTriggerAction<ForceBroActionInfo>
    {
        protected override void ExecuteAction(bool isLevelStart)
        {
            try
            {
                if (info.EnableForced)
                {
                    List<StoredHero> storedHeroes = new List<StoredHero>();
                    foreach (string broName in info.ForcedBros)
                    {
                        StoredHero hero = BroMakerStorage.GetStoredHeroByName(broName);
                        if (hero != null)
                        {
                            storedHeroes.Add(hero);
                        }
                    }
                    if (storedHeroes.Count > 0)
                    {
                        if (isLevelStart)
                        {
                            CustomTriggerStateManager.SetForLevelStart("forceCustomBros", true);
                            CustomTriggerStateManager.SetForLevelStart("forcedCustomBroList", storedHeroes);
                        }
                        else
                        {
                            CustomTriggerStateManager.SetDuringLevel("forceCustomBros", true);
                            CustomTriggerStateManager.SetDuringLevel("forcedCustomBroList", storedHeroes);
                        }
                    }
                }
                else
                {
                    Map.MapData.forcedBro = HeroType.Random;
                    if (isLevelStart)
                    {
                        CustomTriggerStateManager.SetForLevelStart("forceCustomBros", false);
                    }
                    else
                    {
                        CustomTriggerStateManager.SetDuringLevel("forceCustomBros", false);
                    }
                }
            }
            catch (Exception ex)
            {
                BMLogger.Log("Exception in force bro trigger: " + ex.ToString());
            }
        }
    }
}
