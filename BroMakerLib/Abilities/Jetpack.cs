using BroMakerLib.Attributes;
using RocketLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Net = Networking.Networking;

namespace BroMakerLib.Abilities
{
    [AbilityPreset("JetPack")]
    public class Jetpack : CharacterAbility
    {
        public bool activeSprite = false;
        public bool active = false;
        public float jetpackMaxTime = 0.66f;
        public bool blastOffFlames = false;
        public int blastOffFlamesDamages = 3;

        protected Countdown jetpackTime;
        protected float flameCounter;

        public override void Use()
        {
            UseJetpack();
        }

        public override void UseSpecial()
        {
            UseJetpack();
        }

        protected override void Awake()
        {
            jetpackTime = new Countdown(jetpackMaxTime);
        }

        public override void Jump(bool wallJump)
        {
            UseJetpack();
        }

        public override void Land()
        {
            active = false;
        }

        protected override void Update()
        {
            // BroBase.Update() -> part of jetpack
            if (active)
            {
                jetpackTime.Update(DT);
                if (jetpackTime.IsOver)
                {
                    NoMoreFuel();
                }
                else if (jetpackTime.Time >= 0.16f)
                {
                    float num = Mathf.Clamp(1.33f - owner.yI / 250f, 0f, 1.5f);
                    if (jetpackTime.Time > 0.36f)
                    {
                        owner.yI += 500f * DT * num;
                    }
                    owner.yI += 600f * DT * num;

                    flameCounter += DT;
                    if (flameCounter > 0.0225f)
                    {
                        flameCounter -= 0.02f;
                        CreateJetpackFlames(Vector3.down);
                    }
                }
            }
        }

        protected virtual void UseJetpack()
        {
            if (owner.Y < Map.GetGroundHeight(owner.X, owner.Y + 2f) + 20f)
            {
                if(blastOffFlames)
                    CreateBlastOffFlames(Map.GetGroundHeight(owner.X, owner.Y + 2f));
                owner.yI += 30f;
                if (owner.yI < 140f)
                {
                    owner.yI = 140f;
                }
            }
            else
            {
                owner.yI += 60f;
                if (owner.yI < -50f)
                {
                    owner.yI = -50f;
                }
            }
            active = true;
            jetpackTime.Reset();
            SetSpriteActive(activeSprite);
        }


        protected virtual void NoMoreFuel()
        {
            SetSpriteActive(false);
            active = false;
        }

        protected virtual void SetSpriteActive(bool active)
        {
            var bro = owner as BroBase;
            if (bro && bro.jetPackSprite != null)
            {
                bro.jetPackSprite.gameObject.SetActive(false);
            }
        }

        // BroBase
        protected virtual void CreateJetpackFlames(Vector3 currentJetpackDirection)
        {
            EffectsController.CreatePlumeParticle(owner.X - 2.5f - owner.transform.localScale.x * 3f, owner.Y + 7f, 4f, 4f, currentJetpackDirection.x * 0.15f, ((owner.yI <= 0f) ? 0f : (owner.yI * 0.5f)) - 70f, 0.4f, 1.3f);
            EffectsController.CreatePlumeParticle(owner.X + 2.5f - owner.transform.localScale.x * 3f, owner.Y + 7f, 4f, 4f, currentJetpackDirection.x * 0.15f, ((owner.yI <= 0f) ? 0f : (owner.yI * 0.5f)) - 70f, 0.4f, 1.3f);
        }

        // BroBase
        protected virtual void CreateBlastOffFlames(float yPos)
        {
            if (blastOffFlames && !Map.isEditing && owner.IsMine)
            {
                FlameWallExplosion obj = Net.Instantiate(EffectsController.instance.liftOffBlastFlameWall, new Vector3(owner.X - owner.transform.localScale.x * 5f, yPos + 9f, 0f), Quaternion.identity);
                DirectionEnum arg = DirectionEnum.Any;
                Net.RPC(PID.TargetAll, new RpcSignature<int, TestVanDammeAnim, DirectionEnum>(obj.Setup), owner.playerNum, owner, arg);
                Map.HitUnits(owner, owner, owner.playerNum, blastOffFlamesDamages, DamageType.Fire, 14f, 10f, owner.X, yPos + 5f, 0f, 50f, true, false, true);
            }
        }
    }
}
