using UnityEngine;
using VisualEvents;
class EventInvoker : MonoBehaviour
{
    public IntEvent myevent;
    public KeyCode invokecode = KeyCode.E;
    public KeyCode unsubcode= KeyCode.U;
    EventResponse response;
    public IntVar randomevent;
    public int num;
    public FloatVariable floater;
    private void Start()
    {
    }
    private void Update()
    {
        if (Input.GetKeyDown(invokecode))
        {
            myevent.Invoke(UnityEngine.Random.Range(0, 999), this);
        }
        if (Input.GetKeyDown(unsubcode))
        {
            if(response!=null)
            myevent.UnSubscribe(response);
        }
    }
    private void OnValidate()
    {
     //   v.Invoke(num, this);
    }
}
