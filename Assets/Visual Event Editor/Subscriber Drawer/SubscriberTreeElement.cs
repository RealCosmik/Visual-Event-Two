using UnityEditor.IMGUI.Controls;
namespace VisualDelegates.Events.Editor
{
    [System.Serializable]
    class SubscriberTreeElement : TreeViewItem
    {
        public BaseEvent CurrentEvent;
        public bool iscollapsed;
        public SubscriberTreeElement(BaseEvent baseEvent) => CurrentEvent = baseEvent;
    } 


}