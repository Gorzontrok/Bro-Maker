using BroMakerLib.Storages;
using BroMakerLib.Unlocks;
using UnityEngine;

namespace BroMakerLib.Triggers
{
    class HeroUnlockCutsceneActionInfo : RocketLib.CustomTriggers.CustomTriggerActionInfo
    {
        public bool finishCampaignAfterCutscene = true;
        public string broName = string.Empty;
        public override void ShowGUI(LevelEditorGUI gui)
        {
            finishCampaignAfterCutscene = GUILayout.Toggle(finishCampaignAfterCutscene, "Finish campaign after cutscene");
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

    public class HeroUnlockCutsceneAction : RocketLib.CustomTriggers.CustomTriggerAction
    {
        HeroUnlockCutsceneActionInfo info;

        public override TriggerActionInfo Info
        {
            get
            {
                return this.info;
            }
            set
            {
                this.info = (HeroUnlockCutsceneActionInfo)value;
            }
        }

        public override void Start()
        {
            base.Start();

            if (BroMakerStorage.GetStoredHeroByName(this.info.broName, out StoredHero hero) && !BroUnlockManager.IsBroUnlocked(this.info.broName))
            {
                Cutscenes.CustomCutsceneController.LoadHeroCutscene(BroMakerUtilities.GetVariantValue(hero.GetInfo().Cutscene, 0), 0.2f, true);
            }
            this.state = TriggerActionState.Done;
        }

        public override void Update()
        {
        }
    }
}
