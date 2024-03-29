﻿using System;
using System.Reflection;
using UnityEngine;
using System.Linq.Expressions;
using System.Collections;
using RawArg = VisualDelegates.RawArgument;
namespace VisualDelegates
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
        [SerializeField] string creationMethod;
        //=======================
        // Variables
        //=======================
        /// <summary>Predefined argument data used if this delegate is marked as not dynamic</summary>
        [SerializeField]
        protected internal RawArgument[] m_arguments;
        /// <summary>Whether or not this delegate is invoked with passed in parameters from a <see cref="VisualDelegate"/> or if it contains predefined arguments</summary>
        [SerializeField]
        protected internal bool m_isDynamic;
        [SerializeField]
        protected internal bool m_isYieldableCall;
        //=======================
        // Arguments
        //=======================
        /// <summary>Gets the predefined <see cref="m_arguments"/> list of the delegate call</summary>
        internal RawArgument[] arguments => m_arguments;

        /// <summary>Gets the <see cref="m_isDynamic"/> value</summary>
        internal bool isDynamic => m_isDynamic;
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
            if (delegateInstance.Target != null)
            {
                bool isArgumentsleaked = false;
                int argumentlength = m_arguments.Length;
                for (int i = 0; i < argumentlength; i++)
                {
                    if (m_arguments[i].isUsingreference && m_arguments[i].call_Reference.delegateInstance?.Target == null)
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
                        if (!isDynamic)
                        {
                            if (!Utility.delegateFieldSetterCreationMethods.TryGetValue(paramtypes, out RunTimeMethod))
                            {
                                RunTimeMethod = raw_calltype.GetMethod(creationMethod, Utility.InstanceFlags).MakeGenericMethod(paramtypes);
                                Utility.delegateFieldSetterCreationMethods.Add(paramtypes, RunTimeMethod);
                            }
                        }
                        else
                        {
                            if (!Utility.delegateDynamicFieldSetterCreationMethods.TryGetValue(paramtypes, out RunTimeMethod))
                            {
                                RunTimeMethod = raw_calltype.GetMethod(creationMethod, Utility.InstanceFlags).MakeGenericMethod(paramtypes);
                                Utility.delegateDynamicFieldSetterCreationMethods.Add(paramtypes, RunTimeMethod);
                            }
                        }
                        break;
                    case MemberTypes.Property:
                        if (!isDynamic)
                        {
                            if (!Utility.delegatePropertySetterCreationMethods.TryGetValue(paramtypes, out RunTimeMethod))
                            {
                                RunTimeMethod = raw_calltype.GetMethod(creationMethod, Utility.InstanceFlags).MakeGenericMethod(paramtypes);
                                Utility.delegatePropertySetterCreationMethods.Add(paramtypes, RunTimeMethod);
                            }
                        }
                        else
                        {
                            if (!Utility.delegateDynamicPropertySetterCreationMethod.TryGetValue(paramtypes, out RunTimeMethod))
                            {
                                RunTimeMethod = raw_calltype.GetMethod(creationMethod, Utility.InstanceFlags).MakeGenericMethod(paramtypes);
                                Utility.delegateDynamicPropertySetterCreationMethod.Add(paramtypes, RunTimeMethod);
                            }
                        }
                        break;
                    case MemberTypes.Method:
                        if (paramtypes == null)
                            return createActionCall0(tMember as MethodInfo);
                        else if (!isDynamic)
                        {
                            if (!Utility.delegateMethodCreationMethods.TryGetValue(paramtypes, out RunTimeMethod))
                            {
                                RunTimeMethod = raw_calltype.GetMethod(creationMethod, Utility.InstanceFlags).MakeGenericMethod(paramtypes);
                                Utility.delegateMethodCreationMethods.Add(paramtypes, RunTimeMethod);
                            }
                        }
                        else
                        {
                            if (!Utility.DelegateDynamicMethodCreationMethods.TryGetValue(paramtypes, out RunTimeMethod))
                            {
                                RunTimeMethod = raw_calltype.GetMethod(creationMethod, Utility.InstanceFlags).MakeGenericMethod(paramtypes);
                                Utility.DelegateDynamicMethodCreationMethods.Add(paramtypes, RunTimeMethod);
                            }
                        }
                        break;
                }
                return RunTimeMethod.Invoke(this, arguments) as Delegate;
            }
            else return null;

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
            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), m_target, tProperty.SetMethod, false) as Action<A>;

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
            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), m_target, tProperty.SetMethod, false) as Action<A>;
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

            Action tempDelegate = Delegate.CreateDelegate(typeof(Action), m_target, tMethod, false) as Action;
            Action tempAction = () =>
            {
                try
                {
                    tempDelegate();
                    haserror = false;
                }

                catch (Exception ex)
                {
                    haserror = true;
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

            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), m_target, tMethod, false) as Action<A>;
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

            Action<A> tempDelegate = Delegate.CreateDelegate(typeof(Action<A>), m_target, tMethod, false) as Action<A>;
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

            Action<A, B> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B>), m_target, tMethod, false) as Action<A, B>;
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

            Action<A, B> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B>), m_target, tMethod, false) as Action<A, B>;
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

            Action<A, B, C> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C>), m_target, tMethod, false) as Action<A, B, C>;
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

            Action<A, B, C> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C>), m_target, tMethod, false) as Action<A, B, C>;
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

            Action<A, B, C, D> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D>), m_target, tMethod, false) as Action<A, B, C, D>;
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

            Action<A, B, C, D> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D>), m_target, tMethod, false) as Action<A, B, C, D>;
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
        protected virtual Action<A, B, C, D, E> createAction5<A, B, C, D, E>(MethodInfo tMethod)
        {

            Action<A, B, C, D, E> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E>), m_target, tMethod, false) as Action<A, B, C, D, E>;
            Action<A, B, C, D, E> tempAction = (A tA, B tB, C tC, D tD, E tE) =>
             {
                 try
                 {
                     tempDelegate(tA, tB, tC, tD, tE);
                 }
                 catch (Exception ex)
                 {
                     Debug.LogError(ex);
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
        protected virtual Action createActionCall5<A, B, C, D, E>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5)
        {

            Action<A, B, C, D, E> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E>), m_target, tMethod, false) as Action<A, B, C, D, E>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Func<E> input_5 = arg5.CreateArgumentDelegate<E>();
            Action tempAction = () =>
            {
                try
                {
                    tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5());
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 6-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 6-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F> createAction6<A, B, C, D, E, F>(MethodInfo tMethod)
        {

            Action<A, B, C, D, E, F> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F>), m_target, tMethod, false) as Action<A, B, C, D, E, F>;
            Action<A, B, C, D, E, F> tempAction = (A tA, B tB, C tC, D tD, E tE, F Tf) =>
             {
                 try
                 {
                     tempDelegate(tA, tB, tC, tD, tE, Tf);
                 }
                 catch (Exception ex)
                 {
                     Debug.LogError(ex);
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
        protected virtual Action createActionCall6<A, B, C, D, E, F>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6)
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
                try
                {
                    tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6());
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 7-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 7-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F, G> createAction7<A, B, C, D, E, F, G>(MethodInfo tMethod)
        {

            Action<A, B, C, D, E, F, G> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F, G>), m_target, tMethod, false) as Action<A, B, C, D, E, F, G>;
            Action<A, B, C, D, E, F, G> tempAction = (A tA, B tB, C tC, D tD, E tE, F tF, G tG) =>
                  {
                      try
                      {
                          tempDelegate(tA, tB, tC, tD, tE, tF, tG);
                      }
                      catch (Exception ex)
                      {
                          Debug.LogError(ex);
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
        protected virtual Action createActionCall7<A, B, C, D, E, F, G>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6, RawArg arg7)
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
                try
                {
                    tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6(), input_7());
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            };

            return tempAction;
        }

        /// <summary>Utility method for creating a 8-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic 8-parameter action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F, G, H> createAction8<A, B, C, D, E, F, G, H>(MethodInfo tMethod)
        {

            Action<A, B, C, D, E, F, G, H> tempDelegate = Delegate.CreateDelegate(typeof(Action<A, B, C, D, E, F, G, H>), m_target, tMethod, false) as Action<A, B, C, D, E, F, G, H>;
            Action<A, B, C, D, E, F, G, H> tempAction = (A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH) =>
                   {
                       try
                       {
                           tempDelegate(tA, tB, tC, tD, tE, tF, tG, tH);
                       }
                       catch (Exception ex)
                       {
                           Debug.LogError(ex);
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
        protected virtual Action createActionCall8<A, B, C, D, E, F, G, H>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6, RawArg arg7, RawArg arg8)
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
                try
                {
                    tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6(), input_7(), input_8());
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
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

            Func<T> tempDelegate = Delegate.CreateDelegate(typeof(Func<T>), m_target, tMethod, false) as Func<T>;
            Action tempAction;
            if (m_isYieldableCall)
            {
                tempAction = () =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate() as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else
            {
                tempAction = () =>
                {
                    try
                    {
                        tempDelegate();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            return tempAction;
        }


        /// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Delegate createFunc1<A, T>(MethodInfo tMethod)
        {
            Func<A, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, T>), m_target, tMethod, false) as Func<A, T>;
            Action<A> tempAction;
            if (m_isYieldableCall)
            {
                tempAction = val =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(val) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else tempAction = (A tA) =>
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
            Action tempAction;
            if (m_isYieldableCall)
            {
                tempAction = () =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(input_1()) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else
            {
                tempAction = () =>
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
            }
            return tempAction;
        }

        /// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Delegate createFunc2<A, B, T>(MethodInfo tMethod)
        {
            Func<A, B, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, T>), m_target, tMethod, false) as Func<A, B, T>;
            Action<A, B> tempAction;
            if (m_isYieldableCall)
            {
                tempAction = (val1, val2) =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(val1, val2) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else tempAction = (val1, val2) =>
            {
                try
                {
                    tempDelegate(val1, val2);
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
            Action tempAction;
            if (m_isYieldableCall)
            {
                tempAction = () =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(input_1(), input_2()) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else
            {
                tempAction = () =>
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
            }
            return tempAction;
        }

        /// <summary>Utility method for creating a 3-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C> createFunc3<A, B, C, T>(MethodInfo tMethod)
        {
            Func<A, B, C, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, T>), m_target, tMethod, false) as Func<A, B, C, T>;
            Action<A, B, C> tempAction;
            if (m_isYieldableCall)
            {
                tempAction = (val1, val2, val3) =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(val1, val2, val3) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else tempAction = (val1, val2, val3) =>
            {
                try
                {
                    tempDelegate(val1, val2, val3);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
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
        protected virtual Action createFuncCall3<A, B, C, T>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3)
        {
            Func<A, B, C, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, T>), m_target, tMethod, false) as Func<A, B, C, T>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Action tempAction;
            if (m_isYieldableCall)
            {
                tempAction = () =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(input_1(), input_2(), input_3()) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else
            {
                tempAction = () =>
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
            }
            return tempAction;
        }

        /// <summary>Utility method for creating a 4-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D> createFunc4<A, B, C, D, T>(MethodInfo tMethod)
        {
            Func<A, B, C, D, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, T>), m_target, tMethod, false) as Func<A, B, C, D, T>;
            Action<A, B, C, D> tempAction;
            if (m_isYieldableCall)
            {
                tempAction = (val1, val2, val3, val4) =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(val1, val2, val3, val4) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else tempAction = (val1, val2, val3, val4) =>
            {
                try
                {
                    tempDelegate(val1, val2, val3, val4);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
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
        protected virtual Action createFuncCall4<A, B, C, D, T>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4)
        {
            Func<A, B, C, D, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, T>), m_target, tMethod, false) as Func<A, B, C, D, T>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Action tempAction;
            if (m_isYieldableCall)
            {
                tempAction = () =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(input_1(), input_2(), input_3(), input_4()) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else
            {
                tempAction = () =>
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
            }
            return tempAction;
        }

        /// <summary>Utility method for creating a 5-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E> createFunc5<A, B, C, D, E, T>(MethodInfo tMethod)
        {
            Func<A, B, C, D, E, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, T>), m_target, tMethod, false) as Func<A, B, C, D, E, T>;
            Action<A, B, C, D, E> tempAction;
            if (m_isYieldableCall)
            {
                tempAction = (val1, val2, val3, val4, val5) =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(val1, val2, val3, val4, val5) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else tempAction = (val1, val2, val3, val4, val5) =>
            {
                try
                {
                    tempDelegate(val1, val2, val3, val4, val5);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
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
        protected virtual Action createFuncCall5<A, B, C, D, E, T>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5)
        {
            Func<A, B, C, D, E, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, T>), m_target, tMethod, false) as Func<A, B, C, D, E, T>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Func<E> input_5 = arg5.CreateArgumentDelegate<E>();
            Action tempAction;
            if (m_isYieldableCall)
            {
                tempAction = () =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5()) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else
            {
                tempAction = () =>
                {
                    try
                    {
                        tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            return tempAction;
        }

        /// <summary>Utility method for creating a 6-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F> createFunc6<A, B, C, D, E, F, T>(MethodInfo tMethod)
        {
            Func<A, B, C, D, E, F, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, T>;
            Action<A, B, C, D, E, F> tempAction;
            if (m_isYieldableCall)
            {
                tempAction = (val1, val2, val3, val4, val5, val6) =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(val1, val2, val3, val4, val5, val6) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else tempAction = (val1, val2, val3, val4, val5, val6) =>
            {
                try
                {
                    tempDelegate(val1, val2, val3, val4, val5, val6);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
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
        protected virtual Action createFuncCall6<A, B, C, D, E, F, T>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6)
        {
            Func<A, B, C, D, E, F, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, T>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Func<E> input_5 = arg5.CreateArgumentDelegate<E>();
            Func<F> input_6 = arg6.CreateArgumentDelegate<F>();
            Action tempAction;
            if (m_isYieldableCall)
            {
                tempAction = () =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6()) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else
            {
                tempAction = () =>
                {
                    try
                    {
                        tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            return tempAction;
        }

        /// <summary>Utility method for creating a 7-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F, G> createFunc7<A, B, C, D, E, F, G, T>(MethodInfo tMethod)
        {
            Func<A, B, C, D, E, F, G, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, G, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, G, T>;
            Action<A, B, C, D, E, F, G> tempAction;
            if (m_isYieldableCall)
            {
                tempAction = (val1, val2, val3, val4, val5, val6, val7) =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(val1, val2, val3, val4, val5, val6, val7) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else tempAction = (val1, val2, val3, val4, val5, val6, val7) =>
            {
                try
                {
                    tempDelegate(val1, val2, val3, val4, val5, val6, val7);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
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
        protected virtual Action createFuncCall7<A, B, C, D, E, F, G, T>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6, RawArg arg7)
        {
            Func<A, B, C, D, E, F, G, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, G, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, G, T>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Func<E> input_5 = arg5.CreateArgumentDelegate<E>();
            Func<F> input_6 = arg6.CreateArgumentDelegate<F>();
            Func<G> input_7 = arg7.CreateArgumentDelegate<G>();
            Action tempAction;
            if (m_isYieldableCall)
            {
                tempAction = () =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6(), input_7()) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else
            {
                tempAction = () =>
                {
                    try
                    {
                        tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6(), input_7());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            return tempAction;
        }

        /// <summary>Utility method for creating a 8-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
        /// <param name="tPublisher"><see cref="VisualDelegate"/> instance passed in the delegate for memory management</param>
        /// <param name="tMethod">MethodInfo used to generate a delegate</param>
        /// <returns>Generic action delegate if successful, null if not able to convert</returns>
        protected virtual Action<A, B, C, D, E, F, G, H> createFunc8<A, B, C, D, E, F, G, H, T>(MethodInfo tMethod)
        {
            Func<A, B, C, D, E, F, G, H, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, G, H, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, G, H, T>;
            Action<A, B, C, D, E, F, G,H> tempAction;
            if (m_isYieldableCall)
            {
                tempAction = (val1, val2, val3, val4, val5, val6, val7,val8) =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(val1, val2, val3, val4, val5, val6, val7,val8) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else tempAction = (val1, val2, val3, val4, val5, val6, val7,val8) =>
            {
                try
                {
                    tempDelegate(val1, val2, val3, val4, val5, val6, val7,val8);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
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
        protected virtual Action createFuncCall8<A, B, C, D, E, F, G, H, T>(MethodInfo tMethod, RawArg arg1, RawArg arg2, RawArg arg3, RawArg arg4, RawArg arg5, RawArg arg6, RawArg arg7, RawArg arg8)
        {
            Func<A, B, C, D, E, F, G, H, T> tempDelegate = Delegate.CreateDelegate(typeof(Func<A, B, C, D, E, F, G, H, T>), m_target, tMethod, false) as Func<A, B, C, D, E, F, G, H, T>;
            Func<A> input_1 = arg1.CreateArgumentDelegate<A>();
            Func<B> input_2 = arg2.CreateArgumentDelegate<B>();
            Func<C> input_3 = arg3.CreateArgumentDelegate<C>();
            Func<D> input_4 = arg4.CreateArgumentDelegate<D>();
            Func<E> input_5 = arg5.CreateArgumentDelegate<E>();
            Func<F> input_6 = arg6.CreateArgumentDelegate<F>();
            Func<G> input_7 = arg7.CreateArgumentDelegate<G>();
            Func<H> input_8 = arg8.CreateArgumentDelegate<H>();
            Action tempAction;
            if (m_isYieldableCall)
            {
                tempAction = () =>
                {
                    try
                    {
                        (m_target as MonoBehaviour).StartCoroutine(tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6(), input_7(), input_8()) as IEnumerator);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            else
            {
                tempAction = () =>
                {
                    try
                    {
                        tempDelegate(input_1(), input_2(), input_3(), input_4(), input_5(), input_6(), input_7(), input_8());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                };
            }
            return tempAction;
        }
    }
}
