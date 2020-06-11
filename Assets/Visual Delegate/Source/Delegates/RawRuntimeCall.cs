using System;
using UnityEngine;
namespace VisualDelegates
{
    public class RawRuntimeCall : RawDelegate
    {
        [SerializeField]
        string TargetType;
        [SerializeField]
        bool isUnityTarget;
        public RawRuntimeCall(Delegate new_delegate) => delegateInstance = new_delegate;
        protected sealed override void Deserialization()
        {
            if (Application.isEditor && methodData.Length > 0)
            {
                var deltype = Type.GetType(TargetType);
                var method = Utility.QuickDeseralizer(deltype, methodData, out paramtypes);
                if (isUnityTarget)
                    delegateInstance = createDelegate(method, m_target);
                else
                    delegateInstance = createDelegate(method, Activator.CreateInstance(deltype));
            }
        }
    }
}

