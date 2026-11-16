using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using BroMakerLib.CustomObjects;
using BroMakerLib.Extensions;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Infos
{
    [Serializable]
    public class CustomBroforceObjectInfo
    {
        /// <summary>
        /// Name of the Custom Object
        /// </summary>
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

        private static readonly Dictionary<string, Component> lifecycleClones = new Dictionary<string, Component>();

        private static readonly HashSet<int> lifecycleCloneIds = new HashSet<int>();

        private void ApplyLifecycleFields(object obj, Dictionary<string, object> map, string stage)
        {
            using (BMLogger.Scope(Mod?.Name, name))
            {
                Dictionary<string, object> single = new Dictionary<string, object>(1);
                foreach (KeyValuePair<string, object> kvp in map)
                {
                    int dot = kvp.Key.IndexOf('.');
                    if (dot > 0 && kvp.Key != InjectedGrenadePlayerNumKey)
                    {
                        string fieldName = kvp.Key.Substring(0, dot);
                        try
                        {
                            UsePrefabCloneForField(obj, fieldName);
                        }
                        catch (Exception e)
                        {
                            BMLogger.Error($"Failed to copy the '{fieldName}' prefab for {stage}", e);
                            continue;
                        }
                    }
                    single.Clear();
                    single.Add(kvp.Key, kvp.Value);
                    obj.DynamicFieldsValueSetter(single, null,
                        (field, key, value) => SetFieldData(field, key, value, obj),
                        context: $"{name} {stage}",
                        onProblem: (key, message, problem) => ReportLifecycleProblem(stage, key, kvp.Value, message, problem));
                }
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

        private void UsePrefabCloneForField(object obj, string fieldName)
        {
            Traverse field = Traverse.Create(obj).Field(fieldName);
            Type fieldType = field.GetValueType();
            if (fieldType == null || !(typeof(Projectile).IsAssignableFrom(fieldType) || typeof(Grenade).IsAssignableFrom(fieldType)))
            {
                return;
            }

            Component current = field.GetValue() as Component;
            if (current == null || current is ICustomProjectile || lifecycleCloneIds.Contains(current.GetInstanceID()))
            {
                return;
            }

            string cacheKey = $"{path}|{name}|{fieldName}|{current.GetInstanceID()}";
            Component clone;
            if (!lifecycleClones.TryGetValue(cacheKey, out clone) || clone == null)
            {
                clone = BroMakerUtilities.ClonePrefab(current);
                lifecycleClones[cacheKey] = clone;
                lifecycleCloneIds.Add(clone.GetInstanceID());
            }
            field.SetValue(clone);
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
                foreach (KeyValuePair<string, object> pair in parameters)
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
            return new StringBuilder(_defaultName)
                .Append('_')
                .Append(Mathf.FloorToInt(UnityEngine.Random.value * 10).ToString())
                .ToString();
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

        protected void SetFieldData(Traverse field, string key, object value, object owner)
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
                    BMLogger.Warning($"Can't set sprite field '{key}' because the bro has no sprite there");
                    return;
                }

                Transform ownerTransform = owner is Component ownerComponent ? ownerComponent.transform : (owner as GameObject)?.transform;
                if (ownerTransform == null)
                {
                    BMLogger.Warning($"Sprite field '{key}' can only be set on a bro, so it was skipped");
                    return;
                }

                // Instantiate(existing) would duplicate the sprite's object and children, the whole bro for its own sprite.
                if (existing.transform.IsChildOf(ownerTransform))
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

                SpriteSM clone = UnityEngine.Object.Instantiate(existing);
                clone.transform.SetParent(ownerTransform, false);
                clone.transform.localPosition = existing.transform.localPosition;
                clone.transform.localRotation = existing.transform.localRotation;
                clone.transform.localScale = existing.transform.localScale;

                if (clone.meshRender == null)
                {
                    BMLogger.Warning($"Can't set the image of sprite field '{key}' because its copy has no renderer");
                    field.SetValue(clone);
                    return;
                }

                if (clone.meshRender.sharedMaterial != null)
                {
                    clone.meshRender.sharedMaterial = new Material(clone.meshRender.sharedMaterial);
                }

                clone.SetTexture(ResourcesController.GetTexture(path, (string)value));
                clone.RefreshTexture();
                field.SetValue(clone);
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
            else if (fieldType.IsEnum)
            {
                if (value is string)
                {
                    field.SetValue(Enum.Parse(fieldType, (string)value));
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
