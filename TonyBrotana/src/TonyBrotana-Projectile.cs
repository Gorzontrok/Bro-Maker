using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using TonyBrotana_LoadMod;


// Unused

/*public class TonyBrotanaRocket : Rocket
{
    public void Setup(SoundHolder attachSoundHolder)
    {
        this.soundHolder = attachSoundHolder;

        this.damage = 5;
        this.damageType = DamageType.Explosion;

        if (this.gameObject == null)
            Main.Log("Rocket Null.");

    }
    protected override void Awake()
    {
        if (this.gameObject == null)
            Main.Log("Rocket Null.");

        base.Awake();
    }
}

public class TonyBrotanaProjectile : BulletRambro
{
    protected override void Start()
    {
        this.damage = 1;
        base.Start();
    }

    public override void Fire(float x, float y, float xI, float yI, float _zOffset, int playerNum, MonoBehaviour FiredBy)
    {
        base.Fire(x, y, xI, yI, _zOffset, playerNum, FiredBy);
        if (BulletRambro.fireCount % 2 == 1)
        {
            this.damage = (this.damageInternal = this.everySecondDamage);
        }
        BulletRambro.fireCount++;
    }
}*/