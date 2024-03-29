﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using RoboRyanTron.SearchableEnum;
using RoboRyanTron.SearchableEnum.Editor;
using UnityEditor.Graphs;

namespace VisualDelegates.Editor
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Inspector class for rendering <see cref="RawArgument"/>s in the inspector</summary>
    [CustomPropertyDrawer(typeof(RawArgument), true)]
    internal class DrawerRawArgument : PropertyDrawer
    {
        const string X1 = "_x1";
        const string X2 = "_x2";
        const string Y1 = "_y1";
        const string Y2 = "_y2";
        const string Z1 = "_z1";
        const string Z2 = "_z2";
        //=======================
        // Render
        //=======================
        /// <summary>Calculates the inspector height</summary>
        /// <param name="tProperty">Serialized <see cref="RawArgument"/> property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        /// <returns>Height of the drawer</returns>
        public override float GetPropertyHeight(SerializedProperty tProperty, GUIContent tLabel)
        {
            var usingref = tProperty.FindPropertyRelative("UseReference").boolValue;
            var argumentCache = ViewCache.GetRawArgumentCache(tProperty);
            float tempHeight = 0;
            if (usingref)
            {
                tempHeight += EditorGUI.GetPropertyHeight(tProperty.FindPropertyRelative("call_Reference"));
            }
            else if (argumentCache.hasCustomType)
            {
                tempHeight = base.GetPropertyHeight(tProperty, tLabel);

                tempHeight = EditorGUI.GetPropertyHeight(tProperty.FindPropertyRelative("custom"));
            }
            else
            {
                tempHeight = base.GetPropertyHeight(tProperty, tLabel);
                string argumentName = tProperty.FindPropertyRelative("FullArgumentName").stringValue;
                if (argumentName == "UnityEngine.Rect")
                    tempHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                else if (argumentName == "UnityEngine.Bounds")
                    tempHeight += (EditorGUIUtility.singleLineHeight) + EditorGUIUtility.standardVerticalSpacing;
                ValidateArgumentCache(tProperty, argumentName, argumentCache);
            }
            return tempHeight;
        }
        private void ValidateArgumentCache(SerializedProperty argumentprop, string argumentTypename, RawArgumentView argumentCache)
        {
            var arg_type = Type.GetType(argumentprop.FindPropertyRelative("stringValue").stringValue);
            if (arg_type != null && argumentTypename == Utility.DOTNET_TYPE_NAME && argumentCache.CurrentScript == null)
            {
                argumentCache.CurrentScript = Resources.FindObjectsOfTypeAll<MonoScript>().First(m => m.GetClass() == arg_type);
            }
        }
        /// <summary>Renders the appropriate input field of the <see cref="RawArgument"/> property</summary>
        /// <param name="tPosition">Inspector position and size of <paramref name="tProperty"/></param>
        /// <param name="tProperty">Serialized <see cref="RawArgument"/> property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        public override void OnGUI(UnityEngine.Rect tPosition, SerializedProperty tProperty, GUIContent paramLabel)
        {
            SerializedProperty useReference = tProperty.FindPropertyRelative("UseReference");
            var argument_cache = ViewCache.GetRawArgumentCache(tProperty);

            var reference_content = new GUIContent("Use ref");
            VisualEditorUtility.StandardStyle.CalcMinMaxWidth(reference_content, out float min, out float max);


            VisualEditorUtility.StandardStyle.CalcMinMaxWidth(paramLabel, out float minoffset, out float maxoffset);
            var labelpos = tPosition;
            labelpos.x -= 50;
            EditorGUI.LabelField(labelpos, paramLabel);


            var argumentpos = labelpos;
            argumentpos.width -= (maxoffset + max);
            argumentpos.x += maxoffset;
          EditorGUI.BeginChangeCheck();
            if (!useReference.boolValue)
                DisplayArgument(argumentpos, tProperty, paramLabel);
            else
                DisplayReference(argumentpos, tProperty, paramLabel, reference_content, argument_cache);
            if (EditorGUI.EndChangeCheck())
            {
                if (EditorApplication.isPlaying)
                    VisualEditorUtility.ReinitializeDelegate(tProperty.GetVisualDelegateObject());
                else
                    tProperty.serializedObject.ApplyModifiedProperties();
            }
            if (!argument_cache.hasCustomType && argument_cache.CurrentCustomType != null)
            {
                argument_cache.hasCustomType = false;
                argument_cache.CurrentCustomType = null;
                tProperty.FindPropertyRelative("custom").managedReferenceValue = null;
                ViewCache.getRawCallCacheFromRawArgument(tProperty).RequiresRecalculation = true;
            }

            var togglerect = argumentpos;
            //  togglerect.x = (argumentpos.width + maxoffset) - 25f;
            //togglerect.width = max+maxoffset;
            togglerect.x = argumentpos.xMax;


            EditorGUI.BeginChangeCheck();
            useReference.boolValue = EditorGUI.Toggle(togglerect, useReference.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                tProperty.FindPropertyRelative("call_Reference").FindPropertyRelative("willdeseralize").boolValue = useReference.boolValue;
                ViewCache.getRawCallCacheFromRawArgument(tProperty).RequiresRecalculation = true;
                tProperty.serializedObject.ApplyModifiedProperties();
                VisualEditorUtility.ReinitializeDelegate(tProperty.GetVisualDelegateObject());
            }
            var reflabelpos = togglerect;
            reflabelpos.x += 20f;
            reflabelpos.width += max * 2f;
            EditorGUI.LabelField(reflabelpos, reference_content);
        }

        private void DisplayArgument(Rect argpos, SerializedProperty tProperty, GUIContent paramLabel)
        {
            RawArgumentView argument_cache = ViewCache.GetRawArgumentCache(tProperty);
            string assemblyTypeName = tProperty.FindPropertyRelative("assemblyQualifiedArgumentName").stringValue;
            string FullTypeName = tProperty.FindPropertyRelative("FullArgumentName").stringValue;
            var propertyID = new PropertyName(FullTypeName);
            argpos.x += 10;
            if (String.IsNullOrEmpty(assemblyTypeName))
            {
                // N/A
                EditorGUI.LabelField(argpos, (tProperty.displayName + " Not Drawable"));
                return;
            }
            if (propertyID == VisualEditorUtility.STRING_TYPE_NAME || propertyID == VisualEditorUtility.CHAR_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                var tempString = tProperty.FindPropertyRelative("stringValue");
                if (!EditorApplication.isPlaying)
                    tempString.stringValue = EditorGUI.TextArea(argpos, tempString.stringValue);
                else tempString.stringValue = EditorGUI.DelayedTextField(argpos, GUIContent.none, tempString.stringValue);
                if (propertyID == VisualEditorUtility.CHAR_TYPE_NAME)
                {
                    string string_value = tempString.stringValue;
                    tempString.stringValue = string_value.Length >= 1 ? string_value[0].ToString() : String.Empty;
                }
            }
            else if (propertyID == VisualEditorUtility.BOOLEAN_TYPE_NAME)
            {

                argument_cache.hasCustomType = false;
                //argpos.x += 40;
               // argpos.width -= 40f;
                var tempX1 = tProperty.FindPropertyRelative("_x1");
                tempX1.floatValue = EditorGUI.ToggleLeft(argpos, GUIContent.none, tempX1.floatValue > 0) ? 1 : -1;
            }
            else if (propertyID == VisualEditorUtility.INTEGER_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                var tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempX1.floatValue = EditorGUI.IntField(argpos, GUIContent.none, (int)tempX1.floatValue);
            }
            else if (propertyID == VisualEditorUtility.LONG_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                SerializedProperty tempLong = tProperty.FindPropertyRelative("longValue");
                tempLong.longValue = EditorGUI.LongField(argpos, GUIContent.none, tempLong.longValue);
            }
            else if (propertyID == VisualEditorUtility.FLOAT_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                var tempX1 = tProperty.FindPropertyRelative("_x1");
                if (!EditorApplication.isPlaying)
                    tempX1.floatValue = EditorGUI.FloatField(argpos, GUIContent.none, tempX1.floatValue);
                else tempX1.floatValue = EditorGUI.DelayedFloatField(argpos, GUIContent.none, tempX1.floatValue);
            }
            else if (propertyID == VisualEditorUtility.DOUBLE_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                SerializedProperty tempDouble = tProperty.FindPropertyRelative("doubleValue");
                if (!EditorApplication.isPlaying)
                    tempDouble.doubleValue = EditorGUI.DoubleField(argpos, GUIContent.none, tempDouble.doubleValue);
                else tempDouble.doubleValue = EditorGUI.DelayedDoubleField(argpos, GUIContent.none, tempDouble.doubleValue);
            }
            else if (propertyID == VisualEditorUtility.VECTOR2_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                var tempX1 = tProperty.FindPropertyRelative("_x1");
                var tempY1 = tProperty.FindPropertyRelative("_y1");
                EditorGUI.BeginChangeCheck();
                Vector2 tempVector2 = EditorGUI.Vector2Field(argpos, GUIContent.none, new Vector2(tempX1.floatValue, tempY1.floatValue));
                if (EditorGUI.EndChangeCheck())
                {
                    tempX1.floatValue = tempVector2.x;
                    tempY1.floatValue = tempVector2.y;
                }
            }
            else if (propertyID == VisualEditorUtility.VECTOR3_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                var tempX1 = tProperty.FindPropertyRelative("_x1");
                var tempY1 = tProperty.FindPropertyRelative("_y1");
                var tempZ1 = tProperty.FindPropertyRelative("_z1");
                EditorGUI.BeginChangeCheck();
                Vector3 tempVector3 = EditorGUI.Vector3Field(argpos, GUIContent.none, new Vector3(tempX1.floatValue, tempY1.floatValue, tempZ1.floatValue));
                if (EditorGUI.EndChangeCheck())
                {
                    tempX1.floatValue = tempVector3.x;
                    tempY1.floatValue = tempVector3.y;
                    tempZ1.floatValue = tempVector3.z;
                }
            }
            else if (propertyID == VisualEditorUtility.VECTOR4_TYPE_NAME || propertyID == VisualEditorUtility.QUATERNION_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                var tempX1 = tProperty.FindPropertyRelative("_x1");
                var tempY1 = tProperty.FindPropertyRelative("_y1");
                var tempZ1 = tProperty.FindPropertyRelative("_z1");
                var tempX2 = tProperty.FindPropertyRelative("_x2");

                EditorGUI.BeginChangeCheck();
                Vector4 tempVector4 = EditorGUI.Vector4Field(argpos, GUIContent.none, new Vector4(tempX1.floatValue, tempY1.floatValue, tempZ1.floatValue, tempX2.floatValue));
                if (EditorGUI.EndChangeCheck())
                {
                    tempX1.floatValue = tempVector4.x;
                    tempY1.floatValue = tempVector4.y;
                    tempZ1.floatValue = tempVector4.z;
                    tempX2.floatValue = tempVector4.w;
                }
            }
            else if (propertyID == VisualEditorUtility.UNITYRECT_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                var tempX1 = tProperty.FindPropertyRelative("_x1");
                var tempY1 = tProperty.FindPropertyRelative("_y1");
                var tempZ1 = tProperty.FindPropertyRelative("_z1");
                var tempX2 = tProperty.FindPropertyRelative("_x2");
                argpos.x += 10;
                EditorGUI.BeginChangeCheck();
                Rect tempRect = EditorGUI.RectField(argpos, GUIContent.none, new Rect(tempX1.floatValue, tempY1.floatValue, tempZ1.floatValue, tempX2.floatValue));
                if (EditorGUI.EndChangeCheck())
                {
                    tempX1.floatValue = tempRect.position.x;
                    tempY1.floatValue = tempRect.position.y;
                    tempZ1.floatValue = tempRect.size.x;
                    tempX2.floatValue = tempRect.size.y;
                }
            }
            else if (propertyID == VisualEditorUtility.UNITYBOUNDS_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                var tempX1 = tProperty.FindPropertyRelative("_x1");
                var tempY1 = tProperty.FindPropertyRelative("_y1");
                var tempZ1 = tProperty.FindPropertyRelative("_z1");
                var tempX2 = tProperty.FindPropertyRelative("_x2");
                SerializedProperty tempY2 = tProperty.FindPropertyRelative("_y2");
                SerializedProperty tempZ2 = tProperty.FindPropertyRelative("_z2");
                argpos.x += 5;
                EditorGUI.BeginChangeCheck();
                Bounds tempBounds = EditorGUI.BoundsField(argpos, GUIContent.none, new Bounds(new Vector3(tempX1.floatValue, tempY1.floatValue, tempZ1.floatValue), new Vector3(tempX2.floatValue, tempY2.floatValue, tempZ2.floatValue)));
                if (EditorGUI.EndChangeCheck())
                {
                    tempX1.floatValue = tempBounds.center.x;
                    tempY1.floatValue = tempBounds.center.y;
                    tempZ1.floatValue = tempBounds.center.z;
                    tempX2.floatValue = tempBounds.size.x;
                    tempY2.floatValue = tempBounds.size.y;
                    tempZ2.floatValue = tempBounds.size.z;
                }
            }
            else if (propertyID == VisualEditorUtility.UNITYCOLOR_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                var tempX1 = tProperty.FindPropertyRelative(X1);
                var tempY1 = tProperty.FindPropertyRelative(Y1);
                var tempZ1 = tProperty.FindPropertyRelative(Z1);
                var tempX2 = tProperty.FindPropertyRelative(X2);

                EditorGUI.BeginChangeCheck();
                Color tempColor = EditorGUI.ColorField(argpos, GUIContent.none, new Color(tempX1.floatValue, tempY1.floatValue, tempZ1.floatValue, tempX2.floatValue));
                if (EditorGUI.EndChangeCheck())
                {
                    tempX1.floatValue = tempColor.r;
                    tempY1.floatValue = tempColor.g;
                    tempZ1.floatValue = tempColor.b;
                    tempX2.floatValue = tempColor.a;
                }
            }
            else if (propertyID == VisualEditorUtility.UNITYCURVE_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                SerializedProperty tempCurve = tProperty.FindPropertyRelative("animationCurveValue");
                tempCurve.animationCurveValue = EditorGUI.CurveField(argpos, GUIContent.none, tempCurve.animationCurveValue);
            }
            else if (propertyID == VisualEditorUtility.DOTNET_TYPE_NAME)
            {
                argument_cache.hasCustomType = false;
                EditorGUI.BeginChangeCheck();
                argument_cache.CurrentScript = EditorGUI.ObjectField(argpos, argument_cache.CurrentScript, typeof(MonoScript), false) as MonoScript;

                if (EditorGUI.EndChangeCheck())
                {
                    var mono_class = argument_cache.CurrentScript?.GetClass();
                    if (mono_class != null)
                        tProperty.FindPropertyRelative("stringValue").stringValue = mono_class.AssemblyQualifiedName;
                    else
                    {
                        argument_cache.CurrentScript = null;
                        tProperty.FindPropertyRelative("stringValue").stringValue = string.Empty;
                    }
                }
            }
            else
            {
                // Convert string to type
                Type current_type = Type.GetType(assemblyTypeName);
                if (current_type != null)
                {
                    // Enumerator
                    if (current_type.IsEnum)
                    {
                        argument_cache.hasCustomType = false;
                        var tempX1 = tProperty.FindPropertyRelative("_x1");

                        if (current_type.IsDefined(typeof(FlagsAttribute), false))
                        {
                            tempX1.floatValue = Convert.ToSingle(EditorGUI.EnumFlagsField(argpos, GUIContent.none, (Enum)Enum.ToObject(current_type, (int)tempX1.floatValue)));
                        }
                        else
                        {
                            VisualEditorUtility.StandardStyle.CalcMinMaxWidth(paramLabel, out float min, out float max);
                            //  argpos.width -= max;\
                            //argpos.width /= 2;
                            //argpos.xMax += max;
                            // argpos.x += argpos.width / 2;
                            //argpos.x += max;
                            argpos.xMin += max/2;
                            DrawSearchableEnum(argpos, current_type, tempX1);
                            //tempX1.floatValue = Convert.ToSingle(EditorGUI.EnumPopup(argpos, GUIContent.none, (Enum)Enum.ToObject(current_type, (int)tempX1.floatValue)));
                        }
                    }
                    // Unity object
                    else if (current_type.IsClass && (current_type == typeof(UnityEngine.Object) || current_type.IsSubclassOf(typeof(UnityEngine.Object))))
                    {
                        argument_cache.hasCustomType = false;
                        SerializedProperty tempObject = tProperty.FindPropertyRelative("objectValue");
                        SerializedProperty argumentvalidity = tProperty.FindPropertyRelative("m_objecthasvalue");
                        var argument_object = EditorGUI.ObjectField(argpos, GUIContent.none, tempObject.objectReferenceValue, current_type, true);
                        tempObject.objectReferenceValue = argument_object;
                        argumentvalidity.boolValue = argument_object != null;
                    }
                    else if (current_type.IsSubclassOf(typeof(Delegate)))
                    {
                        if (current_type.GenericTypeArguments.Length == 1)
                        {
                            tProperty.FindPropertyRelative("UseReference").boolValue = true;
                            tProperty.FindPropertyRelative("call_Reference").FindPropertyRelative("willdeseralize").boolValue = true;
                            tProperty.serializedObject.ApplyModifiedProperties();
                            ViewCache.getRawCallCacheFromRawArgument(tProperty).RequiresRecalculation = true;
                        }
                        else
                        {
                            EditorGUI.LabelField(argpos, (FullTypeName + " Not Drawable"));
                        }
                    }
                    //TODO:Re-implement this when seralized reference atti
                    // custom-user defined types
                    //else if (typeof(IVisualArgument).IsAssignableFrom(current_type))
                    //{
                    //    var customprop = tProperty.FindPropertyRelative("custom");
                    //    if (argument_cache.CurrentCustomType != current_type)
                    //    {
                    //        Debug.Log("catch up");
                    //        argument_cache.hasCustomType = true;
                    //        ViewCache.getRawCallCacheFromRawArgument(tProperty).RequiresRecalculation = true;
                    //        argument_cache.CurrentCustomType = current_type;
                    //        customprop.managedReferenceValue = Activator.CreateInstance(current_type);
                    //    } 
                    //    customprop.isExpanded = true;
                    //    argpos.y -= 50f;
                    //    EditorGUI.PropertyField(argpos, customprop,GUIContent.none,true);
                    //}
                    else
                    {
                        argument_cache.hasCustomType = false;
                        EditorGUI.LabelField(argpos, (FullTypeName + " Not Drawable"));
                    }
                }
                // N/A
                else EditorGUI.LabelField(argpos, (FullTypeName + " Not Drawable"));
            }
        }
        // TODO: 
        //  tProperty.FindPropertyRelative("hasCustomType").boolValue = argument_cache.hasCustomType;
        private void DisplayReference(Rect rect, SerializedProperty argumentprop, GUIContent label, GUIContent labeltwo, RawArgumentView argCache)
        {
            var call = argumentprop.FindPropertyRelative("call_Reference");
            argCache.hasCustomType = false;
            VisualEditorUtility.StandardStyle.CalcMinMaxWidth(label, out float min, out float max);
            VisualEditorUtility.StandardStyle.CalcMinMaxWidth(labeltwo, out float min_two, out float max_twp);
            rect.x += 10f;
            EditorGUI.PropertyField(rect, call, GUIContent.none, true);
        }
        private void DrawSearchableEnum(Rect enumrect, Type enumtype, SerializedProperty enumpropertyvalue)
        {
            int idHash = (enumpropertyvalue.propertyPath.GetHashCode() + enumpropertyvalue.serializedObject.targetObject.GetInstanceID().ToString()).GetHashCode();
            var enum_names = Enum.GetNames(enumtype);
            if ((int)enumpropertyvalue.floatValue >= enum_names.Length || (int)enumpropertyvalue.floatValue < 0)
            {
                enumpropertyvalue.floatValue = Convert.ToSingle(enum_names.Length - 1);
                enumpropertyvalue.serializedObject.ApplyModifiedProperties();
            }
            GUIContent button = new GUIContent();
            button.text = Enum.GetName(enumtype, (int)enumpropertyvalue.floatValue);
            if (DropdownButton(idHash, enumrect, button))
            {
                ;
                Action<int> onselected = index =>
                 {
                     enumpropertyvalue.floatValue = Convert.ToSingle(Enum.Parse(enumtype, enum_names[index]));
                     enumpropertyvalue.serializedObject.ApplyModifiedProperties();
                     if (EditorApplication.isPlaying)
                         VisualEditorUtility.ReinitializeDelegate(enumpropertyvalue.GetVisualDelegateObject());
                 };
                SearchablePopup.Show(enumrect, enum_names, Array.FindIndex(enum_names, name => name == button.text), onselected);
            }
        }
        private static bool DropdownButton(int id, Rect position, GUIContent content)
        {
            Event current = Event.current;
            switch (current.type)
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && current.button == 0)
                    {
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == id && current.character == '\n')
                    {
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.Repaint:
                    EditorStyles.popup.Draw(position, content, id);
                    break;
            }
            return false;
        }
    }
}
