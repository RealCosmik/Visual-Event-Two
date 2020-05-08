using UnityEditor;
using UnityEngine;
namespace VisualDelegates.Editor
{
    [CustomEditor(typeof(DelegateAOT))]
    class DelegateAOTDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate AOT File"))
            {
                DelegateAOT.AOTGeneration();
            }
        }
    }
}