using System;
using HarmonyLib;
using UnityEngine;
using BroMakerLib.Loaders;
using BroMakerLib.CustomObjects.Components;
using BroMakerLib.Loggers;

namespace BroMakerLib
{
    /// <summary>
    /// Base class of custom character
    /// </summary>
    [Obsolete("Use \'CustomBroFromFile\' class instead.")]
    public class BroBaseMaker : BroBase
    {
        /// <summary>
        /// Default material for the sprite
        /// </summary>
        public Material bm_DefaultMaterial
        {
            get
            {
                return defaultMaterial;
            }
            set
            {
                defaultMaterial = value;
            }
        }
        /// <summary>
        /// Sprite of the bro
        /// </summary>
        public SpriteSM Sprite
        {
            get
            {
                return sprite;
            }
            set
            {
                sprite = value;
            }
        }
        /// <summary>
        /// Default material for gun sprite
        /// </summary>
        public Material bm_DefaultGunMaterial;
        /// <summary>
        /// Default material for avatar
        /// </summary>
        public Material bm_avatarMaterial;
        /// <summary>
        /// Default material for ammo icon
        /// </summary>
        public Material bm_ammoMaterial;
        /// <summary>
        /// Default material for a second sprite
        /// </summary>
        public Material bm_secondMaterial;
        /// <summary>
        /// Default material for a second gun sprite
        /// </summary>
        public Material bm_secondGunMaterial;
        /// <summary>
        ///
        /// </summary>
        public Shrapnel bm_bulletShell;
        public bool bm_IsInGame = false;
        /// <summary>
        /// original speed of the character
        /// </summary>
        protected float bm_originalSpeed;
        protected int bm_fireCount;
        /// <summary>
        /// Change the range of the projectile. 400 is default
        /// </summary>
        protected float bm_ProjectileXRange = 400f;

        protected MeleeHolder meleeHolder;
        public virtual void bm_SetupBro(Player player)
        {
            bm_SetupBro();
        }

        /// <summary>
        /// This function is for setup the bro.
        /// </summary>
        public virtual void bm_SetupBro()
        {
            try
            {
               /* try
                {
                    this.player = player;
                   // this.playerNum = player.character.playerNum;
                    Main.Debug("player");
                }
                catch(Exception ex)
                {
                    Main.ExceptionLog("Failed assign player and playerNum", ex);
                }*/

                try
                {
                    Rambro rambroPrefab = HeroController.GetHeroPrefab(HeroType.Rambro).GetComponent<Rambro>();
                    sprite = gameObject.GetComponent<SpriteSM>();
                    gunSprite = HeroController.players[playerNum].character.gunSprite;
                    BMLogger.Debug("sprite");
                }
                catch(Exception ex)
                {
                    BMLogger.ExceptionLog("Failed assign SpriteSM", ex);
                }

                try
                {
                    if (defaultMaterial != null && sprite != null)
                    {
                        this.sprite.MeshRenderer.sharedMaterial = defaultMaterial;
                    }
                    else if(sprite != null)
                    {
                        this.sprite.MeshRenderer.sharedMaterial = defaultMaterial;
                    }
                    BMLogger.Debug("default material");
                    if (bm_DefaultGunMaterial != null && gunSprite != null)
                    {
                        gunSprite.GetComponent<Renderer>().material = bm_DefaultGunMaterial;
                        gunSprite.GetComponent<Renderer>().sharedMaterial = bm_DefaultGunMaterial;
                    }
                    else if (gunSprite != null && bm_DefaultGunMaterial != null)
                    {
                        gunSprite.GetComponent<Renderer>().material = bm_DefaultGunMaterial;
                        gunSprite.GetComponent<Renderer>().sharedMaterial = bm_DefaultGunMaterial;
                    }
                    BMLogger.Debug("gun material");
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog("Failed Assign Player and PlayerNum\n", ex);
                }


                this.transform.SetPositionAndRotation(player.character.transform.position, player.character.transform.rotation);
                UnityEngine.Object.Destroy(player.character.gameObject.GetComponent(BroMaker.GetBroType(player.character.heroType)));
                this.SetUpHero(playerNum, player.character.heroType, true);
                BMLogger.Debug("setup hero");

                if (bm_avatarMaterial != null)
                {
                    HeroController.SetAvatarMaterial(playerNum, bm_avatarMaterial);
                }
                else
                {
                    HeroController.SetAvatarMaterial(playerNum, ResourcesController.GetMaterialResource("avatar_empty.png", BroMaker.Shader1));
                }
                BMLogger.Debug("avatar material");
                if (bm_ammoMaterial != null)
                {
                    Traverse.Create(player.hud).Method("SetGrenadeMaterials", new object[] { bm_ammoMaterial }).GetValue();
                }
                else
                {
                    Traverse.Create(player.hud).Method("SetGrenadeMaterials", new object[] { ResourcesController.GetMaterialResource("grenadeIcon.png", BroMaker.Shader1) }).GetValue();
                }
                BMLogger.Debug("ammo material");

                try
                {
                    Rambro rambroPrefab = HeroController.GetHeroPrefab(HeroType.Rambro).GetComponent<Rambro>();
                    this.soundHolder = rambroPrefab.soundHolder;
                    this.soundHolderFootSteps = rambroPrefab.soundHolderFootSteps;
                    soundHolderVoice = rambroPrefab.soundHolderVoice;
                    parachute = rambroPrefab.parachute;
                    gibs = rambroPrefab.gibs;
                    player1Bubble = rambroPrefab.player1Bubble;
                    player2Bubble = rambroPrefab.player2Bubble;
                    player3Bubble = rambroPrefab.player3Bubble;
                    player4Bubble = rambroPrefab.player4Bubble;
                    blood = rambroPrefab.blood;
                    heroTrailPrefab = rambroPrefab.heroTrailPrefab;
                    high5Bubble = rambroPrefab.high5Bubble;
                }
                catch(Exception ex)
                {
                    BMLogger.ExceptionLog("Failed to takes Rambro variables\n" + ex);
                }

                this.bm_IsInGame = true;
            }
            catch(Exception ex)
            {
                BMLogger.Log(ex);
            }
        }

        /// <summary>
        /// Awake is called when the script instance is being loaded. (Unity)
        /// </summary>
        protected override void Awake()
        {
            // If you encounter glitch, try some of these variable.

            /* ||||The following variable are in comment because this is their default value.|||||
             * -----------------------------------------------------------------------------------
             * // Some action of the bro.
             * this.canGib = true;          // Gib when explode. Already enabled.
             * this.canWallClimb = true;    // If false, still climbing if you press jump but the animation is messed up.
             * this.canPushBlocks = true;
             * this.canDash = true;
             * this.canLedgeGrapple = false;
             * this.breakDoorsOpen = false; // Break door when they are open.
             * this.canBeCoveredInAcid = true;
             * this.canBeStrungUp = false;
             *
             * // Bro "information".
             * this.JUMP_TIME = 0.123f;     // The time you can hold jump for jumping. Be careful adding a small number can really increase time.
             * this.speed = 110.0f;         // Speed of the bro.
             * this.maxFallSpeed = -400f;   // The speed for falling when he is in the air.
             * this.fireRate = 0.0334f;        // The time between each bullet.
             * this.fireDelay = 0.0f;       // the delay between press fire and the bro fire. Example on Brominator.
             * this._jumpForce = 260f;      // Jump height.
             * this.health = 1; // The number of hit the bro takes before dying. If the value is 0 or is not given, the default value is 3.
             *
             * //Improve animation.
             * this.useNewPushingFrames = false;            // Custom animation when pushing something. The gun stay reverse after pushing the block.
             * this.useNewLadderClimbingFrames = false;     // Custom climbing frame that isn't use for bro. Some sprite are missing.
             * this.useLadderClimbingTransition = false;    // Do nothing, still missing Enter and Exit frame on the ladder.
             * this.useNewLedgeGrappleFrames = false; // Better animation who belong '.canLedgeGrapple'
             *
             * this.useDashFrames = true;   // Already enabled. If false, your character will just be faster and look like it slide on the ground.
             * this.useDuckingFrames = true; // Already enabled. If disable you will not see your bro ducking.
             * this.canDoIndependentMeleeAnimation = false; // ?
             *
             * // Extra
             * this.bloodColor = BloodColor.Red; // Change the blood color when hit or gib. Sewerage = "dirt" color.
             * this.bloodCountAmount = 80; // ?
             * this.deathSoundVolume = 0.4f; // All in the name.
             *
             * // ?
             * this.canTouchLeftWalls = false; // ?
             * this.canTouchRightWalls = false; // ?
             * this.canCeilingHang = false;
             */
            // Some action of the bro.
            canChimneyFlip = true; // Do the things when you catch a ledge. !! Glitch if disable. !!
            doRollOnLand = true; // Do roll on land when it fall from far. !! Glitch if disable. !!
            canHear = true;
            canCeilingHang = true;

            meleeType = MeleeType.Knife;

            // Improve animation.
            useDashFrames = true;

            // Better animation.
            useNewFrames = true;
            useNewDuckingFrames = true;
            useNewThrowingFrames = true;
            useNewKnifingFrames = true;
            useNewKnifeClimbingFrames = true;
            useNewHighFivingFrames = true;
            hasNewAirFlexFrames = true;

            // Extra.
            // End of the basic variable

            isHero = true;
            health = 1;
            bm_originalSpeed = speed;
            //gameObject.AddComponent<Parachute>();

            base.Awake();
            this.playerNum = LoadHero.playerNum;
            player = HeroController.players[playerNum];
           /* meleeHolder = base.gameObject.AddComponent<MeleeHolder>();
            meleeHolder.character = this;*/

            bm_SetupBro();
        }

        /*protected override void StartMelee()
        {
            base.counter = 0f;
            this.currentMeleeType = this.meleeType;
            RaycastHit raycastHit;
            if ((Physics.Raycast(new Vector3(base.X, base.Y + 5f, 0f), Vector3.down, out raycastHit, 16f, this.platformLayer) || Physics.Raycast(new Vector3(base.X + 4f, base.Y + 5f, 0f), Vector3.down, out raycastHit, 16f, this.platformLayer) || Physics.Raycast(new Vector3(base.X - 4f, base.Y + 5f, 0f), Vector3.down, out raycastHit, 16f, this.platformLayer)) && raycastHit.collider.GetComponentInParent<Animal>() != null)
            {
                this.currentMeleeType = BroBase.MeleeType.Knife;
            }
            switch (this.currentMeleeType)
            {
                case BroBase.MeleeType.Knife:
                    this.StartKnifeMelee();
                    break;
                case BroBase.MeleeType.Punch:
                case BroBase.MeleeType.JetpackPunch:
                    this.StartPunch();
                    break;
                case BroBase.MeleeType.Disembowel:
                case BroBase.MeleeType.FlipKick:
                case BroBase.MeleeType.Tazer:
                case BroBase.MeleeType.Custom:
                case BroBase.MeleeType.ChuckKick:
                case BroBase.MeleeType.VanDammeKick:
                case BroBase.MeleeType.ChainSaw:
                case BroBase.MeleeType.ThrowingKnife:
                case BroBase.MeleeType.Smash:
                case BroBase.MeleeType.BrobocopPunch:
                case BroBase.MeleeType.PistolWhip:
                case BroBase.MeleeType.HeadButt:
                case BroBase.MeleeType.TeleportStab:
                    this.StartCustomMelee();
                    break;
            }
        }*/


        /// <summary>
        /// Start is called on the frame when a script is enabled just before any of the Update methods are called the first time. (Unity)
        /// </summary>
        protected override void Start()
        {
            // Assign Some variable default value
           /* Rambro rambroPrefab = HeroController.GetHeroPrefab(HeroType.Rambro) as Rambro;
            this.soundHolder = rambroPrefab.soundHolder;
            this.soundHolderFootSteps = rambroPrefab.soundHolderFootSteps;
            soundHolderVoice = rambroPrefab.soundHolderVoice;
            //jetPackSprite = rambroPrefab.jetPackSprite;
            parachute = rambroPrefab.parachute;
            gibs = rambroPrefab.gibs;
            player1Bubble = rambroPrefab.player1Bubble;
            player2Bubble = rambroPrefab.player2Bubble;
            player3Bubble = rambroPrefab.player3Bubble;
            player4Bubble = rambroPrefab.player4Bubble;
            blood = rambroPrefab.blood;
            heroTrailPrefab = rambroPrefab.heroTrailPrefab;
            high5Bubble = rambroPrefab.high5Bubble;*/

            base.Start();
        }

        protected override void CheckRescues()
        {
            if (HeroController.CheckRescueBros(base.playerNum, base.X, base.Y, 12f))
            {
                this.bm_DestroyBro();
            }
            base.CheckRescues();
        }

        protected override void RunGun()
        {
            bm_fireCount++;
            base.RunGun();
        }

        /// <summary>
        /// Call when fire is pressed
        /// </summary>
        protected override void UseFire()
        {
            if (this.doingMelee)
            {
                this.CancelMelee();
            }
            float num = base.transform.localScale.x;
            if (!base.IsMine && base.Syncronize)
            {
                num = (float)this.syncedDirection;
            }
            if (Connect.IsOffline)
            {
                this.syncedDirection = (int)base.transform.localScale.x;
            }
            this.FireWeapon(base.X + num * 10f, base.Y + 8f, num * bm_ProjectileXRange, (float)UnityEngine.Random.Range(-20, 20));
            this.PlayAttackSound();
            Map.DisturbWildLife(base.X, base.Y, 60f, base.playerNum);
        }

        /// <summary>
        /// Call when the weapon shoot.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="xSpeed"></param>
        /// <param name="ySpeed"></param>
        protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            if(bm_bulletShell != null)
            {
                EffectsController.CreateShrapnel(this.bm_bulletShell, x + base.transform.localScale.x * -15f, y + 3f, 1f, 30f, 0.5f, -base.transform.localScale.x * 80f, 170f);
            }
            base.FireWeapon(x, y, xSpeed, ySpeed);
        }

        /// <summary>
        /// Shoot like a shotgun
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="xSpeed"></param>
        /// <param name="ySpeed"></param>
        public virtual void bm_ShotgunShot(Projectile projectile, float x, float y, float xSpeed, float ySpeed)
        {
            ProjectileController.SpawnProjectileLocally(projectile, this, x, y, xSpeed * 0.83f, ySpeed + 40f + UnityEngine.Random.value * 35f, base.playerNum);
            ProjectileController.SpawnProjectileLocally(projectile, this, x, y, xSpeed * 0.9f, ySpeed + 2f + UnityEngine.Random.value * 15f, base.playerNum);
            ProjectileController.SpawnProjectileLocally(projectile, this, x, y, xSpeed * 0.9f, ySpeed - 2f - UnityEngine.Random.value * 15f, base.playerNum);
            ProjectileController.SpawnProjectileLocally(projectile, this, x, y, xSpeed * 0.85f, ySpeed - 40f - UnityEngine.Random.value * 35f, base.playerNum);
            ProjectileController.SpawnProjectileLocally(projectile, this, x, y, xSpeed * 0.85f, ySpeed - 50f + UnityEngine.Random.value * 80f, base.playerNum);
        }
        /// <summary>
        /// Number of burst shots.
        /// </summary>
        protected int bm_burstShotsLeft = 0;
        protected float bm_burstFireCounter = 0;
        /// <summary>
        /// Time between burst shots
        /// </summary>
        protected float bm_burstShotsFireInterval = 0.07f;
        /// <summary>
        /// Burst
        /// </summary>
        public virtual void bm_BurstShots()
        {
            if (this.bm_burstShotsLeft > 0)
            {
                this.bm_burstFireCounter += this.t;
                if (this.bm_burstFireCounter >= bm_burstShotsFireInterval)
                {
                    this.bm_burstShotsLeft--;
                    this.bm_burstFireCounter -= bm_burstShotsFireInterval;
                    this.FireWeapon(base.X + base.transform.localScale.x * 14f, base.Y + 8.5f, base.transform.localScale.x * bm_ProjectileXRange, 0f);
                    this.PlayAttackSound();
                }
            }
        }

        protected override void Update()
        {
            base.Update();
            bm_BurstShots();
        }

        /// <summary>
        /// Call to destroy the bro
        /// </summary>
        protected virtual void bm_DestroyBro()
        {
            destroyed = true;
            UnityEngine.Object.Destroy(base.gameObject);
            //UnityEngine.Object.Destroy(this);
        }

        public override void DestroyCharacter()
        {
            bm_DestroyBro();
        }
        public override void DestroyUnit()
        {
            bm_DestroyBro();
        }
    }
}