using System;
using UnityEngine;
namespace EventsPlus
{
    public class RawDynamicDelegate : RawDelegate
    {
        [SerializeField]
        string TargetType, TargetName;
        public override void initialize()
        {
            if (methodData.Length > 0)
            {
                var deltype = Type.GetType(TargetType);
                var method = Utility.QuickDeseralizer(deltype, methodData);
                if (m_target != null)
                    delegateInstance = createDelegate(method, m_target);
                else
                    delegateInstance = createDelegate(method, Activator.CreateInstance(deltype));
            }
        }
        public RawDynamicDelegate(Delegate new_delegate) => delegateInstance = new_delegate;
    }
}

