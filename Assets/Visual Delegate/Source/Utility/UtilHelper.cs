using UnityEngine;
using System.Collections;
namespace VisualEvent
{
    public static class UtilHelper
    {
        private static void LogMessage(string message, LogType logtype, UnityEngine.Object context)
        {
            switch (logtype)
            {
                case LogType.Standard:
                    Debug.Log(message, context);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message, context);
                    break;
                case LogType.Error:
                    Debug.LogError(message, context);
                    break;
                default:
                    break;
            }
        }
        private static void PrintThis()
        {
            Debug.Log("printing this number");
        }

    }
}
