using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("ScorpionBro")]
    public class ScorpionBroSpecial : SpecialAbility
    {
        public float launchForce = 250f;
        public float launchDuration = 1f;

        [JsonIgnore]
        private bool isInScorpionMode;
        [JsonIgnore]
        private bool isLaunching;
        [JsonIgnore]
        private Vector2 launchDirection;
        [JsonIgnore]
        private float defaultJumpForce;
        [JsonIgnore]
        private Material scorpionModeMaterial;
        [JsonIgnore]
        private Material normalMaterial;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            defaultJumpForce = owner.jumpForce;

            var scorpionBro = owner as ScorpionBro;
            if (scorpionBro == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.ScorpionBro);
                scorpionBro = prefab as ScorpionBro;
            }
            if (scorpionBro != null)
            {
                scorpionModeMaterial = scorpionBro.scorpionModeMaterial;
                launchForce = scorpionBro.launchForce;
                launchDuration = scorpionBro.launchDuration;
            }
        }

        public override void UseSpecial()
        {
            if (isInScorpionMode)
            {
                if (owner.IsMine)
                {
                    ExitScorpionMode();
                }
            }
            else
            {
                if (owner.SpecialAmmo > 0)
                {
                    if (owner.IsMine)
                    {
                        EnterScorpionMode();
                    }
                }
                else
                {
                    HeroController.FlashSpecialAmmo(PlayerNum);
                    hero.ActivateGun();
                }
                hero.PressSpecialFacingDirection = 0;
            }
        }

        private void EnterScorpionMode()
        {
            if (normalMaterial == null)
            {
                normalMaterial = owner.GetComponent<Renderer>().sharedMaterial;
            }
            isInScorpionMode = true;
            isLaunching = false;
            if (scorpionModeMaterial != null)
            {
                owner.GetComponent<Renderer>().sharedMaterial = scorpionModeMaterial;
            }
            hero.DeactivateGun();
        }

        private void ExitScorpionMode()
        {
            isInScorpionMode = false;
            owner.jumpForce = defaultJumpForce;
            isLaunching = false;
            if (normalMaterial != null)
            {
                owner.GetComponent<Renderer>().sharedMaterial = normalMaterial;
            }
            hero.ActivateGun();
        }

        private Vector2 GetInputVector()
        {
            Vector2 dir = Vector2.zero;
            if (owner.left) dir.x = -1f;
            else if (owner.right) dir.x = 1f;
            if (owner.up) dir.y = 1f;
            else if (owner.down) dir.y = -1f;
            return dir;
        }

        private void ScorpionLaunch(Vector2 direction)
        {
            isLaunching = false;
            if (owner.IsHanging())
            {
                owner.CallMethod("StopHanging");
            }
            launchDirection = direction.normalized;
            RaycastHit hit;
            if (Physics.Raycast(owner.transform.position + Vector3.up * 6f, launchDirection.normalized, out hit, 160f,
                hero.GroundLayer | owner.GetFieldValue<LayerMask>("fragileLayer")))
            {
                isLaunching = true;
            }
        }

        private void StickToSurface(bool isCeiling)
        {
            owner.xI = 0f;
            owner.yI = 0f;
            owner.Stop();
            isLaunching = false;
            if (isCeiling)
            {
                owner.CallMethod("StartHanging");
            }
        }

        public override bool HandleJump(bool wallJump)
        {
            if (isInScorpionMode && !hero.WallDrag)
            {
                ScorpionLaunch(GetInputVector());
                if (isLaunching)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool HandleRunMovement()
        {
            if (isInScorpionMode && isLaunching)
            {
                owner.xI = launchDirection.x * launchForce;
                owner.yI = launchDirection.y * launchForce;
            }
            return true;
        }

        public override bool HandleApplyNormalGravity()
        {
            return !isInScorpionMode || !isLaunching;
        }

        public override bool HandleStartFiring()
        {
            return !isInScorpionMode;
        }

        public override bool HandleRunFiring()
        {
            return !isInScorpionMode;
        }

        public override bool HandleStartMelee()
        {
            if (isInScorpionMode && (owner.IsHanging() || hero.WallDrag))
            {
                return false;
            }
            return true;
        }

        public override void HandleAfterHitCeiling()
        {
            if (isInScorpionMode && isLaunching)
            {
                StickToSurface(true);
            }
        }

        public override void HandleAfterHitLeftWall()
        {
            if (isInScorpionMode && isLaunching)
            {
                StickToSurface(false);
            }
        }

        public override void HandleAfterHitRightWall()
        {
            if (isInScorpionMode && isLaunching)
            {
                StickToSurface(false);
            }
        }

        public override bool HandleClampWallDragYI(ref float yIT)
        {
            if (isInScorpionMode)
            {
                owner.yI = 0f;
                yIT = 0f;
                return false;
            }
            return true;
        }

        public override bool HandleRunHanging()
        {
            if (!isInScorpionMode)
            {
                return true;
            }

            if (owner.up || owner.buttonJump)
            {
                owner.yI = 20f;
            }
            else
            {
                owner.yI = 0f;
            }
            if (!owner.right && !owner.left)
            {
                owner.xI *= 1f - hero.DeltaTime * 17f;
            }
            if (owner.up || owner.buttonJump)
            {
                owner.SetFieldValue("hangGrace", 0.1f);
            }

            bool canClimb = true;
            HandleCanCheckClimbAlongCeiling(ref canClimb);
            if (!canClimb || owner.down)
            {
                owner.SetFieldValue("hangGrace", 0f);
                owner.CallMethod("StopHanging");
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(X + 4f, Y + 5f, 0f), Vector3.up, out hit, owner.headHeight, hero.GroundLayer))
                {
                    owner.doodadCurrentlyHangingFrom = hit.collider.gameObject.GetComponent<JiggleDoodad>();
                }
                else if (Physics.Raycast(new Vector3(X - 4f, Y + 5f, 0f), Vector3.up, out hit, owner.headHeight, hero.GroundLayer))
                {
                    owner.doodadCurrentlyHangingFrom = hit.collider.gameObject.GetComponent<JiggleDoodad>();
                }
                else
                {
                    owner.CallMethod("StopHanging");
                    owner.SetFieldValue("currentFootStepGroundType", "Stone");
                }
            }
            return false;
        }

        public override bool HandleCanCheckClimbAlongCeiling(ref bool result)
        {
            if (isInScorpionMode)
            {
                result = !hero.Ducking && !owner.down;
                return false;
            }
            return true;
        }

        public override bool HandleCheckClimbAlongCeiling()
        {
            if (!isInScorpionMode)
            {
                return true;
            }

            bool canCheck = true;
            HandleCanCheckClimbAlongCeiling(ref canCheck);
            bool canCeilingHang = owner.GetFieldValue<bool>("canCeilingHang");
            if (canCeilingHang && canCheck && owner.actionState == ActionState.Jumping
                && !owner.up && !owner.buttonJump)
            {
                bool wasConstrainedLeft = owner.GetFieldValue<bool>("wasConstrainedLeft");
                bool constrainedLeft = owner.GetFieldValue<bool>("constrainedLeft");
                bool wasConstrainedRight = owner.GetFieldValue<bool>("wasConstrainedRight");
                bool constrainedRight = owner.GetFieldValue<bool>("constrainedRight");
                float halfWidth = owner.GetFieldValue<float>("halfWidth");
                float headHeight = owner.headHeight;

                if ((wasConstrainedLeft && !constrainedLeft) || (wasConstrainedRight && !constrainedRight))
                {
                    RaycastHit hit;
                    if (owner.right && Physics.Raycast(new Vector3(X + halfWidth + 4f, Y + 5f, 0f), Vector3.up, out hit, headHeight + 14f, hero.GroundLayer))
                    {
                        owner.doodadCurrentlyHangingFrom = hit.collider.gameObject.GetComponent<JiggleDoodad>();
                        owner.SetFieldValue("hangGrace", owner.GetFieldValue<float>("hangGraceTime"));
                        owner.SetXY(X, Map.GetBlockCenterY(owner.GetFieldValue<int>("row")) - 16f);
                        owner.yI = 50f;
                    }
                    else if (owner.left && Physics.Raycast(new Vector3(X - halfWidth - 4f, Y + 5f, 0f), Vector3.up, out hit, headHeight + 14f, hero.GroundLayer))
                    {
                        owner.doodadCurrentlyHangingFrom = hit.collider.gameObject.GetComponent<JiggleDoodad>();
                        owner.SetFieldValue("hangGrace", owner.GetFieldValue<float>("hangGraceTime"));
                        owner.SetXY(X, Map.GetBlockCenterY(owner.GetFieldValue<int>("row")) - 16f);
                        owner.yI = 50f;
                    }
                }
            }
            return false;
        }
    }
}
