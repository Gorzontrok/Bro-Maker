using BroMakerLib.Loggers;
using BroMakerLib.Cutscenes;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using RocketLib;

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

        public CustomIntroCutscene cutscene = new CustomIntroCutscene();

        public Dictionary<string, object> parameters = new Dictionary<string, object>();
        // Ability names: [On + Method Name, *]
        public Dictionary<string, string[]> abilities = new Dictionary<string, string[]>();

        public Dictionary<string, object> beforeAwake = new Dictionary<string, object>();
        public Dictionary<string, object> afterAwake = new Dictionary<string, object>();
        public Dictionary<string, object> beforeStart = new Dictionary<string, object>();
        public Dictionary<string, object> afterStart = new Dictionary<string, object>();

        protected Dictionary<string, string[]> callableAbilities = new Dictionary<string, string[]>();

        [JsonIgnore]
        public string path = string.Empty;

        protected string _defaultName = "BroforceObject";
        public CustomBroforceObjectInfo()
        {
            name = GetUnknowName();
        }

        public CustomBroforceObjectInfo(string name)
        {
            if (string.IsNullOrEmpty(name))
                this.name = GetUnknowName();
            else
                this.name = name;
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
                throw new Exception("beforeAwake null");
            try
            {
                obj.DynamicFieldsValueSetter(beforeAwake, null, SetFieldData);
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }
        public void AfterAwake(object obj)
        {
            if (afterAwake == null)
                throw new Exception("afterAwake null");
            try
            {
                obj.DynamicFieldsValueSetter(afterAwake, null, SetFieldData);
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }
        public void BeforeStart(object obj)
        {
            if (beforeStart == null)
                throw new Exception("beforeStart null");
            try
            {
                obj.DynamicFieldsValueSetter(beforeStart, null, SetFieldData);
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }
        public void AfterStart(object obj)
        {
            if (afterStart == null)
                throw new Exception("afterStart null");
            try
            {
                obj.DynamicFieldsValueSetter(afterStart, null, SetFieldData);
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }

        public virtual void Initialize()
        {
            if(beforeAwake == null)
                beforeAwake = new Dictionary<string, object>();
            if(afterAwake == null)
                afterAwake = new Dictionary<string, object>();
            if(beforeStart == null)
                beforeStart = new Dictionary<string, object>();
            if(afterStart == null)
                afterStart = new Dictionary<string, object>();
            if(abilities == null)
                abilities = new Dictionary<string, string[]>();
            if (cutscene == null)
                cutscene = new CustomIntroCutscene();
        }
        public override string ToString()
        {
            return name;
        }
        public virtual void ReadParameters(object obj)
        { }

        public virtual string SerializeJSON()
        {
            return null;
        }
        public virtual string SerializeJSON(string folderPath)
        {
            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, settings);
            File.WriteAllText(Path.Combine(folderPath, this.name + ".json"), json);
            return json;
        }
        protected T GetParameterValue<T>(string key)
        {
            if (parameters.IsNullOrEmpty() || !parameters.ContainsKey(key)) return default(T);
            try
            {
                T val = (T)parameters[key];
                return val;
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog($"{key} value is not type of {typeof(T)}", ex);
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


        protected void SetFieldData(Traverse field, string key, object value)
        {
            try
            {
                Type fieldType = field.GetValueType();
                if (fieldType == typeof(SpriteSM))
                {
                    if (!(value is string))
                        throw new InvalidCastException("can't cast value to string on 'SpriteSM' field");

                    SpriteSM sprite = field.GetValue<SpriteSM>();
                    sprite.SetTexture(ResourcesController.CreateTexture(path, (string)value));
                    field.SetValue(sprite);
                }
                else if (fieldType == typeof(Material))
                {
                    if (!(value is string))
                        throw new InvalidCastException("can't cast value to string on 'Material' field");
                    Material material = field.GetValue<Material>();
                    material.mainTexture = ResourcesController.CreateTexture(path, (string)value);
                    field.SetValue(material);
                }
                else if (fieldType == typeof(Texture))
                {
                    if (!(value is string))
                        throw new InvalidCastException("can't cast value to string on 'Texture' field");
                    field.SetValue(ResourcesController.CreateTexture(path, (string)value));
                }
                else if (fieldType == typeof(Texture2D))
                {
                    if (!(value is string))
                        throw new InvalidCastException("can't cast value to string on 'Texture2D' field");
                    field.SetValue((Texture2D)ResourcesController.CreateTexture(path, (string)value));
                }
                else if (fieldType == typeof(Enum))
                {
                    if (value is string)
                    {
                        field.SetValue(Enum.Parse(fieldType, (string)value));
                    }
                    field.SetValue((int)value);
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
                else if (fieldType == typeof(Projectile) || fieldType.IsAssignableFrom(typeof(Projectile)))
                {
                    var name = (string)value;
                    var proj = CustomProjectileInfo.GetProjectileFromName(name);
                    if (proj != null)
                        field.SetValue(proj);
                }
                else if (fieldType == typeof(Grenade) || fieldType.IsAssignableFrom(typeof(Grenade)))
                {
                    var name = (string)value;
                    var grenade = CustomGrenadeInfo.GetGrenadeFromName(name);
                    if (grenade != null)
                        field.SetValue(grenade);
                }
                else
                {
                    field.SetValue(value);
                }
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog($"Key: {key} ; Value: {value}\n", ex);
            }
        }
    }
}
