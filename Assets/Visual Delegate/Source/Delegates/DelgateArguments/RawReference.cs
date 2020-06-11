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
        [SerializeField] bool m_isDelegate;
        [SerializeField] bool willdeseralize;
        const string FIELD_GETTER_NAME = "CreateFieldGetter";
        protected sealed override void Deserialization()
        {
            if (willdeseralize)
                delegateInstance = createDelegate(Utility.QuickDeseralizer(m_target.GetType(), methodData, out paramtypes), m_target);
        }
        private protected sealed override Delegate createDelegate(MemberInfo tMember, object target)
        {
            if (tMember is FieldInfo field_info)
            {
                if(!Utility.DelegateFieldReturnCreationMethods.TryGetValue(paramtypes,out MethodInfo getfieldmethod))
                {
                    getfieldmethod = GetType().GetMethod(FIELD_GETTER_NAME, Utility.memberBinding).MakeGenericMethod(field_info.FieldType);
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
        private Delegate CreateFieldGetter<A>(UnityEngine.Object target, FieldInfo info)
        {
            var targetExpression = Expression.Constant(target);
            var fieldexprssion = Expression.Field(targetExpression, info);
            var boxed_object = Expression.Convert(fieldexprssion, typeof(object));
            var boxed_field = Expression.Lambda<Func<object>>(boxed_object);
            var FieldGetter = boxed_field.Compile();
            if (isparentargstring)
                return new Func<string>(() => FieldGetter.Invoke().ToString());
            else
            {
                Func<A> safegetter = () => (A)FieldGetter.Invoke();
                if (m_isDelegate)
                {
                    Func<Func<A>> nested_func = () => safegetter;
                    return nested_func;
                }
                else return safegetter;
            }
        }
    }

}
