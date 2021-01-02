using UnityEngine;
using System;
namespace VisualEvents
{
    public sealed class VoidEvent : BaseEvent
    {
        public void Invoke(UnityEngine.Object sender)
        {
            bool haserror = false;
            isinvoke = true;
            if (ReferenceEquals(sender, null))
                throw new System.ArgumentNullException(nameof(sender));
            var priorites = m_EventResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = m_EventResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    var eventresponse = m_EventResponses[i][j];
                    if (eventresponse != null && eventresponse.isActive)
                    {
                        try
                        {
                            eventresponse.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in {name}\n {ex}", sender);
                            haserror = true;
                        }
                    }
                }
            }
            if (debugHistory)
                UpdateEventHistory(sender: sender, error: in haserror);

        }
        public RuntimeResponse Subscribe(System.Action response, in int priority, UnityEngine.Object subscriber)
        {
            if (response == null)
                throw new System.ArgumentNullException(nameof(response));

            var eventresponse = new RuntimeResponse(response, priority, subscriber);
            Subscribe(eventresponse);
            return eventresponse;
        }
        protected sealed override void Clear() { }
        protected sealed override void EditorInvoke() => Invoke(this);
    }
}
