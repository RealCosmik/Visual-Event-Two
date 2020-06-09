using UnityEngine;
class EventInvoker : MonoBehaviour
{
    public VisualDelegates.Events.IntEvent myevent;
    public KeyCode invokecode = KeyCode.E;
    private void Update()
    {
        if (Input.GetKeyDown(invokecode))
        {
            Debug.Log("invok!");
            myevent.Invoke(UnityEngine.Random.Range(0, 999), this);
        }
    }
}
