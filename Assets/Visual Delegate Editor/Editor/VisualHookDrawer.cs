using UnityEngine;
using UnityEditor;
namespace VisualDelegates.Editor
{
    [CustomEditor(typeof(VisualHooks))]
    class VisualHookDrawer : UnityEditor.Editor
    {
        [SerializeField] bool awakeFold,startFold , enableFold, disableFold, updateFold,destroyFold;
        public override void OnInspectorGUI()
        {
            DrawVisualHook("onAwake", ref awakeFold);
            DrawVisualHook("onStart", ref startFold);
            DrawVisualHook("onEnable", ref enableFold);
            DrawVisualHook("onDisable", ref disableFold);
            DrawVisualHook("onUpdate", ref updateFold);
            DrawVisualHook("onDestory", ref destroyFold);
        }

        private void DrawVisualHook(string delegatefieldname, ref bool foldstatus)
        {
            foldstatus = EditorGUILayout.BeginFoldoutHeaderGroup(foldstatus, delegatefieldname);
            if (foldstatus)
            {
                var prop = serializedObject.FindProperty(delegatefieldname);
                prop.isExpanded = true;
                EditorGUILayout.PropertyField(prop);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
