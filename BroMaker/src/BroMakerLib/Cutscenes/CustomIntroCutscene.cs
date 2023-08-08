using BroMakerLib.Infos;
using RocketLib.JsonConverters;
using Newtonsoft.Json;
using System;
using System.IO;
using TFBGames.Systems;
using UnityEngine;

namespace BroMakerLib.Cutscenes
{
    [Serializable]
    public class CustomIntroCutscene
    {
        [JsonIgnore, EditorIgnore]
        public static CutsceneIntroData rambroCutscene;

        public string heading = string.Empty;
        public string subtitle1 = string.Empty;
        public string subtitle2 = string.Empty;
        public float headingScale = 0.132f;
        public float subtitleScale = 0.15f;
        public string spritePath = string.Empty;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 spriteSize = Vector2.zero;
        [JsonConverter(typeof(RectConverter))]
        public Rect spriteRect = Rect.zero;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 spriteAnimRateFramesWidth = Vector3.zero;

        // To Implement
        //public AudioClip bark;
        //public AudioClip introFanfare;
        //public AnimationClip animClip; // I don't think i can implement it. Maybe swaping between the existing animation clips.

        public static T DeserializeJSON<T>(string jsonPath) where T : CustomBroforceObjectInfo
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(jsonPath));
        }
        public string SerializeJSON(string folderPath, string fileName)
        {
            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, settings);
            File.WriteAllText(Path.Combine(folderPath, fileName + ".json"), json);
            return json;
        }

        public CutsceneIntroData ToCutsceneIntroData(CutsceneIntroRoot root)
        {
            // get Rambro cutscene
            if (rambroCutscene == null)
                LoadRambro(root);

            var data = new CutsceneIntroData();
            data.subtitle1 = subtitle1;
            data.subtitle2 = subtitle2;
            data.headingScale = headingScale;
            data.subtitleScale = subtitleScale;

            if (string.IsNullOrEmpty(spritePath))
                data.spriteTexture = rambroCutscene.spriteTexture;
            else
                data.spriteTexture = ResourcesController.CreateTexture(spritePath);
            data.spriteSize = spriteSize;
            data.spriteRect = spriteRect;
            data.spriteAnimRateFramesWidth = spriteAnimRateFramesWidth;


            data.animClip = rambroCutscene.animClip;
            data.bark = rambroCutscene.bark;
            data.introFanfare = rambroCutscene.introFanfare;

            return data;
        }

        private void LoadRambro(CutsceneIntroRoot root)
        {
            ResourceManager resourceManager = GameSystems.ResourceManager;
            if (resourceManager != null)
            {
                var _curIntroResourceName = string.Format("{0}:{1}", root.bundleName, "Intro_Bro_Rambro");
                resourceManager.Load(_curIntroResourceName, false, OnLoadComplete);
            }
        }
        private void OnLoadComplete(string resourceName, object asset)
        {
            rambroCutscene = (CutsceneIntroData)asset;
        }
    }
}
