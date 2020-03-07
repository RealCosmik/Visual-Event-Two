using System;
using System.Reflection;
using UnityEngine;

namespace EventsPlus
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
		protected RawArgument[] m_arguments;
		/// <summary>Whether or not this delegate is invoked with passed in parameters from a <see cref="Publisher"/> or if it contains predefined arguments</summary>
		[SerializeField]
		protected bool m_isDynamic;
		
        //=======================
        // Arguments
        //=======================
        /// <summary>Gets the predefined <see cref="m_arguments"/> list of the delegate call</summary>
        public RawArgument[] arguments => m_arguments;

        /// <summary>Gets the <see cref="m_isDynamic"/> value</summary>
        public bool isDynamic => m_isDynamic;

        //=======================
        // Initialization
        //=======================
        /// <summary>Initializes and deserializes the <see cref="RawDelegate.m_target"/> and <see cref="RawDelegate._member"/> information into an actual delegate using a <see cref="Publisher"/> reference</summary>
        /// <param name="tPublisher">Publisher passed in the delegate used for automatic memory management</param>
        public virtual void initialize(Publisher tPublisher)
        {
            if (m_target != null)
                delegateInstance = createDelegate(tPublisher, Utility.QuickDeseralizer(m_target.GetType(), methodData));
        }

        //=======================
        // Delegate
        //=======================
        /// <summary>Creates a delegate instance using the <see cref="RawDelegate.m_target"/> and an input <see cref="System.Reflection.MemberInfo"/></summary>
        /// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
        /// <param name="tMember">MemberInfo to be cast into a delegate</param>
        /// <returns>Member delegate if successful, null if not able to properly convert</returns>
        public virtual System.Delegate createDelegate( Publisher tPublisher, MemberInfo tMember )
		{
            if (tMember != null)
            {
                switch (tMember.MemberType)
                {
                    case MemberTypes.Field:
                        FieldInfo tempField = tMember as FieldInfo;
                        if (m_isDynamic)
                        {
                            return this.GetType().GetMethod("createFieldAction", Utility.InstanceFlags).MakeGenericMethod(tempField.ReflectedType, tempField.FieldType).Invoke(this, new object[] { tPublisher, m_target, tempField }) as System.Delegate;
                        }
                        dynamic arg = m_arguments[0].genericValue;
                        return this.GetType().GetMethod("createFieldCall", Utility.InstanceFlags).MakeGenericMethod(tempField.ReflectedType, tempField.FieldType).Invoke(this, new object[] { tPublisher, m_target, tempField,arg }) as System.Delegate;
                    case MemberTypes.Property:
                        PropertyInfo tempProperty = tMember as PropertyInfo;
                        if (m_isDynamic)
                        {
                            return this.GetType().GetMethod("createPropertyAction", Utility.InstanceFlags).MakeGenericMethod(tempProperty.PropertyType).Invoke(this, new object[] { tPublisher, tempProperty }) as System.Delegate;
                        }
                        arg = m_arguments[0].genericValue;
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
                        if (m_arguments == null)
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
                                tempArguments[i + 2] = m_arguments[i].genericValue;
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
                member was renamed or removed on gameobject ""{m_target.name}""",m_target));
		}
		
		//=======================
		// Field
		//=======================
		/// <summary>Utility method for creating a field delegate from a <see cref="System.Reflection.FieldInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tTarget">Target owner of the <paramref name="tField"/></param>
		/// <param name="tField">FieldInfo used to generate a delegate</param>
		/// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A> createFieldAction<T,A>( Publisher tPublisher, T tTarget, FieldInfo tField )
		{
				Action<A> tempAction = ( A tA ) =>
				{
					if ( tTarget == null )
					{
						tPublisher.removeCall( this );
					}
					else
					{
						tField.SetValue( tTarget, tA );
					}
				};
				return tempAction;
		}
		
		/// <summary>Utility method for creating a field delegate with a predefined argument value from a <see cref="System.Reflection.FieldInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tTarget">Target owner of the <paramref name="tField"/></param>
		/// <param name="tField">FieldInfo used to generate a delegate</param>
		/// <param name="tValue">Predefined argument value</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFieldCall<T,A>( Publisher tPublisher, T tTarget, FieldInfo tField, A tValue )
		{
            Action tempCall = () =>
            {
                if (tTarget == null)
                {
                    tPublisher.removeCall(this);
                }
                else
                {
                    tField.SetValue(tTarget, tValue);
                }
            };
            return tempCall;
		}
		
		//=======================
		// Property
		//=======================
		/// <summary>Utility method for creating a property delegate from a <see cref="System.Reflection.PropertyInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tProperty">PropertyInfo used to generate a delegate</param>
		/// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A> createPropertyAction<A>( Publisher tPublisher, PropertyInfo tProperty )
		{
			Action<A> tempDelegate = Delegate.CreateDelegate( typeof( Action<A> ), m_target, tProperty.GetSetMethod(), false ) as Action<A>;
			Action<A> tempAction = ( A tA ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a delegate from a <see cref="System.Reflection.PropertyInfo"/> that applies a predefined value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tProperty">PropertyInfo used to generate a delegate</param>
		/// <param name="tValue">Predefined argument value</param>
		/// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createPropertyCall<A>( Publisher tPublisher, PropertyInfo tProperty, A tValue )
		{
			Action<A> tempDelegate = Delegate.CreateDelegate( typeof( Action<A> ), m_target, tProperty.GetSetMethod(), false ) as Action<A>;
			Action tempCall = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tValue );
				}
			};
			
			return tempCall;
		}
		
		//=======================
		// Action
		//=======================
		/// <summary>Utility method for creating a method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action createActionCall0( Publisher tPublisher, MethodInfo tMethod )
		{
            Action tempDelegate = Delegate.CreateDelegate( typeof( Action ), m_target, tMethod, false ) as Action;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate();
				}
			};
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A> createAction1<A>( Publisher tPublisher, MethodInfo tMethod )
		{
			Action<A> tempDelegate = Delegate.CreateDelegate( typeof( Action<A> ), m_target, tMethod, false ) as Action<A>;
			Action<A> tempAction = ( A tA ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies a predefined value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createActionCall1<A>( Publisher tPublisher, MethodInfo tMethod, A tA )
		{
			Action<A> tempDelegate = Delegate.CreateDelegate( typeof( Action<A> ), m_target, tMethod, false ) as Action<A>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic 2-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B> createAction2<A,B>( Publisher tPublisher, MethodInfo tMethod )
		{
			Action<A,B> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B> ), m_target, tMethod, false ) as Action<A,B>;
			Action<A,B> tempAction = ( A tA, B tB ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <returns>Generic 2-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createActionCall2<A,B>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB )
		{
			Action<A,B> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B> ), m_target, tMethod, false ) as Action<A,B>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 3-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MemberInfo used to generate a delegate</param>
		/// <returns>Generic 3-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C> createAction3<A,B,C>( Publisher tPublisher, MethodInfo tMethod )
		{
			Action<A,B,C> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C> ), m_target, tMethod, false ) as Action<A,B,C>;
			Action<A,B,C> tempAction = ( A tA, B tB, C tC ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 3-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <returns>Generic 3-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createActionCall3<A,B,C>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC )
		{
			Action<A,B,C> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C> ), m_target, tMethod, false ) as Action<A,B,C>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 4-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic 4-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D> createAction4<A,B,C,D>( Publisher tPublisher, MethodInfo tMethod )
		{
			Action<A,B,C,D> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D> ), m_target, tMethod, false ) as Action<A,B,C,D>;
			Action<A,B,C,D> tempAction = ( A tA, B tB, C tC, D tD ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 4-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <returns>Generic 4-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createActionCall4<A,B,C,D>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD )
		{
			Action<A,B,C,D> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D> ), m_target, tMethod, false ) as Action<A,B,C,D>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 5-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic 5-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E> createAction5<A,B,C,D,E>( Publisher tPublisher, MethodInfo tMethod )
		{
			Action<A,B,C,D,E> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E> ), m_target, tMethod, false ) as Action<A,B,C,D,E>;
			Action<A,B,C,D,E> tempAction = ( A tA, B tB, C tC, D tD, E tE ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 5-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <param name="tE">Predefined argument value</param>
		/// <returns>Generic 5-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createActionCall5<A,B,C,D,E>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE )
		{
			Action<A,B,C,D,E> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E> ), m_target, tMethod, false ) as Action<A,B,C,D,E>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 6-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic 6-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E,F> createAction6<A,B,C,D,E,F>( Publisher tPublisher, MethodInfo tMethod )
		{
			Action<A,B,C,D,E,F> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E,F> ), m_target, tMethod, false ) as Action<A,B,C,D,E,F>;
			Action<A,B,C,D,E,F> tempAction = ( A tA, B tB, C tC, D tD, E tE, F tF ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 6-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <param name="tE">Predefined argument value</param>
		/// <param name="tF">Predefined argument value</param>
		/// <returns>Generic 6-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createActionCall6<A,B,C,D,E,F>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF )
		{
			Action<A,B,C,D,E,F> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E,F> ), m_target, tMethod, false ) as Action<A,B,C,D,E,F>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 7-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic 7-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E,F,G> createAction7<A,B,C,D,E,F,G>( Publisher tPublisher, MethodInfo tMethod )
		{
			Action<A,B,C,D,E,F,G> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E,F,G> ), m_target, tMethod, false ) as Action<A,B,C,D,E,F,G>;
			Action<A,B,C,D,E,F,G> tempAction = ( A tA, B tB, C tC, D tD, E tE, F tF, G tG ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 7-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <param name="tE">Predefined argument value</param>
		/// <param name="tF">Predefined argument value</param>
		/// <param name="tG">Predefined argument value</param>
		/// <returns>Generic 7-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createActionCall7<A,B,C,D,E,F,G>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF, G tG )
		{
			Action<A,B,C,D,E,F,G> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E,F,G> ), m_target, tMethod, false ) as Action<A,B,C,D,E,F,G>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 8-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic 8-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E,F,G,H> createAction8<A,B,C,D,E,F,G,H>( Publisher tPublisher, MethodInfo tMethod )
		{
			Action<A,B,C,D,E,F,G,H> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E,F,G,H> ), m_target, tMethod, false ) as Action<A,B,C,D,E,F,G,H>;
			Action<A,B,C,D,E,F,G,H> tempAction = ( A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 8-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
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
		protected virtual Action createActionCall8<A,B,C,D,E,F,G,H>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH )
		{
			Action<A,B,C,D,E,F,G,H> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E,F,G,H> ), m_target, tMethod, false ) as Action<A,B,C,D,E,F,G,H>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 9-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic 9-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E,F,G,H,I> createAction9<A,B,C,D,E,F,G,H,I>( Publisher tPublisher, MethodInfo tMethod )
		{
			Action<A,B,C,D,E,F,G,H,I> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E,F,G,H,I> ), m_target, tMethod, false ) as Action<A,B,C,D,E,F,G,H,I>;
			Action<A,B,C,D,E,F,G,H,I> tempAction = ( A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH, I tI ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH, tI );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 9-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <param name="tE">Predefined argument value</param>
		/// <param name="tF">Predefined argument value</param>
		/// <param name="tG">Predefined argument value</param>
		/// <param name="tH">Predefined argument value</param>
		/// <param name="tI">Predefined argument value</param>
		/// <returns>Generic 9-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createActionCall9<A,B,C,D,E,F,G,H,I>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH, I tI )
		{
			Action<A,B,C,D,E,F,G,H,I> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E,F,G,H,I> ), m_target, tMethod, false ) as Action<A,B,C,D,E,F,G,H,I>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH, tI );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 10-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/></summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic 10-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E,F,G,H,I,J> createAction10<A,B,C,D,E,F,G,H,I,J>( Publisher tPublisher, MethodInfo tMethod )
		{
			Action<A,B,C,D,E,F,G,H,I,J> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E,F,G,H,I,J> ), m_target, tMethod, false ) as Action<A,B,C,D,E,F,G,H,I,J>;
			Action<A,B,C,D,E,F,G,H,I,J> tempAction = ( A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH, I tI, J tJ ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH, tI, tJ );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 10-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <param name="tE">Predefined argument value</param>
		/// <param name="tF">Predefined argument value</param>
		/// <param name="tG">Predefined argument value</param>
		/// <param name="tH">Predefined argument value</param>
		/// <param name="tI">Predefined argument value</param>
		/// <param name="tJ">Predefined argument value</param>
		/// <returns>Generic 10-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createActionCall10<A,B,C,D,E,F,G,H,I,J>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH, I tI, J tJ )
		{
			Action<A,B,C,D,E,F,G,H,I,J> tempDelegate = Delegate.CreateDelegate( typeof( Action<A,B,C,D,E,F,G,H,I,J> ), m_target, tMethod, false ) as Action<A,B,C,D,E,F,G,H,I,J>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH, tI, tJ );
				}
			};
			
			return tempAction;
		}
		
		//=======================
		// Func
		//=======================
		/// <summary>Utility method for creating a method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFuncCall0<T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<T> tempDelegate = Delegate.CreateDelegate( typeof( Func<T> ), m_target, tMethod, false ) as Func<T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate();
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A> createFunc1<A,T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<A,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,T> ), m_target, tMethod, false ) as Func<A,T>;
			Action<A> tempAction = ( A tA ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 1-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies a predefined value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <returns>Generic 1-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFuncCall1<A,T>( Publisher tPublisher, MethodInfo tMethod, A tA )
		{
			Func<A,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,T> ), m_target, tMethod, false ) as Func<A,T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B> createFunc2<A,B,T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<A,B,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,T> ), m_target, tMethod, false ) as Func<A,B,T>;
			Action<A,B> tempAction = ( A tA, B tB ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{   
					tempDelegate( tA, tB );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 2-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <returns>Generic 2-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFuncCall2<A,B,T>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB )
		{
			Func<A,B,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,T> ), m_target, tMethod, false ) as Func<A,B,T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 3-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C> createFunc3<A,B,C,T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<A,B,C,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,T> ), m_target, tMethod, false ) as Func<A,B,C,T>;
			Action<A,B,C> tempAction = ( A tA, B tB, C tC ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 3-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <returns>Generic 3-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFuncCall3<A,B,C,T>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC )
		{
			Func<A,B,C,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,T> ), m_target, tMethod, false ) as Func<A,B,C,T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 4-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D> createFunc4<A,B,C,D,T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<A,B,C,D,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,T> ), m_target, tMethod, false ) as Func<A,B,C,D,T>;
			Action<A,B,C,D> tempAction = ( A tA, B tB, C tC, D tD ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 4-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <returns>Generic 4-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFuncCall4<A,B,C,D,T>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD )
		{
			Func<A,B,C,D,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,T> ), m_target, tMethod, false ) as Func<A,B,C,D,T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 5-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E> createFunc5<A,B,C,D,E,T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<A,B,C,D,E,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,T>;
			Action<A,B,C,D,E> tempAction = ( A tA, B tB, C tC, D tD, E tE ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 5-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <param name="tE">Predefined argument value</param>
		/// <returns>Generic 5-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFuncCall5<A,B,C,D,E,T>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE )
		{
			Func<A,B,C,D,E,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 6-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E,F> createFunc6<A,B,C,D,E,F,T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<A,B,C,D,E,F,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,F,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,F,T>;
			Action<A,B,C,D,E,F> tempAction = ( A tA, B tB, C tC, D tD, E tE, F tF ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 6-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <param name="tE">Predefined argument value</param>
		/// <param name="tF">Predefined argument value</param>
		/// <returns>Generic 6-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFuncCall6<A,B,C,D,E,F,T>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF )
		{
			Func<A,B,C,D,E,F,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,F,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,F,T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 7-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E,F,G> createFunc7<A,B,C,D,E,F,G,T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<A,B,C,D,E,F,G,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,F,G,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,F,G,T>;
			Action<A,B,C,D,E,F,G> tempAction = ( A tA, B tB, C tC, D tD, E tE, F tF, G tG ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 7-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <param name="tE">Predefined argument value</param>
		/// <param name="tF">Predefined argument value</param>
		/// <param name="tG">Predefined argument value</param>
		/// <returns>Generic 7-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFuncCall7<A,B,C,D,E,F,G,T>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF, G tG )
		{
			Func<A,B,C,D,E,F,G,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,F,G,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,F,G,T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 8-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E,F,G,H> createFunc8<A,B,C,D,E,F,G,H,T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<A,B,C,D,E,F,G,H,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,F,G,H,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,F,G,H,T>;
			Action<A,B,C,D,E,F,G,H> tempAction = ( A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 8-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
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
		protected virtual Action createFuncCall8<A,B,C,D,E,F,G,H,T>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH )
		{
			Func<A,B,C,D,E,F,G,H,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,F,G,H,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,F,G,H,T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 9-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E,F,G,H,I> createFunc9<A,B,C,D,E,F,G,H,I,T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<A,B,C,D,E,F,G,H,I,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,F,G,H,I,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,F,G,H,I,T>;
			Action<A,B,C,D,E,F,G,H,I> tempAction = ( A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH, I tI ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH, tI );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 9-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <param name="tE">Predefined argument value</param>
		/// <param name="tF">Predefined argument value</param>
		/// <param name="tG">Predefined argument value</param>
		/// <param name="tH">Predefined argument value</param>
		/// <param name="tI">Predefined argument value</param>
		/// <returns>Generic 9-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFuncCall9<A,B,C,D,E,F,G,H,I,T>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH, I tI )
		{
			Func<A,B,C,D,E,F,G,H,I,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,F,G,H,I,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,F,G,H,I,T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH, tI );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 10-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> that has a return value</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <returns>Generic action delegate if successful, null if not able to convert</returns>
		protected virtual Action<A,B,C,D,E,F,G,H,I,J> createFunc10<A,B,C,D,E,F,G,H,I,J,T>( Publisher tPublisher, MethodInfo tMethod )
		{
			Func<A,B,C,D,E,F,G,H,I,J,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,F,G,H,I,J,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,F,G,H,I,J,T>;
			Action<A,B,C,D,E,F,G,H,I,J> tempAction = ( A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH, I tI, J tJ ) =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH, tI, tJ );
				}
			};
			
			return tempAction;
		}
		
		/// <summary>Utility method for creating a 10-parameter method delegate from a <see cref="System.Reflection.MethodInfo"/> (with a return type) that applies predefined values</summary>
		/// <param name="tPublisher"><see cref="Publisher"/> instance passed in the delegate for memory management</param>
		/// <param name="tMethod">MethodInfo used to generate a delegate</param>
		/// <param name="tA">Predefined argument value</param>
		/// <param name="tB">Predefined argument value</param>
		/// <param name="tC">Predefined argument value</param>
		/// <param name="tD">Predefined argument value</param>
		/// <param name="tE">Predefined argument value</param>
		/// <param name="tF">Predefined argument value</param>
		/// <param name="tG">Predefined argument value</param>
		/// <param name="tH">Predefined argument value</param>
		/// <param name="tI">Predefined argument value</param>
		/// <param name="tJ">Predefined argument value</param>
		/// <returns>Generic 10-parameter action delegate if successful, null if not able to convert</returns>
		protected virtual Action createFuncCall10<A,B,C,D,E,F,G,H,I,J,T>( Publisher tPublisher, MethodInfo tMethod, A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH, I tI, J tJ )
		{
			Func<A,B,C,D,E,F,G,H,I,J,T> tempDelegate = Delegate.CreateDelegate( typeof( Func<A,B,C,D,E,F,G,H,I,J,T> ), m_target, tMethod, false ) as Func<A,B,C,D,E,F,G,H,I,J,T>;
			Action tempAction = () =>
			{
				if ( m_target == null )
				{
					tPublisher.removeCall( this );
				}
				else
				{
					tempDelegate( tA, tB, tC, tD, tE, tF, tG, tH, tI, tJ );
				}
			};
			
			return tempAction;
		}
	}
}
