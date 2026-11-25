using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BroMakerLib.CustomObjects;
using BroMakerLib.Extensions;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace BroMakerLib.Infos
{
    [Serializable]
    public class CustomBroforceObjectInfo
    {
        /// <summary>Name of the Custom Object</summary>
        [JsonProperty(Required = Required.Always, Order = -9)]
        public string name;

        public Dictionary<string, object> parameters = new Dictionary<string, object>();

        public Dictionary<string, object> beforeAwake = new Dictionary<string, object>();
        public Dictionary<string, object> afterAwake = new Dictionary<string, object>();
        public Dictionary<string, object> beforeStart = new Dictionary<string, object>();
        public Dictionary<string, object> afterStart = new Dictionary<string, object>();

        [JsonIgnore] public string path = string.Empty;

        [JsonIgnore] internal BroMakerMod Mod;

        protected string _defaultName = "BroforceObject";

        public CustomBroforceObjectInfo()
        {
            name = GetUnknowName();
        }

        public CustomBroforceObjectInfo(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                this.name = GetUnknowName();
            }
            else
            {
                this.name = name;
            }
        }

        #region Static Methods
        public static T DeserializeJSON<T>(string jsonPath) where T : CustomBroforceObjectInfo
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(jsonPath));
        }
        #endregion

        public void BeforeAwake(object obj)
        {
            if (beforeAwake == null)
            {
                throw new Exception("beforeAwake null");
            }
            ApplyLifecycleFields(obj, beforeAwake, "beforeAwake");
        }

        public void AfterAwake(object obj)
        {
            if (afterAwake == null)
            {
                throw new Exception("afterAwake null");
            }
            ApplyLifecycleFields(obj, afterAwake, "afterAwake");
        }

        public void BeforeStart(object obj)
        {
            if (beforeStart == null)
            {
                throw new Exception("beforeStart null");
            }
            ApplyLifecycleFields(obj, beforeStart, "beforeStart");
        }

        public void AfterStart(object obj)
        {
            if (afterStart == null)
            {
                throw new Exception("afterStart null");
            }
            ApplyLifecycleFields(obj, afterStart, "afterStart");
        }

        private const string InjectedGrenadePlayerNumKey = "specialGrenade.playerNum";

        private static readonly Dictionary<string, Component> ownCopies = new Dictionary<string, Component>();

        private void ApplyLifecycleFields(object obj, Dictionary<string, object> map, string stage)
        {
            using (BMLogger.Scope(Mod?.Name, name))
            {
                ApplyFieldValues(obj, map, path, name, stage, $"{name} {stage}", (key, message, problem) => ReportLifecycleProblem(stage, key, map[key], message, problem));
            }
        }

        internal static void ApplyFieldValues(object target, Dictionary<string, object> map, string path, string broName, string stage, string context,
            Action<string, string, Exception> onProblem)
        {
            Dictionary<string, object> single = new Dictionary<string, object>(1);
            foreach (KeyValuePair<string, object> kvp in map.OrderBy(pair => pair.Key.Count(c => c == '.')))
            {
                if (kvp.Key.IndexOf('.') > 0 && kvp.Key != InjectedGrenadePlayerNumKey && !UseOwnCopiesAlongPath(target, kvp.Key, path, broName, stage))
                {
                    continue;
                }
                single.Clear();
                single.Add(kvp.Key, kvp.Value);
                target.DynamicFieldsValueSetter(single, null, (field, key, value) => SetFieldData(field, key, value, path, broName, target), context: context,
                    onProblem: onProblem);
            }
        }

        private static void ReportLifecycleProblem(string stage, string key, object value, string message, Exception problem)
        {
            if (problem == null)
            {
                BMLogger.Warning(message);
            }
            else
            {
                BMLogger.Error($"Failed to set {stage} field '{key}' (value: {value})", problem);
            }
        }

        private static bool UseOwnCopiesAlongPath(object target, string key, string path, string broName, string stage)
        {
            string[] segments = key.Split('.');
            object current = target;
            // A struct hop is a boxed copy, so a field set inside it wouldn't reach the bro.
            for (int i = 0; i < segments.Length - 1 && current != null && !current.GetType().IsValueType; i++)
            {
                Traverse field = Traverse.Create(current).Field(segments[i]);
                if (field.GetValueType() == null)
                {
                    return true;
                }
                current = field.GetValue();
                Component shared = current as Component;
                if (shared == null || !IsSharedAsset(shared))
                {
                    continue;
                }

                string hopPath = string.Join(".", segments, 0, i + 1);
                string cacheKey = $"{path}|{broName}|{target.GetType().FullName}|{hopPath}|{shared.GetInstanceID()}";
                Component copy;
                try
                {
                    if (!ownCopies.TryGetValue(cacheKey, out copy) || copy == null)
                    {
                        copy = BroMakerUtilities.ClonePrefab(shared);
                        ownCopies[cacheKey] = copy;
                    }
                    field.SetValue(copy);
                }
                catch (Exception e)
                {
                    BMLogger.Error($"Failed to copy the '{hopPath}' prefab for {stage}, so '{key}' was skipped", e);
                    return false;
                }
                current = copy;
            }
            return true;
        }

        // A prefab asset belongs to no scene; anything instantiated, BroMaker's own copies under DontDestroyOnLoad included, belongs to one.
        private static bool IsSharedAsset(Component component)
        {
            if (component is ICustomProjectile)
            {
                return false;
            }
            return !component.gameObject.scene.IsValid();
        }

        private static bool SetCopyImage(SpriteSM sprite, Texture2D texture, string key, string fileName)
        {
            if (texture == null)
            {
                BMLogger.Warning($"Can't set the image of sprite field '{key}' because '{fileName}' loaded no image, so it keeps its own image");
                return false;
            }
            // The clone never runs Awake, which is where SpriteSM assigns meshRender.
            if (sprite.meshRender == null)
            {
                sprite.meshRender = sprite.GetComponent<MeshRenderer>();
            }
            Renderer renderer = sprite.meshRender;
            if (renderer == null || renderer.sharedMaterial == null)
            {
                BMLogger.Warning($"Can't set the image of sprite field '{key}' because its sprite has no renderer or material, so it keeps its own image");
                return false;
            }
            if (renderer.sharedMaterial.mainTexture == texture)
            {
                return true;
            }
            Material material = new Material(renderer.sharedMaterial);
            material.mainTexture = texture;
            // Instances spawned from the clone read their texture from this material in their own Awake.
            renderer.sharedMaterial = material;
            return true;
        }

        public virtual void Initialize()
        {
            if (beforeAwake == null)
            {
                beforeAwake = new Dictionary<string, object>();
            }
            if (afterAwake == null)
            {
                afterAwake = new Dictionary<string, object>();
            }
            if (beforeStart == null)
            {
                beforeStart = new Dictionary<string, object>();
            }
            if (afterStart == null)
            {
                afterStart = new Dictionary<string, object>();
            }
        }

        public override string ToString()
        {
            return name;
        }

        public virtual void ReadParameters(object obj)
        {
            using (BMLogger.Scope(Mod?.Name, name))
            {
                foreach (KeyValuePair<string, object> pair in parameters.OrderBy(entry => entry.Key == "HaloPreset"))
                {
                    try
                    {
                        string key = pair.Key;
                        MethodInfo methodInfo = PresetManager.GetParameterMethod(key);
                        if (methodInfo == null)
                        {
                            BMLogger.Error($"There is no parameter '{key}'. Check its spelling, or whether it needs a mod that isn't installed");
                            continue;
                        }

                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        Type firstParameterType = parameters[0].ParameterType;
                        Type askerType = obj.GetType();
                        if (askerType != firstParameterType && !askerType.IsSubclassOf(firstParameterType))
                        {
                            BMLogger.Error($"Parameter '{key}' doesn't work on '{name}'");
                            continue;
                        }

                        Type secondMethodParameterType = parameters[1].ParameterType;
                        Type valueType = pair.Value.GetType();
                        if (valueType != secondMethodParameterType && !valueType.IsSubclassOf(secondMethodParameterType))
                        {
                            string given = BroMakerUtilities.DescribeJsonValue(valueType) ?? "that kind of value";
                            string needed = BroMakerUtilities.DescribeJsonValue(secondMethodParameterType);
                            BMLogger.Error(needed != null ? $"Parameter '{key}' needs {needed}, not {given}" : $"Parameter '{key}' can't take {given}");
                            continue;
                        }
                        methodInfo.Invoke(null, new object[] { obj, pair.Value });
                    }
                    catch (Exception e)
                    {
                        BMLogger.Error($"Exception applying parameter '{pair.Key}' for '{name}'", e);
                    }
                }
            }
        }

        public virtual string SerializeJSON()
        {
            return null;
        }

        public virtual string SerializeJSON(string folderPath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(this, Formatting.Indented, settings);
            File.WriteAllText(Path.Combine(folderPath, name + ".json"), json);
            return json;
        }

        protected T GetParameterValue<T>(string key)
        {
            if (parameters.IsNullOrEmpty() || !parameters.ContainsKey(key))
            {
                return default(T);
            }
            try
            {
                T val = (T)parameters[key];
                return val;
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Parameter '{key}' needs a value of type {typeof(T)}", ex, Mod?.Name, name);
            }
            return default(T);
        }

        protected virtual string GetUnknowName()
        {
            return new StringBuilder(_defaultName).Append('_').Append(Mathf.FloorToInt(UnityEngine.Random.value * 10).ToString()).ToString();
        }


        private static void RepointMaterialFields(object owner, Material oldMaterial, Material newMaterial)
        {
            for (Type type = owner.GetType(); type != null && type != typeof(MonoBehaviour); type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType == typeof(Material) && ReferenceEquals(field.GetValue(owner), oldMaterial))
                    {
                        field.SetValue(owner, newMaterial);
                    }
                }
            }
        }

        private static readonly Dictionary<Type, string[]> StructComponentNames = new Dictionary<Type, string[]>
        {
            { typeof(Vector2), new string[] { "x", "y" } },
            { typeof(Vector3), new string[] { "x", "y", "z" } },
            { typeof(Color), new string[] { "r", "g", "b", "a" } }
        };

        private static int MinimumStructArrayLength(Type fieldType)
        {
            return fieldType == typeof(Color) ? 3 : StructComponentNames[fieldType].Length;
        }

        private static string DescribeStructForms(Type fieldType)
        {
            string[] names = StructComponentNames[fieldType];
            int minimum = MinimumStructArrayLength(fieldType);
            string count = minimum == names.Length ? names.Length.ToString() : $"{minimum} or {names.Length}";
            return $"an array of {count} numbers or an object with any of {string.Join(", ", names)}";
        }

        private static bool IsJsonNumber(JToken token)
        {
            return token.Type == JTokenType.Integer || token.Type == JTokenType.Float;
        }

        private static object ReadStructComponents(Type fieldType, object current, object value)
        {
            string[] names = StructComponentNames[fieldType];
            float[] components = new float[names.Length];
            if (current is Vector2 currentVector2)
            {
                components[0] = currentVector2.x;
                components[1] = currentVector2.y;
            }
            else if (current is Vector3 currentVector3)
            {
                components[0] = currentVector3.x;
                components[1] = currentVector3.y;
                components[2] = currentVector3.z;
            }
            else if (current is Color currentColor)
            {
                components[0] = currentColor.r;
                components[1] = currentColor.g;
                components[2] = currentColor.b;
                components[3] = currentColor.a;
            }

            if (value is JArray array)
            {
                if (array.Count < MinimumStructArrayLength(fieldType) || array.Count > names.Length)
                {
                    return null;
                }
                for (int i = 0; i < names.Length; i++)
                {
                    if (i >= array.Count)
                    {
                        components[i] = 1f;
                    }
                    else if (IsJsonNumber(array[i]))
                    {
                        components[i] = array[i].Value<float>();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else if (value is JObject jObject)
            {
                foreach (JProperty property in jObject.Properties())
                {
                    int index = Array.IndexOf(names, property.Name);
                    if (index < 0 || !IsJsonNumber(property.Value))
                    {
                        return null;
                    }
                    components[index] = property.Value.Value<float>();
                }
            }
            else
            {
                return null;
            }

            if (fieldType == typeof(Vector2))
            {
                return new Vector2(components[0], components[1]);
            }
            if (fieldType == typeof(Vector3))
            {
                return new Vector3(components[0], components[1], components[2]);
            }
            return new Color(components[0], components[1], components[2], components[3]);
        }

        internal static void SetFieldData(Traverse field, string key, object value, string path, string broName, object owner)
        {
            Type fieldType = field.GetValueType();
            if (fieldType == typeof(SpriteSM))
            {
                if (!(value is string))
                {
                    throw new InvalidCastException("can't cast value to string on 'SpriteSM' field");
                }

                SpriteSM existing = field.GetValue<SpriteSM>();
                if (existing == null)
                {
                    BMLogger.Warning($"Can't set sprite field '{key}' because it holds no sprite");
                    return;
                }

                if (BroMakerUtilities.IsPrefabClone(existing))
                {
                    SetCopyImage(existing, ResourcesController.GetTexture(path, (string)value), key, (string)value);
                    return;
                }

                if (!IsSharedAsset(existing))
                {
                    Renderer renderer = existing.meshRender != null ? existing.meshRender : existing.GetComponent<Renderer>();
                    Material oldMaterial = renderer != null ? renderer.sharedMaterial : null;
                    Material material = ResourcesController.GetMaterial(path, (string)value);
                    existing.SetMaterialAndRefresh(material);
                    // Vanilla restore code writes cached materials back to the renderer, so caches of the old material must follow the swap.
                    if (oldMaterial != null)
                    {
                        RepointMaterialFields(owner, oldMaterial, material);
                    }
                    return;
                }

                string cacheKey = $"{path}|{broName}|SpriteSM|{key}|{value}|{existing.GetInstanceID()}";
                Component copy;
                if (!ownCopies.TryGetValue(cacheKey, out copy) || copy == null)
                {
                    Texture2D texture = ResourcesController.GetTexture(path, (string)value);
                    SpriteSM spriteCopy = BroMakerUtilities.ClonePrefab(existing);
                    if (!SetCopyImage(spriteCopy, texture, key, (string)value))
                    {
                        UnityEngine.Object.Destroy(spriteCopy.gameObject);
                        return;
                    }
                    copy = spriteCopy;
                    ownCopies[cacheKey] = copy;
                }
                field.SetValue(copy);
            }
            else if (fieldType == typeof(Material))
            {
                if (!(value is string))
                {
                    throw new InvalidCastException("can't cast value to string on 'Material' field");
                }
                field.SetValue(ResourcesController.GetMaterial(path, (string)value));
            }
            else if (fieldType == typeof(Texture))
            {
                if (!(value is string))
                {
                    throw new InvalidCastException("can't cast value to string on 'Texture' field");
                }
                field.SetValue(ResourcesController.GetTexture(path, (string)value));
            }
            else if (fieldType == typeof(Texture2D))
            {
                if (!(value is string))
                {
                    throw new InvalidCastException("can't cast value to string on 'Texture2D' field");
                }
                field.SetValue((Texture2D)ResourcesController.GetTexture(path, (string)value));
            }
            else if (fieldType == typeof(AudioClip))
            {
                if (!(value is string))
                {
                    throw new InvalidCastException("can't cast value to string on 'AudioClip' field");
                }
                field.SetValue(ResourcesController.GetAudioClip(path, (string)value));
            }
            else if (fieldType == typeof(AudioClip[]))
            {
                string[] fileNames;
                if (value is JArray jArray)
                {
                    fileNames = jArray.Select(t => t.ToString()).ToArray();
                }
                else if (value is string[] strArray)
                {
                    fileNames = strArray;
                }
                else
                {
                    throw new InvalidCastException("can't cast value to string[] on 'AudioClip[]' field");
                }

                AudioClip[] clips = new AudioClip[fileNames.Length];
                for (int i = 0; i < fileNames.Length; i++)
                {
                    clips[i] = ResourcesController.GetAudioClip(path, fileNames[i]);
                }
                field.SetValue(clips);
            }
            else if (fieldType.IsEnum)
            {
                if (value is string)
                {
                    field.SetValue(Enum.Parse(fieldType, (string)value, true));
                }
                else
                {
                    field.SetValue(Convert.ToInt32(value));
                }
            }
            else if (fieldType == typeof(float))
            {
                if (value is double)
                {
                    value = Convert.ToSingle(value);
                }
                field.SetValue(value);
            }
            else if (fieldType == typeof(Int32))
            {
                value = Convert.ToInt32(value);
                field.SetValue(value);
            }
            else if (Nullable.GetUnderlyingType(fieldType) != null)
            {
                // JSON numbers arrive as Int64 or double, and reflection won't widen them into a Nullable<T>.
                Type underlyingType = Nullable.GetUnderlyingType(fieldType);
                if (value == null)
                {
                    field.SetValue(null);
                }
                else if (underlyingType.IsEnum)
                {
                    field.SetValue(Enum.Parse(underlyingType, value.ToString(), true));
                }
                else
                {
                    field.SetValue(Convert.ChangeType(value, underlyingType));
                }
            }
            else if (fieldType == typeof(Vector2) || fieldType == typeof(Vector3) || fieldType == typeof(Color))
            {
                object converted = ReadStructComponents(fieldType, field.GetValue(), value);
                if (converted == null)
                {
                    BMLogger.Warning(
                        $"Can't set field '{key}' to {(value is JToken token ? token.ToString(Formatting.None) : value)}. It should be {DescribeStructForms(fieldType)}");
                    return;
                }
                field.SetValue(converted);
            }
            else if (typeof(Projectile).IsAssignableFrom(fieldType))
            {
                string name = (string)value;
                Projectile proj = LoadBroforceObjects.GetProjectileFromName(name);
                if (proj != null)
                {
                    field.SetValue(proj);
                }
            }
            else if (typeof(Grenade).IsAssignableFrom(fieldType))
            {
                string name = (string)value;
                Grenade grenade = LoadBroforceObjects.GetGrenadeFromName(name);
                if (grenade != null)
                {
                    field.SetValue(grenade);
                }
            }
            else
            {
                field.SetValue(value);
            }
        }
    }
}
