using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Bronnar Jensen's remote-controlled explosive car special.</summary>
    [SpecialPreset("BronnarJensen")]
    public class BronnarJensenSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.BronnarJensen;

        [JsonIgnore]
        private RemoteControlExplosiveCar remoteControlVehiclePrefab;
        [JsonIgnore]
        private RemoteControlExplosiveCar remoteVehicle;
        [JsonIgnore]
        private float projectileTime;
        [JsonIgnore]
        private float controllingRCVDelay;

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);

            var bronnar = owner as BronnarJensen;
            if (bronnar == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.BronnarJensen);
                bronnar = prefab as BronnarJensen;
            }
            if (bronnar != null)
            {
                remoteControlVehiclePrefab = bronnar.remoteControlVehiclePrefab;
            }
        }

        public override void PressSpecial()
        {
            if (!hero.DoingMelee && owner.SpecialAmmo > 0 && owner.health > 0)
            {
                hero.UsingSpecial = true;
                owner.frame = 0;
                hero.GunFrame = 4;
                hero.ChangeFrame();
            }
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.ActivateGun();
            hero.FrameRate = frameRate;
            hero.SetGunSprite(5 - owner.frame, 0);
            if (owner.frame == 0)
            {
                UseSpecial();
            }
            if (owner.frame >= 5)
            {
                hero.GunFrame = 0;
                owner.frame = 0;
                hero.UsingSpecial = false;
            }
        }

        public override void UseSpecial()
        {
            if (owner.GetFieldValue<bool>("hasBeenCoverInAcid")) return;

            if (owner.SpecialAmmo > 0 && remoteVehicle == null)
            {
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    projectileTime = Time.time;
                    remoteVehicle = Networking.Networking.Instantiate<RemoteControlExplosiveCar>(
                        remoteControlVehiclePrefab, owner.transform.position, Quaternion.identity, false);
                    remoteVehicle.playerNum = PlayerNum;
                    remoteVehicle.Knock(DamageType.Bounce, owner.transform.localScale.x * 100f, 100f, false);
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
        }

        public override void HandleAfterCheckInput()
        {
            if (remoteVehicle != null && remoteVehicle.gameObject.activeInHierarchy)
            {
                remoteVehicle.wasUp = remoteVehicle.up;
                remoteVehicle.wasDown = remoteVehicle.down;
                remoteVehicle.wasLeft = remoteVehicle.left;
                remoteVehicle.wasRight = remoteVehicle.right;
                remoteVehicle.wasButtonJump = remoteVehicle.buttonJump;
                remoteVehicle.up = owner.up;
                remoteVehicle.down = owner.down;
                remoteVehicle.left = owner.left;
                remoteVehicle.right = owner.right;
                remoteVehicle.buttonJump = owner.buttonJump;
                owner.up = false;
                owner.left = false;
                owner.right = false;
                owner.down = false;
                owner.buttonJump = false;
                if (Time.time - projectileTime > 0.56f && ((owner.special && !owner.wasSpecial) || (owner.fire && !owner.wasFire)))
                {
                    hero.ControllingProjectile = false;
                    hero.UsingSpecial = false;
                    remoteVehicle.Explode();
                }
                hero.UsingSpecial = false;
                owner.fire = false;
                controllingRCVDelay = 0.25f;
            }
            else
            {
                if (controllingRCVDelay > 0f)
                {
                    controllingRCVDelay -= hero.DeltaTime;
                    owner.up = false;
                    owner.left = false;
                    owner.right = false;
                    owner.down = false;
                    hero.UsingSpecial = false;
                    owner.fire = false;
                }
                hero.ControllingProjectile = false;
            }
        }

        public override bool HandleDeath(float xI, float yI, DamageObject damage)
        {
            if (remoteVehicle != null)
            {
                remoteVehicle.Explode();
            }
            return true;
        }
    }
}
