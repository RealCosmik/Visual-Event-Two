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
        static UnityEngine.Object[] inspectorwindows;
        static InvocationTracker()
        {
            inspectorwindows = Resources.FindObjectsOfTypeAll(inspector_type);
            EditorApplication.update += CheckNewInvocation;
            EditorApplication.playModeStateChanged += OnPlay;
        }

        private static void OnPlay(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredEditMode)
                inspectorwindows = Resources.FindObjectsOfTypeAll(inspector_type);
        }

        private static void CheckNewInvocation()
        {
            if (requestRepaint)
            {
                var length = inspectorwindows.Length;
                for (int i = 0; i < length; i++)
                {
                    (inspectorwindows[i] as EditorWindow).Repaint();
                }
                requestRepaint = false;
            }
        }
    }

}
