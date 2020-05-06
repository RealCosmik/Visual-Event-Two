using UnityEditor;
using UnityEngine;
namespace VisualEvent.Editor
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
            cache.TargetName = ParseTargetName(target_type.FullName);
        }
        private string ParseMethodName(string methodname)
        {
            if (methodname[0] == '<')//anon method
            {
                return $@"Method: Anonymous method created in ""{methodname.Substring(1, methodname.IndexOf('>') - 1)}""";
            }
            else return $@"Method: ""{methodname}""";
        }
        private string ParseTargetName(string TargetType)
        {
            if (TargetType.Contains("+"))
            {
                return $@"Target: Anonymous Type Created in ""{TargetType.Substring(0, TargetType.IndexOf('+'))}""";
            }
            return $@"Target: ""{TargetType}""";
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorApplication.isPlaying&& ViewCache.GetDelegateView(property, out RawDynamicDelegateView cache))
            { 
                var methodData_prop = property.FindPropertyRelative("methodData");
                if (methodData_prop.arraySize == 0)
                {
                    var dynamicdelgate = property.GetTarget<RawRuntimeCall>();
                    AddTarget(property, cache, dynamicdelgate.delegateInstance.Target);
                    var seralizedMethodData = Utility.QuickSeralizer(dynamicdelgate.delegateInstance.Method);
                    VisualEdiotrUtility.CopySeralizedMethodDataToProp(methodData_prop, seralizedMethodData);
                    cache.MethodName = ParseMethodName(seralizedMethodData[1]);
                    cache.CalcHeight();
                    property.serializedObject.ApplyModifiedProperties();
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
