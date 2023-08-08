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
        private static readonly string[] _objectNames = new string[] { "Bros"/*, "Grenade", "Weapon", "Cutscene", "Projectile", "Ability"*/ };
        private static string _fileName = string.Empty;
        private static readonly MakerObjectType[] _makerObjectTypes = new MakerObjectType[] { MakerObjectType.Bros/*, MakerObjectType.Grenade, MakerObjectType.Weapon, MakerObjectType.Cutscene, MakerObjectType.Projectile, MakerObjectType.Ability*/ };
        private static int _objectTypeSelected = 0;

        static CreateFileEditor()
        {
           // _makerObjectTypes = (MakerObjectType[])Enum.GetValues(typeof(MakerObjectType));
        }

        public static void UnityUI()
        {
            _objectTypeSelected = RGUI.ArrowList(_objectNames, _objectTypeSelected, 200);
            GUILayout.Label("Object Name");
            GUILayout.BeginHorizontal();
            _fileName = GUILayout.TextField(_fileName, GUILayout.Width(200));
            if (GUILayout.Button("Create New JSON " + _objectNames[_objectTypeSelected]))
            {
                CreateJsonFile(_fileName);
            }
            GUILayout.EndHorizontal();
        }

        public static void CreateJsonFile(string fileName)
        {
            try
            {
                switch (_makerObjectTypes[_objectTypeSelected])
                {
                    case MakerObjectType.Bros:
                        CustomBroInfo bInfo = new CustomBroInfo(fileName);
                        bInfo.Initialize();
                        bInfo.SerializeJSON(DirectoriesManager.BrosDirectory);
                        return;
                    case MakerObjectType.Grenade:
                        CustomGrenadeInfo gInfo = new CustomGrenadeInfo(fileName);
                        gInfo.Initialize();
                        gInfo.SerializeJSON(DirectoriesManager.GrenadesDirectory);
                        return;
                    case MakerObjectType.Weapon:
                        CustomWeaponInfo wInfo = new CustomWeaponInfo(fileName);
                        wInfo.Initialize();
                        wInfo.SerializeJSON(DirectoriesManager.WeaponsDirectory);
                        return;
                    case MakerObjectType.Cutscene:
                        CustomIntroCutscene c = new CustomIntroCutscene();
                        c.SerializeJSON(DirectoriesManager.CutscenesDirectory, _fileName);
                        return;
                    case MakerObjectType.Projectile:
                        /*CustomIntroCutscene c = new CustomIntroCutscene();
                        c.SerializeJSON(DirectoriesManager.CutscenesDirectory, _fileName);*/
                        return;
                    case MakerObjectType.Ability:
                        var a = new CustomAbilityInfo(fileName);
                        a.Initialize();
                        a.SerializeJSON(DirectoriesManager.AbilitiesDirectory);
                        return;
                }
            }
            catch (Exception ex)
            {
                Main.Log(ex.ToString(), LogType.Exception);
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
    }

    public enum MakerObjectType
    {
        Bros,
        Grenade,
        Weapon,
        Cutscene,
        Projectile,
        Ability,
    }
}
