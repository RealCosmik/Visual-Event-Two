using UnityEditor.IMGUI.Controls;
namespace VisualDelegates.Events.Editor
{
    [System.Serializable]
    class PriorityTreeElement : TreeViewItem
    {
        public int Priority;
        public PriorityTreeElement(int newpriority)
        {
            Priority = newpriority;
        }
    }
}
