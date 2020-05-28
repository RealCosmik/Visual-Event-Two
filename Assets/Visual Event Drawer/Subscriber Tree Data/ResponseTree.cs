using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace VisualDelegates.Events.Editor
{
    class ResponseTree : TreeView
    {
        BaseEvent m_event;
        TreeViewItem draggedItem;
        public ResponseTree(TreeViewState treeViewState, MultiColumnHeader header, BaseEvent currenetevent) : base(treeViewState, header)
        {
            extraSpaceBeforeIconAndLabel = 20;
            base.columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            m_event = currenetevent;
            this.useScrollView = true;
            Reload();
        }
        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            if (args.draggedItem is PriorityTreeElement || args.draggedItem is ResponseTreeElement)
            {
                draggedItem = args.draggedItem;
                return true;
            }
            else return false;
        }
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            Debug.Log(args.insertAtIndex);
            Debug.Log(args.parentItem);
            Debug.Log(rootItem.children.Count);
            if (isDragging && args.performDrop)
            {
                if (args.parentItem is PriorityTreeElement selectedpriorityelement && draggedItem is PriorityTreeElement draggedpriorityelement)
                {
                    var dragged_responses = m_event.AllResponses[draggedpriorityelement.Priority];
                    var selected_responses = m_event.AllResponses[selectedpriorityelement.Priority];
                    m_event.AllResponses[selectedpriorityelement.Priority] = dragged_responses;
                    SwapResponsesToNewPriority(dragged_responses, selectedpriorityelement.Priority);
                    m_event.AllResponses[draggedpriorityelement.Priority] = selected_responses;
                    SwapResponsesToNewPriority(selected_responses, draggedpriorityelement.Priority);
                    Debug.Log("SWAP");
                }
                else if (draggedItem is PriorityTreeElement draggedpriority && args.parentItem.id == -1 &&
                    args.insertAtIndex >= 0 && args.insertAtIndex < m_event.AllResponses.Count)
                {
                    var dragged_responses = m_event.AllResponses[draggedpriority.Priority];
                    var selected_resposnes = m_event.AllResponses[args.insertAtIndex];
                    m_event.AllResponses[args.insertAtIndex] = dragged_responses;
                    m_event.AllResponses[draggedpriority.Priority] = selected_resposnes;
                    SwapResponsesToNewPriority(dragged_responses, args.insertAtIndex);
                    SwapResponsesToNewPriority(selected_resposnes, draggedpriority.Priority);
                }
                else if (draggedItem is ResponseTreeElement responseElement)
                {
                    var response = m_event.AllResponses[(responseElement.parent as PriorityTreeElement).Priority][responseElement.eventindex];
                    m_event.AllResponses[(responseElement.parent as PriorityTreeElement).Priority].RemoveAt(responseElement.eventindex);
                    if (args.parentItem is PriorityTreeElement priorityElement)
                    { 
                        m_event.AllResponses[priorityElement.Priority].Add(response);
                        UpdateResponsePriority(responseElement, priorityElement.Priority);
                    }
                    else if (args.parentItem is ResponseTreeElement otherResponseElement)
                    {
                        var otherResponseElementParent = otherResponseElement.parent as PriorityTreeElement;
                        m_event.AllResponses[otherResponseElementParent.Priority].Add(response);
                        UpdateResponsePriority(responseElement, otherResponseElementParent.Priority);
                    }
                    else if (args.insertAtIndex==rootItem.children.Count)
                    {
                        Debug.Log("OH YEAH");
                        m_event.AllResponses.Add(new List<EventResponse>());
                        m_event.AllResponses[args.insertAtIndex].Add(response);
                        UpdateResponsePriority(responseElement, args.insertAtIndex);
                    }  
                }
                Debug.Log(m_event.AllResponses.Count);
                Reload();
            }
            return DragAndDropVisualMode.Move;
        }
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.StartDrag("TreeDrag");
            DragAndDrop.SetGenericData("TreeDrag", draggedItem);
        }
        private void SwapResponsesToNewPriority(List<EventResponse> respones, int newpriority)
        {
            for (int i = 0; i < respones.Count; i++)
                MoveSingleResponse(respones[i], newpriority);
        }
        private void MoveSingleResponse(EventResponse response, int newpriority)
        {
            var priorityprop = new SerializedObject(EditorUtility.InstanceIDToObject(response.senderID)).FindProperty("responses")
                    .GetArrayElementAtIndex(response.responseIndex).FindPropertyRelative("priority");
            priorityprop.intValue = newpriority;
            priorityprop.serializedObject.ApplyModifiedProperties();
        }
        private void UpdateResponsePriority(ResponseTreeElement responseElement, int newpriority)
        {
            var priorityprop = new SerializedObject(EditorUtility.InstanceIDToObject(responseElement.responderID)).FindProperty("responses")
                .GetArrayElementAtIndex(responseElement.responseindex).FindPropertyRelative("priority");
            priorityprop.intValue = newpriority;
            priorityprop.serializedObject.ApplyModifiedProperties();

        }
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            if (item is ResponseTreeElement responseElement)
            {
                Debug.Log(responseElement.responseindex);
                var serialized_object = new SerializedObject(UnityEditor.EditorUtility.InstanceIDToObject(responseElement.responderID));
                return EditorGUI.GetPropertyHeight(serialized_object.FindProperty("responses").GetArrayElementAtIndex(responseElement.responseindex));
            }
            else return base.GetCustomRowHeight(row, item);
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
            int counter = 0;
            int priorities = m_event.AllResponses.Count;
            for (int i = 0; i < priorities; i++)
            {
                counter++;
                var priorityroot = new PriorityTreeElement(i) { displayName = $"priority {i}", id = counter };
                for (int j = 0; j < m_event.AllResponses[i].Count; j++)
                {
                    counter++;
                    var currentresponse = m_event.AllResponses[i][j];
                    priorityroot.AddChild(new ResponseTreeElement(currentresponse.senderID, currentresponse.responseIndex, j) { id = counter });
                }
                root.AddChild(priorityroot);
            }
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                DrawTreeElement(i, ref args);
                //   DrawItem(args.GetCellRect(i), args.item as ResponseTreeElement, i);
                // args.rowRect = args.GetCellRect(i);
            }
            // base.RowGUI(args);
        }
        private void DrawTreeElement(int coll, ref RowGUIArgs args)
        {
            switch (coll)
            {
                case 0:
                    if (args.item is PriorityTreeElement)
                        DrawPrioirty(args.GetCellRect(0), args.item as PriorityTreeElement);
                    break;
                case 1:
                    if (args.item is ResponseTreeElement)
                        DrawSubscriberGO(args.GetCellRect(1), args.item as ResponseTreeElement);
                    break;
                case 2:
                    if (args.item is ResponseTreeElement)
                        DrawResponse(args.GetCellRect(2), args.item as ResponseTreeElement);
                    break;
                default:
                    break;
            }
        }
        private void DrawPrioirty(Rect cell, PriorityTreeElement priorityelement)
        {
            cell.x += 15f;
            EditorGUI.LabelField(cell, priorityelement.Priority.ToString());
        }
        private void DrawSubscriberGO(Rect cellrect, ResponseTreeElement response_element)
        {
            cellrect.height = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);
            var response_object = (UnityEditor.EditorUtility.InstanceIDToObject(response_element.responderID) as MonoBehaviour).gameObject;
            GUI.enabled = false;
            EditorGUI.ObjectField(cellrect, response_object, typeof(UnityEngine.Object), true);
            GUI.enabled = true;
        }
        private void DrawResponse(Rect cellrect, ResponseTreeElement response_element)
        {
            var serialized_object = new SerializedObject(UnityEditor.EditorUtility.InstanceIDToObject(response_element.responderID));
            EditorGUI.GetPropertyHeight(serialized_object.FindProperty("responses").GetArrayElementAtIndex(response_element.responseindex).FindPropertyRelative("response"));
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(cellrect, serialized_object.FindProperty("responses").GetArrayElementAtIndex(response_element.responseindex).FindPropertyRelative("response"));
            if (EditorGUI.EndChangeCheck())
            {
                this.RefreshCustomRowHeights();
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    serialized_object.ApplyModifiedProperties();
            }
        }
    }
}
