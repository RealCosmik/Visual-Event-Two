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
    [CustomEditor(typeof(BaseEvent), true, isFallback = true)]
    class BaseEventDrawer : UnityEditor.Editor
    {
        const string RESPONSE_FIELD_NAME = "responses";
        const string ARGUMENT = "argument";
        const string EVENT_DETAILS = "Event Details";
        EventResponseTree responsetree;
        HistoryTree historyTree;
        [SerializeField] bool detailsfolded = true;
        [SerializeField] bool debugfold, invocationfold, historyfold, argumentfold;
        TreeViewState responseState;
        int genericCount;
        Action<UnityEngine.Object> editorInvocation;
        Func<bool> getInvocationStatus;
        int ticks;
        Vector2 scroll;

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
                           allowToggleVisibility=false,
                           headerTextAlignment = TextAlignment.Left
                       },
                       new MultiColumnHeaderState.Column()
                       {
                           headerContent = new GUIContent("Subscribers"),
                           width = 75,
                           minWidth = 75,
                           maxWidth = 150,
                           autoResize = true,
                           allowToggleVisibility=false,
                           headerTextAlignment = TextAlignment.Left
                       },
                       new MultiColumnHeaderState.Column()
                       {
                           headerContent = new GUIContent("Response"),
                           width = 300,
                           minWidth = 100,
                           maxWidth = 350,
                           autoResize = true,
                           allowToggleVisibility=false,
                           headerTextAlignment = TextAlignment.Center
                       },
                       new MultiColumnHeaderState.Column()
                       {
                           headerContent= new GUIContent("Response Note"),
                             width = 130,
                           minWidth = 130,
                           maxWidth = 150,
                           autoResize = true,
                           headerTextAlignment = TextAlignment.Left
                       }
         };
            return new MultiColumnHeader(new MultiColumnHeaderState(collumns));
        }

        private void OnEnable()
        {
            genericCount = target.GetType().BaseType.GenericTypeArguments.Length;
            PopulateSubscribers();
            responseState = responseState ?? new TreeViewState();
            if ((target as BaseEvent).EventResponses.Count > 0)
            {
                responsetree = new EventResponseTree(responseState, GetEventCollumns(), target as BaseEvent);
            }

            if (historyTree == null)
                historyTree = new HistoryTree(new TreeViewState(), HistoryTree.CreateHistoryHeader(), target as BaseEvent,
                    serializedObject.FindProperty("historycapacity").intValue);
            if (getInvocationStatus == null)
            {
                var flags = BindingFlags.NonPublic | BindingFlags.Instance;
                getInvocationStatus = Delegate.CreateDelegate(typeof(Func<bool>), target, (typeof(BaseEvent).GetMethod("GetisInvoke", flags)), true) as Func<bool>;
            }
        }
        private void OnDisable()
        {
            responsetree = null;
            historyTree = null;
            ticks = 0;
        }

        private void autoreload()
        {
            if (ticks != 5)
            {
                ticks++;
                if (ticks == 5)
                    responsetree?.Reload();
            }
        }
        private void DrawVarialbe()
        {
            if (target is IVisualVariable)
            {
                bool isplaying = EditorApplication.isPlaying;
                GUI.enabled = !isplaying;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("initialValue"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                }
                GUI.enabled = true;
                if (isplaying)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("currentValue"));
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                    serializedObject.UpdateIfRequiredOrScript();
                }
            }
        }
        private void DrawNoteField()
        {
            var style = new GUIStyle("textField");
            style.wordWrap = true;
            var note_property = serializedObject.FindProperty("EventNote");
            detailsfolded = EditorGUILayout.BeginFoldoutHeaderGroup(detailsfolded, EVENT_DETAILS);
            if (detailsfolded)
            {  
                EditorGUI.BeginChangeCheck();
                note_property.stringValue = EditorGUILayout.TextArea(note_property.stringValue, style, GUILayout.ExpandHeight(true));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        private void DrawDebuggingData()
        {

            invocationfold = EditorGUILayout.BeginFoldoutHeaderGroup(invocationfold, "Test invocation");
            if (invocationfold)
                DrawTestInvocation();
            EditorGUILayout.EndFoldoutHeaderGroup();

            historyfold = EditorGUILayout.BeginFoldoutHeaderGroup(historyfold, "Event History");
            if (historyfold)
                DrawEventHistory();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        private void DrawTestInvocation()
        {
            EditorGUI.indentLevel++;
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Invoke"))
            {
                if (editorInvocation == null)
                {
                    var binding = BindingFlags.Instance | BindingFlags.NonPublic;
                    var event_type = target.GetType();
                    var editormethod = event_type.GetMethod("EditorInvoke", binding);
                    editorInvocation = Delegate.CreateDelegate(typeof(Action<UnityEngine.Object>), target, editormethod, true) as Action<UnityEngine.Object>;
                }
                editorInvocation.Invoke(target);
                serializedObject.Update();
            }
            GUI.enabled = true;
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < genericCount; i++)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(ARGUMENT + (i + 1)));
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();


        }
        private void DrawEventHistory()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            var capacityrect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth * .3f, EditorGUI.GetPropertyHeight(SerializedPropertyType.Integer, null));
            EditorGUI.LabelField(capacityrect, new GUIContent("History Capacity"));
            var valurect = capacityrect;
            valurect.width *= .3f;
            valurect.x += (valurect.width + 40);
            EditorGUI.BeginChangeCheck();
            var historyprop = serializedObject.FindProperty("historycapacity");
            historyprop.intValue = EditorGUI.IntField(valurect, historyprop.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                if (historyprop.intValue < 5)
                    historyprop.intValue = 5;
                serializedObject.ApplyModifiedProperties();
                historyTree?.Reload();
            }
            // EditorGUILayout.IntField("History capactity",25);
            historyTree?.OnGUI(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth * .3f, 100f));
            EditorGUILayout.EndVertical();
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(EditorGUIUtility.currentViewWidth * .6f), GUILayout.Height(125));
            EditorGUILayout.TextArea(historyTree.activeTrace, EditorStyles.textArea, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            UpdateEventHistory();
        }
        private void UpdateEventHistory()
        {
            if (EditorApplication.isPlaying)
            {
                if (getInvocationStatus())
                {
                    historyTree?.Reload();
                    var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                    typeof(BaseEvent).GetField("isinvoke", flags).SetValue(target, false);
                }
            }
        }
        private void DynamicRegistartionReload()
        {
            if (responsetree == null && (target as BaseEvent).EventResponses.Count > 0)
            {
                OnEnable();
            }
        }
        public override void OnInspectorGUI()
        {
            DrawVarialbe();
            DrawNoteField();
            DrawDebuggingData();
            DynamicRegistartionReload();
            responsetree?.OnGUI(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, responsetree.totalHeight));
            autoreload();
        }

        private void PopulateSubscribers()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                var binding = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                (typeof(BaseEvent).GetField("m_EventResponses", binding).GetValue(target) as List<List<EventResponse>>).Clear();
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
                                (target as BaseEvent).Subscribe(event_responses[k]);
                            }
                        }
                    }
                }
            }
        }
    }
}
