using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace VisualDelegates.Events.Editor
{
    /// <summary>
    /// Element used to represent priorites in a <see cref="EventResponseTree"/>
    /// </summary>
    [System.Serializable]
    class PriorityTreeElement : TreeViewItem
    {
        public int Priority { get; private set; }
        public GUIContent content { get; private set; }
        public string priorityString { get; private set; }
        public bool expanisonrefresh;
        public PriorityTreeElement(int newpriority)
        {
            Priority = newpriority;
            priorityString = newpriority.ToString();
            content = new GUIContent(priorityString);
        }
    }
}
