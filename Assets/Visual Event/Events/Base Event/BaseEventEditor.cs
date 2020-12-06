using UnityEngine;
using System.Collections.Generic;

namespace VisualEvents
{
    public abstract partial class BaseEvent : ISerializationCallbackReceiver
    {
        [SerializeField] string EventNote;
        [SerializeField] int historycapacity = 5;
        [SerializeField] protected bool debugHistory;
        private protected int overwriteIndex = 0;
        private protected List<HistoryEntry> eventHistory = new List<HistoryEntry>();
        protected abstract void Clear();
        protected abstract void EditorInvoke();
        private protected bool isinvoke;
        private bool GetisInvoke() => isinvoke;
        private protected void UpdateEventHistory(Object sender, in bool error, params object[] args)
        {
            int senderID;
            try
            {
                senderID = sender.GetInstanceID();
            }
            catch (System.Exception)
            {
                throw;
            }
            if (Application.isEditor)
            {
                eventHistory = eventHistory ?? new List<HistoryEntry>();
                if (eventHistory.Count < historycapacity)
                    eventHistory.Add(new HistoryEntry(senderID, Time.frameCount, args, System.Environment.StackTrace, error));
                else
                {
                    eventHistory[overwriteIndex].entryData = args;
                    eventHistory[overwriteIndex].SenderID = senderID;
                    eventHistory[overwriteIndex].entryTrace = System.Environment.StackTrace;
                    eventHistory[overwriteIndex].haserror = error;
                    eventHistory[overwriteIndex].frame= Time.frameCount;

                    // wrapping
                    overwriteIndex = overwriteIndex == historycapacity - 1 ? overwriteIndex = 0 : overwriteIndex += 1;
                }
            }
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize() => overwriteIndex = 0;
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!Application.isEditor)
                Clear();
        }
    }
}
