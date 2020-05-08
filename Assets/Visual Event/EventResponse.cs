using UnityEngine;
namespace VisualDelegates.Events
{
    [System.Serializable]
    public class EventResponse
    {
        [UnityEngine.SerializeReference] VisualDelegateBase response;
        [SerializeField] ScriptableObject currentEvent;
        [SerializeField] int priority;

    }
}
