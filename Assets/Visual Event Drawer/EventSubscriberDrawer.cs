using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using NUnit.Framework.Constraints;

namespace VisualDelegates.Events.Editor
{
    [CustomEditor(typeof(EventSubscriber))]
    public class EventSubscriberDrawer : UnityEditor.Editor
    {
        TreeViewState m_TreeViewState;
        SubscriberTree m_subscribertree;
        MultiColumnHeader header;
        float currentoffset;
        SerializedProperty responses;
        private MultiColumnHeader CreateCollumnHeader()
        {
            var collumns = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column()
                {
                    headerContent=new GUIContent("Events"),
                    width=50,
                    minWidth=100,
                    maxWidth=500,
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
            Debug.Log("enable");
            //  m_TreeViewState = new TreeViewState();
            //m_subscribertree = new SubscriberTree(m_TreeViewState,CreateCollumnHeader(),serializedObject);
            header = CreateCollumnHeader();
            responses = serializedObject.FindProperty("responses");
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Add Event Response"))
            {
                responses.InsertArrayElementAtIndex(responses.arraySize);
                responses.GetArrayElementAtIndex(responses.arraySize - 1).FindPropertyRelative("response").managedReferenceValue = new randomdel();
                serializedObject.ApplyModifiedProperties();
                // m_subscribertree.Reload();
            }
            var myrect = EditorGUILayout.GetControlRect();
            header.OnGUI(myrect, currentoffset);
            int size = responses.arraySize;
            for (int i = 0; i < size; i++)
            {
                DrawResponse(responses.GetArrayElementAtIndex(i));
            }
        }
        private void DrawResponse(SerializedProperty Currentresponse)
        {
            var eventrect = GUILayoutUtility.GetRect(header.GetColumnRect(0).width,
                EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none));
            eventrect.width = header.GetColumnRect(0).width;
            //eventrect.x= header.GetColumnRect(0).x;
            EditorGUI.PropertyField(eventrect, Currentresponse.FindPropertyRelative("currentEvent"), GUIContent.none);

            var responserect = GUILayoutUtility.GetRect(header.GetColumnRect(1).width,
                 EditorGUI.GetPropertyHeight(Currentresponse.FindPropertyRelative("response"), GUIContent.none));
            responserect.width = header.GetColumnRect(1).width;
            // responserect.x += header.GetColumnRect(1).xMin;
           // responserect.x += header.GetColumnRect(1).x + 0f;
            responserect.y -= 35f;
            EditorGUI.PropertyField(responserect, Currentresponse.FindPropertyRelative("response"), GUIContent.none);
        }
        //public override void OnInspectorGUI()
        //{
        //    if (GUILayout.Button("Add Event Response"))
        //    {
        //        var all_events = serializedObject.FindProperty("base_events");
        //        all_events.InsertArrayElementAtIndex(all_events.arraySize);
        //        serializedObject.ApplyModifiedProperties();
        //        m_subscribertree.Reload();
        //    }
        //    if (GUILayout.Button("reload"))
        //    {
        //        m_subscribertree.Reload();
        //    }
        //    if (GUILayout.Button("rebuild"))
        //    {
        //        m_subscribertree.refresh();
        //    }
        //    var rect = GUILayoutUtility.GetRect(500, m_subscribertree.totalHeight);
        //    m_subscribertree.OnGUI(rect);
        //}
    }
    [System.Serializable]
    public class randomdel : VisualDelegate<int>
    {

    }

}