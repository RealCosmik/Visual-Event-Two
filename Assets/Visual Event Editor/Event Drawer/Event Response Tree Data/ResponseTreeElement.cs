using UnityEngine;
using UnityEditor;

namespace VisualEvents.Editor
{
    [System.Serializable]
    class ResponseTreeElement : GenericResponseElement
    {
        /// <summary>
        /// index of this response on the subscriber GO
        /// </summary>
        public int subscriberIndex;
        public readonly SerializedObject serializedSender;
        public GUIContent noteContent;
        public Vector2 scroll;
        public bool isexpanded;
        public ResponseTreeElement(int id, int subscriberIndex, int newpriority, int newEventIndex) : base(id, newpriority, newEventIndex)
        {
            this.subscriberIndex = subscriberIndex;
            priority = newpriority;
            serializedSender = new SerializedObject(Subscriber);
            var noteprop = serializedSender.FindProperty("responses")?.GetArrayElementAtIndex(this.subscriberIndex)?.FindPropertyRelative("responseNote");
            if (noteprop != null)
            {
                noteContent = new GUIContent(noteprop.stringValue);
            }
        }
    }
}

