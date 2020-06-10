using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace VisualDelegates.Events.Editor
{
    [System.Serializable]
    class SubscriberTreeElement : TreeViewItem
    {
        public BaseEvent CurrentEvent;
        public bool iscollapsed;
        public Vector2 scroll;
        public GUIContent responseNote;
        public SubscriberTreeElement(BaseEvent baseEvent,string note)
        {
            CurrentEvent = baseEvent;
            responseNote = new GUIContent(note);
        } 
    } 


}