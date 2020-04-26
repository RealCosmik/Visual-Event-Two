using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(DelegateAOT))]
class DelegateAOTDrawer : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("button"))
        {
            (target as DelegateAOT).Trythis();
        }
        EditorGUILayout.IntField("value",2);
    }
}
