using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(DelegateAOT))]
class DelegateAOTDrawer : Editor
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
