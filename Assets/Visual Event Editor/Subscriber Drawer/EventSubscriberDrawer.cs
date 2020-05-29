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
        private void OnEnable()
        {
            if (serializedObject.FindProperty("responses").arraySize > 0)
                currentResponseTree = new ResponseTree(new TreeViewState(), CreateCollumnHeader(), serializedObject);
        }
        public override void OnInspectorGUI()
        {

            if (currentResponseTree != null && serializedObject.FindProperty("responses").arraySize > 0)
            {
                float width = 0f;
                for (int i = 0; i < 2; i++)
                {
                    width += currentResponseTree.multiColumnHeader.GetVisibleColumnIndex(i);
                }
                var tree_rect = GUILayoutUtility.GetRect(width, currentResponseTree.totalHeight);
                currentResponseTree.OnGUI(tree_rect);
            }
        }
    }
    [System.Serializable]
    public class randomdel : VisualDelegate<int>
    {

    }

}