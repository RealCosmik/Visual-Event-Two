using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using VisualDelegates.Editor;

namespace VisualDelegates.Cinema.Editor
{
    [CustomEditor(typeof(VisualSignalReciever))]
    public class VisualSignalRecieverDrawer : UnityEditor.Editor
    {
        SignalTree signalTree;
        TreeViewState state;
        SerializedProperty assetList;
        int refresh;
        private MultiColumnHeader CreateCollumnHeader()
        {
            var collumns = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column()
                {
                    headerContent=new GUIContent("Signal"),
                    width=100,
                    minWidth=100,
                    maxWidth=150,
                    autoResize=true,
                    allowToggleVisibility=false,
                    headerTextAlignment=TextAlignment.Center
                },
                 new MultiColumnHeaderState.Column()
                {
                    headerContent=new GUIContent("Responses"),
                    width=300,
                    minWidth=250,
                    maxWidth=450,
                    autoResize=true,
                    allowToggleVisibility=false,
                    headerTextAlignment=TextAlignment.Center
                },
            };
            return new MultiColumnHeader(new MultiColumnHeaderState(collumns));
        }
        private void CreateTree()
        {
            state = new TreeViewState();
            signalTree = new SignalTree(state, CreateCollumnHeader(), serializedObject);
        }
        private void OnDisable()
        { 
            signalTree = null;
            refresh = -1;
        }
        public override void OnInspectorGUI()
        {
            assetList = assetList ?? serializedObject.FindProperty("signalAssets");
            var size = assetList.arraySize;
            if (GUILayout.Button("Add Signal"))
            { 
                assetList.InsertArrayElementAtIndex(size);
                assetList.GetArrayElementAtIndex(size).objectReferenceValue = null;
                var signalresponses = serializedObject.FindProperty("signalResponses");
                signalresponses.InsertArrayElementAtIndex(size);
                signalresponses.GetArrayElementAtIndex(size).FindPropertyRelative("m_calls").ClearArray();
                signalresponses.GetArrayElementAtIndex(size).FindPropertyRelative("validResponse").boolValue = false;
                serializedObject.ApplyModifiedProperties();
                if (signalTree == null)
                    CreateTree();
                else
                {
                    Debug.Log("reload");
                    signalTree.UpdateSingalTree(size + 1);
                }
            }
            if (size > 0)
            {
                if (signalTree == null)
                    CreateTree();
                var tree_rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, signalTree.totalHeight);
                signalTree.OnGUI(tree_rect);
                if (refresh < 5)
                {
                    refresh += 1;
                    if (refresh == 5)
                        signalTree.Reload();
                }
            }
        }
    }
}
