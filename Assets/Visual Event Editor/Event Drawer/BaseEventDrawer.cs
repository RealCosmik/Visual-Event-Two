using UnityEditor;
using UnityEngine;
using VisualDelegates.Editor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace VisualDelegates.Events.Editor
{
    [CustomEditor(typeof(BaseEvent), true)]
    class BaseEventDrawer : UnityEditor.Editor
    {
        SuscriberTree responsetree;
        const string RESPONSE_FIELD_NAME = "responses";
        const string ARGUMENT = "argument";
        int ticks;
        [SerializeField] bool detailsfolded = true;
        [SerializeField] bool argumentfold = false;
        TreeViewState currentState;
        Type[] genericArguments;
        Action editorInvocation;
        private void OnEnable()
        {
            genericArguments = target.GetType().BaseType.GenericTypeArguments;
            PopulateSubscribers();
            currentState = currentState ?? new TreeViewState();
            if ((target as BaseEvent).AllResponses.Count > 0)
                responsetree = new SuscriberTree(currentState, GetEventCollumns(), target as BaseEvent);
        }
        private MultiColumnHeader GetEventCollumns()
        {
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
                           width = 75,
                           minWidth = 75,
                           maxWidth = 150,
                           autoResize = true,
                           headerTextAlignment = TextAlignment.Left
                       },
                       new MultiColumnHeaderState.Column()
                       {
                           headerContent = new GUIContent("Response"),
                           width = 300,
                           minWidth = 100,
                           maxWidth = 350,
                           autoResize = true,
                           headerTextAlignment = TextAlignment.Center
                       },
         };
            return new MultiColumnHeader(new MultiColumnHeaderState(collumns));
        }
        private void OnDisable()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
            }
            responsetree = null;
            ticks = 0;
        }
        private float GetTreewidth()
        {
            float width = 0f;
            for (int i = 0; i < 2; i++)
                width += responsetree.multiColumnHeader.GetVisibleColumnIndex(i);
            return width;
        }
        private void autoreload()
        {
            if (ticks != 5)
            {
                ticks++;
                if (ticks == 5)
                {
                    responsetree?.Reload();
                }
            }
        }
        public override void OnInspectorGUI()
        {
            DrawNoteField();
            DrawInvoke();
            responsetree?.OnGUI(GUILayoutUtility.GetRect(GetTreewidth(), responsetree.totalHeight));
            autoreload();
        }
        private void DrawNoteField()
        {
            var style = new GUIStyle("textField");
            style.wordWrap = true;
            var note_property=serializedObject.FindProperty("EventNote");
            detailsfolded = EditorGUILayout.BeginFoldoutHeaderGroup(detailsfolded, "Event Details");
            if (detailsfolded)
            {
                EditorGUI.BeginChangeCheck();
                note_property.stringValue = EditorGUILayout.TextArea(note_property.stringValue, style, GUILayout.ExpandHeight(true));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawInvoke()
        {
            var argument_count = genericArguments.Length;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            argumentfold = EditorGUILayout.BeginFoldoutHeaderGroup(argumentfold, "Arguments");
            if (argumentfold)
            {
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < argument_count; i++)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(ARGUMENT + (i + 1)));
                }
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (GUILayout.Button("Invoke"))
            {
                if (editorInvocation == null)
                {
                    var binding = BindingFlags.Instance | BindingFlags.NonPublic;
                    var event_type = target.GetType();
                    var editormethod = event_type.GetMethod("EditorInvoke", binding);
                    editorInvocation = Delegate.CreateDelegate(typeof(Action), target, editormethod, true) as Action;
                }
                editorInvocation.Invoke();
            }
            
            EditorGUILayout.EndHorizontal();
            
          
        }
        private void PopulateSubscribers()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                (target as BaseEvent).AllResponses.Clear();
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
