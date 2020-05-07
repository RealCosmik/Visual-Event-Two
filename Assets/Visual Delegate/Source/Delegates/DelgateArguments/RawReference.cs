using UnityEngine;
using System.Linq.Expressions;
using System;
using System.Reflection;
namespace VisualEvent
{
    [System.Serializable]
    public class RawReference : RawDelegate
    {
        [SerializeField] bool m_isvaluetype;
        [SerializeField] bool isparentargstring;
        [SerializeField] bool m_isDelegate;
        [SerializeField] bool willdeseralize;
        public bool isDelegate => m_isDelegate;
        public bool isvaluetype => m_isvaluetype;
        public bool isparentargumentstring => isparentargstring;
        const string FIELD_GETTER_NAME = "CreateFieldGetter";
        public override void OnAfterDeserialize()
        {
            if (willdeseralize)
            {
                delegateInstance = createDelegate(Utility.QuickDeseralizer(m_target.GetType(), methodData, out paramtypes), m_target);
            }
        }
        private protected sealed override Delegate createDelegate(MemberInfo tMember, object target)
        {
            if (tMember is FieldInfo field_info)
            {
                if(!Utility.DelegateFieldReturnCreationMethods.TryGetValue(paramtypes,out MethodInfo getfieldmethod))
                {
                    getfieldmethod = typeof(RawDelegate).GetMethod(FIELD_GETTER_NAME, Utility.memberBinding).MakeGenericMethod(field_info.FieldType);
                    Utility.DelegateFieldReturnCreationMethods.Add(paramtypes, getfieldmethod);
                }
                return getfieldmethod.Invoke(this, new object[] { m_target, field_info }) as Delegate;
            }
            else if (tMember is PropertyInfo prop_info)
            {
                return Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(prop_info.PropertyType), target, prop_info.GetGetMethod(), false);
            }
            else return base.createDelegate(tMember, target);
        }
       
    }
}
