using UnityEditor;
using UnityEngine;
using System;
namespace VisualDelegates.Editor
{
    [CustomPropertyDrawer(typeof(RawReference), true)]
    class DrawerRawReferenceView : DrawerRawDelegateView<RawReferenceView>
    {
        public override float GetPropertyHeight(SerializedProperty tProperty, GUIContent tLabel)
        {
            return base.GetPropertyHeight(tProperty, tLabel);
        }
        public override void OnGUI(Rect tPosition, SerializedProperty tProperty, GUIContent tLabel)
        {
            if (ViewCache.GetDelegateView(tProperty, out RawReferenceView delegateview))
            {
                if (delegateview.serializationError)
                {
                }

                var rawcallview = ViewCache.GetRawCallCacheFromRawReference(tProperty);
                int argument_index = tProperty.GetRawArgumentIndexFromArgumentReference();
                var argument_type = rawcallview.arguments[argument_index].type;
                bool isdelegate = false;
                if (argument_type.IsSubclassOf(typeof(Delegate)))
                {
                    if (argument_type.GenericTypeArguments.Length == 1)
                    {
                        isdelegate = true;
                        argument_type = argument_type.GenericTypeArguments[0];
                    }
                }

                //if (delgateview.CurrentTarget == null && tProperty.FindPropertyRelative("m_target").objectReferenceValue != null)
                //{
                //    validate(tProperty, delgateview);
                //}

                if (argument_type != delegateview.reference_type)
                {
                    
                    // tProperty.FindPropertyRelative("methodData").ClearArray();
                    delegateview.SetNewReferenceType(argument_type);
                    tProperty.FindPropertyRelative("m_isDelegate").boolValue = isdelegate;
                    tProperty.FindPropertyRelative("m_isvaluetype").boolValue = delegateview.SelectedMember?.isvaluetype ?? false;
                    tProperty.FindPropertyRelative("isparentargstring").boolValue = argument_type == typeof(string);
                    handleMemberUpdate(tProperty, delegateview);
                }


                EditorGUI.BeginChangeCheck();
                base.OnGUI(tPosition, tProperty, tLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    // Debug.Log("local type change");
                    tProperty.FindPropertyRelative("m_isvaluetype").boolValue = delegateview.SelectedMember?.isvaluetype ?? false;
                    tProperty.FindPropertyRelative("isparentargstring").boolValue = argument_type == typeof(string);
                    handleMemberUpdate(tProperty, delegateview);
                }
                if (delegateview.CurrentTarget == null)
                {
                    tProperty.FindPropertyRelative("methodData").ClearArray();
                    tProperty.FindPropertyRelative("m_target").objectReferenceValue = null;
                }
            }
        }
        protected override void handleMemberUpdate(SerializedProperty tProperty, RawReferenceView tCache)
        {
            if (tCache.CurrentTarget != null && tCache.SelectedMember != null)
                base.handleMemberUpdate(tProperty, tCache);
        }
        protected override void handleTargetUpdate(SerializedProperty tProperty, RawReferenceView tCache)
        {
            if (tCache.CurrentTarget != null && tCache.SelectedMember != null)
                base.handleTargetUpdate(tProperty, tCache);
        }
    }
}
