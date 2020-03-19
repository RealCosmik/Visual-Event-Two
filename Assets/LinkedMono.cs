using UnityEngine;
using VisualEvent;
using System.Collections.Generic;
class LinkedMono : MonoBehaviour
{ 
    public VisualDelegate del;
    public TMPro.TextMeshProUGUI visual_Text;
    public Vector3 vecfield = Vector3.one;
    public int RandomPropp { get => 2; set { Debug.Log("set value to be" + value); } }
    public int mynum;
    public void methodforikram() => Debug.Log("this is for ikram");
    public void methodforikram(int x) => Debug.Log("printing as an int");
    public void methodforikram(string s) => Debug.Log("print as a string");
    private void Start()
    {
        del.Addmethod(() => Debug.Log("attempt for a hot fix"));
        del.Addmethod(() => Debug.LogWarning("this is a warning"));
        del.initialize();
    }
    private void Testmethod()
    {
        Debug.Log("real method");
    }
    private void Update()
    {
        UnityEngine.Profiling.Profiler.BeginSample("delgate invoke", this);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            del.Invoke();
           // UnityEditor.EditorApplication.isPlaying = false;
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

}
[System.Serializable]
public class intpub : VisualDelegate<int> { }
[System.Serializable]
public class stringpub : VisualDelegate<string> { }
