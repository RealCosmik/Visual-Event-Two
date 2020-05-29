using UnityEditor.IMGUI.Controls;
namespace VisualDelegates.Events.Editor
{
    class ResponseTreeElement : TreeViewItem
    {
        public int responseIndex;
        public bool iscollapsed;
        public bool iscached;
        public ResponseTreeElement(int index) => responseIndex = index;
    }


}