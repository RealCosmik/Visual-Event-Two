using UnityEngine;
using System.Collections.Generic;
namespace VisualDelegates.Events
{
    public class EventSubscriber : MonoBehaviour
    {
        [SerializeField] List<EventResponse> responses = new List<EventResponse>();

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
            for (int i = 0; i < count; i++)
            {
                if (responses[i].CurrentResponse != null)
                {
                    if (iseditor)
                    {
                        responses[i].senderID = GetInstanceID();
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
                    responses[i].CurrentResponse.Release();
                    responses[i].currentEvent.UnSubscribe(responses[i]);
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