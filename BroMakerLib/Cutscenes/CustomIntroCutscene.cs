using System;
using System.IO;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using Newtonsoft.Json;
using RocketLib.JsonConverters;
using TFBGames.Systems;
using UnityEngine;

namespace BroMakerLib.Cutscenes
{
    [Serializable]
    public class CustomIntroCutscene
    {
        public static readonly Color HeadingDefaultColor = new Color(0.925f, 0.016f, 0.082f, 1.0f);
        public static readonly string Subtitle1DefaultText = "Joins the Battle!";
        public static readonly Color SubtitleDefaultColor = new Color(0.278f, 0.376f, 0.953f, 1.0f);

        [JsonIgnore, EditorIgnore]
        public static CutsceneIntroData rambroCutscene, requestedCutscene;

        [JsonIgnore]
        public string path = string.Empty;

        public bool playCutsceneOnFirstSpawn = false;

        // Heading
        public string heading = "Brononymous";
        public float headingScale = 0.15f;
        [JsonConverter(typeof(ColorJsonConverter))]
        public Color headingColor = HeadingDefaultColor;

        // Subtitle 1
        public string subtitle1 = Subtitle1DefaultText;
        [JsonConverter(typeof(ColorJsonConverter))]
        public Color subtitle1Color = SubtitleDefaultColor;
        public float subtitleScale = 0.10f;

        // Subtitle 2
        public string subtitle2 = string.Empty; // This won't be shown unless we use a cutscene with an existing mesh for this or create our own mesh
        [JsonConverter(typeof(ColorJsonConverter))]
        public Color subtitle2Color = SubtitleDefaultColor;
        public float subtitle2Scale = 1f;

        // Sprite
        public string spritePath = string.Empty;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 spriteSize = Vector2.zero;
        [JsonConverter(typeof(RectConverter))]
        public Rect spriteRect = Rect.zero;

        // Sprite Animation
        public bool isAnimated = false;
        public float animationStartDelay = 0.0f;
        public bool animationPingPong = false;
        public bool animationLoop = false;
        [JsonConverter(typeof(CutsceneSpriteAnimatedJsonConverter))]
        public Vector3 spriteAnimRateFramesWidth = Vector3.zero;

        public string barkPath = string.Empty;
        public bool playDefaultFanfare = true;
        public string fanfarePath = string.Empty;
        public string anim = string.Empty;

        public string backgroundPath = string.Empty;

        // To Implement
        //public AnimationClip animClip; // I don't think i can implement it. Maybe swaping between the existing animation clips.
        // I also looked into this anim clip thing, it seems to be possible to create with a script but I didn't have any luck getting it to work
        // I'll leave this here for future reference - https://docs.unity3d.com/2021.2/Documentation/ScriptReference/AnimationClip.SetCurve.html

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
            {
                LoadCutscene("Intro_Bro_Rambro");
                rambroCutscene = requestedCutscene;
            }

            var data = new CutsceneIntroDataExtra();
            data.heading = heading;
            data.subtitle1 = subtitle1;
            data.subtitle2 = subtitle2;
            data.headingScale = headingScale;
            data.subtitleScale = subtitleScale;

            if (string.IsNullOrEmpty(spritePath))
            {
                // Load default sprite when the Json doesn't provide one
                data.spriteTexture = ResourcesController.CreateTexture(ResourcesController.ExtractResource("BroMakerLib.Cutscenes.rambroSilhouette.png"));
            }
            else
            {
                try
                {
                    data.spriteTexture = ResourcesController.GetTexture(path, spritePath);
                }
                catch (Exception ex)
                {
                    BMLogger.Log(ex);
                    data.spriteTexture = rambroCutscene.spriteTexture;
                }
            }
            data.spriteSize = spriteSize;
            data.spriteRect = spriteRect;
            data.spriteAnimRateFramesWidth = spriteAnimRateFramesWidth;

            if (anim.IsNullOrEmpty())
            {
                data.animClip = rambroCutscene.animClip;
            }
            else
            {
                LoadCutscene(anim);
                data.animClip = requestedCutscene.animClip;
            }

            if (barkPath.IsNullOrEmpty())
            {
                data.bark = null;
            }
            else
            {
                try
                {
                    AudioClip bark = ResourcesController.GetAudioClip(path, barkPath);
                    data.bark = bark;
                }
                catch (Exception ex)
                {
                    BMLogger.Log(ex);
                    data.bark = null;
                }
            }

            if (fanfarePath.IsNullOrEmpty())
            {
                data.introFanfare = null;
            }
            else
            {
                try
                {
                    AudioClip fanfare = ResourcesController.GetAudioClip(path, fanfarePath);
                    data.introFanfare = fanfare;
                }
                catch (Exception ex)
                {
                    BMLogger.Log(ex);
                    data.introFanfare = null;
                }
            }

            // CutsceneIntroDataExtra
            data.headingColor = headingColor;
            data.subtitle1Color = subtitle1Color;
            data.subtitle2Color = subtitle2Color;
            data.isAnimated = isAnimated;
            data.animationStartDelay = animationStartDelay;
            data.animationPingPong = animationPingPong;
            data.animationLoop = animationLoop;
            data.subtitle2Scale = subtitle2Scale;

            if (!string.IsNullOrEmpty(backgroundPath))
            {
                // Load default sprite when the Json doesn't provide one
                try
                {
                    data.backgroundTexture = ResourcesController.GetTexture(path, backgroundPath);
                }
                catch (Exception ex)
                {
                    BMLogger.Log(ex);
                }
            }

            return data;
        }

        private void LoadCutscene(string resourceName)
        {
            ResourceManager resourceManager = GameSystems.ResourceManager;
            if (resourceManager != null)
            {
                var _curIntroResourceName = string.Format("{0}:{1}", "cutscenes", resourceName);
                resourceManager.Load(_curIntroResourceName, false, OnLoadComplete);
            }
        }
        private void OnLoadComplete(string resourceName, object asset)
        {
            requestedCutscene = (CutsceneIntroData)asset;
        }
    }
}

