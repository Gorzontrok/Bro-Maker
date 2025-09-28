using System;
using System.Collections.Generic;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using UnityEngine;

namespace BroMakerLib.Triggers
{
    class ForceBroTriggerActionInfo : RocketLib.CustomTriggers.CustomTriggerActionInfo
    {
        public List<string> ForcedBros = new List<string>();
        public bool RunAtLevelStart = true;
        public bool EnableForced = true;

        public override void ShowGUI(LevelEditorGUI gui)
        {
            EnableForced = GUILayout.Toggle(EnableForced, "Enable custom bro forced spawn");
            EnableForced = !GUILayout.Toggle(!EnableForced, "Disable custom bro forced spawn");
            RunAtLevelStart = GUILayout.Toggle(RunAtLevelStart, "Run at level start");
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

    public class ForceBroTriggerAction : RocketLib.CustomTriggers.CustomTriggerAction
    {
        ForceBroTriggerActionInfo info;

        public override TriggerActionInfo Info
        {
            get
            {
                return this.info;
            }
            set
            {
                this.info = (ForceBroTriggerActionInfo)value;
                if (this.info.RunAtLevelStart)
                {
                    this.RunAction();
                }
            }
        }

        public override void Start()
        {
            base.Start();

            if (!this.info.RunAtLevelStart)
            {
                this.RunAction();
            }
            this.state = TriggerActionState.Done;
        }

        public override void Update()
        {
        }

        private void RunAction()
        {
            try
            {
                if (this.info.EnableForced)
                {
                    List<StoredHero> storedHeroes = new List<StoredHero>();
                    foreach (string broName in this.info.ForcedBros)
                    {
                        StoredHero hero = BroMakerStorage.GetStoredHeroByName(broName);
                        if (hero != null)
                        {
                            storedHeroes.Add(hero);
                        }
                    }
                    if (storedHeroes.Count > 0)
                    {
                        // Only set this value if we're running at level start, otherwise midlevel triggers could carry over into other levels
                        if (this.info.RunAtLevelStart)
                        {
                            BroSpawnManager.StartForcingCustom = true;
                        }
                        // Set this if not running at level start
                        else
                        {
                            BroSpawnManager.ForceCustomThisLevel = true;
                        }
                        BroSpawnManager.ForcedCustoms = storedHeroes;
                    }
                }
                else
                {
                    Map.MapData.forcedBro = HeroType.Random;
                    BroSpawnManager.ForceCustomThisLevel = false;
                }
            }
            catch (Exception ex)
            {
                BMLogger.Log("Exception in force bro trigger: " + ex.ToString());
            }
        }
    }
}
