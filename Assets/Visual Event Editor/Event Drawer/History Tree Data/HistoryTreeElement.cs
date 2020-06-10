using UnityEditor.IMGUI.Controls;
namespace VisualDelegates.Events.Editor
{
    public class HistoryTreeElement :TreeViewItem
    {
        public string argumentData;
        public HistoryTreeElement(object[] args) => argumentData = string.Join(",", args);
    }
}