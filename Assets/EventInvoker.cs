using UnityEngine;
using VisualDelegates.Events;
class EventInvoker : MonoBehaviour
{
    public VisualDelegates.Events.IntEvent myevent;
    public KeyCode invokecode = KeyCode.E;
    public KeyCode unsubcode= KeyCode.U;
    EventResponse response;
    public IntVar v;
    public int num;
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
        v.Invoke(num, this);
    }
}
