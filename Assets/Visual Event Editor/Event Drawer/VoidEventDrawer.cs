using UnityEngine;
using UnityEditor;

namespace VisualDelegates.Events.Editor
{
    [CustomEditor(typeof(VoidEvent), true)]
    class VoidEventDrawer : BaseEventDrawer
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Invoke"))
                (target as VoidEvent).invoke();
            base.OnInspectorGUI();
        }
    }
}
