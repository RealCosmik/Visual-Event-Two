using UnityEngine;
public class UtilitySO : ScriptableObject
{
    public int value;
    public string DataText;
    public void LogMessage(string message,LogType logtype,UnityEngine.Object context)
    {
        switch (logtype)
        {
            case LogType.Log:
                Debug.Log(message,context);
                break;
            case LogType.Warning:
                Debug.LogWarning(message,context);
                break;
            case LogType.Error:
                Debug.LogError(message,context);
                break;
            default:
                break;
        }
    }
}
