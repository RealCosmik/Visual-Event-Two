using UnityEditor.IMGUI.Controls;
namespace VisualDelegates.Events.Editor
{
    [System.Serializable]
    class ResponseTreeElement : TreeViewItem
    {
        public BaseEvent CurrentEvent;
        public bool iscollapsed;
        public ResponseTreeElement(BaseEvent baseEvent) => CurrentEvent = baseEvent;
    } 


}