using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BroMakerLib.Cutscenes
{
    public class CutsceneConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<CustomIntroCutscene>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                // Direct array of cutscenes
                return serializer.Deserialize<List<CustomIntroCutscene>>(reader);
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return new List<CustomIntroCutscene>();
            }
            else
            {
                // Single cutscene object - check for variantSprites
                JObject jo = JObject.Load(reader);
                JArray variantSprites = (JArray)jo["variantSprites"];
                
                if (variantSprites != null && variantSprites.Count > 0)
                {
                    // Remove variantSprites so it doesn't interfere with deserialization
                    jo.Remove("variantSprites");
                    
                    var cutscenes = new List<CustomIntroCutscene>();
                    var processedSprites = new HashSet<string>();
                    
                    // Check if there's an existing spritePath
                    string baseSpritePathStr = jo["spritePath"]?.ToString();
                    
                    // If base sprite exists and isn't in variantSprites, add it first
                    if (!string.IsNullOrEmpty(baseSpritePathStr))
                    {
                        bool isInVariants = false;
                        foreach (var sprite in variantSprites)
                        {
                            if (sprite.ToString() == baseSpritePathStr)
                            {
                                isInVariants = true;
                                break;
                            }
                        }
                        
                        if (!isInVariants)
                        {
                            // Add the base sprite as the first cutscene
                            var baseCutscene = jo.ToObject<CustomIntroCutscene>();
                            cutscenes.Add(baseCutscene);
                            processedSprites.Add(baseSpritePathStr);
                        }
                    }
                    
                    // Create a cutscene for each variant sprite (avoiding duplicates)
                    foreach (var sprite in variantSprites)
                    {
                        string spritePath = sprite.ToString();
                        if (!processedSprites.Contains(spritePath))
                        {
                            var cutsceneObj = jo.DeepClone() as JObject;
                            cutsceneObj["spritePath"] = sprite;
                            
                            // Deserialize and set the path
                            var cutscene = cutsceneObj.ToObject<CustomIntroCutscene>();
                            cutscenes.Add(cutscene);
                            processedSprites.Add(spritePath);
                        }
                    }
                    
                    return cutscenes;
                }
                else
                {
                    // No variantSprites - normal single cutscene
                    CustomIntroCutscene item = jo.ToObject<CustomIntroCutscene>();
                    return new List<CustomIntroCutscene> { item };
                }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            List<CustomIntroCutscene> list = (List<CustomIntroCutscene>)value;
            if (list.Count == 1)
            {
                serializer.Serialize(writer, list[0]);
            }
            else
            {
                serializer.Serialize(writer, list);
            }
        }
    }
    
    // Keep the generic version for backwards compatibility if needed elsewhere
    public class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize<List<T>>(reader);
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return new List<T>();
            }
            else
            {
                T item = serializer.Deserialize<T>(reader);
                return new List<T> { item };
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            List<T> list = (List<T>)value;
            if (list.Count == 1)
            {
                serializer.Serialize(writer, list[0]);
            }
            else
            {
                serializer.Serialize(writer, list);
            }
        }
    }
}