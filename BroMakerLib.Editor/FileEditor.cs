using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Editor
{
    public static class FileEditor
    {
        public static MakerObjectType makerObjectType = MakerObjectType.Bros;
        private static string _fileName = string.Empty;

        public static void UnityUI(object obj)
        {
            try
            {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button(new GUIContent("Save To File"), GUILayout.ExpandWidth(false)))
                {
                    SaveFile(obj, false);
                }
                if (GUILayout.Button(new GUIContent("Save To File As Clone"), GUILayout.ExpandWidth(false)))
                {
                    SaveFile(obj, true);
                }
                GUILayout.EndHorizontal();

                FieldsEditor.MakeUnityGUI(obj);
            }
            catch(Exception e)
            {
                FieldsEditor.editHasError = true;
                BMLogger.ExceptionLog(e);
            }

        }

        private static void SaveFile(object obj, bool clone)
        {
            string fileName = clone ? CreateFileEditor.GetCloneFileName(Path.Combine(GetDirectory(), (obj as CustomBroforceObjectInfo).name)) : (obj as CustomBroforceObjectInfo).name + ".json";
            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
            File.WriteAllText(Path.Combine(GetDirectory(), fileName), json);
        }

        private static string GetDirectory()
        {
            switch(makerObjectType)
            {
                case MakerObjectType.Bros:
                    return DirectoriesManager.BrosDirectory;
                case MakerObjectType.Grenade:
                    return DirectoriesManager.GrenadesDirectory;
                case MakerObjectType.Weapon:
                    return DirectoriesManager.WeaponsDirectory;
                default:
                    return null;
            }
        }
    }
}
