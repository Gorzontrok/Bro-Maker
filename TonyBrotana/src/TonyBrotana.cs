using System;
using UnityEngine;
using HarmonyLib;
using BroMakerLib;
using TonyBrotanaMod;

public class TonyBrotana : BroBaseMaker
{
    private AudioSource chainsawAudio;
    private AudioClip chainsawStart;
    private AudioClip chainsawSpin;
    private AudioClip chainsawWindDown;
    private Unit chainSawMeleedUnit;
    private int chainsawHits;
    private bool haveSwitchAvatar;
    private float lastChainSawMeleeDamageTimeStamp;

    protected int ChainSawMeleeLoopCounter;
    protected int initialDirection;

    public Projectile projRocket;
    public Shrapnel RocketShell;

    public override void bm_SetupBro(Player player)
    {
        try
        {
            this.bm_DefaultMaterial = TonyBrotanaMod.Assets.TonyBrotana_anim;
            this.bm_DefaultGunMaterial = TonyBrotanaMod.Assets.TonyBrotana_gun_anim;
            Main.Debug("set default mat");

            this.bm_avatarMaterial = TonyBrotanaMod.Assets.TonyBrotana_Avatar;
            this.bm_ammoMaterial = TonyBrotanaMod.Assets.TonyBrotana_Special_HUD;
            Main.Debug("set hud mat");

            base.bm_SetupBro(player);
            Main.Debug("base method");

            //HeroController.players[playerNum].character = this;
            // SetGunPosition(player.character.gunSprite.transform.localPosition.x, player.character.gunSprite.transform.localPosition.y);
        }
        catch (Exception ex)
        {
            Main.Log(ex);
        }
    }

    protected override void Start()
    {
        Main.Log("Spawning " + typeof(TonyBrotana));
        try
        {
            new GameObject(typeof(TonyBrotana).FullName, typeof(TonyBrotana));

        }
        catch (Exception ex)
        {
            Main.Log(ex);
        }
        base.Start();

        Brommando brommando = HeroController.GetHeroPrefab(HeroType.Brommando) as Brommando;
        Rambro rambro = HeroController.GetHeroPrefab(HeroType.Rambro) as Rambro;
        AshBrolliams ashBro = HeroController.GetHeroPrefab(HeroType.AshBrolliams) as AshBrolliams;
        Main.Debug("prefab");

        // PROJECTILE FIRE
        BulletRambro ramboProj = BulletRambro.Instantiate(rambro.projectile as BulletRambro, rambro.projectile.transform);
        Main.Debug("Take Rambo Projectile.");
        // PROJECTILE ROCKET
        Rocket brommandoProj = Rocket.Instantiate(brommando.projectile as Rocket, brommando.projectile.transform);
        Main.Debug("Take Brommando Projectile.");
        brommandoProj.gameObject.GetComponent<MeshRenderer>().sharedMaterial = TonyBrotanaMod.Assets.TonyBrotana_Special;
        Main.Debug("special mat");

        this.projectile = ramboProj;
        this.projRocket = brommandoProj;
        Main.Debug("projectile");

        this.soundHolder.specialAttackSounds = brommando.soundHolder.attackSounds;
        Main.Debug("sound");

        this.projRocket.damage = 10;

        chainsawAudio = Traverse.Create(ashBro).Field("chainsawAudio").GetValue<AudioSource>();
        chainsawStart = ashBro.chainsawStart;
        chainsawSpin = ashBro.chainsawSpin;
        chainsawWindDown = ashBro.chainsawWindDown;

         if (this.chainsawAudio == null)
         {
             this.chainsawAudio = base.gameObject.AddComponent<AudioSource>();
             this.chainsawAudio.rolloffMode = AudioRolloffMode.Linear;
             this.chainsawAudio.dopplerLevel = 0.1f;
             this.chainsawAudio.minDistance = 500f;
             this.chainsawAudio.volume = 0.4f;
         }
    }

    protected override void Awake()
    {
        this.health = 1;
        //this.useNewPushingFrames = true; //i'll use it when the function will be patch
        this.originalSpecialAmmo = 6;

        this.isHero = true;
        base.Awake();

        fireRate = 0.15f;
        meleeType = MeleeType.ChainSaw;
        heroType = HeroType.MadMaxBrotansky;
    }

    // Reposition of the gun if is not at the place we want.
    protected override void SetGunPosition(float xOffset, float yOffset)
    {
        this.gunSprite.transform.localPosition = new Vector3(xOffset, yOffset, -1f);
    }

    protected override void UseSpecial()
    {
        if (this.SpecialAmmo > 0)
        {
            this.PlayThrowLightSound(0.4f);
            if (base.IsMine)
            {
                float x = base.X + base.transform.localScale.x * 12f;
                float y = base.Y + 9f;
                float xSpeed = base.transform.localScale.x * (float)(200 + ((!Demonstration.bulletsAreFast) ? 0 : 150));
                float ySpeed = 0f;

                this.gunFrame = 3;
                this.SetGunSprite(this.gunFrame, 0);
                EffectsController.CreateMuzzleFlashEffect(x, y, -25f, xSpeed * 0.01f, ySpeed * 0.01f, base.transform);
                ProjectileController.SpawnProjectileLocally(this.projRocket, this, x, y, xSpeed, ySpeed, false, base.playerNum, false, false, 0f);
            }
            this.fireDelay = 0.6f;
            this.SpecialAmmo--;
            this.PlaySpecialAttackSound(0.3f);
            Map.DisturbWildLife(base.X, base.Y, 80f, base.playerNum);
        }
        else
        {
            HeroController.FlashSpecialAmmo(base.playerNum);
        }
    }

    protected override void CheckRescues()
    {
        if (HeroController.CheckRescueBros(base.playerNum, base.X, base.Y, 12f))
        {
            UnityEngine.Object.Destroy(projRocket.gameObject.GetComponent<MeshRenderer>());
            UnityEngine.Object.Destroy(projRocket);
        }
        base.CheckRescues();
    }

    // Come from AshBrolliams. It's the custom Melee.
    protected override void AnimateCustomMelee()
    {
        try
        {
            this.chainsawAudio.pitch = Mathf.Lerp(this.chainsawAudio.pitch, 0.7f + UnityEngine.Random.value * 0.5f, Time.deltaTime * 8f);
            base.AnimateMeleeCommon();
            if (base.frame > 1)
            {
                base.KickDoors(25f);
            }
            if (this.jumpingMelee)
            {
                if (base.frame == 5)
                {
                    base.counter -= 0.166f;
                }
            }
            else if (base.frame >= 4)
            {
                if (this.ChainSawMeleeLoopCounter > 4)
                {
                    if (base.frame < 7)
                    {
                        base.frame = 7;
                    }
                }
                else
                {
                    if (base.frame > 5)
                    {
                        base.frame = 4;
                        this.ChainSawMeleeLoopCounter++;
                    }
                    if (base.frame == 4)
                    {
                        base.counter -= 0.0334f;
                    }
                    else if (base.frame == 5)
                    {
                        base.counter -= 0.0334f;
                    }
                }
            }
            int num = 9;
            if (this.jumpingMelee)
            {
                num = 10;
            }
            int num2 = 24 + base.frame;
            this.sprite.SetLowerLeftPixel((float)(num2 * this.spritePixelWidth), (float)(num * this.spritePixelHeight));
            if (base.frame == 5)
            {
                this.PerformChainsawMelee();
            }
            if (this.jumpingMelee)
            {
                if (!this.IsOnGround())
                {
                    if (this.highFive && base.frame > 5)
                    {
                        base.frame = 5;
                    }
                }
                else
                {
                    this.CancelMelee();
                }
            }
            if (base.frame > 7)
            {
                base.frame = 7;
                this.CancelMelee();
            }
        }
        catch (Exception ex) { Main.Log("Exception in AnimateCustomMelee\n" + ex); }

    }
    protected virtual void PerformChainsawMelee()
    {
        try
        {
            float num = 22f;
            float num2 = 8f;
            this.chainSawMeleedUnit = Map.GetNearestEnemyUnit(base.playerNum, (int)num, 16, base.X + (float)base.Direction * num2, base.Y, true, base.Direction, this);
            if (this.chainSawMeleedUnit == null)
            {
                this.chainSawMeleedUnit = Map.GetNearestEnemyUnit(base.playerNum, (int)num, 16, base.X + (float)base.Direction * num2, base.Y, true, -base.Direction, this);
            }
            if (this.lastChainSawMeleeDamageTimeStamp + 0.1f > Time.time)
            {
                return;
            }
            this.lastChainSawMeleeDamageTimeStamp = Time.time;
            if (this.chainSawMeleedUnit != null)
            {
                if (this.initialDirection > 0 && this.chainSawMeleedUnit.X < base.X + 8f)
                {
                    this.chainSawMeleedUnit.X = base.X + 8f;
                    this.chainSawMeleedUnit.SetPosition();
                }
                if (this.initialDirection < 0 && this.chainSawMeleedUnit.X > base.X - 8f)
                {
                    this.chainSawMeleedUnit.X = base.X - 8f;
                    this.chainSawMeleedUnit.SetPosition();
                }
                EffectsController.CreateBloodParticles(this.chainSawMeleedUnit.bloodColor, this.chainSawMeleedUnit.X, this.chainSawMeleedUnit.Y + 8f, 6, 8f, 8f, 60f, (float)(this.DirectionSynced * 5), 100f + UnityEngine.Random.value * 50f);
            }
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, base.X + (float)(base.Direction * 6), base.Y, 0f, 0f, 6f, base.playerNum, out flag, null);
            if (this.chainSawMeleedUnit != null)
            {
                chainsawHits++;
                if(!haveSwitchAvatar && chainsawHits != 0)
                {
                    player.hud.SetAvatar(TonyBrotanaMod.Assets.TonyBrotana_AvatarBloody);
                    haveSwitchAvatar = true;
                }
                this.chainSawMeleedUnit.Damage(4, DamageType.ChainsawImpale, 0f, 0f, base.Direction, this, base.X, base.Y);
                Map.PanicUnits(base.X, base.Y, 80f, 24f, 2f, true, false);
            }
            this.TryMeleeTerrain(8, 2);
        }
        catch (Exception ex) { Main.Log("Exception in PerformChainsawMelee\n" + ex); }

    }

    protected override void StartCustomMelee()
    {
        try
        {
            if (!this.doingMelee)
            {
                base.frame = 0;
                base.counter = -0.05f;
                this.AnimateCustomMelee();
            }
            else
            {
                this.meleeFollowUp = true;
            }
            this.initialDirection = base.Direction;
            this.StartMeleeCommon();
            this.cancelMeleeOnChangeDirection = true;
            if (!this.chainsawAudio.isPlaying || this.chainsawAudio.clip != this.chainsawSpin)
            {
                this.chainsawAudio.loop = true;
                this.chainsawAudio.clip = this.chainsawSpin;
                this.chainsawAudio.Play();
                this.chainsawAudio.pitch = 0.8f;
            }
            this.ChainSawMeleeLoopCounter = 0;
            base.counter = Mathf.Clamp(base.counter - 0.05f, -0.05f, 0f);
            this.AnimateMelee();
        }
        catch (Exception ex) { Main.Log("Exception in StartCustomMelee\n" + ex); }

    }

    protected override void RunCustomMeleeMovement()
    {
        try
        {
            this.ForceFaceDirection(this.initialDirection);
            if (this.jumpingMelee)
            {
                this.ApplyFallingGravity();
                if (this.yI < this.maxFallSpeed)
                {
                    this.yI = this.maxFallSpeed;
                }
                if (this.IsOnGround() && base.frame < 5)
                {
                    base.frame = 5;
                    this.AnimateMelee();
                }
            }
            else if (this.dashingMelee)
            {
                if (base.frame < 2)
                {
                    this.xI = 0f;
                    this.yI = 0f;
                }
                else if (base.frame <= 4)
                {
                    if (this.meleeChosenUnit != null)
                    {
                        float num = this.meleeChosenUnit.X - (float)base.Direction * 12f - base.X;
                        this.xI = num / 0.1f;
                        this.xI = Mathf.Clamp(this.xI, -this.speed * 1.7f, this.speed * 1.7f);
                    }
                    else
                    {
                        this.xI = 0f;
                        this.yI = 0f;
                    }
                }
                else if (base.frame <= 7)
                {
                    this.xI = 0f;
                }
                else
                {
                    this.ApplyFallingGravity();
                }
            }
            else
            {
                this.xI = 0f;
                if (base.Y > this.groundHeight + 1f)
                {
                    this.CancelMelee();
                }
            }
        }
        catch (Exception ex) { Main.Log("Exception in RunCustomMeleeMovement\n" + ex); }

    }

    protected override void CancelMelee()
    {
        base.CancelMelee();
        if (this.chainSawMeleedUnit != null && this.chainSawMeleedUnit.canDisembowel)
        {
            this.chainSawMeleedUnit.GibNow(DamageType.ChainsawImpale, (float)(base.Direction * 100), 100f);
        }
    }
}

