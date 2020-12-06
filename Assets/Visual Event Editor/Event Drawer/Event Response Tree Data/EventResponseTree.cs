using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace VisualEvents.Editor
{
    class EventResponseTree : TreeView
    {
        BaseEvent m_event;
        TreeViewItem draggedItem;
        HashSet<int> refrehsedpriorities;
        const float HEIGHT_PADDING = 10f;
        const string SUBSCRIBER_RESPONSE = "SubscriberResponse";
        static Type subscriberResponseType;
        static FieldInfo responseIndex_field = null;
        List<List<EventResponse>> allResponses;
        static EventResponseTree()
        {
            subscriberResponseType = subscriberResponseType = TypeCache.GetTypesDerivedFrom<EventResponse>().FirstOrDefault(t => t.Name == SUBSCRIBER_RESPONSE);
            if (subscriberResponseType != null)
            {
                var binding_flags = BindingFlags.Instance | BindingFlags.NonPublic;
                responseIndex_field = subscriberResponseType.GetField("responseIndex", binding_flags);
            }
        }
        public EventResponseTree(TreeViewState treeViewState, BaseEvent currenetevent) : base(treeViewState, GetEventCollumns())
        {
            extraSpaceBeforeIconAndLabel = 20;
            base.columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            m_event = currenetevent;
            refrehsedpriorities = new HashSet<int>();
            var flag = BindingFlags.NonPublic | BindingFlags.Instance;
            allResponses = m_event.GetType().GetField("m_EventResponses", flag).GetValue(m_event) as List<List<EventResponse>>;
            Reload();
        }
        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            if (args.draggedItem is PriorityTreeElement || args.draggedItem is GenericResponseElement)
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
                else if (draggedItem is GenericResponseElement draggedElement)
                {
                    var currentprioritylist = allResponses[(draggedElement.parent as PriorityTreeElement).Priority];
                    var draggedResponse = currentprioritylist[draggedElement.eventIndex];
                    //allResponses[(draggedElement.parent as PriorityTreeElement).Priority].RemoveAt(draggedElement.eventIndex);
                    if (args.parentItem is PriorityTreeElement priorityElement)
                    {
                        var response_count = allResponses[priorityElement.Priority].Count;
                        if (args.insertAtIndex == response_count || args.insertAtIndex == -1)
                        {
                            allResponses[priorityElement.Priority].Add(draggedResponse);
                            allResponses[draggedElement.priority].RemoveAt(draggedElement.eventIndex);
                        }
                        else if (args.insertAtIndex >= 0 && args.insertAtIndex < response_count)
                        {
                            allResponses[priorityElement.Priority].Insert(args.insertAtIndex, draggedResponse);

                            if (args.parentItem == draggedElement.parent && args.insertAtIndex < draggedElement.eventIndex) // inserted and removed inside the same priorty
                                allResponses[draggedElement.priority].RemoveAt(draggedElement.eventIndex + 1);
                            else allResponses[draggedElement.priority].RemoveAt(draggedElement.eventIndex);
                        }
                        UpdateResponsePriority(draggedElement, priorityElement.Priority);
                    }
                    else if (args.parentItem is GenericResponseElement otherResponseElement)
                    {
                        var otherResponseElementParent = otherResponseElement.parent as PriorityTreeElement;
                        allResponses[otherResponseElementParent.Priority].Add(draggedResponse);
                        allResponses[draggedElement.priority].RemoveAt(draggedElement.eventIndex);
                        UpdateResponsePriority(draggedElement, otherResponseElementParent.Priority);
                    }
                    else if (args.insertAtIndex == rootItem.children.Count)
                    {
                        Debug.Log("swap here");
                        allResponses.Add(new List<EventResponse>());
                        allResponses[args.insertAtIndex].Add(draggedResponse);
                        UpdateResponsePriority(draggedElement, args.insertAtIndex);
                    }
                    else if (args.parentItem == null)
                    {
                        Debug.Log("maybe droped in between");
                    }
                    Debug.Log(args.insertAtIndex);
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
            if (response.subscriberID != -1)
            {
                var priorityprop = new SerializedObject(EditorUtility.InstanceIDToObject(response.subscriberID)).FindProperty("responses")
                        .GetArrayElementAtIndex((int)responseIndex_field.GetValue(response)).FindPropertyRelative("priority");
                priorityprop.intValue = newpriority;
                priorityprop.serializedObject.ApplyModifiedProperties();
            }
        }
        private void UpdateResponsePriority(GenericResponseElement responseElement, int newpriority)
        {
            if (responseElement is ResponseTreeElement re && responseElement.SubscriberId != -1)
            {
                var priorityprop = re.serializedSender.FindProperty("responses")
                    .GetArrayElementAtIndex(re.subscriberIndex).FindPropertyRelative("priority");
                priorityprop.intValue = newpriority;
                priorityprop.serializedObject.ApplyModifiedProperties();
            }
        }
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            if (item is ResponseTreeElement responseElement)
            {
                var serialized_object = responseElement.serializedSender;
                return EditorGUI.GetPropertyHeight(serialized_object.FindProperty("responses").GetArrayElementAtIndex(responseElement.subscriberIndex)
                    .FindPropertyRelative("response")) + HEIGHT_PADDING;
            }
            else if (item is DynamicResponseTreeElement)
            {
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.String, GUIContent.none) * 2;
            }
            else
            {
                var row_height = base.GetCustomRowHeight(row, item);
                if (showingHorizontalScrollBar)
                    return row_height * 2f;
                else return row_height;
            }
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
                    if (currentresponse is RuntimeResponse rt&&EditorApplication.isPlaying)
                        priorityroot.AddChild(new DynamicResponseTreeElement(rt.response, rt.subscriberID, i, j) { id = counter });
                    else if (subscriberResponseType != null && currentresponse.GetType() == subscriberResponseType)
                    {
                        priorityroot.AddChild(new ResponseTreeElement(currentresponse.subscriberID, (int)responseIndex_field.GetValue(currentresponse), i, j) { id = counter });
                    }
                }
                root.AddChild(priorityroot);
            }
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                if (isValidElement(in args))
                    DrawTreeElement(i, in args);
                else
                {
                    Reload();
                    break;
                }
                //   DrawItem(args.GetCellRect(i), args.item as ResponseTreeElement, i);
                // args.rowRect = args.GetCellRect(i);
            }
            // base.RowGUI(args);
        }
        /// <summary>
        /// Ensures that the currently elements inside the tree are still valid representations of responses within <see cref="BaseEvent.m_EventResponses"/>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool isValidElement(in RowGUIArgs args)
        {
            if (args.item is GenericResponseElement genericElement)
            {
                if (genericElement.priority >= allResponses.Count)
                    return false;
                else if (genericElement.eventIndex >= allResponses[genericElement.priority].Count)
                    return false;
                else return true;
            }
            else return true;
        }
        private void DrawTreeElement(int coll, in RowGUIArgs args)
        {
            var cell = args.GetCellRect(coll);
            switch (coll)
            {
                case 0:
                    if (args.item is PriorityTreeElement priority_element)
                        DrawPrioirty(cell, priority_element);
                    break;
                case 1:
                    DrawSubscriberObject(cell, args.item);
                    break;
                case 2:
                    if (args.item is ResponseTreeElement)
                        DrawResponse(cell, args.item as ResponseTreeElement);
                    else if (args.item is DynamicResponseTreeElement dynamic_element)
                        DrawDynamicResponse(cell, dynamic_element);
                    break;
                case 3:
                    DrawResponseActivityStatus(cell, args.item);
                    break;
                case 4:
                    if (args.item is ResponseTreeElement response && response.noteContent != null)
                        DrawResponseNote(cell, response);
                    break;
                default:
                    break;
            }
        }
        private void DrawPrioirty(Rect cell, PriorityTreeElement priorityelement)
        {
            cell.x += 15f;
            cell.height = EditorStyles.label.CalcHeight(priorityelement.content, cell.width);
            EditorGUI.LabelField(cell, priorityelement.priorityString);
            if (!refrehsedpriorities.Contains(priorityelement.id) && IsExpanded(priorityelement.id))
            {
                refrehsedpriorities.Add(priorityelement.id);
                Reload();
                //RefreshCustomRowHeights();
            }
        }
        private void DrawSubscriberObject(Rect cellrect, TreeViewItem item)
        {
            if (item is PriorityTreeElement)
                return;

            UnityEngine.Object subscriber;
            if (item is ResponseTreeElement re)
                subscriber = re.Subscriber;
            else subscriber = (item as DynamicResponseTreeElement).Subscriber;

            cellrect.height = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);
            EditorGUI.ObjectField(cellrect, subscriber, typeof(UnityEngine.Object), true);
        }
        private void DrawResponse(Rect cellrect, ResponseTreeElement response_element)
        {
            var serialized_object = response_element.serializedSender;
            cellrect.x += 15f;
            cellrect.width -= 15f;
            EditorGUI.BeginChangeCheck();
            var responseprop = serialized_object.FindProperty("responses").GetArrayElementAtIndex(response_element.subscriberIndex);
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
            if (delegateproperty.isExpanded != response_element.isexpanded)
            {
                RefreshCustomRowHeights();
                response_element.isexpanded = delegateproperty.isExpanded;
            }
            GUI.enabled = true;
        }
        private void DrawDynamicResponse(Rect cell, DynamicResponseTreeElement response_element)
        {
                var targetrect = cell;
                targetrect.height *= .5f;
                EditorGUI.LabelField(targetrect, response_element.targetMessage);
                var methodrect = targetrect;
                methodrect.y += methodrect.height;
                EditorGUI.LabelField(methodrect, response_element.methodMessage);
        }
        private void DrawResponseActivityStatus(Rect cell, TreeViewItem item)
        {
            if (item is GenericResponseElement responseElement)
            {
                    EventResponse currentResponse = allResponses[responseElement.priority][responseElement.eventIndex];
                    cell.x += 10;
                    GUI.enabled = EditorApplication.isPlaying;
                    currentResponse.isActive = EditorGUI.Toggle(cell, currentResponse.isActive);
                    GUI.enabled = true;
            }
        }
        private void DrawResponseNote(Rect cell, ResponseTreeElement element)
        {
            cell.height -= HEIGHT_PADDING;
            var noteprop = element.serializedSender.FindProperty("responses").GetArrayElementAtIndex(element.subscriberIndex)
                 .FindPropertyRelative("responseNote");
            var customheight = EditorStyles.textArea.CalcHeight(element.noteContent, cell.width);
            EditorGUI.BeginChangeCheck();
            if (customheight > cell.height)
            {
                var textrect = cell;
                textrect.height = customheight;
                element.scroll = GUI.BeginScrollView(cell, element.scroll, textrect);
                noteprop.stringValue = EditorGUI.TextArea(textrect, noteprop.stringValue, EditorStyles.textArea);
                GUI.EndScrollView();
            }
            else
                noteprop.stringValue = EditorGUI.TextArea(cell, noteprop.stringValue, EditorStyles.textArea);
            if (EditorGUI.EndChangeCheck())
            {
                element.serializedSender.ApplyModifiedProperties();
                element.noteContent = new GUIContent(noteprop.stringValue);
            }
        }
        private static MultiColumnHeader GetEventCollumns()
        {
            GUIContent prioritycontent = new GUIContent("Priorities");
            GUIStyle.none.CalcMinMaxWidth(prioritycontent, out float min, out float max);
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
                           headerContent= new GUIContent("Active"),
                             width = 50,
                           minWidth = 50,
                           maxWidth = 50,
                           autoResize = true,
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
    }
}
