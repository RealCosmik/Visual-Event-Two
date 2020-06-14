using System;
using UnityEngine;
namespace VisualDelegates
{
    [System.Serializable]
    public class CollisionResponse : ISerializationCallbackReceiver
    {
        [SerializeField] string[] typeInfos;
        [SerializeField] string collisionNote;
        [SerializeField] ObjectDelegate onCollision;
        public Type[] targedTypes { get; private set; }
        public void Initialize() => onCollision.initialize();
        public void onCollide(UnityEngine.Object collision = null) => onCollision.Invoke(collision);
        public void Release() => onCollision.Release();
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            var length = typeInfos.Length;
            targedTypes = new Type[length];
            for (int i = 0; i < length; i++)
            {
                targedTypes[i] = Type.GetType(typeInfos[i]);
            }
        }
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
