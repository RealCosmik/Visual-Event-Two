using UnityEngine;
using System.Collections.Generic;
namespace VisualDelegates.Events
{
    public class EventSubscriber : MonoBehaviour
    {
        [SerializeField] List<EventResponse> responses;

        private void Awake()
        {
            SetSubscriptions();
        }
        public void SetSubscriptions()
        {
            for (int i = 0; i < responses.Count; i++)
            { 
                responses[i].senderID = GetInstanceID();
                responses[i].responseIndex = i;
                responses[i].response.initialize();
                Debug.Log("deserial");
                responses[i].currentEvent?.Subscribe(responses[i], responses[i].priority);
            }
        }
        private void OnDestroy()
        {
            var responsecount = responses.Count;
            for (int i = 0; i < responsecount; i++)
            {
                responses[i].response?.Release();
                responses[i].currentEvent.UnSubscribe(responses[i]);
            }
        }
    }
}