using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
namespace VisualDelegates.Editor
{
    public abstract class CollisionHookDrawer : UnityEditor.Editor
    {
       static MonoScript[] allscripts;
       protected List<HookResponseView> allCollsionHooks;
        private void OnEnable()
        { 
            if (allscripts == null)
                allscripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            if (allCollsionHooks == null)
                allCollsionHooks = new List<HookResponseView>();
            SetHookViews(allscripts);
           
        }
        protected abstract void SetHookViews(MonoScript[] allscripts);
        public override void OnInspectorGUI()
        {
            var length = allCollsionHooks.Count;
            for (int i = 0; i < length; i++)
            {
                allCollsionHooks[i].LayoutHookResponse();
            }
        }
    }


}
