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
        private static Color errorColor = new Color(.81f, .3f, .3f, .4f);
        public string activeTrace { get; private set; }
        List<HistoryEntry> cachedEntries;
        public HistoryTree(TreeViewState state, MultiColumnHeader header, BaseEvent newEvent, int maxentries) : base(state, header)
        {
            this.useScrollView = true;
            currentEvent = newEvent;
            capacity = maxentries;
            Reload();
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1, "root");
            if (!BuildHistoryTree(root))
                root.AddChild(new TreeViewItem());
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }
        protected override void SingleClickedItem(int id)
        {
            if (FindItem(id, rootItem) is HistoryTreeElement historyelement)
                activeTrace = cachedEntries[historyelement.id].entryTrace;
        }
        private bool BuildHistoryTree(TreeViewItem root)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
            cachedEntries = typeof(BaseEvent).GetField("eventHistory", flags).GetValue(currentEvent) as List<HistoryEntry>;
            for (int i = 0; i < capacity && i < cachedEntries.Count; i++)
                root.AddChild(new HistoryTreeElement(cachedEntries[i].entryData) { id = i });
            return cachedEntries.Count > 0;
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
        private void DrawHistoricEntry(int Collumnindex, ref RowGUIArgs args)
        {
            var cellrect = args.GetCellRect(Collumnindex);
            var treeItem = args.item as HistoryTreeElement;
            if (cachedEntries[treeItem.id].haserror)
                EditorGUI.DrawRect(args.rowRect, errorColor);
            GUI.enabled = false;
            switch (Collumnindex)
            {
                case 0:
                    var sender_object = EditorUtility.InstanceIDToObject(cachedEntries[treeItem.id].SenderID);
                    EditorGUI.ObjectField(cellrect, sender_object, typeof(UnityEngine.Object), true);
                    break;
                case 1:
                    EditorGUI.LabelField(cellrect,treeItem.argumentData);
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
