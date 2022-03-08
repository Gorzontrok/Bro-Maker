using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.IO;
using BroMakerLib;

namespace TonyBrotanaMod
{
    static class Main
    {
        public static UnityModManager.ModEntry mod;
        public static bool enabled;
        public static Settings settings;

        internal static HeroType HeroBase = HeroType.Rambro;


        // The path of the image who will be convert to sprite
        internal static string CharacterImgPath;
        internal static string GunImgPath;
        internal static string SpecialImgPath;
        internal static string AvatarImgPath;
        internal static string AvatarBloodyImgPath;
        internal static string SpecialHUDImgPath;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = OnGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnSaveGUI = OnSaveGUI;
            settings = Settings.Load<Settings>(modEntry);

            mod = modEntry;

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


            StartMod();

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            settings.TestMode = GUILayout.Toggle(settings.TestMode, "Test mode");
            GUILayout.FlexibleSpace();
            settings.DebugMode = GUILayout.Toggle(settings.DebugMode, "Debug log");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        static void StartMod()
        {
            CharacterImgPath = mod.Path + @"/assets/TonyBrotana_anim.png";
            GunImgPath = mod.Path + @"/assets/TonyBrotana_gun_anim.png";
            SpecialImgPath = mod.Path + @"/assets/TonyBrotana_Special.png";
            AvatarImgPath = mod.Path + @"assets//avatar_TonyBrotana.png";
            AvatarBloodyImgPath = mod.Path + @"assets//avatar_TonyBrotanaBloody.png";
            SpecialHUDImgPath = mod.Path + @"/assets/TonyBrotana_SpecialHUD.png";

            new NewBroInfo(typeof(TonyBrotana), "Tony Brotana");
        }

        /*  static void swapManualold() // Keep it in the case i need it.
          {

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

                  Main.Debug("Load the base component.");

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
                      Main.Debug("Load Character sprite.");
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
                      Main.Debug("Load Gun sprite.");
                  }

                  // PROJECTILE FIRE
                  BulletRambro ramboProj = (rambro as Rambro).projectile as BulletRambro;
                  Main.Debug("Take Rambo Projectile.");
                  // PROJECTILE ROCKET
                  Rocket brommandoProj = (brommando as Brommando).projectile as Rocket;
                  Main.Debug("Take Brommando Projectile.");

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
                       Main.Debug("Set Special Texture");
                   }

                  tonyBrotana.projectile = ramboProj;
                  tonyBrotana.projRocket = brommandoProj;
                  tonyBrotana.soundHolder = rambro.soundHolder;

                  tonyBrotana.SetupBro(sprite, HeroController.players[0], HeroController.players[0].character.playerNum);
                  IsTonyBrotana = true;

                  UnityEngine.Object.Destroy(HeroController.players[0].character.gameObject.GetComponent(newbro.GetType()));

                  tonyBrotana.SetUpHero(0, CurrentBro, true);

                  Main.Debug("Finish setup");
              }
              catch (Exception ex)
              {
                  Main.Log(ex);
              }
          }

          static void swapAutoold()
          {
              if (!CanSwapToCustom) return;
              if (IsTonyBrotana) return;

              try
              {
                  // Get the prefab of the 'basic' bro
                  TestVanDammeAnim currentHero = HeroController.GetHeroPrefab(CurrentBro);

                  TestVanDammeAnim brommando = HeroController.GetHeroPrefab(HeroType.Brommando);
                  TestVanDammeAnim rambro = HeroController.GetHeroPrefab(HeroType.Rambro);
                  TestVanDammeAnim ashBro = HeroController.GetHeroPrefab(HeroType.AshBrolliams);

                  //Traverse oldVanDamm = Traverse.Create(currentHero);

                  // Get the base of the bro
                  tonyBrotana = HeroController.players[0].character.gameObject.AddComponent<TonyBrotana>();
                  //UnityEngine.Object.Destroy(HeroController.players[0].character.gameObject.GetComponent<WavyGrassEffector>());

                  SpriteSM sprite = tonyBrotana.gameObject.GetComponent<SpriteSM>();

                  var renderer = tonyBrotana.GetComponent<Renderer>().material;

                  Main.Debug("Load the base component.");

                  try
                  { // Load the character sprite with BroMaker
                      sprite.meshRender.sharedMaterial.SetTexture("_MainTex", RocketLib.CreateTexFromMat(CharacterImgPath, renderer));
                      tonyBrotana.materialNormal = sprite.meshRender.sharedMaterial;
                      Main.Debug("Load Character sprite.");
                  }catch(Exception ex) { Main.Log("Exception throw will loading the character sprite !\n" + ex); }

                  try
                  {// Load the gun sprite with BroMaker
                      tonyBrotana.gunSprite = HeroController.players[0].character.gunSprite;

                      SpriteSM gunSpriteCopy = SpriteSM.Instantiate(rambro.gunSprite);
                      tonyBrotana.gunSprite.Copy(gunSpriteCopy);

                      Texture origGun = rambro.gunSprite.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");

                      var texGun = RocketLib.CreateTexFromTexture(GunImgPath, origGun);
                      tonyBrotana.gunSprite.GetComponent<Renderer>().material.mainTexture = texGun;
                      tonyBrotana.gunSprite.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", texGun);
                      Main.Debug("Load Gun sprite.");
                  }catch (Exception ex) { Main.Log("Exception throw will loading the gun sprite !\n" + ex); }

                  // PROJECTILE FIRE
                  Projectile ramboProj = (rambro as Rambro).projectile as Projectile;
                  Main.Debug("Take Rambo Projectile.");
                  // PROJECTILE ROCKET
                  Projectile brommandoProj = (brommando as Brommando).projectile as Projectile;
                  Main.Debug("Take Brommando Projectile.");
                  // SHRAPNEL
                  Shrapnel ramboShrapnel = (rambro as Rambro).bulletShell as Shrapnel;
                  Main.Debug("Take Rambro Shrapnel.");

                  try
                  {// Load projectile sprite
                      MeshRenderer meshRenderer = brommandoProj.gameObject.GetComponent<MeshRenderer>();
                      OrigRocketTex = meshRenderer.material.mainTexture;
                      meshRenderer.material.mainTexture = RocketLib.CreateTexFromMeshRenderer(SpecialImgPath, meshRenderer);
                      Main.Debug("Set Special Texture");
                  }catch (Exception ex) { Main.Log("Exception throw will loading the sprite of the rocket !\n" + ex); }

                  Projectile ramboProjClone = Projectile.Instantiate(ramboProj, ramboProj.transform);
                  //UnityEngine.Object.Destroy(ramboProjClone.gameObject.GetComponent<BulletRambro>());

                  // Assign the variable
                  tonyBrotana.projectile = ramboProjClone.gameObject.AddComponent<TonyBrotanaProjectile>();
                  tonyBrotana.projRocket = Projectile.Instantiate(brommandoProj, brommandoProj.transform);
                  tonyBrotana.bulletShell = ramboShrapnel;

                  tonyBrotana.FireSound = rambro.soundHolder;
                  tonyBrotana.SpecialSound = brommando.soundHolder;
                  tonyBrotana.soundHolderFootSteps = rambro.soundHolderFootSteps;
                  tonyBrotana.soundHolderVoice = (rambro as Rambro).soundHolderVoice as SoundHolderVoice;

                 /* tonyBrotana.chainsawAudio = Traverse.Create(typeof(AshBrolliams)).Field("chainsawAudio").GetValue() as AudioSource;
                  tonyBrotana.chainsawStart = (ashBro as AshBrolliams).chainsawStart as AudioClip;
                  tonyBrotana.chainsawSpin = (ashBro as AshBrolliams).chainsawSpin as AudioClip;
                  tonyBrotana.chainsawWindDown = (ashBro as AshBrolliams).chainsawWindDown as AudioClip;


                  //tonyBrotana.SetupBro(sprite, HeroController.players[0], );
                  IsTonyBrotana = true;

                  UnityEngine.Object.Destroy(HeroController.players[0].character.gameObject.GetComponent(currentHero.GetType()));

                  tonyBrotana.SetUpHero(0, CurrentBro, true);

                  Main.Debug("Finish setup");
              }
              catch(Exception ex)
              {
                  Main.Log(ex);
              }
          }
  */
        internal static bool CantSwapWithThisHeroType(HeroType CurrentBro)
        {
            List<HeroType> DontSwapWithThem = new List<HeroType>() { HeroType.BrondleFly, HeroType.IndianaBrones, HeroType.TankBro };

            if(HeroUnlockController.IsExpendaBro(CurrentBro)) return true;

            foreach (HeroType hero in DontSwapWithThem)
            {
                if (hero == CurrentBro)
                {
                    return true;
                }
            }
            return false;
        }


        internal static void Log(object str)
        {
            mod.Logger.Log(str.ToString());
        }

        internal static void Debug(object str)
        {
            if (!settings.DebugMode) return;

            mod.Logger.Log("[DEBUG] " + str.ToString());
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
        public bool DebugMode;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }
    /*
    [HarmonyPatch(typeof(PlayerHUD), "LateUpdate")]
    static class SetAvatar_Patch
    {
        static void Prefix(PlayerHUD __instance)
        {
            try
            {
                HeroType CurrentHero = __instance.heroType;
                /*if (__instance.heroType == Main.HeroBase) Main.CanSwapToCustom = true; // In case that switching with another bro don't work.
                else Main.CanSwapToCustom = false;

                if (Main.CantSwapWithThisHeroType(CurrentHero)) Main.CanSwapToCustom = false; // The sprite is messed up or problem appear
                else Main.CanSwapToCustom = true;

                /*if (Main.IsTonyBrotana)
                {
                    SpriteSM avatarSprite = RocketLib.CreateAvatarSprite(Main.AvatarImgPath, ref __instance);
                    Traverse.Create(typeof(PlayerHUD)).Field("avatar").SetValue(avatarSprite);

                    Material TonyBrotanaIcon = Material.Instantiate(__instance.rambroIcon);
                    TonyBrotanaIcon.mainTexture = BroMaker.CreateTexFromMaterial(Main.SpecialHUDImgPath, TonyBrotanaIcon);
                    TonyBrotanaIcon.name = "TonyBrotanaIcon";
                    Traverse.Create(__instance).Method("SetGrenadeMaterials", new object[] { TonyBrotanaIcon }).GetValue();
                }
                Main.CurrentBro = CurrentHero;
            }
            catch (Exception ex)
            {
                Main.Log(ex);
                Main.IsTonyBrotana = false;
            }
        }
    }*/

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
    /*
    [HarmonyPatch(typeof(GameModeController), "Update")]
    static class LevelFinish_Patch
    {
        static void Prefix(GameModeController __instance)
        {
            if (GameModeController.LevelFinished) Main.IsTonyBrotana = false;
        }
    }

    [HarmonyPatch(typeof(PauseMenu), "ReturnToMenu")]
    static class PauseMenu_ReturnToMenu_Patch
    {
        static void Prefix(PauseMenu __instance)
        {
            Main.IsTonyBrotana = false;
        }

    }
    [HarmonyPatch(typeof(PauseMenu), "ReturnToMap")]
    static class PauseMenu_ReturnToMap_Patch
    {
        static void Prefix(PauseMenu __instance)
        {
            Main.IsTonyBrotana = false;
        }

    }
    [HarmonyPatch(typeof(PauseMenu), "RestartLevel")]
    static class PauseMenu_RestartLevel_Patch
    {
        static void Prefix(PauseMenu __instance)
        {
            Main.IsTonyBrotana = false;
        }
    }*/
}