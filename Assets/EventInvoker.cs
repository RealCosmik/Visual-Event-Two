using System;
using UnityEngine;
using VisualDelegates.Events;

class EventInvoker : MonoBehaviour
{
    public VisualDelegates.Events.IntEvent myevent;
    public KeyCode invokecode = KeyCode.E;
    public KeyCode unsubcode= KeyCode.U;
    EventResponse response;
    private void Start()
    {
       // response = myevent.Subscribe(val => throw new UnityException("random exception"), 1);
        //myevent.Subscribe(val => Debug.Log("another thing"), 1);
    }
    private void Update()
    {
        if (Input.GetKeyDown(invokecode))
        {
            myevent.Invoke(UnityEngine.Random.Range(0, 999), this);
        }
        if (Input.GetKeyDown(unsubcode))
        {
            myevent.UnSubscribe(response);
        }
    }
}
