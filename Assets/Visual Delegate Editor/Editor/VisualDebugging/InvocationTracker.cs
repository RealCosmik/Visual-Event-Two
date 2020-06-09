using UnityEditor;
using UnityEngine;
using System;
namespace VisualDelegates.Editor
{
    /// <summary>
    /// used to repain the inspector when a delegate is invoked
    /// </summary>
   internal static class InvocationTracker
    {
        public static bool requestRepaint;
        static Type inspector_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void HookEditorUpdate()
        {
            EditorApplication.update += CheckNewInvocation;
        }
        private static void CheckNewInvocation()
        {
            if (requestRepaint)
            {
                var inspectorwindows = Resources.FindObjectsOfTypeAll(inspector_type);
                var length = inspectorwindows.Length;
                for (int i = 0; i < length; i++)
                {
                    (inspectorwindows[i] as EditorWindow).Repaint();
                }
            }
        }
    }

}
