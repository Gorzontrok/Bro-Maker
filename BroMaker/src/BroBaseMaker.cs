using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.IO;
using BroMakerLoadMod;

namespace BroMakerLib
{
    /// <summary>
    /// Base class of custom character
    /// </summary>
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

        /// <summary>
        ///
        /// </summary>
        public bool bm_IsInGame = false;

        /// <summary>
        /// original speed of the character
        /// </summary>
        protected float bm_originalSpeed;
        /// <summary>
        ///
        /// </summary>
        protected int bm_fireCount;


        /// <summary>
        /// This function is for setup the bro.
        /// </summary>
        public virtual void bm_SetupBro(Player player)
        {
            try
            {
                try
                {
                    this.player = player;
                    this.playerNum = player.character.playerNum;
                    Main.Debug("player");
                }
                catch(Exception ex)
                {
                    Main.ExceptionLog("Failed assign player and playerNum", ex);
                }

                //pockettedSpecialAmmo = (player.character as BroBase).pockettedSpecialAmmo;
                try
                {
                    Rambro rambroPrefab = HeroController.GetHeroPrefab(HeroType.Rambro) as Rambro;
                    sprite = gameObject.GetComponent<SpriteSM>();
                    //sprite = SpriteSM.Instantiate(player.character.GetComponent<SpriteSM>());
                    gunSprite = SpriteSM.Instantiate(rambroPrefab.gunSprite, base.transform);
                    //gunSprite.Copy(rambroPrefab.gunSprite);
                    UnityEngine.Object.Destroy(HeroController.players[playerNum].character.gunSprite.gameObject);
                    Main.Debug("sprite");
                }
                catch(Exception ex)
                {
                    Main.ExceptionLog("Failed assign SpriteSM", ex);
                }

                try
                {
                    if (defaultMaterial != null && sprite != null)
                    {
                        this.sprite.MeshRenderer.sharedMaterial = defaultMaterial;
                       // base.GetComponent<Renderer>().material = defaultMaterial;
                       // sprite.GetComponent<Renderer>().sharedMaterial = defaultMaterial;
                       // sprite.GetComponent<Renderer>().material = defaultMaterial;
                    }
                    else if(sprite != null)
                    {
                        bm_DefaultMaterial = BroMakerLib.Assets.EmptyCharacter;
                        this.sprite.MeshRenderer.sharedMaterial = defaultMaterial;
                    }
                    Main.Debug("default material");
                    if (bm_DefaultGunMaterial != null && gunSprite != null)
                    {
                        gunSprite.GetComponent<Renderer>().material = bm_DefaultGunMaterial;
                        gunSprite.GetComponent<Renderer>().sharedMaterial = bm_DefaultGunMaterial;
                    }
                    else if (gunSprite != null)
                    {
                        bm_DefaultGunMaterial = BroMakerLib.Assets.EmptyGun;
                        gunSprite.GetComponent<Renderer>().material = bm_DefaultGunMaterial;
                        gunSprite.GetComponent<Renderer>().sharedMaterial = bm_DefaultGunMaterial;
                    }
                    Main.Debug("gun material");
                }
                catch (Exception ex)
                {
                    Main.ExceptionLog("Failed Assign Player and PlayernUm", ex);
                }



                UnityEngine.Object.Destroy(player.character.gameObject.GetComponent(HeroController.GetHeroPrefab(player.character.heroType).GetType()));
                this.SetUpHero(playerNum, player.character.heroType, true);
                Main.Debug("setup hero");

                if (bm_avatarMaterial != null)
                {
                    HeroController.SetAvatarMaterial(playerNum, bm_avatarMaterial);
                }
                else
                {
                    HeroController.SetAvatarMaterial(playerNum, BroMakerLib.Assets.EmptyAvatar);
                }
                Main.Debug("avatar material");
                if (bm_ammoMaterial != null)
                {
                    Traverse.Create(player.hud).Method("SetGrenadeMaterials", new object[] { bm_ammoMaterial }).GetValue();
                }
                else
                {
                    Traverse.Create(player.hud).Method("SetGrenadeMaterials", new object[] { BroMakerLib.Assets.DefaultGrenadeIcon }).GetValue();
                }
                Main.Debug("ammo material");

                this.bm_IsInGame = true;
            }
            catch(Exception ex)
            {
                Main.Log(ex);
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
             * this.canCeilingHang = false; // Only for enemy
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
            //useNewHighFivingFrames = true;
            //hasNewAirFlexFrames = true;

            // Extra.
            // End of the basic variable

            isHero = true;
            health = 1;
            bm_originalSpeed = speed;
            base.Awake();
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any of the Update methods are called the first time. (Unity)
        /// </summary>
        protected override void Start()
        {
            // Assign Some variable default value
            Rambro rambroPrefab = HeroController.GetHeroPrefab(HeroType.Rambro) as Rambro;
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
            high5Bubble = rambroPrefab.high5Bubble;

            base.Start();
            base.gameObject.AddComponent<WavyGrassEffector>();
        }

        /// <summary>
        ///
        /// </summary>
        protected override void CheckRescues()
        {
            if (HeroController.CheckRescueBros(base.playerNum, base.X, base.Y, 12f))
            {
                this.DestroyUnit();
            }
            base.CheckRescues();
        }

        /// <summary>
        ///
        /// </summary>
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
        /// Change the range of the projectile. 400 is default
        /// </summary>
        protected float bm_ProjectileXRange = 400f;

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
        /// Call to destroy the bro
        /// </summary>
        protected virtual void bm_DestroyBro()
        {
            destroyed = true;
            UnityEngine.Object.Destroy(base.gameObject);
            UnityEngine.Object.Destroy(this);
        }

        /// <summary>
        ///
        /// </summary>
        public override void DestroyCharacter()
        {
            bm_DestroyBro();
        }
        /// <summary>
        ///
        /// </summary>
        public override void DestroyUnit()
        {
            bm_DestroyBro();
        }
    }
}