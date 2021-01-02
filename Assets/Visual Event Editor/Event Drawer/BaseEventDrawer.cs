using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using System.Linq;

namespace VisualEvents.Editor
{
    [CustomEditor(typeof(BaseEvent), true, isFallback = true)]
    class BaseEventDrawer : UnityEditor.Editor
    {
        const string RESPONSE_FIELD_NAME = "responses";
        const string EVENT_DETAILS = "Event Details";
        EventResponseTree responsetree;
        HistoryTree historyTree;
        [SerializeField] bool detailsfolded = true;
        [SerializeField] bool debugfold, invocationfold, historyfold, argumentfold;
        TreeViewState responseState;
        int genericCount;
        Action editorInvocation;
        Func<bool> getInvocationStatus;
        int ticks;
        Vector2 scroll;
        Type ResponseType;
        FieldInfo responseIndex_field, invocation_field;
        Type VisualSubscriber;
        const int HEIGHT_PADDING = 30;
        SerializedProperty[] invocationProperties;
        List<Type> genericArguments;
        Dictionary<string, FieldInfo> unityArguments;
        public override bool RequiresConstantRepaint() => true;
        private void OnEnable()
        {
            SetInitalVariableValue();
            VisualSubscriber = TypeCache.GetTypesWithAttribute<VisualSubscriber>().FirstOrDefault();
            SetDebugProperties();
            PopulateVisualSubscribers();
            responseState = responseState ?? new TreeViewState();
            if ((target as BaseEvent).EventResponses.Count > 0)
            {
                responsetree = new EventResponseTree(responseState, target as BaseEvent);
            }

            if (historyTree == null)
                historyTree = new HistoryTree(new TreeViewState(), target as BaseEvent,
                    serializedObject.FindProperty("historycapacity").intValue);
            if (getInvocationStatus == null)
            {
                var flags = BindingFlags.NonPublic | BindingFlags.Instance;
                getInvocationStatus = Delegate.CreateDelegate(typeof(Func<bool>), target, (typeof(BaseEvent).GetMethod("GetisInvoke", flags)), true) as Func<bool>;
            }
        }

        private void SetDebugProperties()
        {
            genericArguments = target.GetType().BaseType.GenericTypeArguments.ToList();
            genericCount = genericArguments.Count;
            if (target is IVisualVariable)
            {
                genericCount -= 1;
                genericArguments.RemoveAt(genericCount);
            }
            var targetType = target.GetType();
            var binding = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            unityArguments = new Dictionary<string, FieldInfo>(genericCount);
            string argument = "argument";
            invocationProperties = new SerializedProperty[genericCount];
            for (int i = 0; i < genericCount; i++)
            {
                string propertyName = argument + (i + 1);
                var argumentproperty = serializedObject.FindProperty(propertyName);
                invocationProperties[i] = argumentproperty;
                if (argumentproperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    unityArguments.Add(propertyName, targetType.GetField(propertyName, binding));
                }
            }
        }
        private void SetInitalVariableValue()
        {
            if (target is IVisualVariable)
            {
                var binding = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                var var_type = target.GetType();
                var_type.GetMethod("InitializeVariable", binding).Invoke(target, null);
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
            // if (EditorApplication.isPlaying)
            {
                invocationfold = EditorGUILayout.Foldout(invocationfold, "Test invocation");
                if (invocationfold)
                    DrawTestInvocation();
            }

            historyfold = EditorGUILayout.Foldout(historyfold, "Event History");
            if (historyfold)
                DrawEventHistory();
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
                    editorInvocation = Delegate.CreateDelegate(typeof(Action), target, editormethod, true) as Action;
                }
                editorInvocation.Invoke();
                serializedObject.Update();
            }
            GUI.enabled = true;
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < genericCount; i++)
            {
                EditorGUI.BeginChangeCheck();
                UnityEngine.Object value = null;
                var prop = invocationProperties[i];
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    value = EditorGUILayout.ObjectField(prop.displayName, unityArguments[prop.name].GetValue(target) as UnityEngine.Object, genericArguments[i], true);
                }
                else EditorGUILayout.PropertyField(prop);

                if (EditorGUI.EndChangeCheck())
                {
                    if (!ReferenceEquals(value, null))
                    {
                        unityArguments[prop.name].SetValue(target, value);
                    }
                    else serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawEventHistory()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            var debugToggleRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth * .3f, EditorGUI.GetPropertyHeight(SerializedPropertyType.Boolean, null));
            var debugproperty = serializedObject.FindProperty("debugHistory");
            EditorGUI.LabelField(debugToggleRect, new GUIContent("Debug History"));
            var togglerect = debugToggleRect;
            togglerect.width *= .1f;
            togglerect.x += 105;
            EditorGUI.BeginChangeCheck();
            debugproperty.boolValue = EditorGUI.Toggle(togglerect, debugproperty.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                historyTree?.Reload();
            }
            if (debugproperty.boolValue)
            {
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
                serializedObject.UpdateIfRequiredOrScript();
            }
            else
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }
        private void UpdateEventHistory()
        {
            if (EditorApplication.isPlaying)
            {
                if (getInvocationStatus())
                {
                    historyTree?.Reload();
                    if (invocation_field == null)
                    {
                        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                        invocation_field = typeof(BaseEvent).GetField("isinvoke", flags);
                    }
                    invocation_field.SetValue(target, false);
                }
            }
        }
        private void DynamicRegistartionReload()
        {
            if (responsetree == null && (target as BaseEvent).EventResponses.Count > 0)
            {
                //  OnEnable();
            }
        }
        public override void OnInspectorGUI()
        {
            DrawVarialbe();
            DrawNoteField();
            DrawDebuggingData();
            // DynamicRegistartionReload();
            responsetree?.OnGUI(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, responsetree.totalHeight + HEIGHT_PADDING));
            autoreload();
        }

        private Scene[] GetLoadedScenes()
        {
            var scenecount = EditorSceneManager.sceneCount;
            Scene[] loadedScenes = new Scene[scenecount];
            for (int i = 0; i < scenecount; i++)
            {
                loadedScenes[i] = EditorSceneManager.GetSceneAt(i);
            }
            return loadedScenes;
        }
        private void PopulateVisualSubscribers()
        {
            if (VisualSubscriber == null)
                return;

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                var prefabstage = PrefabStageUtility.GetCurrentPrefabStage();
                var binding = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                (typeof(BaseEvent).GetField("m_EventResponses", binding).GetValue(target) as List<List<EventResponse>>).Clear();

                var loadedScenes = GetLoadedScenes();
                for (int s = 0; s < loadedScenes.Length && prefabstage == null; s++)
                {
                    var scene = loadedScenes[s];
                    if (scene.isLoaded)
                    {
                        var root_objects = loadedScenes[s].GetRootGameObjects();
                        var length = root_objects.Length;
                        for (int i = 0; i < length; i++)
                        {
                            var event_subscribers = root_objects[i].GetComponentsInChildren(VisualSubscriber);
                            for (int j = 0; j < event_subscribers.Length; j++)
                            {
                                var event_responses = VisualSubscriber.GetField(RESPONSE_FIELD_NAME, binding).GetValue(event_subscribers[j]) as IEnumerable<EventResponse>;
                                for (int k = 0; k < event_responses.Count(); k++)
                                {
                                    var response = event_responses.ElementAt(k);
                                    if (response.currentEvent == target)
                                    {
                                        if (responseIndex_field == null)
                                        {
                                            ResponseType = response.GetType();
                                            responseIndex_field = ResponseType.GetField("responseIndex", binding);
                                        }
                                        responseIndex_field.SetValue(response, k);
                                        response.subscriberID = event_subscribers[j].GetInstanceID();
                                        (target as BaseEvent).Subscribe(response);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
