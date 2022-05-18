using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BroMakerLib;

namespace TonyBrotanaMod
{
    public static class Assets
    {

        public static Material TonyBrotana_anim
        {
            get
            {
                if (_TonyBrotana_anim == null)
                {
                    _TonyBrotana_anim = BroMaker.CreateMaterialFromFile(Main.CharacterImgPath);
                }
                return _TonyBrotana_anim;
            }
        }
        private static Material _TonyBrotana_anim;

        public static Material TonyBrotana_gun_anim
        {
            get
            {
                if (_TonyBrotana_gun_anim == null)
                {
                    _TonyBrotana_gun_anim = BroMaker.CreateMaterialFromFile(Main.GunImgPath);
                }
                return _TonyBrotana_gun_anim;
            }
        }
        private static Material _TonyBrotana_gun_anim;
        public static Material TonyBrotana_Special
        {
            get
            {
                if (_TonyBrotana_Special == null)
                {
                    _TonyBrotana_Special = BroMaker.CreateMaterialFromFile(Main.SpecialImgPath);
                }
                return _TonyBrotana_Special;
            }
        }
        private static Material _TonyBrotana_Special;
        public static Material TonyBrotana_Special_HUD
        {
            get
            {
                if (_TonyBrotana_Special_HUD == null)
                {
                    _TonyBrotana_Special_HUD = BroMaker.CreateMaterialFromFile(Main.SpecialHUDImgPath);
                }
                return _TonyBrotana_Special_HUD;
            }
        }
        private static Material _TonyBrotana_Special_HUD;
        public static Material TonyBrotana_Avatar
        {
            get
            {
                if (_TonyBrotana_Avatar == null)
                {
                    _TonyBrotana_Avatar = BroMaker.CreateMaterialFromFile(Main.AvatarImgPath);
                    //_TonyBrotana_Avatar = BroMaker.CreateMaterialFromResources(Main.AvatarImgPath, BroMaker.AvatarShader);
                }
                return _TonyBrotana_Avatar;
            }
        }
        private static Material _TonyBrotana_Avatar;

        public static Material TonyBrotana_AvatarBloody
        {
            get
            {
                if (_TonyBrotana_AvatarBloody == null)
                {
                    _TonyBrotana_AvatarBloody = BroMaker.CreateMaterialFromFile(Main.AvatarBloodyImgPath);
                }
                return _TonyBrotana_AvatarBloody;
            }
        }
        private static Material _TonyBrotana_AvatarBloody;
    }
}
