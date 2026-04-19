using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Passives
{
    /// <summary>Broden's double-tap-down teleport.</summary>
    [PassivePreset("Broden")]
    public class BrodenPassive : PassiveAbility
    {
        protected override HeroType SourceBroType => HeroType.Broden;

        protected override bool IsOwnerRedundant(BroBase owner) => owner is Broden;

        /// <summary>Maximum time between two down-presses that counts as a double-tap, in seconds.</summary>
        public float doubleTapWindow = 0.2f;
        /// <summary>Upward velocity impulse applied at the midpoint of the teleport animation.</summary>
        public float teleportYBoost = 160f;
        public int reverseAnimationColumn = 25;
        /// <summary>Radius of the close-range mook search for teleport target selection, in world units.</summary>
        public float searchRangeNear = 32f;
        /// <summary>Horizontal radius of the far-range mook search for teleport target selection, in world units.</summary>
        public float searchRangeFarX = 90f;
        /// <summary>Vertical radius of the far-range mook search for teleport target selection, in world units.</summary>
        public float searchRangeFarY = 44f;
        /// <summary>Duration after teleport during which nearby mooks are not alerted, in seconds.</summary>
        public float alertSuppressionTime = 0.5f;

        /// <summary>Sounds played at teleport start and end.</summary>
        public AudioClip[] teleportSounds;

        public BrodenPassive()
        {
            animationRow = 13;
            frameRate = 0.025f;
        }

        [JsonIgnore] private bool teleporting;
        [JsonIgnore] private int teleportFrame;
        [JsonIgnore] private float lastDownPressTime;

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as Broden;
            if (sourceBro == null) return;
            if (teleportSounds == null) teleportSounds = sourceBro.soundHolder.attackSounds.CloneArray();
        }

        public override void HandleAfterCheckInput()
        {
            if (owner.health <= 0 || teleporting || hero.UsingSpecial) return;
            if (owner.down && !owner.wasDown && owner.actionState != ActionState.ClimbingLadder)
            {
                if (Time.realtimeSinceStartup - lastDownPressTime < doubleTapWindow)
                {
                    StartTeleporting();
                }
                lastDownPressTime = Time.realtimeSinceStartup;
            }
        }

        private void StartTeleporting()
        {
            teleporting = true;
            teleportFrame = 0;
            hero.ChangeFrame();
            owner.invulnerable = true;
            hero.FrameRate = frameRate;
            sound.PlaySoundEffectAt(teleportSounds, 0.3f, owner.transform.position, 0.5f, true, false, false, 0f);
        }

        public override bool HandleChangeFrame()
        {
            if (!teleporting) return true;

            hero.FrameRate = frameRate;
            hero.DeactivateGun();
            teleportFrame++;
            if (teleportFrame <= 12)
            {
                hero.Sprite.SetLowerLeftPixel((float)(teleportFrame * hero.SpritePixelWidth), (float)(animationRow * hero.SpritePixelHeight));
            }
            else if (teleportFrame <= 25)
            {
                hero.Sprite.SetLowerLeftPixel((float)((reverseAnimationColumn - teleportFrame) * hero.SpritePixelWidth), (float)(animationRow * hero.SpritePixelHeight));
            }

            if (teleportFrame == 12)
            {
                teleportFrame += 5;
                owner.yI = teleportYBoost;
                Reposition();
            }

            if (teleportFrame == 25)
            {
                owner.invulnerable = false;
                teleporting = false;
                lastDownPressTime = 0f;
                owner.SetFieldValue("lastAlertTime", Time.time + alertSuppressionTime);
                sound.PlaySoundEffectAt(teleportSounds, 0.1f, owner.transform.position, 0.66f, true, false, false, 0f);
                hero.ActivateGun();
            }
            return false;
        }

        private void Reposition()
        {
            float sign = owner.transform.localScale.x;
            Mook mook = Map.GetNearbyMook(searchRangeNear, searchRangeNear, X + sign * 7f, Y, (int)sign, false);
            if (mook == null)
            {
                mook = Map.GetNearbyMook(searchRangeFarX, searchRangeFarY, X + sign * 7f, Y, (int)sign, false);
            }

            if (mook != null)
            {
                Map.ForgetPlayer(PlayerNum, X, Y, 48f, true, false);
                if (!Map.IsBlockSolid(mook.X + sign * 24f, mook.Y + 1f) && Map.IsBlockSolid(mook.X + sign * 22f, mook.Y - 8f))
                {
                    owner.X = mook.X + sign * 24f;
                    owner.Y = mook.Y;
                }
                else if (!Map.IsBlockSolid(mook.X + sign * 14f, mook.Y + 1f) && Map.IsBlockSolid(mook.X + sign * 13f, mook.Y - 8f))
                {
                    owner.X = mook.X + sign * 14f;
                    owner.Y = mook.Y;
                }
                else
                {
                    owner.X = mook.X + sign * 8f;
                    owner.Y = mook.Y;
                }
                owner.xI = sign * -0.01f;
                owner.xIBlast = 0f;
                owner.CallMethod("CheckFacingDirection");
                return;
            }

            if (TryStep(60f, 0f)) return;
            if (TryStep(60f, 16f)) return;
            if (TryStep(60f, -16f)) return;
            if (TryStep(44f, 0f)) return;
            if (TryStep(44f, 16f)) return;
            if (TryStep(44f, -16f)) return;
            if (TryStep(72f, 0f)) return;
            if (TryStep(72f, 16f)) return;
            if (TryStep(72f, -16f)) return;
            if (TryStep(88f, 0f)) return;
            if (TryStep(28f, 0f)) return;
            if (TryStep(12f, 0f)) return;
            TryStep(6f, 0f);
        }

        private bool TryStep(float dx, float dy)
        {
            float sign = owner.transform.localScale.x;
            if (!Map.IsBlockEmpty(Map.GetCollumn(X + dx), Map.GetRow(Y + dy))) return false;
            owner.X += sign * dx;
            owner.Y += dy;
            return true;
        }

        public override bool HandleIsInStealthMode(ref bool result)
        {
            if (teleporting) { result = true; return false; }
            return true;
        }
    }
}
