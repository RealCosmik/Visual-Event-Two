#if USING_ADDRESSABLES
using UnityEngine.AddressableAssets;

namespace VisualEvents
{
    [System.Serializable]
    public class EventReference<EventType> : AssetReferenceT<EventType> where EventType : BaseEvent
    {
        public EventReference(string guid) : base(guid) { }
        public EventResponse eventResponse;
        public new EventType Asset => base.Asset as EventType;
        public sealed override void ReleaseAsset()
        {
            eventResponse?.UnsubscribeAndRelease();
            base.ReleaseAsset();
        }
    }
}
#endif