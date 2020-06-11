using UnityEngine;
using System.Collections.Generic;
namespace VisualDelegates.Events
{
    public abstract partial class BaseEvent: ISerializationCallbackReceiver
    {
        [SerializeField] string EventNote;
        [SerializeField] int historycapacity = 5;
        private protected bool isinvoke;
        private protected int overwriteIndex = 0;
        private protected List<HistoryEntry> eventHistory = new List<HistoryEntry>();
        protected  abstract void Clear();
        protected  abstract void EditorInvoke();
        private protected bool GetisInvoke() => isinvoke;

        private protected void UpdateEventHistory(Object sender,bool error, params object[] args)
        {
            if (Application.isEditor)
            {
                eventHistory = eventHistory ?? new List<HistoryEntry>();
                if (eventHistory.Count < historycapacity)
                    eventHistory.Add(new HistoryEntry(sender?.GetInstanceID() ?? -999, args, System.Environment.StackTrace,error));
                else
                {
                    eventHistory[overwriteIndex].entryData = args;
                    eventHistory[overwriteIndex].SenderID = sender?.GetInstanceID() ?? -990;
                    eventHistory[overwriteIndex].entryTrace = System.Environment.StackTrace;
                    eventHistory[overwriteIndex].haserror = error;
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
