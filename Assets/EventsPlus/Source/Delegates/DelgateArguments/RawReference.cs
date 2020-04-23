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

        private void fixer()
        {
            CreateFieldGetter<int>(null, null);
        }
        public override void OnAfterDeserialize()
        {
            if (willdeseralize)
                delegateInstance = createDelegate(Utility.QuickDeseralizer(m_target.GetType(), methodData, out paramtypes), m_target);
            else Debug.LogWarning("skip me");
        }
        private protected sealed override Delegate createDelegate(MemberInfo tMember, object target)
        {
            if (tMember is FieldInfo field_info)
            {
                return typeof(RawReference).GetMethod("CreateFieldGetter", Utility.memberBinding).MakeGenericMethod(field_info.FieldType)
                    .Invoke(this, new object[] { target, field_info }) as System.Delegate;
            }
            else if (tMember is PropertyInfo prop_info)
            {
                return Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(prop_info.PropertyType), target, prop_info.GetGetMethod(), false);
            }
            else return base.createDelegate(tMember, target);
        }
        protected virtual Delegate CreateFieldGetter<A>(UnityEngine.Object target, FieldInfo info)
        {
            var targetExpression = Expression.Constant(target);
            var fieldexprssion = Expression.Field(targetExpression, info);
            var boxed_object = Expression.Convert(fieldexprssion, typeof(object));
            var boxed_field = Expression.Lambda<Func<object>>(boxed_object);
            var FieldGetter = boxed_field.Compile();
            Debug.Log((this as RawReference)?.isparentargumentstring);
            if ((this as RawReference)?.isparentargumentstring == true)
                return new Func<string>(() => FieldGetter.Invoke().ToString());
            else
            {
                Func<A> safegetter = () => (A)FieldGetter.Invoke();
                if (this is RawReference rawref && rawref.isDelegate)
                {
                    Debug.LogWarning("is del");
                    Func<Func<A>> nested_func = () => safegetter;
                    return nested_func;
                }
                else return safegetter;
            }

        }
    }
}
