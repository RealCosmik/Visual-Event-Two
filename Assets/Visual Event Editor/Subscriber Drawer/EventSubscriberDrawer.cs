using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
namespace VisualDelegates.Events.Editor
{
    [CustomEditor(typeof(EventSubscriber))]
    class EventSubscriberDrawer : UnityEditor.Editor
    {
        ResponseTree currentResponseTree;
        int tick = 0;
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
           currentResponseTree = null;
            tick = 0;
        }
        private void OnResponseAdded()
        {
            if (GUILayout.Button("Add Response"))
            {
                var responseListProperty = serializedObject.FindProperty("responses");
                var size = responseListProperty.arraySize;
                responseListProperty.InsertArrayElementAtIndex(size);
                responseListProperty.GetArrayElementAtIndex(size).FindPropertyRelative("currentEvent").objectReferenceValue = null;
                responseListProperty.GetArrayElementAtIndex(size).FindPropertyRelative("priority").intValue = 0;
                PrefabUtility.RecordPrefabInstancePropertyModifications(responseListProperty.serializedObject.targetObject);
                responseListProperty.serializedObject.ApplyModifiedProperties();
                if (currentResponseTree == null)
                    currentResponseTree = new ResponseTree(new TreeViewState(), CreateCollumnHeader(), serializedObject);
                else
                {
                    // Debug.Log("reload");
                    currentResponseTree.Reload();
                }
            }
        }
        private void OnResponseRemoved()
        {
            if (GUILayout.Button("Remove response"))
            {
                var selectedElements= currentResponseTree.GetSelection();
                var selectionCount = selectedElements.Count;
                var responseListProperty = serializedObject.FindProperty("responses");
                for (int i = 0; i < selectionCount; i++)
                {
                    responseListProperty.DeleteArrayElementAtIndex(selectedElements[i]);    
                }
                responseListProperty.serializedObject.ApplyModifiedProperties();
                if(responseListProperty.arraySize>0)
                currentResponseTree.Reload();
            }
        }
        private void autoRefresh()
        {
            if (tick != 5)
            {
                tick++;
                if (tick == 5)
                    currentResponseTree.Reload();
            }
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            OnResponseAdded();
            OnResponseRemoved();
            EditorGUILayout.EndHorizontal();
            if (serializedObject.FindProperty("responses").arraySize > 0)
            {
               currentResponseTree=currentResponseTree?? new ResponseTree(new TreeViewState(), CreateCollumnHeader(), serializedObject); 
                float width = 0f;
                for (int i = 0; i < 2; i++)
                {
                    width += currentResponseTree.multiColumnHeader.GetVisibleColumnIndex(i);
                }
                var tree_rect = GUILayoutUtility.GetRect(width, currentResponseTree.totalHeight);
                //EditorGUI.BeginProperty(tree_rect, GUIContent.none, serializedObject.FindProperty("responses"));
                currentResponseTree.OnGUI(tree_rect);
                //EditorGUI.EndProperty();
                autoRefresh();
            }
        }
    }
}