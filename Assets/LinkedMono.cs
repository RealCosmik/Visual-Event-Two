using UnityEngine;
using EventsPlus;
using System.Collections.Generic;
class LinkedMono : MonoBehaviour
{ 
    public intpub pub;
    public TMPro.TextMeshProUGUI visual_Text;
    public Vector3 vecfield = Vector3.one;
    public int RandomPropp { get => 2; set { Debug.Log("set value to be" + value); } }
    public int mynum;
    public void methodforikram() => Debug.Log("this is for ikram");
    public void methodforikram(int x) => Debug.Log("this is for ikram");
    private void DelegateInspector(System.Action del)
    {
    }
    private void Start()
    {
        pub.initialize();
        //ub.AddMethod(Testmethod);
    }
    private void Testmethod()
    {
        Debug.Log("real method");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            pub.publish();
        }
    }

}
[System.Serializable]
public class intpub : VisualDelegate<int> { }
[System.Serializable]
public class stringpub : VisualDelegate<string> { }
