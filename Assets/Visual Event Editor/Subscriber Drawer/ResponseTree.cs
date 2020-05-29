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
        public ResponseTree(TreeViewState state, MultiColumnHeader header, SerializedObject newSerializedSubscriber) : base(state, header)
        {
            serializedSubscriber = newSerializedSubscriber;
            extraSpaceBeforeIconAndLabel = 20;
            base.columnIndexForTreeFoldouts = 0;
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
                root.AddChild(new ResponseTreeElement(i) { id = i, iscollapsed = subscriberResponses.GetArrayElementAtIndex(i).isExpanded });
        }
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            var responseElement = item as ResponseTreeElement;
            var eventResponseProperty = serializedSubscriber.FindProperty("responses").GetArrayElementAtIndex(responseElement.responseIndex);

            var subscribedEvent = eventResponseProperty.FindPropertyRelative("currentEvent").objectReferenceValue;
            if (subscribedEvent == null)
            {
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none) +
                    EditorGUI.GetPropertyHeight(SerializedPropertyType.Integer, GUIContent.none);
            }
            else
            {
                var x = EditorGUI.GetPropertyHeight(eventResponseProperty.FindPropertyRelative("response"), GUIContent.none);
                Debug.Log(x);
                return x;

            }
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var visiable_colls = args.GetNumVisibleColumns();
            for (int i = 0; i < visiable_colls; i++)
            {
                if (args.item is ResponseTreeElement responseElement)
                    DrawResponse(i, ref args);
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
            var event_rect = cell;
            event_rect.height = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);
            EditorGUI.PropertyField(event_rect, currentEventProperty, GUIContent.none);

            var priorityrect = event_rect;
            priorityrect.y += priorityrect.height;
            priorityrect.width = cell.width / 3;
            EditorGUI.IntField(priorityrect, 2);

            var increaseRect = priorityrect;
            increaseRect.x += priorityrect.width;
            increaseRect.width = cell.width / 3;
            GUI.Button(increaseRect, UP_ARROW);

            var decreaseRect = increaseRect;
            decreaseRect.x += increaseRect.width;
            GUI.Button(decreaseRect, DOWN_ARROW);
        }
        private void DrawDelegate(Rect cell, ResponseTreeElement element)
        {
            var currentDelegate = serializedSubscriber.FindProperty("responses").GetArrayElementAtIndex(element.responseIndex)
                .FindPropertyRelative("response");
            EditorGUI.GetPropertyHeight(currentDelegate);
            EditorGUI.BeginChangeCheck();
            cell.x += 15f;
            cell.width -= 15f;
            EditorGUI.PropertyField(cell, currentDelegate);
            if (EditorGUI.EndChangeCheck())
            {
                currentDelegate.serializedObject.ApplyModifiedProperties();
                RefreshCustomRowHeights();
            }
            if (element.iscollapsed != currentDelegate.isExpanded)
            {
                element.iscollapsed = currentDelegate.isExpanded;
                currentDelegate.serializedObject.ApplyModifiedProperties();
                RefreshCustomRowHeights();
            }
        }
    }
}
