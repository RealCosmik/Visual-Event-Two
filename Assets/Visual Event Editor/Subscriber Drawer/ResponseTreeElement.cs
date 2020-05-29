using UnityEditor.IMGUI.Controls;
namespace VisualDelegates.Events.Editor
{
    [System.Serializable]
    class ResponseTreeElement : TreeViewItem
    {
        public int responseIndex;
        public BaseEvent CurrentEvent;
        public ResponseTreeElement(int index) => responseIndex = index;
    } 


}