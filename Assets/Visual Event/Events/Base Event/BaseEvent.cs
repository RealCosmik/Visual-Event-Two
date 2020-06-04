using UnityEngine;
using System.Collections.Generic;
namespace VisualDelegates.Events
{
    public abstract class BaseEvent : ScriptableObject
    {
        [SerializeField] string EventNote;
        [System.NonSerialized] public List<List<EventResponse>> AllResponses = new List<List<EventResponse>>();
    
        public void Subscribe(EventResponse response, int priortiy)
        {
                var count = AllResponses.Count;
                Debug.LogWarning($"insering at priority {priortiy}");
                Debug.LogWarning($"response count pre sub {count}");
                if (priortiy >= count)
                {
                    int delta = priortiy - AllResponses.Count;
                    for (int i = 0; i <= delta; i++)
                    {
                        Debug.Log(i);
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
        public void UnSubscribe(EventResponse response)
        {
            var prioritylist=AllResponses[response.priority];
            prioritylist.RemoveAt(response.subscriptionindex);
        }
        public void UnSubscribe(int priority,int subscriptionIndex)
        {
            AllResponses[priority].RemoveAt(subscriptionIndex);
        }
        private protected abstract void EditorInvoke();
    }
}
