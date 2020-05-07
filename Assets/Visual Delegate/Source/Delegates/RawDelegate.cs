using System;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;
namespace VisualEvent
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Serialized form of a delegate</summary>
    [Serializable]
    public abstract class RawDelegate : ISerializationCallbackReceiver
    {
        //=======================
        // Variables
        //=======================
        /// <summary>Target owner of the delegate's member</summary>
        [SerializeField]
        private protected UnityEngine.Object m_target;
        /// <summary>Serialized name of the target member</summary>
        [SerializeField]
        private protected string[] methodData;
        /// <summary>Cached delegate instance generated upon initialization</summary>
        public System.Delegate delegateInstance { get; internal set; }
        [SerializeField]
        protected bool m_isYieldableCall;
        protected Type[] paramtypes;
        [SerializeField] internal bool haserror;
        public bool isYieldable => m_isYieldableCall;
        [SerializeField] private bool serializationError = false;
        /// <summary>
        /// Checks delegate for potential memory leaks 
        /// </summary>
        /// <returns></returns>
        public virtual bool isDelegateLeaking() => delegateInstance?.Target== null;
        /// <summary>
        /// Creates a new type array that contains the return type of the method for delegates that will be funcs
        /// </summary>
        /// <param name="method_info"></param>
        /// <returns></returns>
        private protected Type[] CreateFuncParam(MethodInfo method_info)
        {
            int length = paramtypes?.Length ?? 0;
            Type[] func_paramtypes = new Type[length + 1];
            if (paramtypes != null)
                paramtypes.CopyTo(func_paramtypes, 0);
            func_paramtypes[func_paramtypes.Length - 1] = method_info.ReturnType;
            return func_paramtypes;
        }

        /// <summary>Creates a delegate instance using the <see cref="m_target"/> and an input <see cref="System.Reflection.MemberInfo"/></summary>
        /// <param name="tMember">MemberInfo to be cast into a delegate</param>
        /// <returns>Member delegate if successful, null if not able to properly convert</returns>
        private protected virtual System.Delegate createDelegate(MemberInfo tMember, object target)
        {
            if (tMember != null)
            {
                switch (tMember.MemberType)
                {
                    case MemberTypes.Field:
                        FieldInfo tempField = tMember as FieldInfo;
                        return typeof(RawDelegate).GetMethod("CreateFieldAction", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(tempField.FieldType).Invoke(this, new object[] { target, tempField }) as System.Delegate;
                    case MemberTypes.Property:
                        PropertyInfo tempProperty = tMember as PropertyInfo;
                        return Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(tempProperty.PropertyType), target, tempProperty.GetSetMethod(), false);
                    case MemberTypes.Method:
                        MethodInfo tempMethod = tMember as MethodInfo;
                        bool tempIsAction = tempMethod.ReturnType == typeof(void);

                        // Parameters
                        int tempParametersLength = paramtypes?.Length ?? 0;
                        // Action
                        if (tempIsAction)
                        {
                            switch (tempParametersLength)
                            {
                                case 0:
                                    return Delegate.CreateDelegate(typeof(Action), target, tempMethod, false);
                                case 1:
                                    return Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(paramtypes), target, tempMethod, false);
                                case 2:
                                    return Delegate.CreateDelegate(typeof(Action<,>).MakeGenericType(paramtypes), target, tempMethod, false);
                                case 3:
                                    return Delegate.CreateDelegate(typeof(Action<,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                                case 4:
                                    return Delegate.CreateDelegate(typeof(Action<,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                                case 5:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                                case 6:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                                case 7:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                                case 8:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                                case 9:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                                case 10:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                                default:
                                    break;
                            }

                            return null;
                        }
                        switch (tempParametersLength-1)
                        {
                            case 0:
                                return Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(paramtypes), target, tempMethod, false);
                            case 1:
                                return Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(paramtypes), target, tempMethod, false);
                            case 2:
                                return Delegate.CreateDelegate(typeof(Func<,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                            case 3:
                                return Delegate.CreateDelegate(typeof(Func<,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                            case 4:
                                return Delegate.CreateDelegate(typeof(Func<,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                            case 5:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                            case 6:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                            case 7:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                            case 8:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                            case 9:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                            case 10:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,,,,,,>).MakeGenericType(paramtypes), target, tempMethod, false);
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }

            return null;
        }

        //=======================
        // Field
        //=======================
        /// <summary>Utility function for creating a field delegate from a <see cref="System.Reflection.FieldInfo"/></summary>
        /// <param name="tTarget">Target owner of the <paramref name="tField"/></param>
        /// <param name="tField">FieldInfo used to generate a delegate</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A> CreateFieldAction<A>(UnityEngine.Object target, FieldInfo tField)
        {
            var field_instance = Expression.Field(Expression.Constant(target), tField);
            var param_expression = Expression.Parameter(typeof(object), "Value");
            BinaryExpression assign_expression;
            if (typeof(A).IsValueType)
                assign_expression = Expression.Assign(field_instance, Expression.Unbox(param_expression, typeof(A)));
            else assign_expression = Expression.Assign(field_instance, param_expression);
            var boxed_lamda = Expression.Lambda<Action<object>>(assign_expression, param_expression).Compile();
            return val => boxed_lamda(val);

        }
        protected virtual Delegate CreateFieldGetter<A>(UnityEngine.Object target, FieldInfo info)
        {
            var targetExpression = Expression.Constant(target);
            var fieldexprssion = Expression.Field(targetExpression, info);
            var boxed_object = Expression.Convert(fieldexprssion, typeof(object));
            var boxed_field = Expression.Lambda<Func<object>>(boxed_object);
            var FieldGetter = boxed_field.Compile();
            if ((this as RawReference).isparentargumentstring == true)
                return new Func<string>(() => FieldGetter.Invoke().ToString());
            else
            {
                Func<A> safegetter = () => (A)FieldGetter.Invoke();
                if ((this as RawReference).isDelegate)
                {
                    Func<Func<A>> nested_func = () => safegetter;
                    return nested_func;
                }
                else return safegetter;
            }
        }
        public virtual void Release()
        {
            delegateInstance = null;
            m_target = null;
        }
        public void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
            try
            {
                if (!serializationError)
                    delegateInstance = createDelegate(Utility.QuickDeseralizer(m_target.GetType(), methodData, out paramtypes), m_target);
                else delegateInstance = new Action(() => Debug.LogError(Utility.CreateDelegateErrorMessage(methodData,m_target),m_target));
            }
            catch (Exception)
            {
                delegateInstance = new Action(() => Debug.LogError(Utility.CreateDelegateErrorMessage(methodData, m_target),m_target));
            }
            
        }
    }
}