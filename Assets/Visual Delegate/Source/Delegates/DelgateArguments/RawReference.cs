using UnityEngine;
using System.Linq.Expressions;
using System;
using System.Reflection;
namespace VisualDelegates
{
    [System.Serializable]
    public class RawReference : RawDelegate
    {
        [SerializeField] bool m_isvaluetype;
        [SerializeField] bool isparentargstring;
        public bool ParentArgString => isparentargstring;
        [SerializeField] bool m_isDelegate;
        public bool isDelegate => m_isDelegate;
        [SerializeField] bool willdeseralize;
        const string FIELD_GETTER_NAME = "CreateFieldGetter";
        const string TO_STRING_DELEGATE = "ToStringDelegate";
        protected sealed override void Deserialization()
        {
            if (willdeseralize)
                delegateInstance = createDelegate(Utility.QuickDeseralizer(m_target.GetType(), methodData, out paramtypes), m_target);
        }
        private protected sealed override Delegate createDelegate(MemberInfo tMember, object target)
        {
            if (tMember is FieldInfo field_info)
            {
                if(!Utility.DelegateFieldGetterCreationMethods.TryGetValue(paramtypes,out MethodInfo getfieldmethod))
                {
                    getfieldmethod = typeof(RawReference).GetMethod(FIELD_GETTER_NAME, Utility.memberBinding).MakeGenericMethod(paramtypes);
                    Utility.DelegateFieldGetterCreationMethods.Add(paramtypes, getfieldmethod);
                }
                return getfieldmethod.Invoke(this, new object[] { m_target, field_info }) as Delegate;
            }
            else if (tMember is PropertyInfo prop_info)
            {
                var getter = Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(paramtypes), target, prop_info.GetGetMethod(), false);
                if (isparentargstring)
                {
                    if (!Utility.delegatePropertyGetterCreationMethod.TryGetValue(paramtypes, out MethodInfo conversionmethod))
                    {
                        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
                        conversionmethod = typeof(RawDelegate).GetMethod(TO_STRING_DELEGATE, flags).MakeGenericMethod(prop_info.PropertyType);
                        Utility.delegatePropertyGetterCreationMethod.Add(paramtypes, conversionmethod);
                    }
                    getter = conversionmethod.Invoke(this, new object[] { getter }) as Func<string>;
                }
                return getter;
            }
            else return base.createDelegate(tMember, target);
        }
      
    }

}
