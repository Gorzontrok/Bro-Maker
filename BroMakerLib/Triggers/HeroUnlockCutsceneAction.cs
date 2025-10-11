using BroMakerLib.Storages;
using BroMakerLib.Unlocks;
using UnityEngine;

namespace BroMakerLib.Triggers
{
    public class HeroUnlockCutsceneActionInfo : RocketLib.CustomTriggers.CustomTriggerActionInfo
    {
        public bool finishCampaignAfterCutscene = true;
        public string broName = string.Empty;
        public float delay = 1f;
        public override void ShowGUI(LevelEditorGUI gui)
        {
            finishCampaignAfterCutscene = GUILayout.Toggle(finishCampaignAfterCutscene, "Finish campaign after cutscene");
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Delay:", new GUILayoutOption[0]);
            float.TryParse(GUILayout.TextField(delay.ToString("0.00"), new GUILayoutOption[0]), out delay);
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            foreach (StoredHero hero in BroMakerStorage.Bros)
            {
                bool thisBro = hero.name == broName;
                if (thisBro)
                {
                    if (!GUILayout.Toggle(true, hero.name))
                    {
                        this.broName = string.Empty;
                    }
                }
                else if (GUILayout.Toggle(false, hero.name))
                {
                    this.broName = hero.name;
                }
            }
        }
    }

    public class HeroUnlockCutsceneAction : RocketLib.CustomTriggers.CustomTriggerAction<HeroUnlockCutsceneActionInfo>
    {
        public float timer = 0f;
        StoredHero hero;

        public override void Start()
        {
            base.Start();

            // Skip cutscene if already unlocked or if not found
            if (!BroMakerStorage.GetStoredHeroByName(this.info.broName, out this.hero) || BroUnlockManager.IsBroUnlocked(this.info.broName))
            {
                this.state = TriggerActionState.Done;
            }
            else
            {
                this.state = TriggerActionState.Busy;
            }
        }

        public override void Update()
        {
            timer += Time.deltaTime;
            if (timer >= this.info.delay)
            {
                try
                {
                    BroUnlockManager.CheckLevelUnlocks(LevelSelectionController.currentCampaign.name);
                    Cutscenes.CustomCutsceneController.LoadHeroCutscene(BroMakerUtilities.GetVariantValue(hero.GetInfo().Cutscene, 0), 0.2f, true);
                    this.state = TriggerActionState.Done;
                }
                catch
                {
                    this.state = TriggerActionState.Done;
                }
            }
        }
    }
}
