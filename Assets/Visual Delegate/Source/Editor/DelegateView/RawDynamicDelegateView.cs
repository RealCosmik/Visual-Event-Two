using UnityEngine;
using UnityEditor;
namespace VisualEvent
{
    public class RawDynamicDelegateView: RawDelegateView
    {
        public string MethodName, TargetName;
        public GUIContent MethodContent { get; private set; }
        public GUIContent TargetContent { get; private set; }
        /// <summary>
        /// Calculates the height based on the method name and target name of this runtime
        /// </summary>
        /// <returns></returns>
        public float CalcHeight()
        {
            var style = VisualEdiotrUtility.StandardStyle;
            MethodContent = new GUIContent(MethodName);
            TargetContent = new GUIContent(TargetName);
            style.CalcMinMaxWidth(MethodContent, out float minMethodWidith, out float MaxMethodWidth);
            style.CalcMinMaxWidth(TargetContent, out float minTargetWidith, out float MaxTargetWidth);
            var methodHeight = style.CalcHeight(MethodContent, MaxMethodWidth);
            var TargetHeight= style.CalcHeight(TargetContent, MaxTargetWidth);
            return Height = methodHeight + TargetHeight;
        }
    }
}
