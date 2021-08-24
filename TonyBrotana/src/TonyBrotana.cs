using System;
using UnityEngine;
using TonyBrotana_LoadMod;
using BroMaker;

public class TonyBrotana : Maker._BroBase
{
    private Rambro rambo;
    public void Setup(SpriteSM attachSprite, Player attachPlayer, int attachplayerNum)
    {
        sprite = attachSprite;
        player = attachPlayer;
        playerNum = attachplayerNum;
        //this.bulletShell = rambo.bulletShell; 
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
    }
     
    protected override void Awake()
    {
        // !!! Never put something who change the projectile behaviour here !!!!!!
        this.health = 1;
        //this.useNewPushingFrames = true; //i'll use it when the function will be patch
        this.SetTotalSpecialAmmo(6);

        
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
        this.gunFrame = 3;
        this.SetGunSprite(this.gunFrame, 1);
        EffectsController.CreateShrapnel(this.bulletShell, x + base.transform.localScale.x * -15f, y + 3f, 1f, 30f, 1f, -base.transform.localScale.x * 80f, 170f);
        EffectsController.CreateMuzzleFlashEffect(x, y, -25f, xSpeed * 0.15f, ySpeed * 0.15f, base.transform);
        ProjectileController.SpawnProjectileLocally(this.projectile, this, x, y, xSpeed, ySpeed, base.playerNum);
    }

    protected override void UseSpecial()
    {
            //Main.Log("Special is use");
            if (this.SpecialAmmo > 0)
            {
		        this.PlayThrowLightSound(0.4f);

                if (base.IsMine)
                {
                    float x = base.X + base.transform.localScale.x * 12f;
                    float y = base.Y + 9f;
                    float xSpeed = base.transform.localScale.x * (float)(150 + ((!Demonstration.bulletsAreFast) ? 0 : 150));
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
}

