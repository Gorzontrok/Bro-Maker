using System;
using UnityEngine;
using TonyBrotana_LoadMod;
using BroMaker;

public class TonyBrotana : Maker._BroBase
{
    public void Setup(SpriteSM attachSprite, Player attachPlayer, int attachplayerNum)
    {
        sprite = attachSprite;
        player = attachPlayer;
        playerNum = attachplayerNum;
    }

    protected override void Start()
    {
        Main.Log("Spawning " + typeof(TonyBrotana));
        try
        {
            new GameObject(typeof(TonyBrotana).FullName, typeof(TonyBrotana));
            this.projRocket.damage = 10;
        }
        catch(Exception ex)
        {
           Main.Log(ex);
        }
        base.Start();

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
        // !!! Never put something who change the projectile behaviour here !!!!!!
        this.health = 1;
        //this.useNewPushingFrames = true; //i'll use it when the function will be patch
        this.SetTotalSpecialAmmo(6);
        this.canDoIndependentMeleeAnimation = false;
                
        this.isHero = true;
        base.Awake();
    }

    // Reposition of the gun if is not at the place we want.
    protected override void SetGunPosition(float xOffset, float yOffset)
    {
        this.gunSprite.transform.localPosition = new Vector3(xOffset, yOffset, -1f);
    }

    protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed) // What the gun does when you shoot.
    {
        this.soundHolder = FireSound;
        EffectsController.CreateShrapnel(this.bulletShell, x + base.transform.localScale.x * -15f, y + 3f, 1f, 30f, 1f, -base.transform.localScale.x * 80f, 170f);
        base.FireWeapon(x, y, xSpeed, ySpeed);
    }

    protected override void UseSpecial()
    {
        this.soundHolder = SpecialSound;
            if (this.SpecialAmmo > 0)
            {
		        this.PlayThrowLightSound(0.4f);
                if (base.IsMine)
                {
                    float x = base.X + base.transform.localScale.x * 12f;
                    float y = base.Y + 9f;
                    float xSpeed = base.transform.localScale.x * (float)(400 + ((!Demonstration.bulletsAreFast) ? 0 : 150));
                    float ySpeed = 0f;

                    this.gunFrame = 3;
                    this.SetGunSprite(this.gunFrame, 0);
                    EffectsController.CreateMuzzleFlashEffect(x, y, -25f, xSpeed * 0.01f, ySpeed * 0.01f, base.transform);
                    ProjectileController.SpawnProjectileLocally(this.projRocket, this, x, y, xSpeed, ySpeed, false, base.playerNum, false, false, 0f);
                }
                this.fireDelay = 0.6f;
                this.SpecialAmmo--;
                this.PlayAttackSound();
                Map.DisturbWildLife(base.X, base.Y, 80f, base.playerNum);
            }
            else
            {
                HeroController.FlashSpecialAmmo(base.playerNum);
            }
    }


    protected override void Gib(DamageType damageType, float xI, float yI)
    {
        Main.IsTonyBrotana = false;
        base.Gib(damageType, xI, yI);
    }

    public override void Death(float xI, float yI, DamageObject damage)
    {
        Main.IsTonyBrotana = false;
        base.Death(xI, yI, damage);
    }

    protected override void CheckRescues()
    {
        if (HeroController.CheckRescueBros(base.playerNum, base.X, base.Y, 12f))
        {
            this.DestroyUnit();
            Main.IsTonyBrotana = false;
        }
        base.CheckRescues();
    }

    public Projectile projRocket; 
    public Shrapnel RocketShell;

    public SoundHolder FireSound;
    public SoundHolder SpecialSound;

    // Come from AshBrolliams. It's the custom Melee.
  /*   protected override void AnimateCustomMelee()
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

      protected virtual void PerformChainsawMelee()
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
              this.chainSawMeleedUnit.Damage(4, DamageType.ChainsawImpale, 0f, 0f, base.Direction, this, base.X, base.Y);
              Map.PanicUnits(base.X, base.Y, 80f, 24f, 2f, true, false);
          }
          this.TryMeleeTerrain(8, 2);
      }

      protected override void StartCustomMelee()
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
      }*/

    public AudioSource chainsawAudio;

    public AudioClip chainsawStart;

    public AudioClip chainsawSpin;

    public AudioClip chainsawWindDown;

    protected int ChainSawMeleeLoopCounter;

    protected int initialDirection;

    private float lastChainSawMeleeDamageTimeStamp;

    private Unit chainSawMeleedUnit;
}

