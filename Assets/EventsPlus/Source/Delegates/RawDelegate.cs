using System;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;
namespace EventsPlus
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
        [SerializeField]
        private protected string GenericTypeName;
        /// <summary>Cached delegate instance generated upon initialization</summary>
        public System.Delegate delegateInstance { get; private protected set; }
        /// <summary>Gets the <see cref="m_target"/> object of the delegate</summary>
        public UnityEngine.Object target => m_target;

        //=======================
        // Initialization
        //=======================
        /// <summary>Initializes and deserializes the <see cref="m_target"/> and <see cref="_member"/> information into an actual delegate</summary>
        public virtual void initialize()
        {
            if (m_target != null)
                delegateInstance = createDelegate(Utility.QuickDeseralizer(m_target.GetType(), methodData), m_target);
            else throw new UnityException("cannot create delegate with null target");
        }
        private void fixer()
        {
            CreateFieldGetter<UtilitySO, int>(null, null);
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
                            return typeof(RawDelegate).GetMethod("CreateFieldGetter", Utility.memberBinding).MakeGenericMethod(tempField.ReflectedType, tempField.FieldType).Invoke(this, new object[] { target, tempField }) as Delegate;
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
        protected virtual Action<A> CreateFieldAction<T, A>(T tTarget, FieldInfo tField)
        {
#if UNITY_IOS
				Action<A> tempAction = ( A tA ) =>
				{
					tField.SetValue( tTarget, tA );
				};
				
				return tempAction;
#else
            Action<T, A> tempSetter = CreateFieldSetter<T, A>(tField);
            Action<A> tempAction = (A tA) =>
            {
                tempSetter(tTarget, tA);
            };

            return tempAction;
#endif
        }
        protected virtual Delegate CreateFieldGetter<T, A>(T target, FieldInfo info)
        {
            Debug.Log("wow");
            var refdel = this as RawReference;
            var targetExpression = Expression.Constant(target, typeof(T));
            var fieldexprssion = Expression.Field(targetExpression, info);
            var isvaluetype = refdel.isvaluetype;
            Delegate FieldGetter;
            if (isvaluetype)
            {
                Debug.Log("value typing");
                var boxed_object = Expression.Convert(fieldexprssion, typeof(object));
                var boxed_field = Expression.Lambda<Func<object>>(boxed_object);
                FieldGetter = boxed_field.Compile();
            }
            else
            {
                Debug.Log("non val");
                var field_object = Expression.Convert(fieldexprssion, typeof(A));
                var get_field = Expression.Lambda<Func<A>>(field_object);
                FieldGetter = get_field.Compile();
            }
            if (refdel.isparentargumentstring)
            {
                if (isvaluetype)
                {
                    var fieldfunc = FieldGetter as Func<object>;
                    return new Func<string>(() => fieldfunc.Invoke().ToString());
                }
                else
                {
                    var fieldfunc = FieldGetter as Func<A>;
                    Debug.Log(fieldfunc == null);
                    return new Func<string>(() => fieldfunc.Invoke().ToString());
                }
            }
            else
            {
                if (isvaluetype)
                {
                    var fieldfunc = FieldGetter as Func<object>;
                    var newdel = new Func<A>(() => (A)fieldfunc.Invoke());
                    return newdel;
                }
                return FieldGetter;
            }
        }

#if !UNITY_IOS
        /// <summary>Utility function for creating a field delegate from a <see cref="System.Reflection.FieldInfo"/></summary>
        /// <param name="tField">FieldInfo used to generate a delegate</param>
        /// <returns>Generic 2-parameter action delegate if successful, null if not able to convert</returns>
        protected static Action<T, A> CreateFieldSetter<T, A>(FieldInfo tField)
        {
            return null;
            //DynamicMethod tempMethod = new DynamicMethod( ( tField.ReflectedType.FullName + ".set_" + tField.Name ), null, new Type[2] { tField.ReflectedType, tField.FieldType }, true );

            //ILGenerator tempGenerator = tempMethod.GetILGenerator();
            //tempGenerator.Emit( OpCodes.Ldarg_0 );
            //tempGenerator.Emit( OpCodes.Ldarg_1 );
            //tempGenerator.Emit( OpCodes.Stfld, tField );
            //tempGenerator.Emit( OpCodes.Ret );

            //return tempMethod.CreateDelegate( typeof( Action<T,A> ) ) as Action<T,A>;
        }

#endif
    }
}