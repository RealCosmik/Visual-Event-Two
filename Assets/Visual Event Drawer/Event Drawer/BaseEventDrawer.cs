using UnityEditor;
using UnityEngine;
using VisualDelegates.Editor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace VisualDelegates.Events.Editor
{
    [CustomEditor(typeof(BaseEvent), true)]
    class BaseEventDrawer : UnityEditor.Editor
    {
        ResponseTree responsetree;
        const string RESPONSE_FIELD_NAME = "responses";
        private void OnEnable()
        {
            PopulateSubscribers();
            GUIContent prioritycontent = new GUIContent("Priorities");
            VisualEditorUtility.StandardStyle.CalcMinMaxWidth(prioritycontent, out float min, out float max);
            var collumns = new MultiColumnHeaderState.Column[]
           {
                       new MultiColumnHeaderState.Column()
                       {
                           headerContent = prioritycontent,
                           width = max+10,
                           minWidth = max+10,
                           maxWidth = max+10,
                           autoResize = true,
                           headerTextAlignment = TextAlignment.Left
                       },
                       new MultiColumnHeaderState.Column()
                       {
                           headerContent = new GUIContent("Subscribers"),
                           width = 150,
                           minWidth = 75,
                           maxWidth = 300,
                           autoResize = true,
                           headerTextAlignment = TextAlignment.Left
                       },
                       new MultiColumnHeaderState.Column()
                       {
                           headerContent = new GUIContent("Response"),
                           width = 200,
                           minWidth = 100,
                           maxWidth = 300,
                           autoResize = true,
                           headerTextAlignment = TextAlignment.Center
                       },
           };
            var collumnheader = new MultiColumnHeader(new MultiColumnHeaderState(collumns));
            if((target as BaseEvent).AllResponses.Count>0)
           responsetree = new ResponseTree(new TreeViewState(), collumnheader, target as BaseEvent);
        }
        private void OnDisable()
        { 
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                (target as BaseEvent).AllResponses.Clear();
            }
        }
         
        public override void OnInspectorGUI()
        {
            if (responsetree != null)
            {
                float width = 0f;
                for (int i = 0; i < 2; i++)
                {
                    width += responsetree.multiColumnHeader.GetVisibleColumnIndex(i);
                }
                var rect = GUILayoutUtility.GetRect(width, responsetree.totalHeight);
                responsetree.OnGUI(rect);
            }
        }
        private void PopulateSubscribers()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                var binding = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                var root_objects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
                var length = root_objects.Length;
                for (int i = 0; i < length; i++)
                {
                    var event_subscribers = root_objects[i].GetComponentsInChildren<EventSubscriber>();
                    for (int j = 0; j < event_subscribers.Length; j++)
                    {
                        var event_responses = event_subscribers[j].GetType().GetField(RESPONSE_FIELD_NAME, binding).GetValue(event_subscribers[j]) as List<EventResponse>;
                        for (int k = 0; k < event_responses.Count; k++)
                        {
                            if (event_responses[k].currentEvent == target)
                            {
                                event_responses[k].responseIndex = k;
                                event_responses[k].senderID = event_subscribers[j].GetInstanceID();
                                (target as BaseEvent).Subscribe(event_responses[k], event_responses[k].priority);
                            }
                        }
                    }
                }
            }
        }
    }
}
