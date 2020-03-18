using UnityEditor;
using UnityEngine;
namespace VisualEvent
{
    [CustomPropertyDrawer(typeof(RawReference),true)]
    class DrawerRawReferenceView : DrawerRawDelegateView<RawReferenceView>
    {
        public override float GetPropertyHeight(SerializedProperty tProperty, GUIContent tLabel)
        {
            return base.GetPropertyHeight(tProperty, tLabel);
        }
        public override void OnGUI(Rect tPosition, SerializedProperty tProperty, GUIContent tLabel)
        {
            base.OnGUI(tPosition, tProperty, tLabel);
        }
    }
}
