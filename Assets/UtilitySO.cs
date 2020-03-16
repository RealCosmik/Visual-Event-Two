using UnityEngine;
using EventsPlus;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "methodSO", menuName = "Util/methodSO")]

public class UtilitySO : ScriptableObject
{
    public int value;
    public string DataText;
    public void LogMessage(string message,LogType logtype,UnityEngine.Object context)
    {
        switch (logtype)
        {
            case LogType.Standard:
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
    /// <summary>
    /// this method gets called in <see cref="LinkedMono"/>
    /// </summary>
    public void Extramethodattempt()
    {
        Debug.LogError("wow did this work");
    }
    public void TestInt(int value) => Debug.Log($"the value is {value}");
    public void TestString(string value) => Debug.Log($"the value is {value}");
    public void log(string message) => Debug.Log(message);
}
