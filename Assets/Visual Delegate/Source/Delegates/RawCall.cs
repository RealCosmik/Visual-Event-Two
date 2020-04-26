using System;
using System.Reflection;
using UnityEngine;
using System.Linq.Expressions;
using System.Collections;
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
        static Type raw_calltype = typeof(RawCall);
        /// <summary>
        /// name of method calling in reflection. populated during editor time
        /// </summary>
        [SerializeField] string Creationmethod;
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
        public sealed override void Release()
        {
            m_target = null;
            delegateInstance = null;
            for (int i = 0; i < m_arguments.Length; i++)
            {
                m_arguments[i].Release();
            }
        }
        /// <summary>
        /// Checks delegate for memory leaks by checking target and reference arguments
        /// </summary>
        /// <returns></returns>
        public sealed override bool isDelegateLeaking()
        {
            if (m_target != null)
            {
                bool isArgumentsleaked = false;
                int argumentlength = m_arguments.Length;
                for (int i = 0; i < argumentlength; i++)
                {
                    if (m_arguments[i].isUsingreference && m_arguments[i].call_Reference.target == null)
                    {
                        isArgumentsleaked = true;
                        break;
                    }
                }
                return isArgumentsleaked;
            }
            else return true;
        }
        //=======================
        // Delegate
        //=======================
        /// <summary>Creates a delegate instance using the <see cref="RawDelegate.m_target"/> and an input <see cref="System.Reflection.MemberInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMember">MemberInfo to be cast into a delegate</param>
        /// <returns>Member delegate if successful, null if not able to properly convert</returns>
        private protected sealed override System.Delegate createDelegate(MemberInfo tMember, object target)
        {

            if (tMember != null)
            {
                int argumentlength = m_arguments.Length;
                object[] arguments = new object[argumentlength + 1];
                arguments[0] = tMember;
                for (int i = 0; i < argumentlength; i++)
                {
                    arguments[i + 1] = m_arguments[i];
                }
                MethodInfo RunTimeMethod = null;
                switch (tMember.MemberType)
                {
                    case MemberTypes.Field:
                        if (!Utility.DelegateFieldCreationMethods.TryGetValue(paramtypes, out RunTimeMethod))
                        {
                            RunTimeMethod = raw_calltype.GetMethod(Creationmethod, Utility.InstanceFlags).MakeGenericMethod(paramtypes);
                            Utility.DelegateFieldCreationMethods.Add(paramtypes, RunTimeMethod);
                        }
                        break;
                    case MemberTypes.Property:
                        if (!Utility.DelegatePropertyCreationMethod.TryGetValue(paramtypes, out RunTimeMethod))
                        {
                            Debug.LogWarning("first seralize");
                            RunTimeMethod = raw_calltype.GetMethod(Creationmethod, Utility.InstanceFlags).MakeGenericMethod(paramtypes);
                            Utility.DelegatePropertyCreationMethod.Add(paramtypes, RunTimeMethod);
                        }
                        else
                        {
                            Debug.Log(paramtypes[0].FullName);
                            Debug.LogWarning("GOT FROM CACHE");
                        }
                        break;
                    case MemberTypes.Method:
                        if (paramtypes == null)
                        {
                            return createActionCall0(tMember as MethodInfo);
                        }
                        else if (!Utility.DelegateMethodCreationMethods.TryGetValue(paramtypes, out RunTimeMethod))
                        {
                            RunTimeMethod = raw_calltype.GetMethod(Creationmethod, Utility.InstanceFlags).MakeGenericMethod(paramtypes);
                            Utility.DelegateMethodCreationMethods.Add(paramtypes, RunTimeMethod);
                        }
                        break;
                }
                return RunTimeMethod.Invoke(this, arguments) as Delegate;
            }
            else
            {
                if (methodData.Length > 0)
                {
                    MemberTypes mem_type = (MemberTypes)Utility.ConvertStringToInt(methodData[0]);
                    return new Action(() => Debug.LogError($@"Cannot find {mem_type} ""{methodData[1]}"" in class  ""{m_target.GetType().FullName}""+ 
                member was renamed or removed on gameobject ""{m_target.name}""", m_target));
                }
                else return null;
            }

        }
        //=======================
        // Field
        //=======================
        /// <summary>Utility method for creating a field delegate from a <see cref="System.Reflection.FieldInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tTarget">Target owner of the <paramref name="tField"/></param>
        /// <param name="tField">FieldInfo used to generate a delegate</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A> createFieldAction<A>(FieldInfo tField)
        {
            var field_instance = Expression.Field(Expression.Constant(m_target), tField);
            if (typeof(A).IsValueType)
            {
                var field_value_expression = Expression.Parameter(typeof(object));
                var setter_expression = Expression.Assign(field_instance, Expression.Unbox(field_value_expression, typeof(A)));
                var field_setter_lambda = Expression.Lambda<Action<object>>(setter_expression, field_value_expression).Compile();
                return val => field_setter_lambda.Invoke(val);
            }
            else
            {
                var field_value_expression = Expression.Parameter(typeof(A));
                var setter_expression = Expression.Assign(field_instance, field_value_expression);
                var field_setter_lambda = Expression.Lambda<Action<A>>(setter_expression, field_value_expression).Compile();
                return val => field_setter_lambda.Invoke(val);
            }

        }

        /// <summary>Utility method for creating a field delegate with a predefined argument value from a <see cref="System.Reflection.FieldInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tTarget">Target owner of the <paramref name="tField"/></param>
        /// <param name="tField">FieldInfo used to generate a delegate</param>
        /// <param name="tValue">Predefined argument value</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action createFieldCall<A>(FieldInfo tField, RawArg arg1)
        {
            var field_instance = Expression.Field(Expression.Constant(m_target), tField);
            if (typeof(A).IsValueType)
            {
                var field_value_expression = Expression.Parameter(typeof(object));
                var setter_expression = Expression.Assign(field_instance, Expression.Unbox(field_value_expression, typeof(A)));
                var field_setter_lambda = Expression.Lambda<Action<object>>(setter_expression, field_value_expression).Compile();
                Func<A> argument_val = arg1.CreateArgumentDelegate<A>();
                return () => field_setter_lambda.Invoke(argument_val());
            }
            else
            {
                var field_value_expression = Expression.Parameter(typeof(A));
                var setter_expression = Expression.Assign(field_instance, field_value_expression);
                var field_setter_lambda = Expression.Lambda<Action<A>>(setter_expression, field_value_expression).Compile();
                Func<A> argument_val = arg1.CreateArgumentDelegate<A>();
                return () => field_setter_lambda.Invoke(argument_val());
            }
        }

        //=======================
        // Property
        //=======================
        /// <summary>Utility method for creating a property delegate from a <see cref="System.Reflection.PropertyInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tProperty">PropertyInfo used to generate a delegate</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Delegate createPropertyAction<A>(PropertyInfo tProperty)
        {
            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), m_target, tProperty.GetSetMethod(), false) as Action<A>;

            Action<A> tempAction = (A tA) =>
            {
                try
                {
                    tempDelegate(tA);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }

            };
            return tempAction;
        }

        /// <summary>Utility method for creating a delegate from a <see cref="System.Reflection.PropertyInfo"/> that applies a predefined value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tProperty">PropertyInfo used to generate a delegate</param>
        /// <param name="tValue">Predefined argument value</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Delegate createPropertyCall<A>(PropertyInfo tProperty, RawArg arg1)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), delegate_target, tProperty.GetSetMethod(), false) as Action<A>;
            Func<A> property_input = arg1.CreateArgumentDelegate<A>();
            Action tempcall = () =>
             {
                 try
                 {
                     tempDelegate(property_input());
                 }
                 catch (Exception ex)
                 {
                     Debug.LogError(ex);
                 }
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
        protected Delegate createActionCall0(MethodInfo tMethod)
        {
            var delegate_target = isStatic ? null : m_target;
            Action tempDelegate = Delegate.CreateDelegate(typeof(Action), delegate_target, tMethod, false) as Action;
            Action tempAction = () =>
            {
                try
                {
                    tempDelegate();
                }

                catch (Exception ex)
                {
                    Debug.LogError(ex, m_target);
                }
            };
            return tempAction;
        }

        /// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected Delegate createAction1<A>(MethodInfo tMethod)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), delegate_target, tMethod, false) as Action<A>;
            Action<A> tempAction = (A tA) =>
            {
                try
                {
                    tempDelegate(tA);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }

            };
            return tempAction;
        }

        /// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies a predefined value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Delegate createActionCall1<A>(MethodInfo tMethod, RawArg arg1)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), delegate_target, tMethod, false) as Action<A>;
            Func<A> input = arg1.CreateArgumentDelegate<A>();
            Action tempaction = () =>
            {
                try
                {
                    tempDelegate(input());
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }

            };
            return tempaction;
        }

        /// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 2-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B> createAction2<A, B>(MethodInfo tMethod)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B>), delegate_target, tMethod, false) as Action<A, B>;
            Action<A, B> tempAction = (A tA, B tB) =>
             {
                 try
                 {
                     tempDelegate(tA, tB);
                 }
                 catch (Exception ex)
                 {
                     Debug.LogError(ex);
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
        protected virtual Action createActionCall2<A, B>(MethodInfo tMethod, RawArg arg1, RawArg arg2)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B>), delegate_target, tMethod, false) as Action<A, B>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Action tempAction = () =>
            {
                try
                {
                    tempDelegate(input_1(), input_2());
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }

            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 3-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MemberInfo used to generate a delegate</param>
        /// <returns>Generic 3-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C> createAction3<A, B, C>(MethodInfo tMethod)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C>), delegate_target, tMethod, false) as Action<A, B, C>;
            Action<A, B, C> tempAction = (A tA, B tB, C tC) =>
              {
                  try
                  {
                      tempDelegate(tA, tB, tC);
                  }
                  catch (Exception ex)
                  {
                      Debug.LogError(ex);
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
        protected virtual Action createActionCall3<A, B, C>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C>), delegate_target, tMethod, false) as Action<A, B, C>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Action tempAction = () =>
            {
                try
                {
                    tempDelegate(input_1(), input_2(), input_3());
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }

            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 4-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 4-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D> createAction4<A, B, C, D>(MethodInfo tMethod)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C, D> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D>), delegate_target, tMethod, false) as Action<A, B, C, D>;
            Action<A, B, C, D> tempAction = (A tA, B tB, C tC, D tD) =>
               {
                   try
                   {
                       tempDelegate(tA, tB, tC, tD);
                   }
                   catch (Exception ex)
                   {
                       Debug.LogError(ex);
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
        protected virtual Action createActionCall4<A, B, C, D>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C, D> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D>), delegate_target, tMethod, false) as Action<A, B, C, D>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Action tempAction = () =>
            {
                try
                {
                    tempDelegate(input_1(), input_2(), input_3(), input_4());
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 5-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 5-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E> createAction5<A, B, C, D, E>(VisualDelegateBase tPublisher, MethodInfo tMethod)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C, D, E> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E>), delegate_target, tMethod, false) as Action<A, B, C, D, E>;
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
        protected virtual Action createActionCall5<A, B, C, D, E>(VisualDelegateBase tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C, D, E> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E>), delegate_target, tMethod, false) as Action<A, B, C, D, E>;
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
        protected virtual Action<A, B, C, D, E, F> createAction6<A, B, C, D, E, F>(VisualDelegateBase tPublisher, MethodInfo tMethod)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C, D, E, F> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F>), delegate_target, tMethod, false) as Action<A, B, C, D, E, F>;
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
        protected virtual Action createActionCall6<A, B, C, D, E, F>(VisualDelegateBase tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C, D, E, F> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F>), delegate_target, tMethod, false) as Action<A, B, C, D, E, F>;
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
        protected virtual Action<A, B, C, D, E, F, G> createAction7<A, B, C, D, E, F, G>(VisualDelegateBase tPublisher, MethodInfo tMethod)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C, D, E, F, G> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F, G>), delegate_target, tMethod, false) as Action<A, B, C, D, E, F, G>;
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
        protected virtual Action createActionCall7<A, B, C, D, E, F, G>(VisualDelegateBase tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6, RawArg arg7)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C, D, E, F, G> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F, G>), delegate_target, tMethod, false) as Action<A, B, C, D, E, F, G>;
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
        protected virtual Action<A, B, C, D, E, F, G, H> createAction8<A, B, C, D, E, F, G, H>(VisualDelegateBase tPublisher, MethodInfo tMethod)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C, D, E, F, G, H> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F, G, H>), delegate_target, tMethod, false) as Action<A, B, C, D, E, F, G, H>;
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
        protected virtual Action createActionCall8<A, B, C, D, E, F, G, H>(VisualDelegateBase tPublisher, MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6, RawArg arg7, RawArg arg8)
        {
            var delegate_target = isStatic ? null : m_target;
            Action<A, B, C, D, E, F, G, H> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F, G, H>), delegate_target, tMethod, false) as Action<A, B, C, D, E, F, G, H>;
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
        protected virtual Delegate createFuncCall0<T>(MethodInfo tMethod)
        {
            var delegate_target = isStatic ? null : m_target;
            Func<T> tempDelegate = Delegate.CreateDelegate(typeof(Func<T>), delegate_target, tMethod, false) as Func<T>;
            if (isYieldable)
                return tempDelegate;
            else
            {
                Action tempAction = () =>
                {
                    try
                    {
                        tempDelegate();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                };
                return tempDelegate;
            }
        }


        /// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Delegate createFunc1<A, T>(MethodInfo tMethod)
        {
            Func<A, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, T>), m_target, tMethod, false) as Func<A, T>;
            if (isYieldable)
                return tempDelegate;
            Action<A> tempAction = (A tA) =>
            {
                try
                {
                    tempDelegate(tA);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies a predefined value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <param name="tA">Predefined argument value</param>
        /// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Delegate createFuncCall1<A, T>(MethodInfo tMethod, RawArg arg1)
        {

            Func<A, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, T>), m_target, tMethod, false) as Func<A, T>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            if (isYieldable)
            {
                Func<IEnumerator> del = () => tempDelegate(input_1()) as IEnumerator;
                return del;
            }
            Action tempaction = () =>
               {
                   try
                   {
                       tempDelegate(input_1());
                   }
                   catch (Exception ex)
                   {
                       Debug.LogError(ex);
                   }
               };
            return tempaction;
        }

        /// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Delegate  createFunc2<A, B, T>(MethodInfo tMethod)
        {
            Func<A, B, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, T>), m_target, tMethod, false) as Func<A, B, T>;
            if (isYieldable)
                return tempDelegate;
            Action<A, B> tempAction = (A tA, B tB) =>
             {
                 try
                 {
                     tempDelegate(tA, tB);
                 }
                 catch (Exception ex)
                 {
                     Debug.LogError(ex);
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
        protected virtual Delegate createFuncCall2<A, B, T>(MethodInfo tMethod, RawArg arg1, RawArg arg2)
        {
            Func<A, B, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, T>), m_target, tMethod, false) as Func<A, B, T>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();

            if (isYieldable)
            {
                Func<IEnumerator> yieldedDelgate = () =>
                {
                    return tempDelegate(input_1(), input_2()) as IEnumerator;
                };
                return yieldedDelgate;
            }
            Action tempaction = () =>
            {
                try
                {
                    tempDelegate(input_1(), input_2());
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            };
            return tempaction;
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

        //public void OnBeforeSerialize()
        //{
        //}

        //public void OnAfterDeserialize()
        //{
        //    initialize();
        //}
    }
}
