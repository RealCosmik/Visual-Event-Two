using UnityEngine;
using System.Collections.Generic;
using System;

namespace VisualEvents
{
    public abstract partial class BaseEvent : ScriptableObject
    {
        [System.NonSerialized] protected List<List<EventResponse>> m_EventResponses = new List<List<EventResponse>>();
        [System.NonSerialized] readonly Dictionary<int, Queue<int>> responseGaps = new Dictionary<int, Queue<int>>();
        public IReadOnlyList<IReadOnlyList<EventResponse>> EventResponses => m_EventResponses;
        public void Subscribe(EventResponse newResponse)
        {
            if (newResponse == null)
                throw new ArgumentNullException(nameof(newResponse));
            else if (newResponse.priority < 0)
                throw new ArgumentException("prioity must be positive integer!", nameof(newResponse));
            var count = m_EventResponses.Count;
            ref readonly int priority = ref newResponse.GetPriority();
            //int priority =  newResponse.priority;

            if (priority >= count)
            {
                int delta = priority - count;
                for (int i = 0; i <= delta; i++)
                {
                    responseGaps.Add(count + i, new Queue<int>());
                    m_EventResponses.Add(new List<EventResponse>());
                }
                //  Debug.LogError("added");
            }
            else if (count == 0)
            {
                responseGaps.Add(0, new Queue<int>());
                m_EventResponses.Add(new List<EventResponse>());
            }

            var subscriptionlist = m_EventResponses[priority];
            if (responseGaps[priority].Count == 0)
            {
                newResponse.subscriptionIndex = subscriptionlist.Count;
                subscriptionlist.Add(newResponse);
            }
            else
            {
                int openIndex = responseGaps[priority].Dequeue();
                subscriptionlist[openIndex] = newResponse;
                newResponse.subscriptionIndex = openIndex;
            }
            newResponse.currentEvent = this;
        }
        public void UnSubscribe(EventResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            UnSubscribe(priorityindex: in response.priority, subscriptionIndex: in response.subscriptionIndex);
        }
        /// <summary>
        /// Removes the response at the given indicies
        /// </summary>
        /// <param name="priorityindex"></param>
        /// <param name="subscriptionIndex"></param>
        public void UnSubscribe(in int priorityindex, in int subscriptionIndex)
        {
            if (priorityindex < m_EventResponses.Count)
            {
                var subscriptionlist = m_EventResponses[priorityindex];
                if (subscriptionIndex < subscriptionlist.Count)
                {
                    subscriptionlist[subscriptionIndex] = null;
                    responseGaps[priorityindex].Enqueue(subscriptionIndex);
                    //subscriptionlist.RemoveAt(subscriptionIndex);
                }
                else throw new ArgumentOutOfRangeException(nameof(subscriptionIndex), $"{name} Event. \n out of bounds index ({subscriptionIndex}). list count:" +
                    $"({subscriptionlist.Count})");
            }
            else throw new ArgumentOutOfRangeException(nameof(priorityindex), $"{name} Event. \n priority index must be less than {m_EventResponses.Count} not {subscriptionIndex}");
        }
        /// <summary>
        /// Unsubscribes all responses in a given priority
        /// </summary>
        /// <param name="priority"></param>
        public void UnSubscribe(in int priorityindex)
        {
            if (priorityindex < m_EventResponses.Count)
            {
                var subscriptionlist = m_EventResponses[priorityindex];
                subscriptionlist.RemoveRange(0, subscriptionlist.Count);
            }
            else throw new ArgumentOutOfRangeException(nameof(priorityindex), $"priority index must be less than {m_EventResponses.Count}");
        }
        public void UpdateResponsePriority(EventResponse response, in int newpriority)
        {
            UnSubscribe(response);
            response.SetPriority(in newpriority);
            Subscribe(response);
        }

        public void ClearGaps()
        {
            for (int i = 0; i < m_EventResponses.Count; i++)
            {
                var responselist = m_EventResponses[i];
                var gaps = responseGaps[i];
                var count = gaps.Count;
                for (int j = 0; j < count; j++)
                {
                    responselist.RemoveAt(gaps.Dequeue());
                }
            }
        }

    }
}
