using UnityEditor.IMGUI.Controls;
using VisualDelegates;
using UnityEngine;
namespace VisualDelegates.Events.Editor
{
    [System.Serializable]
    class SubscriberTreeElement : TreeViewItem
    {
        public ScriptableObject currentEvent;
        public bool hasvalidEvent;
        public SubscriberTreeElement()
        {

        }
    }
}

