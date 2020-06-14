using UnityEngine;
namespace VisualDelegates
{
    public class CollisionHook2D : CollisionHook
    {
        private void OnCollisionEnter2D(Collision2D collision) => OnCollision(collision.gameObject, collisionEnterResponse);
        private void OnCollisionExit2D(Collision2D collision) => OnCollision(collision.gameObject, collisionExitResponse);
        private void OnCollisionStay2D(Collision2D collision) => OnCollision(collision.gameObject, collisionStayResponse);
       
    }
}
