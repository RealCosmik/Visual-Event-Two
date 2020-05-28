using UnityEditor.IMGUI.Controls;
using VisualDelegates;
using UnityEngine;
namespace VisualDelegates.Events.Editor
{
    [System.Serializable]
    class ResponseTreeElement : TreeViewItem
    {
        public int responderID,responseindex,eventindex;
        public ResponseTreeElement(int id,int newindex,int event_index)
        {
            responderID = id;
            responseindex = newindex;
            eventindex = event_index;
        }
    }
}

