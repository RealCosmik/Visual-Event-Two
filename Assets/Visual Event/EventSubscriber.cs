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
            int count = responses.Count;
            bool iseditor = Application.isEditor;
            for (int i = 0; i < count; i++)
            {
                if (responses[i].response != null)
                {
                    if (iseditor)
                    {
                        responses[i].senderID = GetInstanceID();
                        responses[i].responseIndex = i;
                    }
                    responses[i].response.initialize();
                    responses[i].currentEvent.Subscribe(responses[i], responses[i].priority);
                }
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach (var r in responses)
                    (r.response as VisualDelegate<int>).Invoke(3);
            }
        }
        private void OnDestroy()
        {
            var responsecount = responses.Count;
            for (int i = 0; i < responsecount; i++)
            {
                if (responses[i].response != null)
                {
                    responses[i].response.Release();
                    responses[i].currentEvent.UnSubscribe(responses[i]);
                }

            }
        }
    }
}