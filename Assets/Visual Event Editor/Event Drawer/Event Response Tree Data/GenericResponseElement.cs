using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
namespace VisualEvents.Editor
{
    class GenericResponseElement : TreeViewItem
    {
        public readonly int SubscriberId;
        /// <summary>
        /// the index of this response within the event
        /// </summary>
        public int eventIndex;
        /// <summary>
        /// the priority index of this response
        /// </summary>
        public int priority;
        public readonly UnityEngine.Object Subscriber;
        public GenericResponseElement(int subscriberid, int newpriority, int neweventIndex)
        {
            this.eventIndex = neweventIndex;
            this.priority = newpriority;
            this.SubscriberId = subscriberid;
            Subscriber = EditorUtility.InstanceIDToObject(subscriberid);

        }
    }
}
