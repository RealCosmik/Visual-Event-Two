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
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach(var e in responses)
                {
                    (e.response as VisualDelegate).Invoke();
                }
            }
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
    }
}