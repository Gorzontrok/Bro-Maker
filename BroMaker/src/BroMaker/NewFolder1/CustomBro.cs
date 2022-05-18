using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using BroMakerLoadMod;
using System.IO;


namespace BroMakerLib
{
    /*public class CustomBro
    {
        public readonly string Name = string.Empty;
        public readonly string NameToShow = string.Empty;

        protected readonly string FolderPath = string.Empty;
        protected readonly string CharacterImagePath = string.Empty;-
        protected readonly string GunImagePath = string.Empty;
        protected readonly string AvatarImagePath = string.Empty;

        protected Texture GunTex = null;
        protected Texture CharacterTex = null;
        protected Texture AvatarTex = null;

        public readonly HeroType BaseHeroType = HeroType.None;

        public CustomBro(Bro_Info bro)
        {
            try
            {
                this.Name = (string.IsNullOrEmpty(bro.Name)) ? "NO_NAME" : bro.Name;
                int i = CustomBroController.NumberOfThisName(this.Name);
                this.NameToShow = this.Name;
                if (i > 0) this.NameToShow += i.ToString();

                this.FolderPath = Path.Combine(CustomBroController.AssetsPath, bro.AssetsFolder);

                this.CharacterImagePath = Path.Combine(CustomBroController.AssetsPath, this.Name + "_anim.png");
                this.GunImagePath = Path.Combine(CustomBroController.AssetsPath, this.Name + "_gun_anim.png");
                this.AvatarImagePath = Path.Combine(CustomBroController.AssetsPath, "avatar_" + this.Name + ".png");

                this.BaseHeroType = BroMaker.GetBroHeroType(bro.BaseBro);


                CustomBroController.CustomBros.Add(this);
            }
            catch(Exception ex) { Main.Log(ex); }
        }

        public TestVanDammeAnim GetInstance(TestVanDammeAnim oldInstance)
        {
            this.CheckTexture(oldInstance);
            TestVanDammeAnim NewBro = Traverse.Create(typeof(Player)).Method("InstantiateHero", new object[] { this.BaseHeroType, 0, 0 }).GetValue<TestVanDammeAnim>();

            if (NewBro.gameObject == null)
            {
                Main.Log("Base Bro null");
            }

            if (this.CharacterTex != null)
            {
                if (NewBro.gameObject.GetComponent<SpriteSM>() == null)
                {
                    Main.Log("c null");
                }
                else
                {
                    oldInstance.gameObject.GetComponent<SpriteSM>().meshRender.sharedMaterial.SetTexture("_MainTex", this.CharacterTex);
                    //Basebro.gameObject.GetComponent<SpriteSM>().meshRender.sharedMaterial.SetTexture("_MainTex", this.CharacterTex);
                    //Basebro.GetComponent<Renderer>().material.mainTexture = this.CharacterTex;
                    //Basebro.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", this.CharacterTex);
                }
            }

            if (this.GunTex != null)
            {
                if (NewBro.gunSprite.GetComponent<Renderer>() == null)
                {
                    Main.Log("g null");
                }
                else
                {
                    NewBro.gunSprite.GetComponent<Renderer>().material.mainTexture = this.GunTex;
                    NewBro.gunSprite.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", this.GunTex);
                }
            }

            if (this.AvatarTex != null)
            {
                if (NewBro.player.hud.avatar.meshRender == null)
                {
                    Main.Log("a null");
                    //Basebro.player.hud.avatar.meshRender.material.mainTexture = this.AvatarTex;
                    NewBro.player.hud.avatar.meshRender.sharedMaterial.SetTexture("_MainTex", this.AvatarTex);
                }
            }

            if (oldInstance.player == null) Main.Log("player null");
            //Basebro.player = oldInstance.player;
            //oldInstance = BaseBro;
            if (NewBro == null) Main.Log("ss");
            return NewBro;

        }

        private void CheckTexture(TestVanDammeAnim oldInstance)
        {
            try
            {
                if (this.GunTex == null)
                {
                   // this.GunTex = RocketLib.CreateTexFromSpriteSM(this.GunImagePath, oldInstance.gunSprite);
                }
                if (this.CharacterTex == null)
                {
                    //this.CharacterTex = RocketLib.CreateTexFromTexture(this.CharacterImagePath, oldInstance.gameObject.GetComponent<SpriteSM>().meshRender.sharedMaterial.GetTexture("_MainTex"));
                }
                if (this.AvatarTex == null)
                {
                   //this.AvatarTex = RocketLib.CreateTexFromMat(this.AvatarImagePath, oldInstance.player.hud.avatar.meshRender.material);
                }
            }catch(Exception ex) { Main.Log(ex); }
        }
    }

    public static class CustomBroController
    {
        public static List<CustomBro> CustomBros = new List<CustomBro>();
        public static string AssetsPath
        {
            get
            {
                return Path.Combine(Main.mod.Path, "CustomBro\\Assets");
            }
        }

        public static void Load()
        {

        }

        public static int NumberOfThisName(string _Name)
        {
            int Number = 0;
            foreach (var bro in CustomBros)
            {
                if (bro.Name == _Name) Number++;
            }
            return Number;
        }

        public static string[] GetCustomName()
        {
            List<string> list = new List<string>();
            foreach (var bro in CustomBros)
            {
                list.Add(bro.NameToShow);
            }
            return list.ToArray();
        }
    }*/
}
