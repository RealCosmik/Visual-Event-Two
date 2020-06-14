using UnityEditor;
namespace VisualDelegates.Editor
{
    [CustomEditor(typeof(CollisionHook3D))]
    class CollisionHook3DDrawer : CollisionHookDrawer
    {
        protected override void SetHookViews(MonoScript[] allscripts)
        {
            allCollsionHooks.Add(new HookResponseView(serializedObject.FindProperty("collisionEnterResponse"), "OnCollisionEnter", allscripts));
            allCollsionHooks.Add(new HookResponseView(serializedObject.FindProperty("collisionStayResponse"), "OnCollisionStay", allscripts));
            allCollsionHooks.Add(new HookResponseView(serializedObject.FindProperty("collisionExitResponse"), "OnCollisionExit", allscripts));
        }
    }
}
