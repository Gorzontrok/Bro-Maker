using BroMakerLib.Loggers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BroMakerLib.Editor
{
    public static class FieldsEditor
    {
        public static bool editHasError = false;

        public static bool IsPrimitive(this Type self)
        {
            return self == typeof(string) || (self.IsValueType & self.IsPrimitive);
        }
        private static BindingFlags BindingFlags
        {
            get
            {
                return BindingFlags.Instance | BindingFlags.Public /*| BindingFlags.NonPublic*/ | BindingFlags.FlattenHierarchy;
            }
        }
        private static FieldInfo[] GetFields(Type type)
        {
            return type.GetFields(BindingFlags);
        }

        public static void MakeUnityGUI(object obj)
        {
            if (editHasError) return;
            if(obj == null)
            {
                throw new NullReferenceException(obj.GetType().Name);
            }
            FieldInfo[] fields = null;
            try
            {
                Type type = obj.GetType();
                fields = GetFields(type);
            }
            catch(Exception e)
            {
                BMLogger.ExceptionLog(e);
                editHasError = true;
                return;
            }

            GUILayout.BeginVertical("box", GUILayout.MaxWidth(1000));
            try
            {
                List<string> fieldsAdded = new List<string>(); // To ignore parented variables when a 'new' keyword is on the current
                bool horizontalEnded = false;
                foreach (var fieldInfo in fields)
                {
                    horizontalEnded = false;

                    object[] attributes = fieldInfo.GetCustomAttributes(typeof(JsonIgnoreAttribute), false);
                    attributes.Concat(fieldInfo.GetCustomAttributes(typeof(EditorIgnoreAttribute), false));
                    if (attributes != null && attributes.Length == 0 && !fieldInfo.FieldType.IsArray && !fieldsAdded.Contains(fieldInfo.Name))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent(fieldInfo.Name.UpperWords().AddSpaces()), GUILayout.Width(300));
                        GUILayout.Space(10);
                        if (fieldInfo.FieldType.IsPrimitive() || fieldInfo.FieldType.IsEnum || fieldInfo.FieldType == typeof(Vector2))
                        {
                            HandleValueTypes(fieldInfo, obj);
                            fieldsAdded.Add(fieldInfo.Name);
                        }
                        else
                        {
                            HandleNonPrimitiveValue(fieldInfo, obj, ref horizontalEnded);
                        }
                        fieldsAdded.Add(fieldInfo.Name);
                        if (!horizontalEnded)
                            GUILayout.EndHorizontal();
                    }
                }
            }
            catch(Exception e)
            {
                BMLogger.ExceptionLog(e);
                editHasError = true;
            }
            finally
            {
                GUILayout.EndVertical();
            }
        }

        private static void HandleValueTypes(FieldInfo fieldInfo, object obj)
        {
            object value = fieldInfo.GetValue(obj);
            if(fieldInfo.FieldType == typeof(int))
            {
                int.TryParse(GUILayout.TextField(value.ToString()), out int val);
                value = val;
            }
            else if (fieldInfo.FieldType == typeof(float))
            {
                float.TryParse(GUILayout.TextField(value.ToString()), out float val);
                value = val;
            }
            else if (fieldInfo.FieldType == typeof(string))
            {
                value = GUILayout.TextField((string)value);
            }
            else if (fieldInfo.FieldType == typeof(bool))
            {
                value = GUILayout.Toggle((bool)value, GUIContent.none);
            }
            else if (fieldInfo.FieldType == typeof(Enum) || fieldInfo.FieldType.IsEnum)
            {
                HandleEnums(fieldInfo);
            }
            else if (fieldInfo.FieldType == typeof(Vector2))
            {
                Vector2 val = (Vector2)value;
                GUILayout.Label("X:", GUILayout.ExpandWidth(false)) ;
                float.TryParse(GUILayout.TextField(val.x.ToString()), out val.x);
                GUILayout.Space(5);
                GUILayout.Label("Y:", GUILayout.ExpandWidth(false));
                float.TryParse(GUILayout.TextField(val.y.ToString()), out val.y);
                value = val;
            }
            fieldInfo.SetValue(obj, value);
        }

        private static void HandleNonPrimitiveValue(FieldInfo fieldInfo, object obj, ref bool horizontalEnded)
        {
            object value = fieldInfo.GetValue(obj);
            if (value != null)
            {
                object[] attributes = fieldInfo.GetCustomAttributes(typeof(CantBeNullAttribute), false);
                if (attributes != null && attributes.Length == 0 && GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
                {
                    Type type = fieldInfo.FieldType;
                    fieldInfo.SetValue(obj, null);
                }
                else
                {
                    GUILayout.EndHorizontal();
                    horizontalEnded = true;

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    MakeUnityGUI(value);
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
                {
                    Type type = fieldInfo.FieldType;
                    fieldInfo.SetValue(obj, Activator.CreateInstance(type));
                }
            }
        }
        private static void HandleEnums(FieldInfo fieldInfo)
        {
            GUILayout.Label("ENUM");
        }
    }
}
