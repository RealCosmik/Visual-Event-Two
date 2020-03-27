using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
namespace VisualEvent
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
            var argumentCache = ViewCache.GetRawArgumentCache(tProperty);
            if (argumentCache.hasCustomType)
                tempHeight += EditorGUI.GetPropertyHeight(tProperty.FindPropertyRelative("custom"));
            ValidateArgumentCache(tProperty, argumentName, argumentCache);
            return tempHeight;
        }
        private void ValidateArgumentCache(SerializedProperty argumentprop, string argumentTypename, RawArgumentView argumentCache)
        {
            var arg_type = Type.GetType(argumentprop.FindPropertyRelative("stringValue").stringValue);
            if (arg_type != null && argumentTypename == DotNetType && argumentCache.CurrentScript == null)
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
            var argument_cache = ViewCache.GetRawArgumentCache(tProperty);
            var refpos = tPosition;
            var style = VisualEdiotrUtility.StandardStyle;
            var reference_content = new GUIContent("Use Reference");
            style.CalcMinMaxWidth(reference_content, out float min, out float max);
            refpos.x = (refpos.width - max) - 20;
            EditorGUI.LabelField(refpos, reference_content);
            SerializedProperty useReference = tProperty.FindPropertyRelative("UseReference");
            var togglerect = refpos;
            togglerect.x += max + 10;

            EditorGUI.BeginChangeCheck();
            useReference.boolValue = EditorGUI.Toggle(togglerect, useReference.boolValue);
            if (EditorGUI.EndChangeCheck())
                ViewCache.getRawCallCacheFromRawArgument(tProperty).RequiresRecalculation = true;

            style.CalcMinMaxWidth(paramLabel, out float minoffset, out float maxoffset);
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
                DisplayReference(argumentpos, tProperty, paramLabel, argument_cache);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                tProperty.serializedObject.ApplyModifiedProperties();
                tProperty.GetVisualDelegateObject()?.ReInitialize();
            }
            //if (!argument_cache.hasCustomType&&argument_cache.CurrentCustomType!=null)
            //{
            //    argument_cache.hasCustomType = false;
            //    argument_cache.CurrentCustomType = null;
            //    tProperty.FindPropertyRelative("custom").managedReferenceValue = null;
            //    ViewCache.getRawCallCacheFromRawArgument(tProperty).RequiresRecalculation = true;
            //}
        }

        private void DisplayArgument(Rect argpos, SerializedProperty tProperty, GUIContent paramLabel)
        {
            RawArgumentView argument_cache = ViewCache.GetRawArgumentCache(tProperty);
            string assemblyTypeName = tProperty.FindPropertyRelative("assemblyQualifiedArgumentName").stringValue;
            string FullTypeName = tProperty.FindPropertyRelative("FullArgumentName").stringValue;
            argpos.x += 10;
            switch (FullTypeName)
            {
                case "System.String":
                    argument_cache.hasCustomType = false;
                    SerializedProperty tempString = tProperty.FindPropertyRelative("stringValue");
                    if (!EditorApplication.isPlaying)
                        tempString.stringValue = EditorGUI.TextField(argpos, tempString.stringValue);
                    else tempString.stringValue = EditorGUI.DelayedTextField(argpos, tempString.stringValue);
                    break;
                case "System.Boolean":
                    argument_cache.hasCustomType = false;
                    argpos.x += 40;
                    SerializedProperty tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempX1.floatValue = EditorGUI.ToggleLeft(argpos, GUIContent.none, tempX1.floatValue > 0) ? 1 : -1;
                    break;
                case "System.Int32":
                    argument_cache.hasCustomType = false;
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    if (!EditorApplication.isPlaying)
                        tempX1.floatValue = EditorGUI.IntField(argpos, GUIContent.none, (int)tempX1.floatValue);
                    else tempX1.floatValue = EditorGUI.DelayedIntField(argpos, GUIContent.none, (int)tempX1.floatValue);
                    break;
                case "System.Int64":
                    argument_cache.hasCustomType = false;
                    SerializedProperty tempLong = tProperty.FindPropertyRelative("longValue");
                    tempLong.longValue = EditorGUI.LongField(argpos, GUIContent.none, tempLong.longValue);
                    break;
                case "System.Single":
                    argument_cache.hasCustomType = false;
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    if (!EditorApplication.isPlaying)
                        tempX1.floatValue = EditorGUI.FloatField(argpos, GUIContent.none, tempX1.floatValue);
                    else tempX1.floatValue = EditorGUI.DelayedFloatField(argpos, GUIContent.none, tempX1.floatValue);
                    break;
                case "System.Double":
                    argument_cache.hasCustomType = false;
                    SerializedProperty tempDouble = tProperty.FindPropertyRelative("doubleValue");
                    if (!EditorApplication.isPlaying)
                        tempDouble.doubleValue = EditorGUI.DoubleField(argpos, GUIContent.none, tempDouble.doubleValue);
                    else tempDouble.doubleValue = EditorGUI.DelayedDoubleField(argpos, GUIContent.none, tempDouble.doubleValue);
                    break;
                case "UnityEngine.Vector2":
                    argument_cache.hasCustomType = false;
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
                    argument_cache.hasCustomType = false;
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempY1 = tProperty.FindPropertyRelative("_y1");
                    SerializedProperty tempZ1 = tProperty.FindPropertyRelative("_z1");
                    EditorGUI.BeginChangeCheck();
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
                    argument_cache.hasCustomType = false;
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempY1 = tProperty.FindPropertyRelative("_y1");
                    tempZ1 = tProperty.FindPropertyRelative("_z1");
                    SerializedProperty tempX2 = tProperty.FindPropertyRelative("_x2");

                    EditorGUI.BeginChangeCheck();
                    Vector4 tempVector4 = EditorGUI.Vector4Field(argpos, GUIContent.none, new Vector4(tempX1.floatValue, tempY1.floatValue, tempZ1.floatValue, tempX2.floatValue));
                    if (EditorGUI.EndChangeCheck())
                    {
                        tempX1.floatValue = tempVector4.x;
                        tempY1.floatValue = tempVector4.y;
                        tempZ1.floatValue = tempVector4.z;
                        tempX2.floatValue = tempVector4.w;
                    }
                    break;
                case "UnityEngine.Rect":
                    argument_cache.hasCustomType = false;
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempY1 = tProperty.FindPropertyRelative("_y1");
                    tempZ1 = tProperty.FindPropertyRelative("_z1");
                    tempX2 = tProperty.FindPropertyRelative("_x2");
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
                    break;
                case "UnityEngine.Bounds":
                    argument_cache.hasCustomType = false;
                    tempX1 = tProperty.FindPropertyRelative("_x1");
                    tempY1 = tProperty.FindPropertyRelative("_y1");
                    tempZ1 = tProperty.FindPropertyRelative("_z1");
                    tempX2 = tProperty.FindPropertyRelative("_x2");
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
                    break;
                case "UnityEngine.Color":
                    argument_cache.hasCustomType = false;
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
                    argument_cache.hasCustomType = false;
                    SerializedProperty tempCurve = tProperty.FindPropertyRelative("animationCurveValue");
                    tempCurve.animationCurveValue = EditorGUI.CurveField(argpos, GUIContent.none, tempCurve.animationCurveValue);
                    break;
                case "System.Type":
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
                                argument_cache.hasCustomType = false;
                                tempX1 = tProperty.FindPropertyRelative("_x1");

                                if (current_type.IsDefined(typeof(FlagsAttribute), false))
                                {
                                    tempX1.floatValue = Convert.ToSingle(EditorGUI.EnumFlagsField(argpos, GUIContent.none, (Enum)Enum.ToObject(current_type, (int)tempX1.floatValue)));
                                }
                                else
                                {
                                    tempX1.floatValue = Convert.ToSingle(EditorGUI.EnumPopup(argpos, GUIContent.none, (Enum)Enum.ToObject(current_type, (int)tempX1.floatValue)));
                                }
                            }
                            // Unity object
                            else if (current_type.IsClass && (current_type == typeof(UnityEngine.Object) || current_type.IsSubclassOf(typeof(UnityEngine.Object))))
                            {
                                argument_cache.hasCustomType = false;
                                SerializedProperty tempObject = tProperty.FindPropertyRelative("objectValue");
                                tempObject.objectReferenceValue = EditorGUI.ObjectField(argpos, GUIContent.none, tempObject.objectReferenceValue, current_type, true);
                            }
                            else if (current_type.IsSubclassOf(typeof(Delegate)))
                            {
                                tProperty.FindPropertyRelative("UseReference").boolValue = true;
                                tProperty.serializedObject.ApplyModifiedProperties();
                                ViewCache.getRawCallCacheFromRawArgument(tProperty).RequiresRecalculation = true;
                            }
                            //TODO:Re-implement this when seralized reference atti
                            // custom-user defined types
                            //else if (typeof(IVisualArgument).IsAssignableFrom(current_type))
                            //{
                            //    var customprop = tProperty.FindPropertyRelative("custom");
                            //    if (argument_cache.CurrentCustomType != current_type)
                            //    {
                            //        argument_cache.hasCustomType = true;
                            //        ViewCache.getRawCallCacheFromRawArgument(tProperty).RequiresRecalculation = true;
                            //        argument_cache.CurrentCustomType = current_type;
                            //        customprop.managedReferenceValue = Activator.CreateInstance(current_type);
                            //    }
                            //    customprop.isExpanded = true;
                            //    EditorGUI.PropertyField(argpos, customprop, GUIContent.none, true);
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
                    break;
            }
            // TODO: 
            //  tProperty.FindPropertyRelative("hasCustomType").boolValue = argument_cache.hasCustomType;
        }
        private void DisplayReference(Rect rect, SerializedProperty argumentprop, GUIContent label, RawArgumentView argCache)
        {
            var call = argumentprop.FindPropertyRelative("call_Reference");
            argCache.hasCustomType = false;
            rect.y += 5;
            rect.x += 10;
            EditorGUI.PropertyField(rect, call, true);
        }
    }
}
