using BroMakerLib.Loggers;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace BroMakerLib
{
    public class CutsceneSpriteAnimatedJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Vector3))
            {
                return true;
            }
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var t = serializer.Deserialize(reader);

            Vector3 result = Vector3.zero;

            try
            {
                result = JsonConvert.DeserializeObject<CutsceneSpriteAnimated>(t.ToString()).ToVector3();
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog("'spriteAnimRateFramesWidth' Cant convert to 'CutsceneSpriteAnimated'", ex);
            }

            if (result != Vector3.zero)
                return result;

            try
            {
                result = JsonConvert.DeserializeObject<Vector3>(t.ToString());
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog("'spriteAnimRateFramesWidth' Cant convert to Vector3", ex);
            }
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Vector3 v = (Vector3)value;

            writer.WriteStartObject();
            writer.WritePropertyName("FrameRate");
            writer.WriteValue(v.x);
            writer.WritePropertyName("TotalFrames");
            writer.WriteValue(v.y);
            writer.WritePropertyName("FrameWidth");
            writer.WriteValue(v.z);
            writer.WriteEndObject();
        }


        private class CutsceneSpriteAnimated
        {
            public float FrameRate = 0.0f;
            public float TotalFrames = 0.0f;
            public float FrameWidth = 0.0f;

            public Vector3 ToVector3()
            {
                return new Vector3(FrameRate, TotalFrames, FrameWidth);
            }
        }
    }
}
