using System;
using System.Collections.Generic;
using UnityEngine;
using BroMaker_Mod;

 public class BroAssaultBase : BroBase //Assault bro template
{
    protected override void UseFire() //Fire function
    {
        base.UseFire();
    }
    protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
    {
        base.FireWeapon(x, y, xSpeed, ySpeed);
    }

    protected override void SetGunSprite(int spriteFrame, int spriteRow)
    {
        base.SetGunSprite(spriteFrame, spriteRow);
    }
    protected override void SetGunPosition(float xOffset, float yOffset)
    {
        base.SetGunPosition(xOffset, yOffset);
    }

    //////////// Special /////////////
    protected override void UseSpecial() 
    {
        base.UseSpecial();
    }
    protected override void PressSpecial()
    {
        base.PressSpecial();
    }
    protected override void AnimateSpecial()
    {
        base.AnimateSpecial();
    }
    ////////////////////////////////
   

    // Dash
    protected override void StartDashing()
    {
        base.StartDashing();
    }


    protected override void PlayClimbSound()
    {
        base.PlayClimbSound();
    }


    ////////// Setup ?????? /////////
    public void Setup(SpriteSM attachSprite, Player attachPlayer, int attachplayerNum, SoundHolder attachSoundHolder, float attachFireRate)
    {
        sprite = attachSprite;
        player = attachPlayer;
        playerNum = attachplayerNum;
        soundHolder = attachSoundHolder;
        this.SpecialAmmo = 1;
        this.originalSpecialAmmo = 1;
        this.fireRate = attachFireRate;
        this.health = 1;
    }
    ////////////////////////////////


    protected override void Update()
    {
        base.Update();
    }

    public BoxCollider attachBoxCollider;
    public SoundHolder boomerangSoundHolder;
    public float rotationSpeed;
    public Transform shieldTransform;

    protected bool hasPlayedAttackHitSound;

    protected List<Unit> alreadyHit = new List<Unit>();

    protected int punchingIndex;
    
    protected bool hasHitWithSlice;
    
    protected bool hasHitWithWall;
    

    public Material materialArmless;

    public Material materialNormal;
    
}
