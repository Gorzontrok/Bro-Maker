using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BroMakerLib.Cutscenes;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using RocketLib;
using UnityEngine;

namespace BroMakerLib.Editor
{
    public static class CreateFileEditor
    {
        private static readonly string[] _objectNames = new string[] { "Bro", "Ability" };
        private static string _fileName = string.Empty;
        private static string _modName = string.Empty;
        private static readonly MakerObjectFileType[] _makerObjectTypes = new MakerObjectFileType[] { MakerObjectFileType.Bros, MakerObjectFileType.Ability };
        private static int _objectTypeSelected = 0;

        static CreateFileEditor()
        {
           // _makerObjectTypes = (MakerObjectType[])Enum.GetValues(typeof(MakerObjectType));
        }

        public static void UnityUI()
        {
            GUILayout.Label("Mod Name: (it is also where the object is going to be placed", GUILayout.ExpandWidth(false));
            GUILayout.BeginHorizontal();
            _modName = GUILayout.TextField(_modName, GUILayout.Width(400));
            if (GUILayout.Button("Create New Mod", GUILayout.ExpandWidth(false)))
            {
                CreateJsonFile(_modName, _modName, MakerObjectFileType.Mods);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(30);

            _objectTypeSelected = RGUI.ArrowList(_objectNames, _objectTypeSelected, 200);
            GUILayout.Label("Object Name:");
            GUILayout.BeginHorizontal();
            _fileName = GUILayout.TextField(_fileName, GUILayout.Width(400));
            if (GUILayout.Button("Create New JSON " + _objectNames[_objectTypeSelected], GUILayout.ExpandWidth(false)))
            {
                CreateJsonFile(_fileName, _modName, _makerObjectTypes[_objectTypeSelected]);
            }
            GUILayout.EndHorizontal();
        }

        public static void CreateJsonFile(string fileName)
        {
            try
            {
                switch (_makerObjectTypes[_objectTypeSelected])
                {
                    case MakerObjectFileType.Bros:
                        CustomBroInfo bInfo = new CustomBroInfo(fileName);
                        bInfo.Initialize();
                        bInfo.SerializeJSON(DirectoriesManager.BrosDirectory);
                        return;
                    case MakerObjectFileType.Grenade:
                        CustomGrenadeInfo gInfo = new CustomGrenadeInfo(fileName);
                        gInfo.Initialize();
                        gInfo.SerializeJSON(DirectoriesManager.GrenadesDirectory);
                        return;
                    case MakerObjectFileType.Weapon:
                        CustomWeaponInfo wInfo = new CustomWeaponInfo(fileName);
                        wInfo.Initialize();
                        wInfo.SerializeJSON(DirectoriesManager.WeaponsDirectory);
                        return;
                    case MakerObjectFileType.Cutscene:
                        CustomIntroCutscene c = new CustomIntroCutscene();
                        c.SerializeJSON(DirectoriesManager.CutscenesDirectory, _fileName);
                        return;
                    case MakerObjectFileType.Projectile:
                        /*CustomIntroCutscene c = new CustomIntroCutscene();
                        c.SerializeJSON(DirectoriesManager.CutscenesDirectory, _fileName);*/
                        return;
                    case MakerObjectFileType.Ability:
                        var a = new AbilityInfo(fileName);
                        a.Initialize();
                        a.SerializeJSON(DirectoriesManager.AbilitiesDirectory);
                        return;
                    case MakerObjectFileType.Mods:
                        BroMakerMod mod = new BroMakerMod();
                        mod.SerializeJSON(Path.Combine(DirectoriesManager.StorageDirectory, fileName), fileName);
                        return;
                }
            }
            catch (Exception ex)
            {
                Main.Log(ex.ToString(), LogType.Exception);
            }
        }

        public static void CreateJsonFile(string fileName, string folder, MakerObjectFileType fileType)
        {
            string path = Path.Combine(DirectoriesManager.StorageDirectory, folder.IsNullOrEmpty() ? fileName : folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            switch (fileType)
            {
                case MakerObjectFileType.Mods:
                    BroMakerMod mod = new BroMakerMod();
                    mod.SerializeJSON(Path.Combine(DirectoriesManager.StorageDirectory, fileName), fileName);
                    break;
                default:
                    CustomBroforceObjectInfo info = GetInfo(fileName);
                    if (info != null)
                    {
                        info.SerializeJSON(path);
                    }
                    break;
            }
        }

        public static void DuplicateFile(string filePath)
        {
            string secondFileName = GetCloneFileName(filePath);
            string destFilename = Path.Combine(Path.GetDirectoryName(filePath), secondFileName);
            File.Copy(filePath, destFilename, false);
            BMLogger.Log("File Duplicated to: " +  destFilename);
        }

        public static string GetCloneFileName(string filePath)
        {
            string result = Path.GetFileNameWithoutExtension(filePath) + " - Clone.json";
            string path = Path.GetDirectoryName(filePath);
            int i = 0;
            while (File.Exists(Path.Combine(path, result)))
            {
                result = Path.GetFileNameWithoutExtension(filePath) + " - Clone_" + i + ".json";
            }
            return result;
        }

        public static CustomBroforceObjectInfo GetInfo(string fileName)
        {
            switch (_makerObjectTypes[_objectTypeSelected])
            {
                case MakerObjectFileType.Bros:
                    CustomBroInfo bro = new CustomBroInfo(fileName);
                    bro.Initialize();
                    return bro;
                case MakerObjectFileType.Grenade:
                    CustomGrenadeInfo grenade = new CustomGrenadeInfo(fileName);
                    grenade.Initialize();
                    return grenade;
                case MakerObjectFileType.Weapon:
                    CustomWeaponInfo weapon = new CustomWeaponInfo(fileName);
                    weapon.Initialize();
                    return weapon;
                case MakerObjectFileType.Cutscene:
                    CustomIntroCutscene c = new CustomIntroCutscene();
                    return null;
                case MakerObjectFileType.Projectile:
                    /*CustomIntroCutscene c = new CustomIntroCutscene();
                    c.SerializeJSON(DirectoriesManager.CutscenesDirectory, _fileName);*/
                    return null;
                case MakerObjectFileType.Ability:
                    var ability = new AbilityInfo(fileName);
                    ability.Initialize();
                    return ability;
                default:
                    return null;
            }
        }
    }

    public enum MakerObjectFileType
    {
        Bros,
        Grenade,
        Weapon,
        Cutscene,
        Projectile,
        Ability,
        Mods,
    }
}
