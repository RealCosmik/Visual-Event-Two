using UnityEngine;
namespace VisualDelegates.Events
{
    [System.Serializable]
    public class EventResponse
    {
        [UnityEngine.SerializeReference] public VisualDelegateBase response = null;
        [SerializeField] public BaseEvent currentEvent;
        [SerializeField] public int priority;
        [System.NonSerialized] public int senderID, responseIndex,subscriptionindex;
    }
}
