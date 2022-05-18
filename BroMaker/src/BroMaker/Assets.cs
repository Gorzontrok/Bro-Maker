using System;
using System.Collections.Generic;
using System.IO;
using BroMakerLoadMod;
using UnityEngine;

namespace BroMakerLib
{
    /// <summary>
    /// Assets collection of the mod
    /// </summary>
    public static class Assets
    {
        private static string assetsPath
        {
            get
            {
                return Path.Combine(Main.mod.Path, "assets");
            }
        }

        /// <summary>
        /// Default special icon. Enabled if the given one is null.
        /// </summary>
        public static Material DefaultGrenadeIcon
        {
            get
            {
                if (_defaultGrenadeIcon == null)
                {
                    _defaultGrenadeIcon = BroMaker.CreateMaterialFromFile(Path.Combine(assetsPath, "grenadeIcon.png"), BroMaker.Shader1);
                }
                return _defaultGrenadeIcon;
            }
        }
        private static Material _defaultGrenadeIcon;

        /// <summary>
        /// Default avatar. Enabled if the given one is null.
        /// </summary>
        public static Material EmptyAvatar
        {
            get
            {
                if (_emptyAvatar == null)
                {
                    _emptyAvatar = BroMaker.CreateMaterialFromFile(Path.Combine(assetsPath, "avatar_empty.png"), BroMaker.Shader1);
                }
                return _emptyAvatar;
            }
        }
        private static Material _emptyAvatar;

        /// <summary>
        /// Empty character material.
        /// </summary>
        public static Material EmptyCharacter
        {
            get
            {
                if (_emptyCharacter == null)
                {
                    _emptyCharacter = BroMaker.CreateMaterialFromFile(Path.Combine(assetsPath, "emptyBody_anim.png"), BroMaker.Shader1);
                }
                return _emptyCharacter;
            }
        }
        private static Material _emptyCharacter;


        /// <summary>
        /// Empty gun material.
        /// </summary>
        public static Material EmptyGun
        {
            get
            {
                if (_emptyGun == null)
                {
                    _emptyGun = BroMaker.CreateMaterialFromFile(Path.Combine(assetsPath, "empty_gun_anim.png"), BroMaker.Shader1);
                }
                return _emptyGun;
            }
        }
        private static Material _emptyGun;
    }
}
