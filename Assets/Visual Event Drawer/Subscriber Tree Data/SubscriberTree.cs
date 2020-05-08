using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace VisualDelegates.Events.Editor
{
    class SubscriberTree : TreeView
    {
        SerializedProperty all_events;
        SerializedProperty all_responses;
        SerializedProperty priorities;
        SerializedObject serialobj;
        public SubscriberTree(TreeViewState treeViewState, MultiColumnHeader header, SerializedObject subscriberobject) : base(treeViewState, header)
        {
            Debug.LogWarning("building root");
            serialobj = subscriberobject;
            subscriberobject.Update();
            extraSpaceBeforeIconAndLabel = 20;
            base.columnIndexForTreeFoldouts = 0;
            showBorder = true;
            all_events = subscriberobject.FindProperty("base_events");
            all_responses = subscriberobject.FindProperty("responses");
            priorities = subscriberobject.FindProperty("priorities");
            Reload();
        }
        public void refresh() => RefreshCustomRowHeights();
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {   
            Debug.LogWarning("get custom row height");
            float customheight = 0;
            if (row <= all_responses.arraySize - 1)
            {
                Debug.Log("retrieving prop height from treee");
                var response = all_responses.GetArrayElementAtIndex(row);
                 customheight += EditorGUI.GetPropertyHeight(response, GUIContent.none) + 60f;
                Debug.Log(customheight);
            }
            else
            {
                customheight = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none)+40;
            }
            return customheight;
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            BuildEventResponses(root);
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }
        private void BuildEventResponses(TreeViewItem root)
        {
            int length = all_events.arraySize;
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    root.AddChild(new SubscriberTreeElement { id = i,displayName=null });
                }
            }
            else root.AddChild(new SubscriberTreeElement { id = 0,displayName=null });
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                DrawItem(args.GetCellRect(i), args.item as SubscriberTreeElement, i);
                args.rowRect = args.GetCellRect(i);
            }
            base.RowGUI(args);
        }
        private void DrawItem(Rect cellrect, SubscriberTreeElement item, int collumn)
        {
            if (item.id <= all_events.arraySize - 1)
            {
                switch (collumn)
                {
                    case 0:
                        EditorGUI.BeginChangeCheck();
                        cellrect.height = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference,GUIContent.none);
                        EditorGUI.PropertyField(cellrect, all_events.GetArrayElementAtIndex(item.id), GUIContent.none);
                        if (EditorGUI.EndChangeCheck())
                            OnEventFieldChange(item);
                        break;
                    case 1:
                        if (item.id<=all_responses.arraySize-1)
                        {
                            //cellrect.y -= 20f;
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.PropertyField(cellrect, all_responses.GetArrayElementAtIndex(item.id), GUIContent.none);
                            if (EditorGUI.EndChangeCheck())
                            {
                                RefreshCustomRowHeights();
                                all_responses.serializedObject.ApplyModifiedProperties();
                                RefreshCustomRowHeights();
                            }
                        }
                        break;
                    case 2:
                        cellrect.height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Integer, GUIContent.none);
                        if (item.id <= priorities.arraySize - 1)
                            EditorGUI.PropertyField(cellrect, priorities.GetArrayElementAtIndex(item.id), GUIContent.none);
                        break;
                    default:
                        break;
                }
            }
        }
        private void OnEventFieldChange(SubscriberTreeElement CurrentElement)
        {
            ScriptableObject event_object = all_events.GetArrayElementAtIndex(CurrentElement.id).objectReferenceValue as ScriptableObject;
            CurrentElement.hasvalidEvent = event_object != null;
            if (event_object != null)
            {
                Debug.Log(all_responses == null);
                int size = all_responses.arraySize;
                all_responses.InsertArrayElementAtIndex(size);
                priorities.InsertArrayElementAtIndex(size);
                all_responses.GetArrayElementAtIndex(size).managedReferenceValue = new randomdel();
            }
            else Debug.Log("removed");
            all_events.serializedObject.ApplyModifiedProperties();
            this.RefreshCustomRowHeights();
        }
        private void OnResponseChanged()
        {

        }
    }
}
