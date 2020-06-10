using UnityEngine;
namespace VisualDelegates.Events
{
    [System.Serializable]
    public class EventResponse
    {
        [UnityEngine.SerializeReference] VisualDelegateBase response = null;
        [SerializeField] public BaseEvent currentEvent;
        [SerializeField] int priority;
        [SerializeField] public bool isActive = true;
        [SerializeField] string responseNote;
        [System.NonSerialized] public int senderID, responseIndex,subscriptionindex;

        public EventResponse(VisualDelegateBase newresponse,int newpriority)
        {
            SetPrioirtiy(newpriority);
            response = newresponse;

        }
        public VisualDelegateBase CurrentResponse => response;
        public int GetPriority() => priority;
        public void SetPrioirtiy(int newpriority)
        {
            if (newpriority >= 0)
                priority = newpriority;
            else
                throw new UnityException("Priority values cannot be LESS than 0!");
        }

    }
}
