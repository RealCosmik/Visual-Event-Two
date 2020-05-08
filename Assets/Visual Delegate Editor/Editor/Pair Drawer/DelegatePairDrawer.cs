using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using GitHub.Unity;

namespace VisualDelegates.Editor
{
    public abstract class DelegatePairDrawer : UnityEditor.Editor
    {
        MultiColumnHeaderState.Column DelegateColumn;
        MultiColumnHeaderState.Column ScriptableColumn;
        MultiColumnHeader mainheader;
        float Scrollpos;
        TreeViewState treeState;
        private void OnEnable()
        {
            DelegateColumn = new MultiColumnHeaderState.Column()
            {
                headerContent = new GUIContent("DELEGATES"),
                width = 200,
                minWidth = 100,
                maxWidth = 500,
                autoResize = true,
                headerTextAlignment = TextAlignment.Center
            };
            ScriptableColumn = new MultiColumnHeaderState.Column()
            {
                headerContent = new GUIContent("Assets"),
                width = 200,
                minWidth = 100,
                maxWidth = 500,
                autoResize = true,
                headerTextAlignment = TextAlignment.Center
            };
            var headercollumns = new MultiColumnHeaderState.Column[] { DelegateColumn, ScriptableColumn };
            mainheader = new MultiColumnHeader(new MultiColumnHeaderState(headercollumns));
        }
        public override void OnInspectorGUI()
        {
            var good_rect=GUILayoutUtility.GetRect(100, 100);
            mainheader.height = 0;
            mainheader.OnGUI(good_rect, Scrollpos);
            Debug.Log(mainheader.GetColumnRect(0));
        }
    }
}
