using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.IO;

namespace BroMaker_Mod
{
    public static class Main
    {
        public static UnityModManager.ModEntry mod;
        public static bool enabled;
        public static Settings settings;
        public static BroAssaultBase bro;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = new Harmony(modEntry.Info.Id);
            var assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
            mod = modEntry;

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Trigger swap", GUILayout.Width(100)))
            {
                swapToCustom();
            }
            GUILayout.EndHorizontal();
        }
        static void swapToCustom()
        {
            //CreateCustomBro()
        }

        static void CreateCustomBro(string SpriteArmlessPath, string SpriteCharacterPath, string SpriteGunPath, string SpriteProjectilePath)
        {
            Dictionary<HeroType, HeroController.HeroDefinition> heroDefinition = Traverse.Create(HeroController.Instance).Field("_heroData").GetValue() as Dictionary<HeroType, HeroController.HeroDefinition>;
            Traverse oldVanDamm = Traverse.Create(HeroController.players[0].character);

            float fireRate = 0.166f;

            bro = HeroController.players[0].character.gameObject.AddComponent<BroAssaultBase>();
            UnityEngine.Object.Destroy(HeroController.players[0].character.gameObject.GetComponent<WavyGrassEffector>());

            SpriteSM sprite = bro.gameObject.GetComponent<SpriteSM>();
            SoundHolder soundholder = oldVanDamm.Field("soundHolder").GetValue() as SoundHolder;
            TestVanDammeAnim neobro = HeroController.GetHeroPrefab(HeroType.Rambro);


            // LOADING CHARACTER SPRITE ARMLESS
            {
                var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.LoadImage(File.ReadAllBytes(SpriteArmlessPath));
                tex.wrapMode = TextureWrapMode.Clamp;

                Texture orig = sprite.meshRender.sharedMaterial.GetTexture("_MainTex");

                tex.anisoLevel = orig.anisoLevel;
                tex.filterMode = orig.filterMode;
                tex.mipMapBias = orig.mipMapBias;
                tex.wrapMode = orig.wrapMode;

                Material armless = Material.Instantiate(sprite.meshRender.sharedMaterial);
                armless.mainTexture = tex;
                bro.materialArmless = armless;
            }

            // LOADING CHARACTER SPRITE
            {
                var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.LoadImage(File.ReadAllBytes(SpriteCharacterPath));
                tex.wrapMode = TextureWrapMode.Clamp;

                Texture orig = sprite.meshRender.sharedMaterial.GetTexture("_MainTex");

                tex.anisoLevel = orig.anisoLevel;
                tex.filterMode = orig.filterMode;
                tex.mipMapBias = orig.mipMapBias;
                tex.wrapMode = orig.wrapMode;

                sprite.meshRender.sharedMaterial.SetTexture("_MainTex", tex);
                bro.materialNormal = sprite.meshRender.sharedMaterial;
            }

            // LOADING GUN SPRITE
            bro.gunSprite = HeroController.players[0].character.gunSprite;
            
            var texGun = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texGun.LoadImage(File.ReadAllBytes(SpriteGunPath));
            texGun.wrapMode = TextureWrapMode.Clamp;
            
            Texture origGun = neobro.gunSprite.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");
            bro.gunSprite.Copy(neobro.gunSprite);
            Vector3 offset = bro.gunSprite.offset;
            //offset.x += 0;
            bro.gunSprite.SetOffset(offset);

            texGun.anisoLevel = origGun.anisoLevel;
            texGun.filterMode = origGun.filterMode;
            texGun.mipMapBias = origGun.mipMapBias;
            texGun.wrapMode = origGun.wrapMode;

            bro.gunSprite.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", texGun);

            // LOADING PROJECTILE SPRITE         
            {
                var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.LoadImage(File.ReadAllBytes(SpriteProjectilePath));
                //Main.Log("after load iamge");
                tex.wrapMode = TextureWrapMode.Clamp;

                Texture orig = sprite.meshRender.sharedMaterial.mainTexture;

                tex.anisoLevel = orig.anisoLevel;
                tex.filterMode = orig.filterMode;
                tex.mipMapBias = orig.mipMapBias;
                tex.wrapMode = orig.wrapMode;

                sprite.meshRender.material.mainTexture = tex;
            }

            // PASSING REFERENCES TO NEW VAN DAMM
            bro.Setup(sprite, HeroController.players[0], HeroController.players[0].character.playerNum, soundholder, fireRate);

            UnityEngine.Object.Destroy(HeroController.players[0].character.gameObject.GetComponent<BroMax>());
            
            bro.SetUpHero(0, HeroType.Rambro, true);
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static void Log(String str)
        {
            mod.Logger.Log(str);
        }

    }

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }


}