using UnityEngine;
using EventsPlus;
using System.Collections.Generic;
using System.Reflection.Emit;
class LinkedMono : MonoBehaviour
{
    public Publisher pub;
    public int RandomPropp { get => 2; set { Debug.Log("set value to be" + value); } }
    public int mynum;
    public Vector2 vectest { get; set; }
    public Object reference;
    public Component f;
    [HideInInspector]
    public MonoBehaviour mono;
    public void methodforikram() => Debug.Log("this is for ikram");
    public void testmethod(Vector3 thisisalongertitle, Vector3 duos, int data) => Debug.Log("test");
    public void SetAudio(AudioClip clip) => Debug.Log("cli[[y");
    public void SetSource(AudioSource s) => Debug.Log("soucry");
    public void Testobj(UnityEngine.Object o) => Debug.Log("this should work");
    public void TestSo(UtilitySO s) => Debug.Log("scriptable test");
    public void testbounds(Bounds thisisanewname) => Debug.Log("try the bounds");
    public void testrect(Rect t) => Debug.Log("test rect");
    public void testQuaternion(Quaternion quatern) => Debug.Log("test q");
    public void testbool(bool paramnameis) => Debug.Log("testparam");
    public void numberlog(int num) => Debug.Log(num);
    private void Start()
    {
        Object o = new AudioSource();
        Debug.Log(reference.GetType().FullName);
        pub.initialize();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
      //      pub.initialize();
           pub.publish();
        }
    }

}
[System.Serializable]
public class intpub : Publisher<int> { }
public class stringpub : Publisher<string> { }
