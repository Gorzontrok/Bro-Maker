using BroMakerLib.Attributes;

namespace BroMakerLib.Abilities.Characters
{
    /// <summary>
    /// Use a Jetpack. Not fully functionnal for some reason
    /// </summary>
    [AbilityPreset("JetPack")]
    public class Jetpack : CharacterAbility
    {
        public bool ShowSprite = false;
        public float JetpackMaxTime = 0.66f;
        public bool BlastOffFlames = false;
        public int BlastOffFlamesDamages = 3;

        public override void UseSpecial()
        {
            UseJetpack();
        }

        public override void Jump(bool wallJump)
        {
            if (!wallJump)
                UseJetpack();
        }

        protected virtual void UseJetpack()
        {
            owner.CallMethod("UseJetpack");
            if (BlastOffFlames) // don't works
                owner.CallMethod("CreateBlastOffFlames", Map.GetGroundHeight(owner.X, owner.Y + 2f));
            SetSpriteActive(ShowSprite);
            owner.SetFieldValue("jetPackTime", JetpackMaxTime);
        }

        protected virtual void SetSpriteActive(bool active)
        {
            var bro = owner as BroBase;
            if (bro && bro.jetPackSprite != null)
            {
                bro.jetPackSprite.gameObject.SetActive(active);
            }
        }
    }
}
