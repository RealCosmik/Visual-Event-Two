using UnityEngine;
using UnityEditor;
namespace VisualDelegates.Events.Editor
{
    class VisualVariableDrawer : BaseEventDrawer
    {
        public override void OnInspectorGUI()
        {
            Debug.LogWarning("ok");
            base.OnInspectorGUI();
        }
    }
}
