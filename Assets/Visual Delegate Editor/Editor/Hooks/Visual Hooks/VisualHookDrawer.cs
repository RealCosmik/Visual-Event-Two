using UnityEngine;
using UnityEditor;
namespace VisualDelegates.Editor
{
    [CustomEditor(typeof(VisualHooks))]
    class VisualHookDrawer : UnityEditor.Editor
    {
        [SerializeField] bool awakeFold,startFold , enableFold, disableFold, updateFold,destroyFold;
        Vector2 scroll;
        public override void OnInspectorGUI()
        {
            var logprop = serializedObject.FindProperty("LogHooks");
            EditorGUI.BeginChangeCheck();
            logprop.boolValue = EditorGUILayout.ToggleLeft("Hook Logging", logprop.boolValue);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
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
                var noteprop=serializedObject.FindProperty(delegatefieldname + "Note");
                var width=EditorGUIUtility.currentViewWidth;
                EditorGUILayout.BeginHorizontal();
                var prop = serializedObject.FindProperty(delegatefieldname);
                prop.isExpanded = true;
                EditorGUILayout.PropertyField(prop,GUIContent.none,GUILayout.Width(width*.7f));
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(width * .25f), GUILayout.Height(100));
                EditorGUI.BeginChangeCheck();
                noteprop.stringValue = EditorGUILayout.TextArea(noteprop.stringValue,EditorStyles.textArea, GUILayout.ExpandHeight(true));
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Separator();
        }
    }
}
