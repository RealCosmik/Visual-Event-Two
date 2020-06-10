using System;
using UnityEngine;
class EventInvoker : MonoBehaviour
{
    public VisualDelegates.Events.IntEvent myevent;
    public KeyCode invokecode = KeyCode.E;
    private void Start()
    {
        myevent.Subscribe(0, val => Debug.Log("dynamic registartion"));
    }
    private void Update()
    {
        if (Input.GetKeyDown(invokecode))
        {
            myevent.Invoke(UnityEngine.Random.Range(0, 999), this);
        }
    }
}
