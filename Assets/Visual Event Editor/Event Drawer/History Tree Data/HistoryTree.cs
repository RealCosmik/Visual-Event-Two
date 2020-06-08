using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
namespace VisualDelegates.Events.Editor
{
    public class HistoryTree : TreeView
    {
        BaseEvent currentEvent;
        int capacity;
        public string activeTrace { get; private set; }
        public HistoryTree(TreeViewState state, MultiColumnHeader header,BaseEvent newEvent,int maxentries): base(state,header)
        {
            this.useScrollView = true;
            currentEvent = newEvent;
            capacity = maxentries;
            Reload();
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1, "root");
            BuildHistoryTree(root);
            root.AddChild(new TreeViewItem());
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }
        protected override void SingleClickedItem(int id)
        {
            activeTrace = (FindItem(id, rootItem) as HistoryTreeElement).currentEntry.entryTrace;
        }
        private void BuildHistoryTree(TreeViewItem root)
        { 
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
            var current_history = typeof(BaseEvent).GetField("eventHistory", flags).GetValue(currentEvent) as List<HistoryEntry>;
            for (int i = 0; i < capacity&&i<current_history.Count; i++)
                root.AddChild(new HistoryTreeElement(current_history[i]) { id = i });
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            int colls = args.GetNumVisibleColumns();
            for (int i = 0; i < colls; i++)
            {
                if (args.item is HistoryTreeElement)
                    DrawHistoricEntry(i, ref args);
                else base.RowGUI(args);
            }
        }
        private void DrawHistoricEntry(int Collumnindex,ref RowGUIArgs args)
        {
            var cellrect = args.GetCellRect(Collumnindex);
            HistoryTreeElement element = args.item as HistoryTreeElement;
            GUI.enabled = false;
            switch (Collumnindex)
            {
                case 0:
                        var sender_object = EditorUtility.InstanceIDToObject(element.currentEntry.SenderID);
                        EditorGUI.ObjectField(cellrect, sender_object, typeof(UnityEngine.Object), true);
                    break;
                case 1:
                    var arguments = string.Join(",", element.currentEntry.entryData);
                    EditorGUI.LabelField(cellrect, arguments);
                    break;
                default: 
                    break;
            }
            GUI.enabled = true;
        }
        public static MultiColumnHeader CreateHistoryHeader()
        {
            var collumns = new MultiColumnHeaderState.Column[]
           {
                new MultiColumnHeaderState.Column()
                {
                    headerContent=new GUIContent("Sender"),
                    width=50,
                    minWidth=50,
                    maxWidth=100,
                    autoResize=true,
                    headerTextAlignment=TextAlignment.Center
                },
                 new MultiColumnHeaderState.Column()
                {
                    headerContent=new GUIContent("Data"),
                    width=50,
                    minWidth=50,
                    maxWidth=250,
                    autoResize=true,
                    headerTextAlignment=TextAlignment.Center
                },
           };
            return new MultiColumnHeader(new MultiColumnHeaderState(collumns));
        }
    }
}
