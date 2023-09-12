using RocketLib;
using System;
using UnityEngine;

namespace BroMakerLib.Editor
{
    public static class ToUnityUIExtensions
    {
        #region ValueTypes
        #region String
        /// <summary>
        /// Text Field
        /// </summary>
        public static string ToUnityUI(this string str, params GUILayoutOption[] options)
        {
            return str.ToUnityUI(-1, null, options);
        }
        /// <summary>
        /// Text Field
        /// </summary>
        public static string ToUnityUI(this string str, int maxLenght, params GUILayoutOption[] options)
        {
            return str.ToUnityUI(maxLenght, null, options);
        }
        /// <summary>
        /// Text Field
        /// </summary>
        public static string ToUnityUI(this string str, int maxLenght, GUIStyle style, params GUILayoutOption[] options)
        {
            if (style == null)
                style = GUI.skin.textField;

            str = GUILayout.TextField(str, maxLenght, style, options);
            return str;
        }
        #endregion

        #region Int
        /// <summary>
        /// Text Field
        /// </summary>
        public static int ToUnityUI(this int value, params GUILayoutOption[] options)
        {
            int.TryParse(value.ToString().ToUnityUI(-1, null, options), out value);
            return value;
        }
        /// <summary>
        /// Text Field
        /// </summary>
        public static int ToUnityUI(this int value, int maxLenght, params GUILayoutOption[] options)
        {
            int.TryParse(value.ToString().ToUnityUI(maxLenght, null, options), out value);
            return value;
        }
        /// <summary>
        /// Text Field
        /// </summary>
        public static int ToUnityUI(this int value, int maxLenght, GUIStyle style, params GUILayoutOption[] options)
        {
            int.TryParse(value.ToString().ToUnityUI(maxLenght, style, options), out value);
            return value;
        }
        #endregion
        #region Float
        /// <summary>
        /// Text Field
        /// </summary>
        public static float ToUnityUI(this float value, params GUILayoutOption[] options)
        {
            float.TryParse(value.ToString().ToUnityUI(-1, null, options), out value);
            return value;
        }
        /// <summary>
        /// Text Field
        /// </summary>
        public static float ToUnityUI(this float value, int maxLenght, params GUILayoutOption[] options)
        {
            float.TryParse(value.ToString().ToUnityUI(maxLenght, null, options), out value);
            return value;
        }
        /// <summary>
        /// Text Field
        /// </summary>
        public static float ToUnityUI(this float value, int maxLenght, GUIStyle style, params GUILayoutOption[] options)
        {
            float.TryParse(value.ToString().ToUnityUI(maxLenght, style, options), out value);
            return value;
        }
        #endregion

        #region Bool
        /// <summary>
        /// Toggle
        /// </summary>
        public static bool ToUnityUI(this bool value, params GUILayoutOption[] options)
        {
            return value.ToUnityUI(GUIContent.none, null, options);
        }
        /// <summary>
        /// Text Field
        /// </summary>
        public static bool ToUnityUI(this bool value, GUIContent content, params GUILayoutOption[] options)
        {
            return value.ToUnityUI(content, null, options);
        }
        /// <summary>
        /// Toggle
        /// </summary>
        public static bool ToUnityUI(this bool value, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            if (style == null)
                style = GUI.skin.toggle;

            value = GUILayout.Toggle(value, content, style, options);
            return value;
        }
        #endregion
        #endregion

        #region Enumerators
        public static int ToUnityUI<T>(this T enumerator, int selected) where T : Enum
        {
            T[] values = enumerator.GetValues();
            return RGUI.ArrowList(values.AsStrings(), selected);
        }
        public static int ToUnityUI<T>(this T enumerator, int selected, float width) where T : Enum
        {
            T[] values = enumerator.GetValues();
            return RGUI.ArrowList(values.AsStrings(), selected, width);
        }
        #endregion

        #region Unity structs

        #region Vector2
        /// <summary>
        /// 2 Text Area
        /// </summary>
        public static Vector2 ToUnityUI(this Vector2 value, params GUILayoutOption[] options)
        {
            return value.ToUnityUI(-1, null, options);
        }
        /// <summary>
        /// 2 Text Area
        /// </summary>
        public static Vector2 ToUnityUI(this Vector2 value, int maxLenght, params GUILayoutOption[] options)
        {
            return value.ToUnityUI(maxLenght, null, options);
        }
        /// <summary>
        /// 2 Text Area
        /// </summary>
        public static Vector2 ToUnityUI(this Vector2 value, int maxLenght, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label("X:", GUILayout.ExpandWidth(false));
            value.x = value.x.ToUnityUI(maxLenght, style, options);
            GUILayout.Space(5);
            GUILayout.Label("Y:", GUILayout.ExpandWidth(false));
            value.y = value.y.ToUnityUI(maxLenght, style, options);
            return value;
        }
        #endregion

        #region Vector3
        /// <summary>
        /// 2 Text Area
        /// </summary>
        public static Vector3 ToUnityUI(this Vector3 value, params GUILayoutOption[] options)
        {
            return value.ToUnityUI(-1, null, options);
        }
        /// <summary>
        /// 2 Text Area
        /// </summary>
        public static Vector3 ToUnityUI(this Vector3 value, int maxLenght, params GUILayoutOption[] options)
        {
            return value.ToUnityUI(maxLenght, null, options);
        }
        /// <summary>
        /// 2 Text Area
        /// </summary>
        public static Vector3 ToUnityUI(this Vector3 value, int maxLenght, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label("X:", GUILayout.ExpandWidth(false));
            value.x = value.x.ToUnityUI(maxLenght, style, options);
            GUILayout.Space(5);
            GUILayout.Label("Y:", GUILayout.ExpandWidth(false));
            value.y = value.y.ToUnityUI(maxLenght, style, options);
            GUILayout.Space(5);
            GUILayout.Label("Z:", GUILayout.ExpandWidth(false));
            value.z = value.z.ToUnityUI(maxLenght, style, options);
            return value;
        }
        #endregion

        #endregion


        #region RocketLib
        public static FloatRange ToUnityUI(this FloatRange floatRange, params GUILayoutOption[] options)
        {
            return floatRange.ToUnityUI(-1, null, options);
        }
        /// <summary>
        /// 2 Text Area
        /// </summary>
        public static FloatRange ToUnityUI(this FloatRange floatRange, int maxLenght, params GUILayoutOption[] options)
        {
            return floatRange.ToUnityUI(maxLenght, null, options);
        }
        /// <summary>
        /// 2 Text Area
        /// </summary>
        public static FloatRange ToUnityUI(this FloatRange floatRange, int maxLenght, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label("Min:", GUILayout.ExpandWidth(false));
            floatRange.Min = floatRange.Min.ToUnityUI(maxLenght, style, options);
            GUILayout.Space(5);
            GUILayout.Label("Max:", GUILayout.ExpandWidth(false));
            floatRange.Max = floatRange.Max.ToUnityUI(maxLenght, style, options);
            return floatRange;
        }
        #endregion
    }
}
