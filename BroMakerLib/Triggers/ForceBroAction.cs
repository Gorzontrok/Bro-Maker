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
                        if (BroMakerStorage.GetStoredHeroByName(broName, out StoredHero hero) && BroMakerNetwork.IsBroAvailable(hero))
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
                            CustomTriggerStateManager.RegisterLevelStartAction(() =>
                            {
                                if (Map.MapData != null)
                                {
                                    Map.MapData.forcedBro = HeroType.Rambro;
                                }
                            });
                        }
                        else
                        {
                            CustomTriggerStateManager.SetDuringLevel("forceCustomBros", true);
                            CustomTriggerStateManager.SetDuringLevel("forcedCustomBroList", storedHeroes);
                            if (Map.MapData != null)
                            {
                                Map.MapData.forcedBro = HeroType.Rambro;
                            }
                        }
                    }
                    else if (info.ForcedBros.Count > 0)
                    {
                        BMLogger.Warning("Force Bro Action has no installed, version-compatible custom characters.");
                        if (isLevelStart)
                        {
                            CustomTriggerStateManager.SetForLevelStart("forceCustomBros", false);
                            CustomTriggerStateManager.SetForLevelStart("forcedCustomBroList", new List<StoredHero>());
                        }
                        else
                        {
                            CustomTriggerStateManager.SetDuringLevel("forceCustomBros", false);
                            CustomTriggerStateManager.SetDuringLevel("forcedCustomBroList", new List<StoredHero>());
                        }
                    }
                }
                else
                {
                    if (isLevelStart)
                    {
                        CustomTriggerStateManager.SetForLevelStart("forceCustomBros", false);
                        CustomTriggerStateManager.RegisterLevelStartAction(() =>
                        {
                            if (Map.MapData != null)
                            {
                                Map.MapData.forcedBro = HeroType.Random;
                            }
                        });
                    }
                    else
                    {
                        CustomTriggerStateManager.SetDuringLevel("forceCustomBros", false);
                        if (Map.MapData != null)
                        {
                            Map.MapData.forcedBro = HeroType.Random;
                        }
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
