using UnityEngine;
using EventsPlus;
using System.Collections.Generic;
class LinkedMono : MonoBehaviour
{
    public Publisher pub;
    /// <summary>
    /// <see cref="RandomMethodd"/> this gets called by visual event
    /// </summary>
    public void RandomMethod() => Debug.Log("Random Method");
    public int RandomPropp{ get; set; }
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
