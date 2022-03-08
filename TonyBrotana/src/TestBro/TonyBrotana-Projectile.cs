using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using TonyBrotanaMod;

// Unused

public class TonyBrotanaRocket : Rocket
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

public class TonyBrotanaProjectile : Projectile
{
    protected override void Start()
    {
        base.Start();

        this.health = 3;
        this.damage = 1;
        this.maxHealth = -1;
        this.life = 0.35f;
        this.damageInternal = 1;
        this.projectileSize = 5;
        this.everySecondDamage = 3;
        this.canReflect = true;
        this.sparkCount = 8;
        this.canHitGrenades = true;
        this.affectScenery = true;
        this.soundVolume = 0.1f;
        this.playerNum = -1;
    }

    public override void Fire(float x, float y, float xI, float yI, float _zOffset, int playerNum, MonoBehaviour FiredBy)
    {
        base.Fire(x, y, xI, yI, _zOffset, playerNum, FiredBy);
        if (TonyBrotanaProjectile.fireCount % 2 == 1)
        {
            this.damage = (this.damageInternal = this.everySecondDamage);
        }
        TonyBrotanaProjectile.fireCount++;
    }
	public int everySecondDamage = 2;
	protected static int fireCount;
}