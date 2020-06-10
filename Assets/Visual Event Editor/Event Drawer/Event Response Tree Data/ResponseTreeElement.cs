using UnityEditor.IMGUI.Controls;
using VisualDelegates;
using UnityEngine;
using UnityEditor;

namespace VisualDelegates.Events.Editor
{
    [System.Serializable]
    class ResponseTreeElement : TreeViewItem
    {
        public int responderID;
        /// <summary>
        /// index of this response on the subscriber GO
        /// </summary>
        public int responseindex;
        /// <summary>
        /// the index of this response within the event
        /// </summary>
        public int eventindex;
        /// <summary>
        /// the priority index of this response
        /// </summary>
        public int priority;
        public readonly SerializedObject serializedSender;
        public readonly UnityEngine.Object sender;
        public GUIContent noteContent;
        public Vector2 scroll;
        public ResponseTreeElement(int id,int newindex, int newpriority, int event_index)
        {
            responderID = id;
            responseindex = newindex;
            eventindex = event_index;
            priority = newpriority;
            sender = EditorUtility.InstanceIDToObject(responderID);
            serializedSender = new SerializedObject(sender);
            noteContent = new GUIContent(serializedSender.FindProperty("responses").GetArrayElementAtIndex(responseindex).FindPropertyRelative("responseNote")
                .stringValue);
        }
    }
}

