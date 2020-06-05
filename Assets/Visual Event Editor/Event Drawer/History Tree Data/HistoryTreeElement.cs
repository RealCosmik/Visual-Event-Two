using UnityEditor.IMGUI.Controls;
namespace VisualDelegates.Events.Editor
{
    public class HistoryTreeElement :TreeViewItem
    {
        public HistoryEntry currentEntry { get; private set; }
        public HistoryTreeElement(HistoryEntry newEntry)
        {
            currentEntry = newEntry;
        }
    }
}