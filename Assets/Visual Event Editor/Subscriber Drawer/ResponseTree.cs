using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VisualDelegates.Editor;
namespace VisualDelegates.Events.Editor
{
    class ResponseTree : TreeView
    {
        SerializedObject serializedSubscriber;
        static readonly string UP_ARROW = char.ConvertFromUtf32(0x2191);
        static readonly string DOWN_ARROW = char.ConvertFromUtf32(0x2193);
        int ticks = 0;
        int tickTrigger = 3;
        public ResponseTree(TreeViewState state, MultiColumnHeader header, SerializedObject newSerializedSubscriber) : base(state, header)
        {
            serializedSubscriber = newSerializedSubscriber;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1, "root");
            SetSubscriberElements(root);
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }
        private void SetSubscriberElements(TreeViewItem root)
        {

            var subscriberResponses = serializedSubscriber.FindProperty("responses");
            var responsecount = subscriberResponses.arraySize;
            for (int i = 0; i < responsecount; i++)
            {
                var baseEvent = subscriberResponses.GetArrayElementAtIndex(i).FindPropertyRelative("currentEvent").objectReferenceValue as BaseEvent;
               root.AddChild(new ResponseTreeElement(baseEvent){ id = i });
            }
        }
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            var responseElement = item as ResponseTreeElement;
            var eventResponseProperty = serializedSubscriber.FindProperty("responses").GetArrayElementAtIndex(responseElement.id);
            if (eventResponseProperty.FindPropertyRelative("currentEvent").objectReferenceValue != null)
            {
                var delegateprop = eventResponseProperty.FindPropertyRelative("response");
                if (delegateprop.isExpanded)
                    return EditorGUI.GetPropertyHeight(delegateprop);
                else return EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none) +
                    EditorGUI.GetPropertyHeight(SerializedPropertyType.Integer, GUIContent.none);
            }
            else return EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none) +
                    EditorGUI.GetPropertyHeight(SerializedPropertyType.Integer, GUIContent.none);
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var visiable_colls = args.GetNumVisibleColumns();
            for (int i = 0; i < visiable_colls; i++)
            {
                if (args.item is ResponseTreeElement responseElement)
                    DrawResponse(i, ref args);
            }
            if (ticks != tickTrigger)
            {
                ticks++;
                if (ticks == tickTrigger)
                    Reload();
            }
        }
        private void DrawResponse(int collumn, ref RowGUIArgs rowarg)
        {
            var element = rowarg.item as ResponseTreeElement;
            var currentResponse = serializedSubscriber.FindProperty("responses").GetArrayElementAtIndex(element.id);
            EditorGUI.BeginProperty(rowarg.GetCellRect(1), GUIContent.none, currentResponse);
            switch (collumn)
            {
                case 0:
                    DrawEventAndPriority(rowarg.GetCellRect(0), currentResponse, element);
                    break;
                case 1:
                    var cellrect = rowarg.GetCellRect(1);
                    if (currentResponse.FindPropertyRelative("currentEvent").objectReferenceValue != null)
                        DrawDelegate(cellrect, element);
                    break;
                default:
                    break;
            }
            EditorGUI.EndProperty();
        }
        private void DrawEventAndPriority(Rect cell, SerializedProperty currentResponse, ResponseTreeElement element)
        {
            var currentEventProperty = currentResponse.FindPropertyRelative("currentEvent");
            var priorityProperty = currentResponse.FindPropertyRelative("priority");
            var event_rect = cell;
            event_rect.height = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(event_rect, currentEventProperty, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
                OnSubscribe(currentEventProperty.objectReferenceValue as BaseEvent, element);

            var priorityrect = event_rect;
            priorityrect.y += priorityrect.height;
            priorityrect.width = cell.width / 3;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(priorityrect, priorityProperty, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                serializedSubscriber.ApplyModifiedProperties();
            }

            var increaseRect = priorityrect;
            increaseRect.x += priorityrect.width;
            increaseRect.width = cell.width / 3;
            if (GUI.Button(increaseRect, UP_ARROW))
            {
                priorityProperty.intValue++;
                serializedSubscriber.ApplyModifiedProperties();
            }

            var decreaseRect = increaseRect;
            decreaseRect.x += increaseRect.width;
            if (GUI.Button(decreaseRect, DOWN_ARROW) && priorityProperty.intValue > 0)
            {
                priorityProperty.intValue--;
                serializedSubscriber.ApplyModifiedProperties();
            }
        }
        private void DrawDelegate(Rect cell, ResponseTreeElement element)
        {
            var currentDelegate = serializedSubscriber.FindProperty("responses").GetArrayElementAtIndex(element.id)
                .FindPropertyRelative("response");

            EditorGUI.BeginChangeCheck();
            cell.x += 15f;
            cell.width -= 15f;
            EditorGUI.PropertyField(cell, currentDelegate);

            if (EditorGUI.EndChangeCheck())
            {
                currentDelegate.serializedObject.ApplyModifiedProperties();
                RefreshCustomRowHeights();
                ViewCache.GetVisualDelegateInstanceCache(currentDelegate).UpdateInterncalcall(currentDelegate);
                ticks = 0;
                if (element.iscollapsed != currentDelegate.isExpanded)
                {
                    tickTrigger = 5;
                    element.iscollapsed = currentDelegate.isExpanded;
                }
                else
                    tickTrigger = 3;
            } 
        }
        private void OnSubscribe(BaseEvent subscribedEvent, ResponseTreeElement element)
        {
            if (subscribedEvent != null)
            {
                var binding = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                var responses = serializedSubscriber.targetObject.GetType().GetField("responses", binding)
                    .GetValue(serializedSubscriber.targetObject) as List<EventResponse>;
                if (GetCorrespondingDelegate(subscribedEvent, out Type delegatetype))
                {
                    //var arguments = String.Join(" ", subscribedEvent.GetType().GenericTypeArguments.Select(t => t.FullName));
                    //Debug.LogError($"No delegate found in any loaded assembly matching {arguments}");
                    // Debug.LogWarning("DO THIS");
                    if (EditorApplication.isPlaying)
                    {
                        element.CurrentEvent?.UnSubscribe(responses[element.id]);
                        element.CurrentEvent = subscribedEvent;
                        subscribedEvent.Subscribe(responses[element.id], responses[element.id].priority);
                    }
                    //if (!responses[element.id].GetType().GenericTypeArguments.SequenceEqual(delegatetype.GenericTypeArguments))
                    {
                        Undo.RegisterCompleteObjectUndo(serializedSubscriber.targetObject, "swap prop");
                        var delegateprop = serializedSubscriber.FindProperty("responses").GetArrayElementAtIndex(element.id).FindPropertyRelative("response");
                        delegateprop.isExpanded = true;
                        delegateprop.managedReferenceValue = Activator.CreateInstance(delegatetype);
                    }
                    serializedSubscriber.ApplyModifiedProperties();
                    RefreshCustomRowHeights();
                    ticks = 0;
                    tickTrigger = 3;
                }
                else
                {
                    var arguments = String.Join(" ", subscribedEvent.GetType().GenericTypeArguments.Select(t => t.FullName));
                    Debug.LogError($"No delegate found in any loaded assembly matching {arguments}");
                }

            }
        }
        private bool GetCorrespondingDelegate(BaseEvent baseEvent, out Type delegatetype)
        {
            var eventArguments = baseEvent.GetType().BaseType.GenericTypeArguments;
            for (int i = 0; i < eventArguments.Length; i++)
            {
                Debug.Log(eventArguments[i]);
            }
            var delegateTypes = TypeCache.GetTypesDerivedFrom<VisualDelegateBase>();
            delegatetype = delegateTypes.FirstOrDefault(t => t.BaseType.GenericTypeArguments.SequenceEqual(eventArguments));
            Debug.Log(delegatetype.FullName);
            return delegatetype != null;
        } 
        //private void DrawInvalidEvent(Rect cell)
        //{
        //    var style=new GUIStyle();
        //    style.fontSize += 20;
        //    style.normal.textColor = Color.red;
        //    EditorGUI.LabelField(cell, "No Valid Event",style);
        //}
    }
}
