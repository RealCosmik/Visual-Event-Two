using UnityEngine;
using EventsPlus;
using System.Collections.Generic;
class LinkedMono : MonoBehaviour
{
    public Publisher pub;
    public int RandomPropp { get => 2; set { Debug.Log("set value to be" + value); } }
    public int mynum;
    public Vector2 vectest { get; set; }

    public void methodforikramm() => Debug.Log("this is for ikram");
    public void testmethod(Vector3 uno, Vector3 duos, int data) => Debug.Log("test");
    private void Start()
    {
        pub.initialize();
    }
    private void Update()
    { 
        if (Input.GetKeyDown(KeyCode.Space))
        {
                //pub.initialize();
                pub.publish();
      //      pub.initialize();
        //    pub.publish();
        }
    }

}
public class intpub : Publisher<int> { }
public class stringpub : Publisher<string> { }
