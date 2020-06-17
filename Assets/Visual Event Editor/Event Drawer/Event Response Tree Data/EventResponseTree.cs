using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VisualDelegates.Editor;

namespace VisualDelegates.Events.Editor
{
    class EventResponseTree : TreeView
    {
        BaseEvent m_event;
        TreeViewItem draggedItem;
        HashSet<int> refrehsedpriorities;
        const float  HEIGHT_PADDING = 10f; 
        public EventResponseTree(TreeViewState treeViewState, MultiColumnHeader header, BaseEvent currenetevent) : base(treeViewState, header)
        {
            extraSpaceBeforeIconAndLabel = 20;
            base.columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            m_event = currenetevent;
            refrehsedpriorities = new HashSet<int>();
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
            if (isDragging && args.performDrop)
            {
                var flag = BindingFlags.NonPublic | BindingFlags.Instance;
                var allResponses = m_event.GetType().GetField("m_EventResponses", flag).GetValue(m_event) as List<List<EventResponse>>;
                if (args.parentItem is PriorityTreeElement selectedpriorityelement && draggedItem is PriorityTreeElement draggedpriorityelement)
                {
                    var dragged_responses = allResponses[draggedpriorityelement.Priority];
                    var selected_responses = allResponses[selectedpriorityelement.Priority];
                    allResponses[selectedpriorityelement.Priority] = dragged_responses;
                    SwapResponsesToNewPriority(dragged_responses, selectedpriorityelement.Priority);
                    allResponses[draggedpriorityelement.Priority] = selected_responses;
                    SwapResponsesToNewPriority(selected_responses, draggedpriorityelement.Priority);
                    Debug.Log("SWAP");
                }
                else if (draggedItem is PriorityTreeElement draggedpriority && args.parentItem.id == -1 &&
                    args.insertAtIndex >= 0 && args.insertAtIndex < allResponses.Count)
                {
                    var dragged_responses = allResponses[draggedpriority.Priority];
                    var selected_resposnes = allResponses[args.insertAtIndex];
                    allResponses[args.insertAtIndex] = dragged_responses;
                    allResponses[draggedpriority.Priority] = selected_resposnes;
                    SwapResponsesToNewPriority(dragged_responses, args.insertAtIndex);
                    SwapResponsesToNewPriority(selected_resposnes, draggedpriority.Priority);
                }
                else if (draggedItem is ResponseTreeElement responseElement)
                {
                    var response = allResponses[(responseElement.parent as PriorityTreeElement).Priority][responseElement.eventindex];
                    allResponses[(responseElement.parent as PriorityTreeElement).Priority].RemoveAt(responseElement.eventindex);
                    if (args.parentItem is PriorityTreeElement priorityElement)
                    {
                        allResponses[priorityElement.Priority].Add(response);
                        UpdateResponsePriority(responseElement, priorityElement.Priority);
                    }
                    else if (args.parentItem is ResponseTreeElement otherResponseElement)
                    {
                        var otherResponseElementParent = otherResponseElement.parent as PriorityTreeElement;
                        allResponses[otherResponseElementParent.Priority].Add(response);
                        UpdateResponsePriority(responseElement, otherResponseElementParent.Priority);
                    }
                    else if (args.insertAtIndex == rootItem.children.Count)
                    {
                        allResponses.Add(new List<EventResponse>());
                        allResponses[args.insertAtIndex].Add(response);
                        UpdateResponsePriority(responseElement, args.insertAtIndex);
                    }
                }
              //  Debug.Log(m_event.AllResponses.Count);
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
            if (response.senderID != -1)
            {
                var priorityprop = new SerializedObject(EditorUtility.InstanceIDToObject(response.senderID)).FindProperty("responses")
                        .GetArrayElementAtIndex(response.responseIndex).FindPropertyRelative("priority");
                priorityprop.intValue = newpriority;
                priorityprop.serializedObject.ApplyModifiedProperties();
            }
        }
        private void UpdateResponsePriority(ResponseTreeElement responseElement, int newpriority)
        {
            if (responseElement.responderID != -1)
            {
                var priorityprop = responseElement.serializedSender.FindProperty("responses")
                    .GetArrayElementAtIndex(responseElement.responseindex).FindPropertyRelative("priority");
                priorityprop.intValue = newpriority;
                priorityprop.serializedObject.ApplyModifiedProperties();
            } 
        }
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            if (item is ResponseTreeElement responseElement)
            {
                var serialized_object = responseElement.serializedSender;
                return EditorGUI.GetPropertyHeight(serialized_object.FindProperty("responses").GetArrayElementAtIndex(responseElement.responseindex)
                    .FindPropertyRelative("response")) + HEIGHT_PADDING;
            }
            else if(item is DynamicResponseTreeElement)
            {
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.String,GUIContent.none) * 2;
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
            int priorities = m_event.EventResponses.Count;
            for (int i = 0; i < priorities; i++)
            {
                counter++;
                var priorityroot = new PriorityTreeElement(i) { displayName = $"priority {i}", id = counter };
                for (int j = 0; j < m_event.EventResponses[i].Count; j++)
                {
                    counter++;
                    var currentresponse = m_event.EventResponses[i][j];
                    if (currentresponse.senderID != -1)
                        priorityroot.AddChild(new ResponseTreeElement(currentresponse.senderID, currentresponse.responseIndex, i, j) { id = counter });
                    else
                        priorityroot.AddChild(new DynamicResponseTreeElement(i, j) { id = counter });
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
            var cell = args.GetCellRect(coll);
            switch (coll)
            {
                case 0:
                    if (args.item is PriorityTreeElement)
                        DrawPrioirty(cell, args.item as PriorityTreeElement);
                    break;
                case 1:
                    if (args.item is ResponseTreeElement)
                        DrawSubscriberGO(cell, args.item as ResponseTreeElement);
                    else if(args.item is DynamicResponseTreeElement) 
                        EditorGUI.LabelField(cell, "N/A");
                    break;
                case 2:
                    if (args.item is ResponseTreeElement)
                        DrawResponse(cell, args.item as ResponseTreeElement);
                    else if (args.item is DynamicResponseTreeElement)
                        DrawDynamicResponse(cell, args.item as DynamicResponseTreeElement);
                    break;
                case 3:
                    if (args.item is ResponseTreeElement)
                        DrawResponseNote(cell, args.item as ResponseTreeElement);
                    break;
                default:
                    break;
            }
        }
        private void DrawPrioirty(Rect cell, PriorityTreeElement priorityelement)
        {
            cell.x += 15f;
            EditorGUI.LabelField(cell, priorityelement.Priority.ToString());
            if (!refrehsedpriorities.Contains(priorityelement.id) && IsExpanded(priorityelement.id))
            {   
                refrehsedpriorities.Add(priorityelement.id);
                Reload();
                //RefreshCustomRowHeights();
            }
        }
        private void DrawSubscriberGO(Rect cellrect, ResponseTreeElement response_element)
        {
            if (response_element.responderID != -1)
            {
                cellrect.height = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);
                var response_object =(response_element.sender as MonoBehaviour).gameObject;
                GUI.enabled = false;
                EditorGUI.ObjectField(cellrect, response_object, typeof(UnityEngine.Object), true);
                GUI.enabled = true;
            }
            else
            {
                var style = EditorStyles.label;
                style.fontSize -= 3;
                var runtime = m_event.EventResponses[response_element.priority][response_element.eventindex].CurrentResponse.Calls[0] as RawRuntimeCall;
                EditorGUI.LabelField(cellrect, VisualEditorUtility.ParseDynamicTargetName(runtime.delegateInstance.Target.GetType().FullName),
                    style);
            }
        }
        private void DrawResponse(Rect cellrect, ResponseTreeElement response_element)
        {
            var serialized_object = response_element.serializedSender;
            cellrect.x += 15f; 
            cellrect.width -= 15f;
            EditorGUI.BeginChangeCheck();
            var responseprop = serialized_object.FindProperty("responses").GetArrayElementAtIndex(response_element.responseindex);
            var delegateproperty = responseprop.FindPropertyRelative("response");
            GUI.enabled = responseprop.FindPropertyRelative("isActive").boolValue;
            EditorGUI.PropertyField(cellrect, delegateproperty);
            GUI.enabled = true;
            if (EditorGUI.EndChangeCheck())
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    serialized_object.ApplyModifiedProperties();
                }
                else
                {
                    // ViewCache.GetVisualDelegateInstanceCache(delegateproperty).UpdateInterncalcall(delegateproperty);
                }
                Reload();
            }
        }
        private void DrawDynamicResponse(Rect cell, DynamicResponseTreeElement response_element)
        {
            try
            {
                var runtimecall = m_event.EventResponses[response_element.Priority][response_element.EventIndex].CurrentResponse.Calls[0] as RawRuntimeCall;
                if (response_element.targetMessage == null)
                    response_element.targetMessage =
                VisualEditorUtility.ParseDynamicTargetName(runtimecall.delegateInstance.Target.GetType().FullName);
                if (response_element.methodMessage == null)
                    response_element.methodMessage =
                VisualEditorUtility.ParseDynamicMethodName(runtimecall.delegateInstance.Method.Name);
                var targetrect = cell;
                targetrect.height *= .5f;
                EditorGUI.LabelField(targetrect, response_element.targetMessage);
                var methodrect = targetrect;
                methodrect.y += methodrect.height;
                EditorGUI.LabelField(methodrect, response_element.methodMessage);
            }
            catch (ArgumentOutOfRangeException)
            {
                Reload();
            } 
        }
        private void DrawResponseNote(Rect cell, ResponseTreeElement element)
        {
            cell.height -= HEIGHT_PADDING;
            var noteprop = element.serializedSender.FindProperty("responses").GetArrayElementAtIndex(element.responseindex)
                 .FindPropertyRelative("responseNote");
            var customheight = EditorStyles.textArea.CalcHeight(element.noteContent, cell.width);
            EditorGUI.BeginChangeCheck();
            if (customheight > cell.height)
            {
                var textrect = cell;
                textrect.height = customheight;
                element.scroll = GUI.BeginScrollView(cell, element.scroll, textrect);
                noteprop.stringValue = EditorGUI.TextArea(textrect, noteprop.stringValue,EditorStyles.textArea);
                GUI.EndScrollView();
            }
            else
                noteprop.stringValue = EditorGUI.TextArea(cell, noteprop.stringValue,EditorStyles.textArea);
            if (EditorGUI.EndChangeCheck())
            {
                element.serializedSender.ApplyModifiedProperties();
                element.noteContent = new GUIContent(noteprop.stringValue);
            }
        }
    }
}
