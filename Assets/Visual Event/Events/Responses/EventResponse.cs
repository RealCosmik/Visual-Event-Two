using UnityEngine;
namespace VisualEvents
{
    public abstract class EventResponse
    {
        [SerializeField] public BaseEvent currentEvent; //TODO Remove cyclic referece lucky that this saves by ID
        /// <summary>
        /// The zero-based priority of this response within the event it is subscribed to 
        /// </summary>
        [SerializeField] internal int priority = -1;
        /// <summary>
        /// the index of this event within is priority
        /// </summary>
        internal int subscriptionIndex = -1;
        /// <summary>
        /// determines if this response will be invoked when executed by an event
        /// </summary>
        [SerializeField] public bool isActive = true;

        [System.NonSerialized] public int subscriberID;

        protected EventResponse(int newpriority) => SetPriority(newpriority);
        public ref readonly int GetSubscriptionIndex() => ref subscriptionIndex;
        public ref readonly int GetPriority() => ref priority;
        internal void SetPriority(in int newpriority)
        {
            if (newpriority >= 0)
                priority = newpriority;
            else
                throw new UnityException("Priority values cannot be LESS than 0!");
        }
        /// <summary>
        /// Releases the <see cref="VisualDelegate"/> attached to this response
        /// </summary>
        public abstract void Release();
        public void UnsubscribeAndRelease()
        {
            Release();
            if (!ReferenceEquals(currentEvent, null))
            {
                currentEvent.UnSubscribe(this);
                currentEvent = null;
            }
        }
        public void UnSubscribe()
        {
            if (!ReferenceEquals(currentEvent, null))
            {
                currentEvent.UnSubscribe(this);
                currentEvent = null;
            }
        }
        protected internal abstract void Invoke();
        protected internal abstract void Invoke<Arg1>(Arg1 arg1);
        protected internal abstract void Invoke<Arg1, Arg2>(Arg1 arg1, Arg2 arg2);
        protected internal abstract void Invoke<Arg1, Arg2, Arg3>(Arg1 arg1, Arg2 arg2, Arg3 arg3);
        protected internal abstract void Invoke<Arg1, Arg2, Arg3, Arg4>(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4);
    }
}
