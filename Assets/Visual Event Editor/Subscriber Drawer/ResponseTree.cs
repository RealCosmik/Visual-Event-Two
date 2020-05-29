using System.Collections.Generic;
using System.Text;
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
        int reloadcounter = 0;
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
                root.AddChild(new ResponseTreeElement(i) { id = i });
        }
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            var responseElement = item as ResponseTreeElement;
            var eventResponseProperty = serializedSubscriber.FindProperty("responses").GetArrayElementAtIndex(responseElement.responseIndex);
            ViewCache.GetVisualDelegateInstanceCache(eventResponseProperty.FindPropertyRelative("response"));
            var x = EditorGUI.GetPropertyHeight(eventResponseProperty.FindPropertyRelative("response"));
            return x;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var visiable_colls = args.GetNumVisibleColumns();
            for (int i = 0; i < visiable_colls; i++)
            {
                if (args.item is ResponseTreeElement responseElement)
                    DrawResponse(i, ref args);
            }
            if (reloadcounter != 2)
            {
                reloadcounter++;
                if (reloadcounter == 2)
                    RefreshCustomRowHeights();
            }
        }
        private void DrawResponse(int collumn, ref RowGUIArgs rowarg)
        {
            switch (collumn)
            {
                case 0:
                    DrawEventAndPriority(rowarg.GetCellRect(0), rowarg.item as ResponseTreeElement);
                    break;
                case 1:
                    DrawDelegate(rowarg.GetCellRect(1), rowarg.item as ResponseTreeElement);
                    break;
                default:
                    break;
            }
        }
        private void DrawEventAndPriority(Rect cell, ResponseTreeElement element)
        {
            var currentResponse = serializedSubscriber.FindProperty("responses").GetArrayElementAtIndex(element.responseIndex);
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
        private void OnSubscribe(BaseEvent subscribedEvent, ResponseTreeElement element)
        {
            if (subscribedEvent != null)
            {
                var binding = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                var responses = serializedSubscriber.targetObject.GetType().GetField("responses", binding)
                    .GetValue(serializedSubscriber.targetObject) as List<EventResponse>;
                subscribedEvent.Subscribe(responses[element.responseIndex], responses[element.responseIndex].priority);
                serializedSubscriber.ApplyModifiedProperties();
            }
        }
        private void DrawDelegate(Rect cell, ResponseTreeElement element)
        {
            var currentDelegate = serializedSubscriber.FindProperty("responses").GetArrayElementAtIndex(element.responseIndex)
                .FindPropertyRelative("response");

            EditorGUI.BeginChangeCheck();
            cell.x += 15f;
            cell.width -= 15f;
            EditorGUI.PropertyField(cell, currentDelegate);

            if (EditorGUI.EndChangeCheck())
            {
                reloadcounter = 0;
                currentDelegate.serializedObject.ApplyModifiedProperties();
            }
            // RefreshCustomRowHeights();
        }
    }
}
