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
    public class RawDelegate
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
        public System.Delegate delegateInstance { get; set; }
        /// <summary>Gets the <see cref="m_target"/> object of the delegate</summary>
        public UnityEngine.Object target => m_target;
        [SerializeField]
        protected bool isStatic;
        [SerializeField]
        bool m_isYieldableCall;
        public bool isYieldable => m_isYieldableCall;

        protected Type[] paramtypes;

        //=======================
        // Initialization
        //=======================
        /// <summary>Initializes and deserializes the <see cref="m_target"/> and <see cref="_member"/> information into an actual delegate</summary>
        public virtual void initialize()
        {
            if (m_target != null)
                delegateInstance = createDelegate(Utility.QuickDeseralizer(m_target.GetType(), methodData, out paramtypes), m_target);
            else if (isStatic)
                delegateInstance = createDelegate(Utility.QuickDeseralizer(typeof(UtilHelper), methodData, out paramtypes),null);
            else throw new UnityException("cannot create delegate with null target");
        }
        private void fixer()
        {
            CreateFieldGetter<int>(null, null);
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
                        if (methodData[2] == "GET")
                            return typeof(RawDelegate).GetMethod("CreateFieldGetter", Utility.memberBinding).MakeGenericMethod(tempField.FieldType).Invoke(this, new object[] { target, tempField }) as Delegate;
                        else return typeof(RawDelegate).GetMethod("CreateFieldAction", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(tempField.ReflectedType, tempField.FieldType).Invoke(this, new object[] { target, tempField }) as System.Delegate;
                    case MemberTypes.Property:
                        PropertyInfo tempProperty = tMember as PropertyInfo;
                        return Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(tempProperty.PropertyType), target, tempProperty.GetSetMethod(), false);
                    case MemberTypes.Method:
                        MethodInfo tempMethod = tMember as MethodInfo;
                        bool tempIsAction = tempMethod.ReturnType == typeof(void);

                        // Parameters
                        ParameterInfo[] tempParameters = tempMethod.GetParameters();
                        int tempParametersLength = tempParameters.Length;
                        Type[] tempParameterTypes = null;
                        if (tempParametersLength > 0)
                        {
                            tempParameterTypes = new Type[tempParametersLength + (tempIsAction ? 0 : 1)];
                            for (int i = (tempParametersLength - 1); i >= 0; --i)
                            {
                                tempParameterTypes[i] = tempParameters[i].ParameterType;
                            }
                        }
                        else if (!tempIsAction)
                        {
                            tempParameterTypes = new Type[1];
                        }

                        // Action
                        if (tempIsAction)
                        {
                            switch (tempParametersLength)
                            {
                                case 0:
                                    return Delegate.CreateDelegate(typeof(Action), target, tempMethod, false);
                                case 1:
                                    return Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                                case 2:
                                    return Delegate.CreateDelegate(typeof(Action<,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                                case 3:
                                    return Delegate.CreateDelegate(typeof(Action<,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                                case 4:
                                    return Delegate.CreateDelegate(typeof(Action<,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                                case 5:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                                case 6:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                                case 7:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                                case 8:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                                case 9:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                                case 10:
                                    return Delegate.CreateDelegate(typeof(Action<,,,,,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                                default:
                                    break;
                            }

                            return null;
                        }

                        // Func
                        tempParameterTypes[tempParametersLength] = tempMethod.ReturnType;

                        switch (tempParametersLength)
                        {
                            case 0:
                                return Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                            case 1:
                                return Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                            case 2:
                                return Delegate.CreateDelegate(typeof(Func<,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                            case 3:
                                return Delegate.CreateDelegate(typeof(Func<,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                            case 4:
                                return Delegate.CreateDelegate(typeof(Func<,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                            case 5:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                            case 6:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                            case 7:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                            case 8:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                            case 9:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
                            case 10:
                                return Delegate.CreateDelegate(typeof(Func<,,,,,,,,,,>).MakeGenericType(tempParameterTypes), target, tempMethod, false);
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