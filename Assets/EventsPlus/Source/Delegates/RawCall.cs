using System;
using System.Reflection;
using UnityEngine;
using System.Linq.Expressions;
using RawArg = VisualEvent.RawArgument;
namespace VisualEvent
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Serialized form of a delegate that can contain predefined arguments</summary>
    [Serializable]
    public class RawCall : RawDelegate
    {
        //=======================
        // Variables
        //=======================
        /// <summary>Predefined argument data used if this delegate is marked as not dynamic</summary>
        [SerializeField]
        protected internal RawArgument[] m_arguments;
        /// <summary>Whether or not this delegate is invoked with passed in parameters from a <see cref="VisualDelegate"/> or if it contains predefined arguments</summary>
        [SerializeField]
        protected internal bool m_isDynamic;

        //=======================
        // Arguments
        //=======================
        /// <summary>Gets the predefined <see cref="m_arguments"/> list of the delegate call</summary>
        internal RawArgument[] arguments => m_arguments;

        /// <summary>Gets the <see cref="m_isDynamic"/> value</summary>
        internal bool isDynamic => m_isDynamic;
        public bool m_runtime;
        [SerializeField]
        string TargetType;

        //=======================
        // Initialization
        //=======================
        /// <summary>Initializes and deserializes the <see cref="RawDelegate.m_target"/> and <see cref="RawDelegate._member"/> information into an actual delegate using a <see cref="VisualDelegate"/> reference</summary>
        /// <param name="tPublisher">Publisher passed in the delegate used for automatic memory management</param>
        public virtual void initialize(VisualDelegate tPublisher)
        {
            if (m_target != null)
                delegateInstance = createDelegate(tPublisher, Utility.QuickDeseralizer(m_target.GetType(), methodData));
        }
        public override void initialize()
        {
            if (methodData?.Length > 0)
            {
                var deltype = Type.GetType(TargetType);
                var method = Utility.QuickDeseralizer(deltype, methodData);
                if (m_target != null)
                    delegateInstance = createDelegate(method, m_target);
                else
                    delegateInstance = createDelegate(method, Activator.CreateInstance(deltype));
            }
        }
        public RawCall(bool isruntime) => m_runtime = isruntime;
        //=======================
        // Delegate
        //=======================
        /// <summary>Creates a delegate instance using the <see cref="RawDelegate.m_target"/> and an input <see cref="System.Reflection.MemberInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMember">MemberInfo to be cast into a delegate</param>
        /// <returns>Member delegate if successful, null if not able to properly convert</returns>
        private protected virtual System.Delegate createDelegate(VisualDelegate tPublisher, MemberInfo tMember)
        {
            if (tMember != null)
            {
                switch (tMember.MemberType)
                {
                    case MemberTypes.Field:
                        FieldInfo tempField = tMember as FieldInfo;
                        if (m_isDynamic)
                        {
                            return this.GetType().GetMethod("createFieldAction", Utility.InstanceFlags).MakeGenericMethod(tempField.FieldType).Invoke(this, new object[] { tPublisher, m_target, tempField }) as System.Delegate;
                        }
                        var arg = m_arguments[0];
                        return this.GetType().GetMethod("createFieldCall", Utility.InstanceFlags).MakeGenericMethod(tempField.FieldType).Invoke(this, new object[] { tPublisher, m_target, tempField, arg }) as System.Delegate;
                    case MemberTypes.Property:
                        PropertyInfo tempProperty = tMember as PropertyInfo;
                        if (m_isDynamic)
                        {
                            return this.GetType().GetMethod("createPropertyAction", Utility.InstanceFlags).MakeGenericMethod(tempProperty.PropertyType).Invoke(this, new object[] { tPublisher, tempProperty }) as System.Delegate;
                        }
                        arg = m_arguments[0];
                        return this.GetType().GetMethod("createPropertyCall", Utility.InstanceFlags).MakeGenericMethod(tempProperty.PropertyType).Invoke(this, new object[] { tPublisher, tempProperty, arg }) as System.Delegate;
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
                        // Arguments
                        object[] tempArguments;
                        if (m_arguments == null || m_isDynamic)
                        {
                            tempArguments = new object[] { tPublisher, tempMethod };
                        }
                        else
                        {
                            tempArguments = new object[m_arguments.Length + 2];
                            tempArguments[0] = tPublisher;
                            tempArguments[1] = tempMethod;

                            for (int i = (m_arguments.Length - 1); i >= 0; --i)
                            {
                                tempArguments[i + 2] = m_arguments[i];
                            }
                        }

                        // Action
                        if (tempIsAction)
                        {
                            if (m_isDynamic)
                            {
                                return this.GetType().GetMethod("createAction" + tempParametersLength, Utility.InstanceFlags).MakeGenericMethod(tempParameterTypes).Invoke(this, tempArguments) as System.Delegate;
                            }

                            if (tempParametersLength == 0)
                            {
                                return this.GetType().GetMethod("createActionCall0", Utility.InstanceFlags).Invoke(this, tempArguments) as System.Delegate;
                            }
                            return this.GetType().GetMethod("createActionCall" + tempParametersLength, Utility.InstanceFlags).MakeGenericMethod(tempParameterTypes).Invoke(this, tempArguments) as System.Delegate;
                        }

                        // Func
                        tempParameterTypes[tempParametersLength] = tempMethod.ReturnType;
                        if (m_isDynamic)
                        {
                            return this.GetType().GetMethod("createFunc" + tempParametersLength, Utility.InstanceFlags).MakeGenericMethod(tempParameterTypes).Invoke(this, tempArguments) as System.Delegate;
                        }

                        return this.GetType().GetMethod("createFuncCall" + tempParametersLength, Utility.InstanceFlags).MakeGenericMethod(tempParameterTypes).Invoke(this, tempArguments) as System.Delegate;
                    default:
                        break;
                }
            }
            MemberTypes mem_type = (MemberTypes)int.Parse(methodData[0]);
            return new Action(() => Debug.LogError($@"Cannot find {mem_type} ""{methodData[1]}"" in class  ""{m_target.GetType().FullName}""+ 
                member was renamed or removed on gameobject ""{m_target.name}""", m_target));
        }
        public void AOTFIX()
        {
            createActionCall1<int>(null, null, null);
            createFieldAction<int>(null, null, null);
            createFieldCall<int>(null, null, null, default);
        }
        //=======================
        // Field
        //=======================
        /// <summary>Utility method for creating a field delegate from a <see cref="System.Reflection.FieldInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tTarget">Target owner of the <paramref name="tField"/></param>
        /// <param name="tField">FieldInfo used to generate a delegate</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A> createFieldAction<A>(VisualDelegate tPublisher, UnityEngine.Object target, FieldInfo tField)
        {
            var field_instance = Expression.Field(Expression.Constant(target), tField);
            var param_expression=Expression.Parameter(typeof(object),"Value");
            BinaryExpression assign_expression;
            if (typeof(A).IsValueType)
                assign_expression = Expression.Assign(field_instance, Expression.Unbox(param_expression, typeof(A)));
            else assign_expression = Expression.Assign(field_instance, param_expression);
            var boxed_lamda=Expression.Lambda<Action<object>>(assign_expression,param_expression).Compile();
            return val => boxed_lamda(val);

        }

        /// <summary>Utility method for creating a field delegate with a predefined argument value from a <see cref="System.Reflection.FieldInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tTarget">Target owner of the <paramref name="tField"/></param>
        /// <param name="tField">FieldInfo used to generate a delegate</param>
        /// <param name="tValue">Predefined argument value</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFieldCall<A>(VisualDelegate tPublisher, UnityEngine.Object tTarget, FieldInfo tField, RawArg arg1)
        {
            var field_instance = Expression.Field(Expression.Constant(tTarget), tField);
            var field_value_expression = Expression.Parameter(typeof(object), "Value");
            BinaryExpression setter_expression;
            if (typeof(A).IsValueType)
                setter_expression = Expression.Assign(field_instance, Expression.Unbox(field_value_expression, typeof(A)));
            else setter_expression=Expression.Assign(field_instance,field_value_expression);
            var setter_lamda = Expression.Lambda<Action<object>>(setter_expression,field_value_expression).Compile();
            var value = arg1.CreateArgumentDelegate<A>();
            return () => setter_lamda(value);
        }

        //=======================
        // Property
        //=======================
        /// <summary>Utility method for creating a property delegate from a <see cref="System.Reflection.PropertyInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tProperty">PropertyInfo used to generate a delegate</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A> createPropertyAction<A>(VisualDelegate tPublisher, PropertyInfo tProperty)
        {
            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), m_target, tProperty.GetSetMethod(), false) as Action<A>;
            Action<A> tempAction = (A tA) =>
            {
                if (m_target == null)
                    tPublisher.removeCall(this);
                else
                    tempDelegate(tA);
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a delegate from a <see cref="System.Reflection.PropertyInfo"/> that applies a predefined value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tProperty">PropertyInfo used to generate a delegate</param>
        /// <param name="tValue">Predefined argument value</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createPropertyCall<A>(VisualDelegate tPublisher, PropertyInfo tProperty, RawArg arg1)
        {
            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), m_target, tProperty.GetSetMethod(), false) as Action<A>;
            Func<A> property_input = arg1.CreateArgumentDelegate<A>();
            Action tempcall = () =>
             {
                 if (m_target == null)
                     tPublisher.removeCall(this);
                 else
                     tempDelegate(property_input());
             };
            return tempcall;
        }

        //=======================
        // Action
        //=======================
        /// <summary>Utility method for creating a method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action createActionCall0(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Action tempDelegate = Delegate.CreateDelegate(typeof(Action), m_target, tMethod, false) as Action;
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate();
                }
            };
            return tempAction;
        }

        /// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A> createAction1<A>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), m_target, tMethod, false) as Action<A>;
            Action<A> tempAction = (A tA) =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(tA);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies a predefined value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createActionCall1<A>(VisualDelegate tPublisher, MethodInfo tMethod, RawArg arg1)
        {
            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), m_target, tMethod, false) as Action<A>;
            Func<A> input = arg1.CreateArgumentDelegate<A>();
            Action tempaction = () =>
            {
                if (m_target == null)
                    tPublisher.removeCall(this);
                else tempDelegate(input());
            };
            return tempaction;
        }

        /// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 2-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B> createAction2<A, B>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Action<A, B> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B>), m_target, tMethod, false) as Action<A, B>;
            Action<A, B> tempAction = (A tA, B tB) =>
             {
                 if (m_target == null)
                 {
                     tPublisher.removeCall(this);
                 }
                 else
                 {
                     tempDelegate(tA, tB);
                 }
             };

            return tempAction;
        }

        /// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <returns>Generic 2-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createActionCall2<A, B>(VisualDelegate tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2)
        {
            Action<A, B> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B>), m_target, tMethod, false) as Action<A, B>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Action tempAction = () =>
            {
                if (m_target == null)
                    tPublisher.removeCall(this);
                else
                    tempDelegate(input_1(), input_2());
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 3-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MemberInfo used to generate a delegate</param>
        /// <returns>Generic 3-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C> createAction3<A, B, C>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Action<A, B, C> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C>), m_target, tMethod, false) as Action<A, B, C>;
            Action<A, B, C> tempAction = (A tA, B tB, C tC) =>
              {
                  if (m_target == null)
                  {
                      tPublisher.removeCall(this);
                  }
                  else
                  {
                      tempDelegate(tA, tB, tC);
                  }
              };

            return tempAction;
        }

        /// <summary>Utility method for creating a 3-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <returns>Generic 3-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createActionCall3<A, B, C>(VisualDelegate tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3)
        {
            Action<A, B, C> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C>), m_target, tMethod, false) as Action<A, B, C>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(input_1(), input_2(), input_3());
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 4-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 4-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D> createAction4<A, B, C, D>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Action<A, B, C, D> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D>), m_target, tMethod, false) as Action<A, B, C, D>;
            Action<A, B, C, D> tempAction = (A tA, B tB, C tC, D tD) =>
               {
                   if (m_target == null)
                   {
                       tPublisher.removeCall(this);
                   }
                   else
                   {
                       tempDelegate(tA, tB, tC, tD);
                   }
               };

            return tempAction;
        }

        /// <summary>Utility method for creating a 4-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <param name="tD">Predefined argument value</param>
        /// <returns>Generic 4-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createActionCall4<A, B, C, D>(VisualDelegate tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4)
        {
            Action<A, B, C, D> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D>), m_target, tMethod, false) as Action<A, B, C, D>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(input_1(), input_2(), input_3(), input_4());
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 5-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 5-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E> createAction5<A, B, C, D, E>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Action<A, B, C, D, E> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E>), m_target, tMethod, false) as Action<A, B, C, D, E>;
            Action<A, B, C, D, E> tempAction = (A tA, B tB, C tC, D tD, E tE) =>
                {
                    if (m_target == null)
                    {
                        tPublisher.removeCall(this);
                    }
                    else
                    {
                        tempDelegate(tA, tB, tC, tD, tE);
                    }
                };

            return tempAction;
        }

        /// <summary>Utility method for creating a 5-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <param name="tD">Predefined argument value</param>
        /// <param name="tE">Predefined argument value</param>
        /// <returns>Generic 5-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createActionCall5<A, B, C, D, E>(VisualDelegate tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5)
        {
            Action<A, B, C, D, E> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E>), m_target, tMethod, false) as Action<A, B, C, D, E>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Func<E> input_5 = arg5.CreateArgumentDelegate<E>();
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5());
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 6-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 6-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F> createAction6<A, B, C, D, E, F>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Action<A, B, C, D, E, F> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F>), m_target, tMethod, false) as Action<A, B, C, D, E, F>;
            Action<A, B, C, D, E, F> tempAction = (A tA, B tB, C tC, D tD, E tE, F tF) =>
                 {
                     if (m_target == null)
                     {
                         tPublisher.removeCall(this);
                     }
                     else
                     {
                         tempDelegate(tA, tB, tC, tD, tE, tF);
                     }
                 };

            return tempAction;
        }

        /// <summary>Utility method for creating a 6-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <param name="tD">Predefined argument value</param>
        /// <param name="tE">Predefined argument value</param>
        /// <param name="tF">Predefined argument value</param>
        /// <returns>Generic 6-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createActionCall6<A, B, C, D, E, F>(VisualDelegate tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6)
        {
            Action<A, B, C, D, E, F> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F>), m_target, tMethod, false) as Action<A, B, C, D, E, F>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Func<E> input_5 = arg5.CreateArgumentDelegate<E>();
            Func<F> input_6 = arg6.CreateArgumentDelegate<F>();
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6());
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 7-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 7-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F, G> createAction7<A, B, C, D, E, F, G>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Action<A, B, C, D, E, F, G> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F, G>), m_target, tMethod, false) as Action<A, B, C, D, E, F, G>;
            Action<A, B, C, D, E, F, G> tempAction = (A tA, B tB, C tC, D tD, E tE, F tF, G tG) =>
                  {
                      if (m_target == null)
                      {
                          tPublisher.removeCall(this);
                      }
                      else
                      {
                          tempDelegate(tA, tB, tC, tD, tE, tF, tG);
                      }
                  };

            return tempAction;
        }

        /// <summary>Utility method for creating a 7-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <param name="tD">Predefined argument value</param>
        /// <param name="tE">Predefined argument value</param>
        /// <param name="tF">Predefined argument value</param>
        /// <param name="tG">Predefined argument value</param>
        /// <returns>Generic 7-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createActionCall7<A, B, C, D, E, F, G>(VisualDelegate tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6, RawArg arg7)
        {
            Action<A, B, C, D, E, F, G> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F, G>), m_target, tMethod, false) as Action<A, B, C, D, E, F, G>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Func<E> input_5 = arg5.CreateArgumentDelegate<E>();
            Func<F> input_6 = arg6.CreateArgumentDelegate<F>();
            Func<G> input_7 = arg7.CreateArgumentDelegate<G>();
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6(), input_7());
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 8-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 8-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F, G, H> createAction8<A, B, C, D, E, F, G, H>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Action<A, B, C, D, E, F, G, H> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F, G, H>), m_target, tMethod, false) as Action<A, B, C, D, E, F, G, H>;
            Action<A, B, C, D, E, F, G, H> tempAction = (A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH) =>
                   {
                       if (m_target == null)
                       {
                           tPublisher.removeCall(this);
                       }
                       else
                       {
                           tempDelegate(tA, tB, tC, tD, tE, tF, tG, tH);
                       }
                   };

            return tempAction;
        }

        /// <summary>Utility method for creating a 8-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <param name="tD">Predefined argument value</param>
        /// <param name="tE">Predefined argument value</param>
        /// <param name="tF">Predefined argument value</param>
        /// <param name="tG">Predefined argument value</param>
        /// <param name="tH">Predefined argument value</param>
        /// <returns>Generic 8-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createActionCall8<A, B, C, D, E, F, G, H>(VisualDelegate tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6, RawArg arg7, RawArg arg8)
        {
            Action<A, B, C, D, E, F, G, H> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F, G, H>), m_target, tMethod, false) as Action<A, B, C, D, E, F, G, H>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Func<E> input_5 = arg5.CreateArgumentDelegate<E>();
            Func<F> input_6 = arg6.CreateArgumentDelegate<F>();
            Func<G> input_7 = arg7.CreateArgumentDelegate<G>();
            Func<H> input_8 = arg8.CreateArgumentDelegate<H>();
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6(), input_7(), input_8());
                }
            };

            return tempAction;
        }
        //=======================
        // Func
        //=======================
        /// <summary>Utility method for creating a method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFuncCall0<T>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Func<T> tempDelegate = Delegate.CreateDelegate(typeof(Func<T>), m_target, tMethod, false) as Func<T>;
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate();
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A> createFunc1<A, T>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Func<A, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, T>), m_target, tMethod, false) as Func<A, T>;
            Action<A> tempAction = (A tA) =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(tA);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies a predefined value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFuncCall1<A, T>(VisualDelegate tPublisher, MethodInfo tMethod, RawArg arg1)
        {

            Func<A, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, T>), m_target, tMethod, false) as Func<A, T>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Action tempaction = () =>
               {
                   if (m_target == null)
                       tPublisher.removeCall(this);
                   else
                       tempDelegate(input_1());
               };
            return tempaction;
        }

        /// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B> createFunc2<A, B, T>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Func<A, B, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, T>), m_target, tMethod, false) as Func<A, B, T>;
            Action<A, B> tempAction = (A tA, B tB) =>
             {
                 if (m_target == null)
                 {
                     tPublisher.removeCall(this);
                 }
                 else
                 {
                     tempDelegate(tA, tB);
                 }
             };

            return tempAction;
        }

        /// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <returns>Generic 2-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFuncCall2<A, B, T>(VisualDelegate tPublisher, MethodInfo tMethod, A tA, B tB)
        {
            Func<A, B, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, T>), m_target, tMethod, false) as Func<A, B, T>;
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(tA, tB);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 3-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C> createFunc3<A, B, C, T>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Func<A, B, C, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, T>), m_target, tMethod, false) as Func<A, B, C, T>;
            Action<A, B, C> tempAction = (A tA, B tB, C tC) =>
              {
                  if (m_target == null)
                  {
                      tPublisher.removeCall(this);
                  }
                  else
                  {
                      tempDelegate(tA, tB, tC);
                  }
              };

            return tempAction;
        }

        /// <summary>Utility method for creating a 3-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <returns>Generic 3-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFuncCall3<A, B, C, T>(VisualDelegate tPublisher, MethodInfo tMethod, A tA, B tB, C tC)
        {
            Func<A, B, C, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, T>), m_target, tMethod, false) as Func<A, B, C, T>;
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(tA, tB, tC);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 4-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D> createFunc4<A, B, C, D, T>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Func<A, B, C, D, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, T>), m_target, tMethod, false) as Func<A, B, C, D, T>;
            Action<A, B, C, D> tempAction = (A tA, B tB, C tC, D tD) =>
               {
                   if (m_target == null)
                   {
                       tPublisher.removeCall(this);
                   }
                   else
                   {
                       tempDelegate(tA, tB, tC, tD);
                   }
               };

            return tempAction;
        }

        /// <summary>Utility method for creating a 4-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <param name="tD">Predefined argument value</param>
        /// <returns>Generic 4-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFuncCall4<A, B, C, D, T>(VisualDelegate tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD)
        {
            Func<A, B, C, D, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, T>), m_target, tMethod, false) as Func<A, B, C, D, T>;
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(tA, tB, tC, tD);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 5-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E> createFunc5<A, B, C, D, E, T>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Func<A, B, C, D, E, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, T>), m_target, tMethod, false) as Func<A, B, C, D, E, T>;
            Action<A, B, C, D, E> tempAction = (A tA, B tB, C tC, D tD, E tE) =>
                {
                    if (m_target == null)
                    {
                        tPublisher.removeCall(this);
                    }
                    else
                    {
                        tempDelegate(tA, tB, tC, tD, tE);
                    }
                };

            return tempAction;
        }

        /// <summary>Utility method for creating a 5-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <param name="tD">Predefined argument value</param>
        /// <param name="tE">Predefined argument value</param>
        /// <returns>Generic 5-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFuncCall5<A, B, C, D, E, T>(VisualDelegate tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE)
        {
            Func<A, B, C, D, E, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, T>), m_target, tMethod, false) as Func<A, B, C, D, E, T>;
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(tA, tB, tC, tD, tE);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 6-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F> createFunc6<A, B, C, D, E, F, T>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Func<A, B, C, D, E, F, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, T>;
            Action<A, B, C, D, E, F> tempAction = (A tA, B tB, C tC, D tD, E tE, F tF) =>
                 {
                     if (m_target == null)
                     {
                         tPublisher.removeCall(this);
                     }
                     else
                     {
                         tempDelegate(tA, tB, tC, tD, tE, tF);
                     }
                 };

            return tempAction;
        }

        /// <summary>Utility method for creating a 6-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <param name="tD">Predefined argument value</param>
        /// <param name="tE">Predefined argument value</param>
        /// <param name="tF">Predefined argument value</param>
        /// <returns>Generic 6-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFuncCall6<A, B, C, D, E, F, T>(VisualDelegate tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF)
        {
            Func<A, B, C, D, E, F, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, T>;
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(tA, tB, tC, tD, tE, tF);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 7-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F, G> createFunc7<A, B, C, D, E, F, G, T>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Func<A, B, C, D, E, F, G, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, G, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, G, T>;
            Action<A, B, C, D, E, F, G> tempAction = (A tA, B tB, C tC, D tD, E tE, F tF, G tG) =>
                  {
                      if (m_target == null)
                      {
                          tPublisher.removeCall(this);
                      }
                      else
                      {
                          tempDelegate(tA, tB, tC, tD, tE, tF, tG);
                      }
                  };

            return tempAction;
        }

        /// <summary>Utility method for creating a 7-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <param name="tD">Predefined argument value</param>
        /// <param name="tE">Predefined argument value</param>
        /// <param name="tF">Predefined argument value</param>
        /// <param name="tG">Predefined argument value</param>
        /// <returns>Generic 7-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFuncCall7<A, B, C, D, E, F, G, T>(VisualDelegate tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF, G tG)
        {
            Func<A, B, C, D, E, F, G, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, G, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, G, T>;
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(tA, tB, tC, tD, tE, tF, tG);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 8-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F, G, H> createFunc8<A, B, C, D, E, F, G, H, T>(VisualDelegate tPublisher, MethodInfo tMethod)
        {
            Func<A, B, C, D, E, F, G, H, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, G, H, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, G, H, T>;
            Action<A, B, C, D, E, F, G, H> tempAction = (A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH) =>
                   {
                       if (m_target == null)
                       {
                           tPublisher.removeCall(this);
                       }
                       else
                       {
                           tempDelegate(tA, tB, tC, tD, tE, tF, tG, tH);
                       }
                   };

            return tempAction;
        }

        /// <summary>Utility method for creating a 8-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <param name="tB">Predefined argument value</param>
        /// <param name="tC">Predefined argument value</param>
        /// <param name="tD">Predefined argument value</param>
        /// <param name="tE">Predefined argument value</param>
        /// <param name="tF">Predefined argument value</param>
        /// <param name="tG">Predefined argument value</param>
        /// <param name="tH">Predefined argument value</param>
        /// <returns>Generic 8-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFuncCall8<A, B, C, D, E, F, G, H, T>(VisualDelegate tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH)
        {
            Func<A, B, C, D, E, F, G, H, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, G, H, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, G, H, T>;
            Action tempAction = () =>
            {
                if (m_target == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tempDelegate(tA, tB, tC, tD, tE, tF, tG, tH);
                }
            };

            return tempAction;
        }
    }
}
