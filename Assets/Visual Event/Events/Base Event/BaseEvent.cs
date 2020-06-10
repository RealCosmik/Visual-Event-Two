using UnityEngine;
using System.Collections.Generic;
namespace VisualDelegates.Events
{
    public abstract partial class BaseEvent : ScriptableObject
    {
        [System.NonSerialized] protected List<List<EventResponse>> m_EventResponses = new List<List<EventResponse>>();
        public IReadOnlyList<IReadOnlyList<EventResponse>> EventResponses => m_EventResponses;
        public void Subscribe(EventResponse newResponse)
        {
            var count = m_EventResponses.Count;
            if (newResponse.GetPriority() >= count)
            {
                int delta = newResponse.GetPriority() - m_EventResponses.Count;
                for (int i = 0; i <= delta; i++)
                {
                    m_EventResponses.Add(new List<EventResponse>());
                }
                //  Debug.LogError("added");
            }
            else if (count == 0)
                m_EventResponses.Add(new List<EventResponse>());
            newResponse.subscriptionindex = m_EventResponses[newResponse.GetPriority()].Count;
            //if (Application.isEditor && !AllResponses[priortiy].Contains(response))
            m_EventResponses[newResponse.GetPriority()].Add(newResponse);
            newResponse.currentEvent = this;
        }
        public void UnSubscribe(EventResponse response)
        {
            if (response.subscriptionindex != -1)
            {
                var prioritylist = m_EventResponses[response.GetPriority()];
                prioritylist.RemoveAt(response.subscriptionindex);
                response.subscriptionindex = -1;
                response.currentEvent = null;
            }
            else throw new UnityException("Response subscription index must be greater than -1!");
        }
        public void UnSubscribe(int priority, int subscriptionIndex)
        {
            m_EventResponses[priority].RemoveAt(subscriptionIndex);
        }
        public void UpdateResponsePriority(EventResponse response, int newpriority)
        {
            UnSubscribe(response);
            response.SetPrioirtiy(newpriority);
            Subscribe(response);
        }

    }
}
