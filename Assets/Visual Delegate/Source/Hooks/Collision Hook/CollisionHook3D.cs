using UnityEngine;
namespace VisualDelegates
{
    public class CollisionHook3D: CollisionHook
    {
        private void OnCollisionEnter(Collision collision) => OnCollision(collision.gameObject, collisionEnterResponse);
        private void OnCollisionExit(Collision collision) => OnCollision(collision.gameObject, collisionExitResponse);
        private void OnCollisionStay(Collision collision) => OnCollision(collision.gameObject, collisionStayResponse);
    }
}
