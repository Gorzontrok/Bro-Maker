using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BrondleFly")]
    public class BrondleFlySpecial : SpecialAbility
    {
        public float cooldownTime = 0.12f;
        public float targetRange = 80f;
        public float maxTargetAngle = 50f;
        public float secondarySearchRange = 32f;
        public float teleportMinDistance = 16f;
        public float teleportSoundVolume = 0.5f;
        public float camLerpSpeed = 2f;
        public int satanDamage = 50;
        public float jumpYI = 120f;

        [JsonIgnore]
        private float cooldownTimer;
        [JsonIgnore]
        private float camLerp = 1f;
        [JsonIgnore]
        private Vector3 teleportPos;
        [JsonIgnore]
        private SpriteSM teleportOutAnimation;
        [JsonIgnore]
        private SpriteSM teleportInAnimation;
        public AudioClip teleportSound;
        [JsonIgnore]
        private Material coveredInBloodMaterial;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var brondleFly = owner as BrondleFly;
            if (brondleFly == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.BrondleFly);
                brondleFly = prefab as BrondleFly;
            }
            if (brondleFly != null)
            {
                teleportOutAnimation = brondleFly.teleportOutAnimation;
                teleportInAnimation = brondleFly.teleportInAnimation;
                if (teleportSound == null)
                    teleportSound = brondleFly.teleportSound;
                coveredInBloodMaterial = brondleFly.coveredInBloodMaterial;
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0 && !owner.hasBeenCoverInAcid)
            {
                if (cooldownTimer <= 0f)
                {
                    cooldownTimer = cooldownTime;
                    if (owner.IsMine)
                    {
                        TeleFrag();
                    }
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
            }
        }

        private void TeleFrag()
        {
            teleportPos = owner.transform.position;
            camLerp = 0f;

            Vector3 direction = (owner.left ? Vector3.left : Vector3.zero)
                + (owner.right ? Vector3.right : Vector3.zero)
                + (owner.up ? Vector3.up : Vector3.zero)
                + (owner.down ? Vector3.down : Vector3.zero);
            if (direction != Vector3.zero)
            {
                direction.Normalize();
            }
            else
            {
                direction = new Vector3(owner.transform.localScale.x, 0f, 0f);
            }

            Vector3 targetPos = owner.transform.position + direction * targetRange;
            List<Unit> units = Map.GetUnitsInRange(targetRange, targetRange, X, Y, false);
            float bestAngle = maxTargetAngle;
            Unit bestUnit = null;
            foreach (Unit unit in units)
            {
                if (unit.IsEnemy && !(unit is Tank))
                {
                    Vector3 toUnit = unit.transform.position - owner.transform.position;
                    float angle = Vector3.Angle(toUnit, direction);
                    if (angle < bestAngle)
                    {
                        bestAngle = angle;
                        bestUnit = unit;
                    }
                }
            }

            bool canTeleport = false;
            if (bestUnit == null)
            {
                canTeleport = SearchForOpenSpot(ref targetPos, direction);
                units = Map.GetUnitsInRange(secondarySearchRange, secondarySearchRange, targetPos.x, targetPos.y, false);
                if (units.Count > 0)
                {
                    foreach (Unit unit in units)
                    {
                        if (unit.IsEnemy && !(unit is Tank))
                        {
                            bestUnit = unit;
                            canTeleport = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                canTeleport = true;
            }

            if (bestUnit != null)
            {
                targetPos = bestUnit.transform.position;
                if (!(bestUnit is SatanMiniboss))
                {
                    bestUnit.Damage(bestUnit.health, DamageType.Normal, 500f, 500f, (int)Direction, owner, bestUnit.X, bestUnit.Y);
                    bestUnit.GibNow(DamageType.Crush, 0f, 100f);
                }
                else
                {
                    bestUnit.Damage(satanDamage, DamageType.Knock, 0f, 360f, (int)Direction, owner, bestUnit.X, bestUnit.Y - 2f);
                }
                if (coveredInBloodMaterial != null)
                {
                    owner.SetFieldValue("defaultMaterial", coveredInBloodMaterial);
                    owner.SetFieldValue("isBloody", true);
                    owner.GetComponent<Renderer>().material = coveredInBloodMaterial;
                }
            }

            if (teleportOutAnimation != null)
            {
                SpriteSM outEffect = Object.Instantiate(teleportOutAnimation);
                outEffect.transform.position = owner.transform.position;
            }

            if (canTeleport)
            {
                if (Mathf.Abs(targetPos.x - X) > teleportMinDistance || Mathf.Abs(targetPos.y - Y) > teleportMinDistance)
                {
                    owner.SpecialAmmo--;
                }
                hero.PressSpecialFacingDirection = 0;
                owner.SetXY(targetPos.x, targetPos.y);
                owner.SetPosition();
            }

            owner.xI = 0f;
            if (owner.up)
            {
                owner.SetFieldValue("jumpTime", 0.1f);
                owner.yI = jumpYI;
            }
            else
            {
                owner.yI = 0f;
            }

            if (teleportInAnimation != null)
            {
                SpriteSM inEffect = Object.Instantiate(teleportInAnimation);
                inEffect.transform.parent = owner.transform;
                inEffect.transform.localPosition = -Vector3.forward;
            }

            Sound.GetInstance().PlayAudioClip(teleportSound, owner.transform.position, teleportSoundVolume,
                Random.Range(0.8f, 1.2f), false, false, 0f, false, false);

            hero.UsingSpecial = false;
        }

        private bool SearchForOpenSpot(ref Vector3 pos, Vector3 direction)
        {
            direction.Normalize();
            Vector3 original = pos;
            bool found = false;
            int i = 0;
            while (i < 5 && !found)
            {
                if (!Map.IsWithinBounds(pos) || Physics.CheckSphere(pos, 2f, Map.groundLayer))
                {
                    pos += Vector3.up * 16f;
                }
                else
                {
                    found = true;
                }
                i++;
            }
            if (!found)
            {
                pos = original + direction * 16f + Vector3.up * 4f;
                for (int j = 0; j < 6; j++)
                {
                    if (!Map.IsWithinBounds(pos) || Physics.CheckSphere(pos, 2f, Map.groundLayer))
                    {
                        pos -= direction * 16f;
                    }
                    else
                    {
                        found = true;
                    }
                }
            }
            if (found)
            {
                RaycastHit hit;
                if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 32f, Map.groundLayer))
                {
                    pos = hit.point;
                }
                return true;
            }
            pos = original;
            return false;
        }

        public override void Update()
        {
            if (camLerp < 1f)
            {
                camLerp += Time.deltaTime * camLerpSpeed;
                camLerp = Mathf.Clamp01(camLerp);
            }
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        public override bool HandleGetFollowPosition(ref Vector3 result)
        {
            if (camLerp < 1f)
            {
                Vector3 lerped = Vector3.Lerp(teleportPos, owner.transform.position, camLerp);
                result = new Vector3(lerped.x, lerped.y, 0f);
                return false;
            }
            return true;
        }
    }
}
