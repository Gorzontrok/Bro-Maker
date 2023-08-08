using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using BroMakerLib;
using UnityEngine;

namespace BronobiMod
{
    public class BronobiForceWave : FlameWallExplosion
    {
        protected override void TryAssassinateUnits(float x, float y, int xRange, int yRange, int playerNum)
        {
            Mook closestMook = Map.GetNearbyMook((float)xRange, (float)yRange, x, y, (forceDirection == DirectionEnum.Left ? -1 : 1), true);
            if(closestMook)
            {
                float XI = Mathf.Sign(firedBy.transform.localScale.x) * 310f + (firedBy as TestVanDammeAnim).xI * 0.2f;
                float YI = 220f + (firedBy as TestVanDammeAnim).yI * 0.3f;
                closestMook.xI = XI;
                closestMook.yI = YI;
                closestMook.SetBackFlyingFrame(XI, YI);
                closestMook.transform.parent = firedBy.transform.parent;
                closestMook.Reenable();
                closestMook.StartFallingScream();
                closestMook.EvaluateIsJumping();
                closestMook.ThrowMook(false, base.playerNum);
                closestMook.Blind(10f);
            }
            Map.DeflectProjectiles(this, base.playerNum, 16f, (firedBy as TestVanDammeAnim).X + Mathf.Sign(base.transform.localScale.x) * 6f, (firedBy as TestVanDammeAnim).Y + 6f, Mathf.Sign(base.transform.localScale.x) * 200f, true);

        }

        void Awake()
        {
            try
            {
                SetupLightExplosions();
                assasinateUnits = true;
                damageDoodads = false;
                damageGround = false;
                damageUnits = false;
                knockUnits = false;
                blindUnits = false;
                maxCollumns = 32;
                maxRows = 3;
                rotateExplosionSprite = true;
                totalExplosions = 60;
                explosionRate = 0.04f;
            }
            catch(Exception ex)
            {
                BMLogger.Log($"[{typeof(BronobiForceWave)}] {ex}");
            }
        }

        private void SetupLightExplosions()
        {
            try
            {
                Puff matildaPuff = (HeroController.GetHeroPrefab(HeroType.TheBrofessional) as TheBrofessional).matildaTargettingWavePrefab.lightExplosion;

                lightExplosion = new GameObject("BronobiForceWave_Puff", new Type[] { typeof(Transform), typeof(MeshFilter), typeof(MeshRenderer), typeof(SpriteSM), typeof(Puff) }).GetComponent<Puff>();
                lightExplosion.frameRate = 0.01f;
                lightExplosion.pauseFrame = 12;
                lightExplosion.gameObject.layer = 19;

                MeshRenderer renderer = lightExplosion.gameObject.GetComponent<MeshRenderer>();

                Material mat = new Material(matildaPuff.GetComponent<MeshRenderer>().material);
                Texture2D tex = ResourcesController.CreateTexture(Main.ExtractResource("BronobiMod._Mod.BronobiForceWave.png"));
                if (tex != null)
                {
                    mat.color = Color.cyan;
                    mat.mainTexture = tex;
                }
                renderer.material = mat;
                SpriteSM sprite = lightExplosion.gameObject.GetComponent<SpriteSM>();
                sprite.lowerLeftPixel = new Vector2(0, 64);
                sprite.pixelDimensions = new Vector2(16, 64);

                sprite.plane = SpriteBase.SPRITE_PLANE.XY;
                sprite.width = 16;
                sprite.height = 64;
                sprite.offset = new Vector3(0, 0, -5);
            }
            catch (Exception ex)
            {
                BMLogger.Log($"[{typeof(BronobiForceWave)}] {ex}");
            }
        }

    }
}
