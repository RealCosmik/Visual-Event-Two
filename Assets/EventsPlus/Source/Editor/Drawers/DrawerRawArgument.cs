using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
namespace EventsPlus
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Inspector class for rendering <see cref="RawArgument"/>s in the inspector</summary>
    [CustomPropertyDrawer(typeof(RawArgument), true)]
    public class DrawerRawArgument : PropertyDrawer
    {
        const string DotNetType = "System.Type";
        //=======================
        // Render
        //=======================
        /// <summary>Calculates the inspector height</summary>
        /// <param name="tProperty">Serialized <see cref="RawArgument"/> property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        /// <returns>Height of the drawer</returns>
        public override float GetPropertyHeight(SerializedProperty tProperty, GUIContent tLabel)
        {
            float tempHeight = base.GetPropertyHeight(tProperty, tLabel);
            string argumentName = tProperty.FindPropertyRelative("FullArgumentName").stringValue;
            var usingref = tProperty.FindPropertyRelative("UseReference").boolValue;
            if (argumentName == "UnityEngine.Rect")
                tempHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            else if (argumentName == "UnityEngine.Bounds")
                tempHeight += (EditorGUIUtility.singleLineHeight) + EditorGUIUtility.standardVerticalSpacing;
            if (usingref)
            {
                tempHeight += EditorGUI.GetPropertyHeight(tProperty.FindPropertyRelative("call_Reference"));
            }
            ValidateArgumentCache(tProperty,argumentName);
            return tempHeight;
        }
        private void ValidateArgumentCache(SerializedProperty argumentprop, string argumentTypename)
        {
            var argumentCache = ViewCache.GetRawArgumentCache(argumentprop);
            var arg_type = Type.GetType(argumentprop.FindPropertyRelative("stringValue").stringValue);
            if (arg_type!=null&&argumentTypename == DotNetType&&argumentCache.CurrentScript==null)
            { 
                argumentCache.CurrentScript= Resources.FindObjectsOfTypeAll<MonoScript>().First(m => m.GetClass() == arg_type);
            }
        }
        /// <summary>Renders the appropriate input field of the <see cref="RawArgument"/> property</summary>
        /// <param name="tPosition">Inspector position and size of <paramref name="tProperty"/></param>
        /// <param name="tProperty">Serialized <see cref="RawArgument"/> property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        public override void OnGUI(UnityEngine.Rect tPosition, SerializedProperty tProperty, GUIContent tLabel)
        {
            var refpos = tPosition;
            refpos.x = refpos.width - 50-EditorGUI.indentLevel;
            var style = VisualEdiotrUtility.StandardStyle;
            var reference_content = new GUIContent("useref");
            style.CalcMinMaxWidth(reference_content, out float min, out float max);
            EditorGUI.LabelField(refpos, reference_content);
            SerializedProperty useReference = tProperty.FindPropertyRelative("UseReference");
            var togglerect = refpos;
            togglerect.x += max+10;
            EditorGUI.BeginChangeCheck();
            useReference.boolValue= EditorGUI.Toggle(togglerect, useReference.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                var call_cache= ViewCache.GetRawCallCacheFromRawReference(tProperty.FindPropertyRelative("call_Reference"));
                call_cache.RequiresRecalculation = true;
            }
            var argumentpos = tPosition;
            argumentpos.width -= max;
            if (!useReference.boolValue)
                DisplayArgument(argumentpos, tProperty, tLabel);
            else
                DisplayReference(argumentpos, tProperty, tLabel);
            if (tProperty.serializedObject.hasModifiedProperties)
                tProperty.serializedObject.ApplyModifiedProperties();
        }
        
        private void DisplayArgument(Rect rect, SerializedProperty tProperty,GUIContent paramLabel)
        {
            RawArgumentView argument_cache = ViewCache.GetRawArgumentCache(tProperty);
            var style = VisualEdiotrUtility.StandardStyle;
            style.CalcMinMaxWidth(paramLabel, out float minoffset, out float maxoffset);
            var labelpos = rect;
            labelpos.x -= 50;
            var argpos = rect;
            argpos.width -= maxoffset;
            argpos.x = labelpos.x + maxoffset;
            EditorGUI.LabelField(labelpos, paramLabel);
            string assemblyTypeName = tProperty.FindPropertyRelative("assemblyQualifiedArgumentName").stringValue;
            string FullTypeName = tProperty.FindPropertyRelative("FullArgumentName").stringValue;
            switch (FullTypeName)
            {
                case "System.String":
                    argpos.x += 10;
                    SerializedProperty tempString = tProperty.FindPropertyRelative("stringValue");
                    tempString.stringValue = EditorGUI.TextArea(argpos, tempString.stringValue);
                    EditorGUILayout.EndScrollView();
                    break;
                case "System.Boolean":
                    argpos.x += 50;
                    SerializedProperty tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempX1.floatValue = EditorGUI.ToggleLeft(argpos, GUIContent.none, tempX1.floatValue > 0) ? 1 : -1;
                    break;
                case "System.Int32":
                    argpos.x += 10;
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempX1.floatValue = EditorGUI.DelayedIntField(argpos, GUIContent.none, (int)tempX1.floatValue);
                    break;
                case "System.Int64":
                    argpos.x += 10;
                    SerializedProperty tempLong = tProperty.FindPropertyRelative("longValue");
                    tempLong.longValue = EditorGUI.LongField(argpos, GUIContent.none, tempLong.longValue);
                    break;
                case "System.Single":
                    argpos.x += 10;
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempX1.floatValue = EditorGUI.DelayedFloatField(argpos, GUIContent.none, tempX1.floatValue);
                    break;
                case "System.Double":
                    argpos.x += 10;
                    SerializedProperty tempDouble = tProperty.FindPropertyRelative("doubleValue");
                    tempDouble.doubleValue = EditorGUI.DelayedDoubleField(argpos, GUIContent.none, tempDouble.doubleValue);
                    break;
                case "UnityEngine.Vector2":
                    argpos.x += 10;
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    SerializedProperty tempY1 = tProperty.FindPropertyRelative("_y1");
                    EditorGUI.BeginChangeCheck();
                    Vector2 tempVector2 = EditorGUI.Vector2Field(argpos, GUIContent.none, new Vector2(tempX1.floatValue, tempY1.floatValue));
                    if (EditorGUI.EndChangeCheck())
                    {
                        tempX1.floatValue = tempVector2.x;
                        tempY1.floatValue = tempVector2.y;
                    }
                    break;
                case "UnityEngine.Vector3":
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempY1 = tProperty.FindPropertyRelative("_y1");
                    SerializedProperty tempZ1 = tProperty.FindPropertyRelative("_z1");
                    EditorGUI.BeginChangeCheck();
                    argpos.x += 10;
                    Vector3 tempVector3 = EditorGUI.Vector3Field(argpos, GUIContent.none, new Vector3(tempX1.floatValue, tempY1.floatValue, tempZ1.floatValue));
                    if (EditorGUI.EndChangeCheck())
                    {
                        tempX1.floatValue = tempVector3.x;
                        tempY1.floatValue = tempVector3.y;
                        tempZ1.floatValue = tempVector3.z;
                    }
                    break;
                case "UnityEngine.Vector4":
                case "UnityEngine.Quaternion":
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempY1 = tProperty.FindPropertyRelative("_y1");
                    tempZ1 = tProperty.FindPropertyRelative("_z1");
                    SerializedProperty tempX2 = tProperty.FindPropertyRelative("_x2");

                    EditorGUI.BeginChangeCheck();
                    argpos.x += 10;
                    Vector4 tempVector4 = EditorGUI.Vector4Field(argpos,GUIContent.none, new Vector4(tempX1.floatValue, tempY1.floatValue, tempZ1.floatValue, tempX2.floatValue));
                    if (EditorGUI.EndChangeCheck())
                    {
                        tempX1.floatValue = tempVector4.x;
                        tempY1.floatValue = tempVector4.y;
                        tempZ1.floatValue = tempVector4.z;
                        tempX2.floatValue = tempVector4.w;
                    }
                    break;
                case "UnityEngine.Rect":
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempY1 = tProperty.FindPropertyRelative("_y1");
                    tempZ1 = tProperty.FindPropertyRelative("_z1");
                    tempX2 = tProperty.FindPropertyRelative("_x2");
                    argpos.x += 20;
                    EditorGUI.BeginChangeCheck();
                    Rect tempRect = EditorGUI.RectField(argpos, GUIContent.none, new Rect(tempX1.floatValue, tempY1.floatValue, tempZ1.floatValue, tempX2.floatValue));
                    if (EditorGUI.EndChangeCheck())
                    {
                        tempX1.floatValue = tempRect.position.x;
                        tempY1.floatValue = tempRect.position.y;
                        tempZ1.floatValue = tempRect.size.x;
                        tempX2.floatValue = tempRect.size.y;
                    }
                    break;
                case "UnityEngine.Bounds":
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempY1 = tProperty.FindPropertyRelative("_y1");
                    tempZ1 = tProperty.FindPropertyRelative("_z1");
                    tempX2 = tProperty.FindPropertyRelative("_x2");
                    SerializedProperty tempY2 = tProperty.FindPropertyRelative("_y2");
                    SerializedProperty tempZ2 = tProperty.FindPropertyRelative("_z2");
                    argpos.x += 15;
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
                    break;
                case "UnityEngine.Color":
                    argpos.x += 10;
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempY1 = tProperty.FindPropertyRelative("_y1");
                    tempZ1 = tProperty.FindPropertyRelative("_z1");
                    tempX2 = tProperty.FindPropertyRelative("_x2");

                    EditorGUI.BeginChangeCheck();
                    Color tempColor = EditorGUI.ColorField(argpos, GUIContent.none, new Color(tempX1.floatValue, tempY1.floatValue, tempZ1.floatValue, tempX2.floatValue));
                    if (EditorGUI.EndChangeCheck())
                    {
                        tempX1.floatValue = tempColor.r;
                        tempY1.floatValue = tempColor.g;
                        tempZ1.floatValue = tempColor.b;
                        tempX2.floatValue = tempColor.a;
                    }
                    break;
                case "UnityEngine.AnimationCurve":
                    argpos.x += 10;
                    SerializedProperty tempCurve = tProperty.FindPropertyRelative("animationCurveValue");
                    tempCurve.animationCurveValue = EditorGUI.CurveField(argpos, GUIContent.none, tempCurve.animationCurveValue);
                    break;
                case "System.Type":
                    argpos.x += 10;
                    EditorGUI.BeginChangeCheck();
                    argument_cache.CurrentScript = EditorGUI.ObjectField(argpos, argument_cache.CurrentScript, typeof(MonoScript), false) as MonoScript;

                    if (EditorGUI.EndChangeCheck())
                    {
                        var mono_class = argument_cache.CurrentScript?.GetClass();
                        if(mono_class!=null)
                        tProperty.FindPropertyRelative("stringValue").stringValue = mono_class.AssemblyQualifiedName;
                        else
                        {
                            argument_cache.CurrentScript = null;
                            tProperty.FindPropertyRelative("stringValue").stringValue = string.Empty;
                        }
                    }
                    break;
                default:
                    if (String.IsNullOrEmpty(assemblyTypeName))
                    {
                        // N/A
                        EditorGUI.LabelField(argpos, (tProperty.displayName + " Not Drawable"));
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
                                tempX1 = tProperty.FindPropertyRelative("_x1");

                                if (current_type.IsDefined(typeof(FlagsAttribute), false))
                                {
                                    tempX1.floatValue = Convert.ToSingle(EditorGUI.EnumFlagsField(argpos, GUIContent.none, (Enum)Enum.ToObject(current_type, (int)tempX1.floatValue)));
                                }
                                else
                                {
                                    tempX1.floatValue = Convert.ToSingle(EditorGUI.EnumPopup(argpos, GUIContent.none, (Enum)Enum.ToObject(current_type, (int)tempX1.floatValue)));
                                }

                                return;
                            }
                            // Unity object
                            else if (current_type.IsClass && (current_type == typeof(UnityEngine.Object) || current_type.IsSubclassOf(typeof(UnityEngine.Object))))
                            {
                                SerializedProperty tempObject = tProperty.FindPropertyRelative("objectValue");
                                argpos.x += 10;
                                tempObject.objectReferenceValue = EditorGUI.ObjectField(argpos, GUIContent.none, tempObject.objectReferenceValue, current_type, true);
                                return;
                            }
                        }
                        // N/A
                        EditorGUI.LabelField(argpos, (FullTypeName + " Not Drawable"));
                    }
                    break;
            }
        }
        private void DisplayReference(Rect rect,SerializedProperty argumentprop,GUIContent label)
        {
            RawArgumentView argument_cache = ViewCache.GetRawArgumentCache(argumentprop);
            //styling
            var call = argumentprop.FindPropertyRelative("call_Reference");
            var style = VisualEdiotrUtility.StandardStyle;
            var labelrect = rect;
            labelrect.x-= 50;
            style.CalcMinMaxWidth(label, out float min, out float max);
            EditorGUI.LabelField(labelrect, label);
            var refrect = labelrect;
            refrect.x += max+30;
            refrect.width -= (max+10);
            //data 
            var reference_cache = argument_cache.argumentReference;
            var rawcallview = ViewCache.GetRawCallCacheFromRawReference(call);
            int argumentindex = argumentprop.GetRawArgumentIndex();
            if (reference_cache.reference_type != rawcallview.arguments[argumentindex].type)
            {
                Debug.Log("change");
                var ParentArgumentType = rawcallview.arguments[argumentindex].type;
                reference_cache.SetNewReferenceType(ParentArgumentType);
                call.FindPropertyRelative("methodData").ClearArray();
                call.FindPropertyRelative("m_isvaluetype").boolValue = ParentArgumentType.IsValueType;
                call.FindPropertyRelative("isparentargstring").boolValue = ParentArgumentType == typeof(string);
                call.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(refrect, call, true);
            if (EditorGUI.EndChangeCheck())
            {
            }
        }
    }
}
