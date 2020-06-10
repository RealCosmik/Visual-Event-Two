using UnityEditor.IMGUI.Controls;
namespace VisualDelegates.Events.Editor
{
    /// <summary>
    /// Element used to represent priorites in a <see cref="EventResponseTree"/>
    /// </summary>
    [System.Serializable]
    class PriorityTreeElement : TreeViewItem
    {
        public int Priority;
        public bool expanisonrefresh;
        public PriorityTreeElement(int newpriority)
        {
            Priority = newpriority;
        }
    }
}
