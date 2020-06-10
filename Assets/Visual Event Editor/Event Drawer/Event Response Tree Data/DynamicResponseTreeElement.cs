using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace VisualDelegates.Events.Editor
{
    class DynamicResponseTreeElement : TreeViewItem
    {
        public int Priority { get; private set; }
        public int EventIndex { get; private set; }
        public string methodMessage;
        public string targetMessage;
        public DynamicResponseTreeElement(int priority,int eventindex)
        {
            Priority = priority;
            EventIndex = eventindex;
        }
    }
}