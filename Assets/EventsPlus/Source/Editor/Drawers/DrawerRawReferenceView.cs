﻿using UnityEditor;
using UnityEngine;
using System;
namespace VisualEvent
{
    [CustomPropertyDrawer(typeof(RawReference),true)]
    class DrawerRawReferenceView : DrawerRawDelegateView<RawReferenceView>
    {
        public override float GetPropertyHeight(SerializedProperty tProperty, GUIContent tLabel)
        {
            return base.GetPropertyHeight(tProperty, tLabel);
        }
        public override void OnGUI(Rect tPosition, SerializedProperty tProperty, GUIContent tLabel)
        { 
            if (ViewCache.GetDelegateView(tProperty,out RawReferenceView delgateview))
            {
                var rawcallview = ViewCache.GetRawCallCacheFromRawReference(tProperty);
                int argument_index = tProperty.GetRawArgumentIndexFromArgumentReference();
                var argument_type = rawcallview.arguments[argument_index].type;
                EditorGUI.BeginChangeCheck();
                if (EditorGUI.EndChangeCheck())
                {
                    Debug.Log("local type change");
                    if(delgateview.SelectedMember!=null)
                    tProperty.FindPropertyRelative("m_isvaluetype").boolValue = delgateview.SelectedMember.isvaluetype;
                    tProperty.FindPropertyRelative("isparentargstring").boolValue = argument_type == typeof(string);
                }
                bool isdelegate = false;
                if (argument_type.IsSubclassOf(typeof(Delegate)))
                {
                    isdelegate = true;
                    argument_type = argument_type.GenericTypeArguments[0];
                }

                if (argument_type != delgateview.reference_type)
                {
                   // tProperty.FindPropertyRelative("methodData").ClearArray();
                    tProperty.serializedObject.ApplyModifiedProperties();
                    delgateview.SetNewReferenceType(argument_type);
                    tProperty.FindPropertyRelative("m_isDelegate").boolValue = isdelegate;
                }
            }
            base.OnGUI(tPosition, tProperty, tLabel);
        }

    }
}
