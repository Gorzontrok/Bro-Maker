using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BroMakerLib.CustomObjects.Bros;
using HarmonyLib;

namespace BroMakerLib
{
    public static class BroMakerUtilities
    {
        /// <summary>
        /// Determines if the specified unit is a boss
        /// </summary>
        /// <param name="unit">Unit to check</param>
        /// <returns>True if unit is a boss</returns>
        public static bool IsBoss(Unit unit)
        {
            return unit.CompareTag("Boss") || unit is DolphLundrenSoldier || unit is SatanMiniboss || (unit is AlienSandWorm && !(unit is AlienWormFacehuggerLauncher)) || 
                unit is TankBig || unit is Mookopter || unit is GoliathMech;
        }

        /// <summary>
        /// Changes the specified players special materials
        /// </summary>
        /// <param name="playerNum"></param>
        /// <param name="specialMaterials">List of materials to set special to, if only one is specified, all icons will be set to it</param>
        /// <param name="offset">Offset to move all special icons horizontally or vertically</param>
        /// <param name="spacing">Spacing between special icons</param>
        public static void SetSpecialMaterials(int playerNum, List<Material> specialMaterials, Vector2 offset, float spacing )
        {
            PlayerHUD hud = HeroController.players[playerNum].hud;
            for (int i = 0; i < hud.grenadeIcons.Length; i++)
            {
                if (playerNum % 2 == 0)
                {
                    hud.grenadeIcons[i].SetOffset(new Vector3(offset.x + i * spacing, offset.y, 0f));
                }
                else
                {
                    hud.grenadeIcons[i].SetOffset(new Vector3(-1 * (offset.x + i * spacing), offset.y, 0f));
                }
            }

            if (specialMaterials != null)
            {
                List<Material> specialIcons = specialMaterials;
                if (specialIcons.Count() > 1)
                {
                    for (int i = 0; i < specialIcons.Count(); ++i)
                    {
                        hud.grenadeIcons[i].GetComponent<Renderer>().material = specialIcons[i];
                    }
                }
                else if (specialIcons.Count() > 0)
                {
                    for (int i = 0; i < hud.grenadeIcons.Count(); ++i)
                    {
                        hud.grenadeIcons[i].GetComponent<Renderer>().material = specialIcons[0];
                    }
                }
            }
        }

        /// <summary>
        /// Changes the specified players special materials
        /// </summary>
        /// <param name="playerNum"></param>
        /// <param name="specialMaterial">Material to set special to, all icons will be set to it</param>
        /// <param name="offset">Offset to move all special icons horizontally or vertically</param>
        /// <param name="spacing">Spacing between special icons</param>
        public static void SetSpecialMaterials(int playerNum, Material specialMaterial, Vector2 offset, float spacing)
        {
            PlayerHUD hud = HeroController.players[playerNum].hud;
            for (int i = 0; i < hud.grenadeIcons.Length; i++)
            {
                if (playerNum % 2 == 0)
                {
                    hud.grenadeIcons[i].SetOffset(new Vector3(offset.x + i * spacing, offset.y, 0f));
                }
                else
                {
                    hud.grenadeIcons[i].SetOffset(new Vector3(-1 * (offset.x + i * spacing), offset.y, 0f));
                }
            }

            if (specialMaterial != null)
            {
                for (int i = 0; i < hud.grenadeIcons.Count(); ++i)
                {
                    hud.grenadeIcons[i].GetComponent<Renderer>().material = specialMaterial;
                }
            }
        }

        /// <summary>
        /// Creates a Gib prefab. Be sure the Gib Holder is set to inactive or else your gibs will be destroyed.
        /// </summary>
        /// <param name="name">Name of GameObject</param>
        /// <param name="lowerLeftPixel">Lower left pixel of gib</param>
        /// <param name="pixelDimensions">Width and Height of gib</param>
        /// <param name="spriteWidth"></param>
        /// <param name="spriteHeight"></param>
        /// <param name="spriteOffset"></param>
        /// <param name="localPositionOffset">Sets the local position of the object which is applied as an offset to the spawn position</param>
        /// <param name="doesRotate"></param>
        /// <param name="gibType"></param>
        /// <param name="size"></param>
        /// <param name="hasBloodTrail"></param>
        /// <param name="color"></param>
        /// <param name="bloodyM"></param>
        /// <param name="shrink"></param>
        /// <param name="rotateFrames"></param>
        /// <param name="rotateAtRightAngles"></param>
        /// <param name="hasSmokeTrail"></param>
        /// <param name="smokeTrailBounces"></param>
        /// <param name="forceMultiplier"></param>
        /// <param name="startLife"></param>
        /// <param name="lifeM"></param>
        /// <param name="randomLifeM"></param>
        /// <param name="r"></param>
        /// <param name="rotationSpeedMultiplier"></param>
        /// <param name="bounceM"></param>
        /// <param name="frictionM"></param>
        /// <param name="drag"></param>
        /// <param name="randomiseGravityM"></param>
        /// <param name="gravityMrandomRange"></param>
        /// <param name="fallHitSound"></param>
        /// <param name="isOnFire"></param>
        /// <returns></returns>
        public static Gib CreateGibPrefab(string name, Vector2 lowerLeftPixel, Vector2 pixelDimensions, float spriteWidth, float spriteHeight, Vector3 spriteOffset, Vector3 localPositionOffset, bool doesRotate, DoodadGibsType gibType, 
            float size = 3f, bool hasBloodTrail = true, BloodColor color = BloodColor.None, float bloodyM = 1f,
            bool shrink = false, int rotateFrames = 8, bool rotateAtRightAngles = false, bool hasSmokeTrail = false, int smokeTrailBounces = 3, float forceMultiplier = 1f,
            float startLife = 1f, float lifeM = 1f, float randomLifeM = 1f, float r = 0f, float rotationSpeedMultiplier = 1f, float bounceM = 0.4f,
            float frictionM = 0.5f, float drag = 0,
            bool randomiseGravityM = false, float gravityMrandomRange = 0.1f, AudioClip[] fallHitSound = null, bool isOnFire = false)
        {
            Gib gib = new GameObject(name, new Type[] { typeof(Transform), typeof(MeshFilter), typeof(MeshRenderer), typeof(SpriteSM), typeof(Gib) }).GetComponent<Gib>();

            SpriteSM gibSprite = gib.gameObject.GetComponent<SpriteSM>();
            gibSprite.plane = SpriteBase.SPRITE_PLANE.XY;
            gibSprite.lowerLeftPixel = lowerLeftPixel;
            gibSprite.pixelDimensions = pixelDimensions;
            gibSprite.offset = spriteOffset;
            gibSprite.width = spriteWidth;
            gibSprite.height = spriteHeight;

            Traverse gibTraverse = Traverse.Create(gib);

            gibTraverse.SetFieldValue("lowerLeftPixel", lowerLeftPixel);
            gibTraverse.SetFieldValue("pixelDimensions", pixelDimensions);
            gib.transform.localPosition = localPositionOffset;
            gib.doesRotate = doesRotate;
            gib.gibType = gibType;
            gib.size = size;
            gib.hasBloodTrail = hasBloodTrail;
            gib.color = color;
            gib.bloodyM = bloodyM;
            gib.shrink = shrink;
            gib.rotateFrames = rotateFrames;
            gib.rotateAtRightAngles = rotateAtRightAngles;
            gib.hasSmokeTrail = hasSmokeTrail;
            gibTraverse.SetFieldValue("smokeTrailBounces", smokeTrailBounces);
            gib.forceMultiplier = forceMultiplier;
            gibTraverse.SetFieldValue("startLife", startLife);
            gib.lifeM = lifeM;
            gib.randomLifeM = randomLifeM;
            gib.r = r;
            gib.rotationSpeedMultiplier = rotationSpeedMultiplier;
            gib.bounceM = bounceM;
            gib.frictionM = frictionM;
            gib.drag = drag;
            gib.randomiseGravityM = randomiseGravityM;
            gib.gravityMrandomRange = gravityMrandomRange;
            if ( fallHitSound != null )
            {
                gib.soundHolder = new GameObject(name + "soundHolder", new Type[] { typeof(Transform), typeof(SoundHolder) }).GetComponent<SoundHolder>();
                gib.soundHolder.fallHitSound = fallHitSound;
            }
            gib.isOnFire = isOnFire;
            return gib;
        }
    }
}
