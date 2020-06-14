using UnityEngine;

namespace VisualDelegates
{
    public abstract class CollisionHook : MonoBehaviour
    {
        [SerializeField] protected CollisionResponse collisionEnterResponse;
        [SerializeField] protected CollisionResponse collisionExitResponse;
        [SerializeField] protected CollisionResponse collisionStayResponse;
        private void Awake()
        {
            collisionEnterResponse.Initialize();
            collisionExitResponse.Initialize();
            collisionStayResponse.Initialize();
        }
        protected void OnCollision(GameObject collidedobject, CollisionResponse response)
        {
            var types = response.targedTypes;
            var length = types.Length;
            for (int i = 0; i < length; i++)
            {
                if (collidedobject.TryGetComponent(types[i], out Component c))
                {
                    response.onCollide(collidedobject);
                    break;
                }
            }
        }
    }
}

