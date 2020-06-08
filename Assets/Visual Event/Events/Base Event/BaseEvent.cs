using UnityEngine;
using System.Collections.Generic;
namespace VisualDelegates.Events
{
    public abstract class BaseEvent : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] string EventNote;
        [SerializeField] int historycapacity = 5;
        [System.NonSerialized] public List<List<EventResponse>> AllResponses = new List<List<EventResponse>>();
        private protected List<HistoryEntry> eventHistory = new List<HistoryEntry>();
        private protected int overwriteIndex;
        public bool isinvoke;
        public void Subscribe(EventResponse response, int priortiy)
        {
            var count = AllResponses.Count;
            if (priortiy >= count)
            {
                int delta = priortiy - AllResponses.Count;
                for (int i = 0; i <= delta; i++)
                {
                    AllResponses.Add(new List<EventResponse>());
                }
                //  Debug.LogError("added");
            }
            else if (count == 0)
                AllResponses.Add(new List<EventResponse>());
            response.subscriptionindex = AllResponses[priortiy].Count;
            //if (Application.isEditor && !AllResponses[priortiy].Contains(response))
            AllResponses[priortiy].Add(response);
        }
        public int HistoryCount() => eventHistory.Count;
        public void UnSubscribe(EventResponse response)
        {
            var prioritylist = AllResponses[response.priority];
            prioritylist.RemoveAt(response.subscriptionindex);
        }
        public void UnSubscribe(int priority, int subscriptionIndex)
        {
            AllResponses[priority].RemoveAt(subscriptionIndex);
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!Application.isEditor)
            {
                Clear();
            }
        }
        private protected abstract void Clear();
        private protected abstract void EditorInvoke();
        private protected void UpdateEventHistory(Object sender, params object[] args)
        {
            if (Application.isEditor)
            {
                eventHistory = eventHistory ?? new List<HistoryEntry>();
                if (eventHistory.Count < historycapacity)
                    eventHistory.Add(new HistoryEntry(sender?.GetInstanceID() ?? -999, args, System.Environment.StackTrace));
                else
                {
                    // loop back through history to save memory
                    overwriteIndex = overwriteIndex == historycapacity - 1 ? overwriteIndex = 0 : overwriteIndex += 1;
                    eventHistory[overwriteIndex].entryData = args;
                    eventHistory[overwriteIndex].SenderID = sender?.GetInstanceID() ?? -990;
                    eventHistory[overwriteIndex].entryTrace = System.Environment.StackTrace;
                }
            }
        }
    }
}
