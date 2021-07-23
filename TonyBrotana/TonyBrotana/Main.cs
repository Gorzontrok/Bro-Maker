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

        internal static TonyBrotana tonyBrotana;
        internal static bool IsTonyBrotana;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = OnGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnUpdate = OnUpdate;
            var harmony = new Harmony(modEntry.Info.Id);
            var assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
            mod = modEntry;

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Manual", GUILayout.Width(100)))
            {
                swapManual();
            }
            GUILayout.EndHorizontal();
        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                swapManual();
            }

        }

        static void swapManual()
        {
            // The path of the image who will be convert to sprite
            string CharacterImgPath = mod.Path + @"/assets/TonyBrotana_anim.png";
            string GunImgPath = mod.Path + @"/assets/TonyBrotana_gun_anim.png";
            string SpecialImgPath = mod.Path + @"/assets/TonyBrotana_Special.png";
            try
            {
                HeroType heroBase = HeroType.Rambro;

                // Get the prefab of the 'basic' bro
                TestVanDammeAnim newbro = HeroController.GetHeroPrefab(heroBase);
                TestVanDammeAnim brommando = HeroController.GetHeroPrefab(HeroType.Brommando);

                Traverse oldVanDamm = Traverse.Create(HeroController.players[0].character);
                SoundHolder soundholder = oldVanDamm.Field("soundHolder").GetValue() as SoundHolder;

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

                // PROJECTILE
                    Projectile rocket = (brommando as Brommando).projectile as Projectile;
                    Projectile clone = Projectile.Instantiate(rocket);
                    Main.Log("Take Projectile.");

                    SoundHolder rocketSoundHolder = Traverse.Create(clone).Field("soundHolder").GetValue<SoundHolder>();
                    TonyBrotanaRocket tonyRocket = clone.gameObject.AddComponent<TonyBrotanaRocket>();
                    tonyRocket.Setup(rocketSoundHolder);
                    Main.Log("Rocket setup");


               /* MeshRenderer meshRender = clone.gameObject.GetComponent<MeshRenderer>();
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

                // Set avatar
                {
                    string PathAvatar = mod.Path + @"/assets/avatar_TonyBrotana.png";

                    var texAvatar = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    texAvatar.LoadImage(File.ReadAllBytes(PathAvatar));
                    texAvatar.wrapMode = TextureWrapMode.Clamp;

                    Texture mat = tonyBrotana.player.hud.avatar.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");

                    Texture orig = mat;

                    texAvatar.anisoLevel = orig.anisoLevel;
                    texAvatar.filterMode = orig.filterMode;
                    texAvatar.mipMapBias = orig.mipMapBias;
                    texAvatar.wrapMode = orig.wrapMode;

                    tonyBrotana.player.hud.avatar.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", texAvatar);
                }*/

                IsTonyBrotana = true;
                tonyBrotana.Setup(sprite, HeroController.players[0], HeroController.players[0].character.playerNum, soundholder);
                Main.Log("Finish setup");
            }
            catch (Exception ex)
            {
                Main.Log(ex);
            }
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
        internal static void Log(object str)
        {
            mod.Logger.Log(str.ToString());
            RocketLib.ScreenLogger.ModId = "TonyBrotana";
            RocketLib.ScreenLogger.Log(str, RLogType.Information);
        }
    }

    [HarmonyPatch(typeof(Player), "SpawnHero")]
    static class Player_SpawnHero_Patch
    {
        static void Prefix(Player __instance, ref HeroType nextHeroType)
        {
            if (!Main.enabled)
                return;
            nextHeroType = HeroType.Rambro;
            return;

        }

    }

    [HarmonyPatch(typeof(PlayerHUD), "SetAvatar")]
    static class SetAvatar_Patch
    {
        static void Prefix(PlayerHUD __instance, ref Material avatarMaterial)
        {
            try
            {
                if (Main.IsTonyBrotana)
                {/*
                    SpriteSM avatarSprite = Main.LoadAvatar();
                    Traverse.Create(typeof(PlayerHUD)).Field("avatar").SetValue(avatarSprite);
                    Main.Log("avatar spawn");*/
                }
            }
            catch (Exception ex)
            {
                Main.Log(ex);
            }
            return;
        }
    }
}