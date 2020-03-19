using UnityEditor;
using UnityEngine;
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
            if(ViewCache.GetDelegateView(tProperty,out RawReferenceView delgateview))
            {
                EditorGUI.BeginChangeCheck();
                base.OnGUI(tPosition, tProperty, tLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    Debug.Log("local type change");
                    if(delgateview.SelectedMember!=null)
                    tProperty.FindPropertyRelative("m_isvaluetype").boolValue = delgateview.SelectedMember.isvaluetype;
                }
               var rawcallview= ViewCache.GetRawCallCacheFromRawReference(tProperty);
               int argument_index= tProperty.GetRawArgumentIndexFromArgumentReference();
                var argument_type = rawcallview.arguments[argument_index].type;
                if (argument_type != delgateview.reference_type)
                {
                    Debug.LogWarning("argument type change");
                    tProperty.FindPropertyRelative("methodData").ClearArray();
                    tProperty.FindPropertyRelative("isparentargstring").boolValue = argument_type == typeof(string);
                    tProperty.serializedObject.ApplyModifiedProperties();
                    delgateview.SetNewReferenceType(argument_type);
                }
            }
        }

    }
}
