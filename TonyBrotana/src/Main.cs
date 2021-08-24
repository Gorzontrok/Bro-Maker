using System;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.IO;

namespace TonyBrotana_LoadMod
{
    using BroMaker;
    using RocketLib;

    static class Main
    {
        public static UnityModManager.ModEntry mod;
        public static bool enabled;
        public static Settings settings;


        internal static TonyBrotana tonyBrotana;
        internal static bool IsTonyBrotana;
        internal static bool CanSwapToCustom;
        

        internal static HeroType HeroBase = HeroType.Rambro;
        internal static HeroType CurrentBro;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = OnGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnUpdate = OnUpdate; 
            modEntry.OnSaveGUI = OnSaveGUI;
            settings = Settings.Load<Settings>(modEntry);

            var harmony = new Harmony(modEntry.Info.Id);
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                harmony.PatchAll(assembly);
            }
            catch (Exception ex)
            {
                mod.Logger.Log("Failed to Patch Harmony !\n" + ex.ToString());
            }

            mod = modEntry;

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            settings.TestMode = GUILayout.Toggle(settings.TestMode, "Test mode");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Swap to Tony Brotana"))
            {
                swapAuto();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                swapAuto();
            }
            
            /*if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                swapManual();
            }*/

            if(settings.TestMode) Main.CanSwapToCustom = true;
        }

        static void swapManual() // Keep it in the case i need it.
        {
            // The path of the image who will be convert to sprite
            string CharacterImgPath = mod.Path + @"/assets/TonyBrotana_anim.png";
            string GunImgPath = mod.Path + @"/assets/TonyBrotana_gun_anim.png";
            string SpecialImgPath = mod.Path + @"/assets/TonyBrotana_Special.png";

            if (!CanSwapToCustom) return;
            if (IsTonyBrotana) return;

            try
            {
                // Get the prefab of the 'basic' bro
                TestVanDammeAnim newbro = HeroController.GetHeroPrefab(CurrentBro);

                TestVanDammeAnim brommando = HeroController.GetHeroPrefab(HeroType.Brommando);
                TestVanDammeAnim rambro = HeroController.GetHeroPrefab(HeroType.Rambro);

                Traverse oldVanDamm = Traverse.Create(HeroController.players[0].character);

                // Get the base of the bro
                tonyBrotana = HeroController.players[0].character.gameObject.AddComponent<TonyBrotana>();
                UnityEngine.Object.Destroy(HeroController.players[0].character.gameObject.GetComponent<WavyGrassEffector>());

                SpriteSM sprite = tonyBrotana.gameObject.GetComponent<SpriteSM>();

                Main.Log("Load the base component.");

                { // Manual way to Load Character sprite.
                    var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    tex.LoadImage(File.ReadAllBytes(CharacterImgPath));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    Texture orig = sprite.meshRender.sharedMaterial.GetTexture("_MainTex");

                    tex.anisoLevel = orig.anisoLevel;
                    tex.filterMode = orig.filterMode;
                    tex.mipMapBias = orig.mipMapBias;
                    tex.wrapMode = orig.wrapMode;

                    sprite.meshRender.sharedMaterial.SetTexture("_MainTex", tex);
                    tonyBrotana.materialNormal = sprite.meshRender.sharedMaterial;
                    Main.Log("Load Character sprite.");
                }

                { // Manual way to load the gun sprite.
                    tonyBrotana.gunSprite = HeroController.players[0].character.gunSprite;

                    var texGun = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    texGun.LoadImage(File.ReadAllBytes(GunImgPath));
                    texGun.wrapMode = TextureWrapMode.Clamp;

                    Texture origGun = newbro.gunSprite.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");
                    SpriteSM gunSpriteCopy = SpriteSM.Instantiate(newbro.gunSprite);

                    tonyBrotana.gunSprite.Copy(gunSpriteCopy);

                    texGun.anisoLevel = origGun.anisoLevel;
                    texGun.filterMode = origGun.filterMode;
                    texGun.mipMapBias = origGun.mipMapBias;
                    texGun.wrapMode = origGun.wrapMode;

                    tonyBrotana.gunSprite.GetComponent<Renderer>().material.mainTexture = texGun;
                    tonyBrotana.gunSprite.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", texGun);
                    Main.Log("Load Gun sprite.");
                }

                // PROJECTILE FIRE
                BulletRambro ramboProj = (rambro as Rambro).projectile as BulletRambro;
                Main.Log("Take Rambo Projectile.");
                // PROJECTILE ROCKET
                Rocket brommandoProj = (brommando as Brommando).projectile as Rocket;
                Main.Log("Take Brommando Projectile.");

                MeshRenderer meshRender = brommandoProj.gameObject.GetComponent<MeshRenderer>();
                 {
                     var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                     tex.LoadImage(File.ReadAllBytes(SpecialImgPath));
                     tex.wrapMode = TextureWrapMode.Clamp;

                     Texture orig = meshRender.sharedMaterial.mainTexture;

                     tex.anisoLevel = orig.anisoLevel;
                     tex.filterMode = orig.filterMode;
                     tex.mipMapBias = orig.mipMapBias;
                     tex.wrapMode = orig.wrapMode;

                     meshRender.material.mainTexture = tex;
                     Main.Log("Set Special Texture");
                 }

                tonyBrotana.projectile = ramboProj;
                tonyBrotana.projRocket = brommandoProj;
                tonyBrotana.soundHolder = rambro.soundHolder;

                tonyBrotana.Setup(sprite, HeroController.players[0], HeroController.players[0].character.playerNum);
                IsTonyBrotana = true;

                UnityEngine.Object.Destroy(HeroController.players[0].character.gameObject.GetComponent(newbro.GetType()));

                tonyBrotana.SetUpHero(0, CurrentBro, true);

                Main.Log("Finish setup");
            }
            catch (Exception ex)
            {
                Main.Log(ex);
            }
        }

        static void swapAuto()
        {
            // The path of the image who will be convert to sprite
            string CharacterImgPath = mod.Path + @"/assets/TonyBrotana_anim.png";
            string GunImgPath = mod.Path + @"/assets/TonyBrotana_gun_anim.png";
            string SpecialImgPath = mod.Path + @"/assets/TonyBrotana_Special.png";
            if (!CanSwapToCustom) return;
            if (IsTonyBrotana) return;

            try
            {
                // Get the prefab of the 'basic' bro
                TestVanDammeAnim newbro = HeroController.GetHeroPrefab(CurrentBro);

                TestVanDammeAnim brommando = HeroController.GetHeroPrefab(HeroType.Brommando);
                TestVanDammeAnim rambro = HeroController.GetHeroPrefab(HeroType.Rambro);

                Traverse oldVanDamm = Traverse.Create(HeroController.players[0].character);

                // Get the base of the bro
                tonyBrotana = HeroController.players[0].character.gameObject.AddComponent<TonyBrotana>();
                UnityEngine.Object.Destroy(HeroController.players[0].character.gameObject.GetComponent<WavyGrassEffector>());

                SpriteSM sprite = tonyBrotana.gameObject.GetComponent<SpriteSM>();

                Main.Log("Load the base component.");

                { // Load the character sprite with BroMaker
                    sprite.meshRender.sharedMaterial.SetTexture("_MainTex", BroMaker.CreateCharacterSprite(CharacterImgPath, sprite));
                    tonyBrotana.materialNormal = sprite.meshRender.sharedMaterial;
                    Main.Log("Load Character sprite.");
                }
               

                {// Load the gun sprite with BroMaker
                    tonyBrotana.gunSprite = HeroController.players[0].character.gunSprite;

                    SpriteSM gunSpriteCopy = SpriteSM.Instantiate(newbro.gunSprite);
                    tonyBrotana.gunSprite.Copy(gunSpriteCopy);

                    Texture origGun = newbro.gunSprite.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");

                    var texGun = BroMaker.CreateGunSprite(GunImgPath, origGun);
                    tonyBrotana.gunSprite.GetComponent<Renderer>().material.mainTexture = texGun;
                    tonyBrotana.gunSprite.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", texGun);
                    Main.Log("Load Gun sprite.");
                }

                // PROJECTILE FIRE
                BulletRambro ramboProj = (rambro as Rambro).projectile as BulletRambro;
                Main.Log("Take Rambo Projectile.");
                // PROJECTILE ROCKET
                Rocket brommandoProj = (brommando as Brommando).projectile as Rocket;
                Main.Log("Take Brommando Projectile.");

                {// Load projectile sprite
                    MeshRenderer meshRenderer = brommandoProj.gameObject.GetComponent<MeshRenderer>();
                    meshRenderer.material.mainTexture = BroMaker.CreateAmmoSprite(SpecialImgPath, meshRenderer);
                    Main.Log("Set Special Texture");
                }

                tonyBrotana.projectile = ramboProj;
                tonyBrotana.projRocket = brommandoProj;
                tonyBrotana.soundHolder = rambro.soundHolder;

                tonyBrotana.Setup(sprite, HeroController.players[0], HeroController.players[0].character.playerNum);
                IsTonyBrotana = true;

                UnityEngine.Object.Destroy(HeroController.players[0].character.gameObject.GetComponent(newbro.GetType()));

                tonyBrotana.SetUpHero(0, CurrentBro, true);

                Main.Log("Finish setup");
            }
            catch(Exception ex)
            {
                Main.Log(ex);
            }
        }

        internal static void Log(object str)
        {
            mod.Logger.Log(str.ToString());
            RocketLib.ScreenLogger.ModId = "TonyBrotana";
            RocketLib.ScreenLogger.Log(str, RLogType.Information);
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }
    public class Settings : UnityModManager.ModSettings
    {
        public bool TestMode;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }

    [HarmonyPatch(typeof(PlayerHUD), "LateUpdate")]
    static class SetAvatar_Patch
    {
        static void Prefix(PlayerHUD __instance)
        {
            try
            {
                /*if (__instance.heroType == Main.HeroBase) Main.CanSwapToCustom = true; // In case that switching with another bro don't work.
                else Main.CanSwapToCustom = false;*/

                if (__instance.heroType == HeroType.BrondleFly || HeroUnlockController.IsExpendaBro(__instance.heroType)) Main.CanSwapToCustom = false; // The sprite is messed up
                else Main.CanSwapToCustom = true;

                if (Main.IsTonyBrotana)
                {
                    SpriteSM avatarSprite = BroMaker.CreateAvatar(Main.mod.Path + @"assets//avatar_TonyBrotana.png", ref __instance);
                    Traverse.Create(typeof(PlayerHUD)).Field("avatar").SetValue(avatarSprite);
                }
                Main.CurrentBro = __instance.heroType;
            }
            catch (Exception ex)
            {
                Main.Log(ex);
                Main.IsTonyBrotana = false;
            }
        }
    }

    [HarmonyPatch(typeof(Player), "SpawnHero")]
    static class Player_SpawnHero_Patch
    {
        static void Prefix(Player __instance, ref HeroType nextHeroType)
        {
            if (!Main.enabled)
                return;

            if(Main.settings.TestMode) nextHeroType = Main.HeroBase;
        }
    }

    [HarmonyPatch(typeof(TestVanDammeAnim), "AttachToHelicopter", new Type[] {typeof(float), typeof(Helicopter)})]
    static class AttachToHelicopter_Patch
    {
        static void Prefix(TestVanDammeAnim __instance)
        {
            if (Main.IsTonyBrotana) Main.IsTonyBrotana = false;
        }
    }
}