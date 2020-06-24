using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Timeline;
namespace VisualDelegates.Cinema.Editor
{
    class SignalTree : TreeView
    {
        readonly SerializedProperty assetList;
        readonly SerializedProperty responseList;
        readonly SerializedObject m_reciever;
        readonly GUIContent content = new GUIContent("Response");
        readonly float objectHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);
        int TreeSize;
        public SignalTree(TreeViewState state, MultiColumnHeader header, SerializedObject reciever) : base(state, header)
        {
            assetList = reciever.FindProperty("signalAssets");
            responseList = reciever.FindProperty("signalResponses");
            m_reciever = reciever;
            TreeSize = -1;
            Reload();
        }
        public void UpdateSingalTree(int newsignalCount)
        {
            TreeSize = newsignalCount;
            Reload();
        }
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            var height = objectHeight;

            if (item is SignalTreeElement && item.id < responseList.arraySize)
            {
                height += EditorGUI.GetPropertyHeight(m_reciever.FindProperty("signalResponses").GetArrayElementAtIndex(item.id));
            }
            height += EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);
            return height;
        }
        protected override TreeViewItem BuildRoot()
        {
            TreeSize = assetList.arraySize;
            var root = new TreeViewItem(-1, -1, "root");
            if (TreeSize > 0)
            {
                for (int i = 0; i < TreeSize; i++)
                {
                    var exanpeded = assetList.GetArrayElementAtIndex(i).isExpanded;
                    root.AddChild(new SignalTreeElement() { id = i, iscollapsed = exanpeded });
                }
            }
            else root.AddChild(new TreeViewItem(0));
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var colls = args.GetNumVisibleColumns();
            for (int i = 0; i < colls; i++)
            {
                DrawSignalResponsePair(i, ref args);
            }
        }
        private void DrawSignalResponsePair(int coll, ref RowGUIArgs args)
        {
            var rect = args.GetCellRect(coll);
            switch (coll)
            {
                case 0:
                    DrawSignalAsset(rect, args.item);
                    break;
                case 1:
                    if (args.item is SignalTreeElement element)
                        DrawSignalResponse(rect, element, ref args);
                    break;
            }
        }
        private void DrawSignalAsset(Rect rect, TreeViewItem item)
        {
            if (item is SignalTreeElement)
            {
                rect.height = objectHeight;
                EditorGUI.BeginChangeCheck();
                var signal = EditorGUI.ObjectField(rect, assetList.GetArrayElementAtIndex(item.id).objectReferenceValue, typeof(SignalAsset), false);
                if (EditorGUI.EndChangeCheck())
                    OnSignalAdded(signal, item.id);
                var buttonrect = rect;
                buttonrect.y += buttonrect.height;
                DrawRemovalButton(buttonrect, item.id);
            }
        }

        private void OnSignalAdded(UnityEngine.Object newsignal, int targetIndex)
        {
            var size = assetList.arraySize;
            bool validSignal = true && newsignal != null;
            for (int i = 0; i < size&&validSignal; i++)
            {
                if (assetList.GetArrayElementAtIndex(i).objectReferenceInstanceIDValue == newsignal.GetInstanceID())
                {
                    Debug.LogError($"There is already a signal response for {newsignal.name}");
                    validSignal = false;
                    break;
                }
            }
            if (validSignal || newsignal == null)
            {
                responseList.GetArrayElementAtIndex(targetIndex).FindPropertyRelative("validResponse").boolValue = validSignal;
                assetList.GetArrayElementAtIndex(targetIndex).objectReferenceValue = newsignal;
                assetList.serializedObject.ApplyModifiedProperties();
            }
        }
        private void DrawRemovalButton(Rect rect, int index)
        {
            if (GUI.Button(rect, "Remove"))
            {
                assetList.GetArrayElementAtIndex(index).objectReferenceValue = null;
                assetList.DeleteArrayElementAtIndex(index);
                responseList.DeleteArrayElementAtIndex(index);
                assetList.serializedObject.ApplyModifiedProperties();
                responseList.serializedObject.ApplyModifiedProperties();
                Reload();
            }
        }
        private void DrawSignalResponse(Rect rect, SignalTreeElement element, ref RowGUIArgs arg)
        {
            if (element.id < TreeSize)
            {
                rect.x += 10f;
                var response = responseList.GetArrayElementAtIndex(element.id);
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, response, content);
                if (EditorGUI.EndChangeCheck())
                {
                    Reload();
                    RefreshCustomRowHeights();
                }
                if (element.iscollapsed != response.isExpanded)
                {
                    element.iscollapsed = response.isExpanded;
                    RefreshCustomRowHeights();
                }
            }
        }
    }
}
