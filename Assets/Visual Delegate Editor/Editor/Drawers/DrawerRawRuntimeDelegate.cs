using UnityEditor;
using UnityEngine;
namespace VisualDelegates.Editor
{
    [CustomPropertyDrawer(typeof(RawRuntimeCall))]
    class DrawerRawRuntimeDelegate : DrawerRawDelegateView<RawDynamicDelegateView>
    {
        private void AddTarget(SerializedProperty delegateprop,RawDynamicDelegateView cache,object target)
        {
            var target_type = target.GetType();
            if (target is UnityEngine.Object unity_object)
            {
                delegateprop.FindPropertyRelative("isUnityTarget").boolValue = true;
                delegateprop.FindPropertyRelative("m_target").objectReferenceValue = unity_object;
            }
            else
            {
                delegateprop.FindPropertyRelative("isUnityTarget").boolValue = false;
                delegateprop.FindPropertyRelative("TargetType").stringValue = target_type.AssemblyQualifiedName;
            }
            cache.TargetName = VisualEditorUtility.ParseDynamicTargetName(target_type.FullName);
        }
       
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorApplication.isPlaying&& ViewCache.GetDelegateView(property, out RawDynamicDelegateView cache))
            { 
                var methodData_prop = property.FindPropertyRelative("methodData");
                if (methodData_prop.arraySize == 0)
                {
                    var visualdelegate = property.GetVisualDelegateObject();
                    int index = property.GetRawCallIndex();
                    var dynamicdelgate = visualdelegate.Calls[index];
                    AddTarget(property, cache, dynamicdelgate.delegateInstance.Target);
                    var seralizedMethodData = Utility.QuickSeralizer(dynamicdelgate.delegateInstance.Method);
                    VisualEditorUtility.CopySeralizedMethodDataToProp(methodData_prop, seralizedMethodData);
                    cache.MethodName = VisualEditorUtility.ParseDynamicMethodName(seralizedMethodData[1]);
                    cache.CalcHeight();
                }
                Rect targetRect = position;
                targetRect.height *= .5f;
                Rect methodRect = targetRect;
                methodRect.y += methodRect.height;
                EditorGUI.LabelField(targetRect, cache.TargetContent);
                EditorGUI.LabelField(methodRect, cache.MethodContent);
            }
        }
    }
}
