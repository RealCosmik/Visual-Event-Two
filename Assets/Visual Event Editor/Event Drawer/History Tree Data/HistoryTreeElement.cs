using UnityEditor.IMGUI.Controls;
namespace VisualEvents.Editor
{
    public class HistoryTreeElement :TreeViewItem
    {
        public string argumentData;
        public int frame;
        public HistoryTreeElement(int frame,object[] args)
        {
            argumentData = args.Length > 0 ? string.Join(",", args) : "Void";
            this.frame = frame;
        }
    }
}