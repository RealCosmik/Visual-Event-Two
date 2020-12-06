using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace VisualDelegates.Cinema
{
    public class VisualSignalReciever : MonoBehaviour, INotificationReceiver, ISerializationCallbackReceiver
    {
        [SerializeField] List<SignalAsset> signalAssets;
        [SerializeField] List<SignalResponse> signalResponses;
        Dictionary<SignalAsset, VisualDelegate> assetPairs;
        private void Awake()
        {
            if (!Application.isEditor)
            {
                signalAssets = null;
                signalResponses = null;
            }
        }

        void INotificationReceiver.OnNotify(Playable origin, INotification notification, object context)
        {
            if (Application.isPlaying && notification is SignalEmitter emitter)
            {
                if (assetPairs.TryGetValue(emitter.asset, out VisualDelegate response))
                    response.Invoke();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            assetPairs = new Dictionary<SignalAsset, VisualDelegate>(signalAssets.Count);
            var length = signalAssets.Count;
            for (int i = 0; i < length; i++)
            {
                if (signalResponses[i].IsValidResponse)
                {
                    signalResponses[i].initialize();
                    assetPairs.Add(signalAssets[i], signalResponses[i]);
                }
                else
                {
                    if(signalResponses[i].Calls.Count>0)
                    Debug.LogError("must have valid signal before populating response!");
                    signalResponses[i].Release();

                }
            } 
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }
        private void OnDestroy()
        {
            assetPairs.Clear();
            assetPairs = null;
            signalAssets = null;
            signalResponses = null;
        }
    }
}
