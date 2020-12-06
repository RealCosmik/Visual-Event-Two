using UnityEngine;
using System.Collections.Generic;
namespace VisualEvents.VisualSubscribers
{
    [VisualSubscriber]
    public class EventSubscriber : MonoBehaviour
    {
        [SerializeField] List<SubscriberResponse> responses = new List<SubscriberResponse>();
        //TODO add seperate list with event response 
        private void Awake() => SetSubscriptions();
        private void OnEnable() => SetResponseActiveStatus(true);
        private void OnDisable() => SetResponseActiveStatus(false);
        private void SetResponseActiveStatus(bool isactive)
        {
            int count = responses.Count;
            for (int i = 0; i < count; i++)
                responses[i].isActive = isactive;
        }
        public void SetSubscriptions()
        {
            int count = responses.Count;
            bool iseditor = Application.isEditor;
            var id = GetInstanceID();
            for (int i = 0; i < count; i++)
            {
                if (responses[i].CurrentResponse != null)
                {
                    if (iseditor)
                    {
                        responses[i].subscriberID = id;
                        responses[i].responseIndex = i;
                    }
                    responses[i].CurrentResponse.initialize();
                    responses[i].currentEvent.Subscribe(responses[i]);
                }
            }
        }
        private void OnDestroy()
        {
            var responsecount = responses.Count;
            for (int i = 0; i < responsecount; i++)
            {
                if (responses[i].CurrentResponse != null)
                {
                    responses[i].UnsubscribeAndRelease();
                }

            }
        }
        private void OnValidate()
        {
            if (!Application.isPlaying)
                SetResponseActiveStatus(enabled);
        }
    }
}