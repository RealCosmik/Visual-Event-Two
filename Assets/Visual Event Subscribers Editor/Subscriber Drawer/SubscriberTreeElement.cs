using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace VisualEvents.VisualSubscribers.Editor
{
    [System.Serializable]
    class SubscriberTreeElement : TreeViewItem
    {
        public BaseEvent CurrentEvent;
        public bool iscollapsed;
        public Vector2 scroll;
        public GUIContent responseNote;
        public SubscriberTreeElement(BaseEvent baseEvent)
        {
            CurrentEvent = baseEvent;
        } 
    } 


}