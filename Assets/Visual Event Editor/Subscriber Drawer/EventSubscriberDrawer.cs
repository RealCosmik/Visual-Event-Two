using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Microsoft.CodeAnalysis;

namespace VisualDelegates.Events.Editor
{
    [CustomEditor(typeof(EventSubscriber))]
    class EventSubscriberDrawer : UnityEditor.Editor
    { 
        ResponseTree currentResponseTree;
        int refresh;
        private MultiColumnHeader CreateCollumnHeader()
        {
            var collumns = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column()
                {
                    headerContent=new GUIContent("Events"),
                    width=100,
                    minWidth=100,
                    maxWidth=150,
                    autoResize=true,
                    headerTextAlignment=TextAlignment.Center
                },
                 new MultiColumnHeaderState.Column()
                {
                    headerContent=new GUIContent("Responses"),
                    width=300,
                    minWidth=100,
                    maxWidth=500,
                    autoResize=true,
                    headerTextAlignment=TextAlignment.Center
                },
                //   new MultiColumnHeaderState.Column()
                //{
                //    headerContent=new GUIContent("Priority"),
                //    width=50,
                //    minWidth=100,
                //    maxWidth=500,
                //    autoResize=true,
                //    headerTextAlignment=TextAlignment.Center
                //},
            };
            return new MultiColumnHeader(new MultiColumnHeaderState(collumns));
        } 
        private void OnDisable()
        {
            refresh = 0;
        }
        private void OnEnable()
        {
            if (serializedObject.FindProperty("responses").arraySize > 0)
                currentResponseTree = new ResponseTree(new TreeViewState(), CreateCollumnHeader(), serializedObject);
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Add Response"))
            { 
                var responseListProperty = serializedObject.FindProperty("responses");
                var size = responseListProperty.arraySize;
                responseListProperty.InsertArrayElementAtIndex(size);
                responseListProperty.GetArrayElementAtIndex(size).FindPropertyRelative("response").managedReferenceValue = new randomdel();
                responseListProperty.serializedObject.ApplyModifiedProperties();
                if (currentResponseTree == null)
                    currentResponseTree = new ResponseTree(new TreeViewState(), CreateCollumnHeader(), serializedObject);
                else
                {
                    Debug.Log("reload");
                    currentResponseTree.Reload();
                }
            }
            if (currentResponseTree != null&& serializedObject.FindProperty("responses").arraySize > 0)
            {
                float width = 0f;
                for (int i = 0; i < 2; i++)
                {
                    width += currentResponseTree.multiColumnHeader.GetVisibleColumnIndex(i);
                }
                var tree_rect = GUILayoutUtility.GetRect(width, currentResponseTree.totalHeight);
                currentResponseTree.OnGUI(tree_rect);
                //if (refresh != 3) 
                //{ 
                //    Debug.Log("OK");
                //    refresh++;
                //    if(refresh==3)
                //    currentResponseTree.Reload();
                //}
               
            }
        }
    }
    [System.Serializable]
    public class randomdel : VisualDelegate<int>
    {

    }

}