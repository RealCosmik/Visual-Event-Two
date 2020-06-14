using UnityEditor;
namespace VisualDelegates.Editor
{
    [CustomEditor(typeof(CollisionHook2D))]
    class CollisionHook2DDrawer: CollisionHookDrawer
    {
        protected override void SetHookViews(MonoScript[] allscripts)
        {
            allCollsionHooks.Add(new HookResponseView(serializedObject.FindProperty("collisionEnterResponse"), "OnCollisionEnter2D", allscripts));
            allCollsionHooks.Add(new HookResponseView(serializedObject.FindProperty("collisionStayResponse"), "OnCollisionStay2D", allscripts));
            allCollsionHooks.Add(new HookResponseView(serializedObject.FindProperty("collisionExitResponse"), "OnCollisionExit2D", allscripts));
        }
    }
}

