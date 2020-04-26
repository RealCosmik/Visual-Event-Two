using System;
using UnityEngine;
namespace VisualEvent
{
    public class RawRuntimeDelegate : RawDelegate
    {
        [SerializeField]
        string TargetType;
        public sealed override void OnAfterDeserialize()
        {
            if (methodData.Length > 0)
            {
                var deltype = Type.GetType(TargetType);
                var method = Utility.QuickDeseralizer(deltype, methodData, out paramtypes);
                if (isUnityTarget)
                    delegateInstance = createDelegate(method, m_target);
                else
                    delegateInstance = createDelegate(method, Activator.CreateInstance(deltype));
            }
        }
        public RawRuntimeDelegate(Delegate new_delegate) => delegateInstance = new_delegate;
    }
}

