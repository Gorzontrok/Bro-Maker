using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using TonyBrotana_LoadMod;

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