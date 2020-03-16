using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventsPlus
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Manages delegate event registration and invocation</summary>
	[Serializable]
	public class VisualDelegate
	{
		/// <summary>List of raw <see cref="RawCall"/> objects that this Publisher invokes using predefined arguments</summary>
		[SerializeReference]
		protected List<RawDelegate> m_calls;
		/// <summary>Event for 0-Parameter delegates and calls</summary>
		private Action onVoid;
		
		//=======================
		// Constructor
		//=======================
		/// <summary>Initializes the <see cref="_tag"/></summary>
		/// <param name="tTag">Tag key to bind to this instance</param>
		public VisualDelegate()
		{
		}

		
		/// <summary>Initializes <see cref="m_calls"/> and registers with any potential <see cref="Subscriber"/> instances</summary>
		public virtual void initialize()
		{
			if ( m_calls != null ) 
			{
				int tempListLength = m_calls.Count;
				for ( int i = 0; i < tempListLength; ++i )
				{
					var currentdelegate = m_calls[i];
				//	try
				//	{
						if (currentdelegate is RawCall rawcall)
							rawcall.initialize(this);
						else currentdelegate.initialize();
						effectsCallAdded(m_calls[i]);
				//	}
					//catch (System.Exception ex) //catch any reflection errors or type errors 
					//{
					//	Debug.LogError(ex, _calls[i].target);
					//}

				}
			}
		}
		public void AddMethod(Action new_method) => addCall(new RawDynamicDelegate(new_method));
        public virtual void ReInitialize()
        {
            onVoid = null;
            initialize();
        }
		//=======================
		// Destructor
		//=======================
		/// <summary>Unregisters from <see cref="Subscriber"/> instances and clears memory usage</summary>
		~VisualDelegate()
		{
			
			// Clear memory
			m_calls = null;
			onVoid = null;
		}
		
		//=======================
		// Types
		//=======================
		/// <summary>Gets array of Types that define this instance; this is used by the inspector to manage drop-downs</summary>
		public virtual Type[] types
		{
			get
			{
				return null;
			}
		} 

		//=======================
		// Calls
		//=======================
		/// <summary>Gets/sets the <see cref="m_calls"/> array</summary>
		public List<RawDelegate> calls
		{
			get
			{
				return m_calls;
			}
			set
			{
				// Remove old
				if ( m_calls != null )
				{
					for ( int i = ( m_calls.Count -1 ); i >= 0; --i )
					{
						removeCall( i );
					}
					
					m_calls = null;
				}
				
				// Add new
				if ( value != null )
				{
					int tempListLength = value.Count;
					for ( int i = 0; i < tempListLength; ++i )
					{
						addCall( value[i] );
					}
				}
			}
		}
		
		/// <summary>Attempts to add a <see cref="RawCall"/> to the Publisher's internal array and event(s)</summary>
		/// <param name="tCall">RawCall to add</param>
		/// <returns>True if successful</returns>
		public bool addCall( RawDelegate tCall )
		{
			if ( tCall != null )
			{
				if ( m_calls == null )
				{
					m_calls = new List<RawDelegate>();
				}
				m_calls.Add( tCall );
				
				effectsCallAdded( tCall );
				
				return true;
			}
			
			return false;
		}
		
		/// <summary>Handles the <see cref="RawCall"/> that was added and registers its delegate to the Publisher's matching event(s)</summary>
		/// <param name="tCall">RawCall that was added</param>
		protected virtual void effectsCallAdded( RawDelegate tCall )
		{
			Action tempDelegate = tCall.delegateInstance as Action;
			if (tempDelegate != null)
			{
				onVoid += tempDelegate;
			}
		}
		
		/// <summary>Attempts to remove a <see cref="RawCall"/> from the Publisher's internal array and event(s)</summary>
		/// <param name="tCall">RawCall to remove</param>
		/// <returns>True if successful</returns>
		internal bool removeCall( RawCall tCall )
		{
			return tCall != null && m_calls != null && removeCall( m_calls.IndexOf( tCall ) );
		}
		
		/// <summary>Attempts to remove a <see cref="RawCall"/> from the Publisher's internal array</summary>
		/// <param name="tIndex">Index of <see cref="m_calls"/> array to remove</param>
		/// <returns>True if successful</returns>
		public bool removeCall( int tIndex )
		{
			if ( tIndex >= 0 && m_calls != null && tIndex < m_calls.Count )
			{
				RawDelegate tempCall = m_calls[ tIndex ];
				
				m_calls.RemoveAt( tIndex );
				if ( m_calls.Count == 0 )
				{
					m_calls = null;
				}
				
				effectsCallRemoved( tempCall, tIndex );
				
				return true;
			}
			
			return false;
		}
		
		/// <summary>Handles the <see cref="RawCall"/> that was removed and removes its delegate from the Publisher's matching event(s)</summary>
		/// <param name="tCall">RawCall that was removed</param>
		/// <param name="tIndex">Index of the RawCall that was removed</param>
		protected virtual void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action tempDelegate = tCall.delegateInstance as Action;
			if ( tempDelegate != null )
			{
				onVoid -= tempDelegate;
			}
		}
	
		
		// Publish
		//=======================
		/// <summary>Invokes the <see cref="onVoid"/> event</summary>
		public void publish()
		{
			Action tempVoid = onVoid;
			if ( tempVoid != null )
			{
				tempVoid();
			}
		}
	}
	
	//##########################
	// Class Declaration
	//##########################
	/// <summary>1-Parameter Publisher</summary>
	public class VisualDelegate<A> : VisualDelegate
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Event for 1-Parameter delegates</summary>
		public event Action<A> onEvent;
		
		//=======================
		// Constructor
		//=======================
		//=======================
		// Destructor
		//=======================
		~VisualDelegate()
		{
			onEvent = null;
		}
        public override void ReInitialize()
        {
            onEvent = null;
            base.ReInitialize();
        }
        //=======================
        // Types
        //=======================
        public override Type[] types
		{
			get
			{
				return new Type[] { typeof( A ) };
			}
		}
		
		//=======================
		// Call
		//=======================
		protected override void effectsCallAdded( RawDelegate tCall )
		{
			Action<A> tempDelegate = tCall.delegateInstance as Action<A>;
			if ( tempDelegate == null )
			{
				base.effectsCallAdded( tCall );
			}
			else
			{
				onEvent += tempDelegate;
			}
		}
		
		protected override void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action<A> tempDelegate = tCall.delegateInstance as Action<A>;
			if ( tempDelegate == null )
			{
				base.effectsCallRemoved( tCall, tIndex );
			}
			else
			{
				onEvent -= tempDelegate;
			}
		}

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.onVoid"/> and <see cref="onEvent"/> events</summary>
        public void publish( A tA )
		{
		   publish();
			
			Action<A> tempEvent = onEvent;
			if ( tempEvent != null )
			{
				tempEvent( tA );
			}
		}
	}
	
	//##########################
	// Class Declaration
	//##########################
	/// <summary>2-Parameter Publisher</summary>
	public class VisualDelegate<A,B> : VisualDelegate
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Event for 2-Parameter delegates</summary>
		public event Action<A,B> onEvent;
		
		//=======================
		// Destructor
		//=======================
		~VisualDelegate()
		{
			onEvent = null;
		}
		
		//=======================
		// Types
		//=======================
		public override Type[] types
		{
			get
			{
				return new Type[] { typeof( A ), typeof( B ) };
			}
		}
		
		//=======================
		// Call
		//=======================
		protected override void effectsCallAdded( RawDelegate tCall )
		{
			Action<A,B> tempDelegate = tCall.delegateInstance as Action<A,B>;
			if ( tempDelegate == null )
			{
				base.effectsCallAdded( tCall );
			}
			else
			{
				onEvent += tempDelegate;
			}
		}
		
		protected override void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action<A,B> tempDelegate = tCall.delegateInstance as Action<A,B>;
			if ( tempDelegate == null )
			{
				base.effectsCallRemoved( tCall, tIndex );
			}
			else
			{
				onEvent -= tempDelegate;
			}
		}
		
	
		//=======================
		// Publish
		//=======================
		/// <summary>Invokes the <see cref="VisualDelegate.onVoid"/> and <see cref="onEvent"/> events</summary>
		public void publish( A tA, B tB )
		{
			publish();
			
			Action<A,B> tempEvent = onEvent;
			if ( tempEvent != null )
			{
				tempEvent( tA, tB );
			}
		}
	}
	
	//##########################
	// Class Declaration
	//##########################
	/// <summary>3-Parameter Publisher</summary>
	public class VisualDelegate<A,B,C> : VisualDelegate
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Event for 3-Parameter delegates</summary>
		public event Action<A,B,C> onEvent;
		
		//=======================
		// Destructor
		//=======================
		~VisualDelegate()
		{
			onEvent = null;
		}
		
		//=======================
		// Types
		//=======================
		public override Type[] types
		{
			get
			{
				return new Type[] { typeof( A ), typeof( B ), typeof( C ) };
			}
		}
		
		//=======================
		// Call
		//=======================
		protected override void effectsCallAdded( RawDelegate tCall )
		{
			Action<A,B,C> tempDelegate = tCall.delegateInstance as Action<A,B,C>;
			if ( tempDelegate == null )
			{
				base.effectsCallAdded( tCall );
			}
			else
			{
				onEvent += tempDelegate;
			}
		}
		
		protected override void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action<A,B,C> tempDelegate = tCall.delegateInstance as Action<A,B,C>;
			if ( tempDelegate == null )
			{
				base.effectsCallRemoved( tCall, tIndex );
			}
			else
			{
				onEvent -= tempDelegate;
			}
		}
		
		
		//=======================
		// Publish
		//=======================
		/// <summary>Invokes the <see cref="VisualDelegate.onVoid"/> and <see cref="onEvent"/> events</summary>
		public void publish( A tA, B tB, C tC )
		{
			publish();
			
			Action<A,B,C> tempEvent = onEvent;
			if ( tempEvent != null )
			{
				tempEvent( tA, tB, tC );
			}
		}
	}
	
	//##########################
	// Class Declaration
	//##########################
	/// <summary>4-Parameter Publisher</summary>
	public class VisualDelegate<A,B,C,D> : VisualDelegate
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Event for 4-Parameter delegates</summary>
		public event Action<A,B,C,D> onEvent;
		//=======================
		// Destructor
		//=======================
		~VisualDelegate()
		{
			onEvent = null;
		}
		
		//=======================
		// Types
		//=======================
		public override Type[] types
		{
			get
			{
				return new Type[] { typeof( A ), typeof( B ), typeof( C ), typeof( D ) };
			}
		}
		
		//=======================
		// Call
		//=======================
		protected override void effectsCallAdded( RawDelegate tCall )
		{
			Action<A,B,C,D> tempDelegate = tCall.delegateInstance as Action<A,B,C,D>;
			if ( tempDelegate == null )
			{
				base.effectsCallAdded( tCall );
			}
			else
			{
				onEvent += tempDelegate;
			}
		}
		
		protected override void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action<A,B,C,D> tempDelegate = tCall.delegateInstance as Action<A,B,C,D>;
			if ( tempDelegate == null )
			{
				base.effectsCallRemoved( tCall, tIndex );
			}
			else
			{
				onEvent -= tempDelegate;
			}
		}
		
	
		//=======================
		// Publish
		//=======================
		/// <summary>Invokes the <see cref="VisualDelegate.onVoid"/> and <see cref="onEvent"/> events</summary>
		public void publish( A tA, B tB, C tC, D tD )
		{
			publish();
			
			Action<A,B,C,D> tempEvent = onEvent;
			if ( tempEvent != null )
			{
				tempEvent( tA, tB, tC, tD );
			}
		}
	}
	
	//##########################
	// Class Declaration
	//##########################
	/// <summary>5-Parameter Publisher</summary>
	public class VisualDelegate<A,B,C,D,E> : VisualDelegate
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Event for 5-Parameter delegates</summary>
		public event Action<A,B,C,D,E> onEvent;
		
		
		//=======================
		// Destructor
		//=======================
		~VisualDelegate()
		{
			onEvent = null;
		}
		
		//=======================
		// Types
		//=======================
		public override Type[] types
		{
			get
			{
				return new Type[] { typeof( A ), typeof( B ), typeof( C ), typeof( D ), typeof( E ) };
			}
		}
		
		//=======================
		// Call
		//=======================
		protected override void effectsCallAdded( RawDelegate tCall )
		{
			Action<A,B,C,D,E> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E>;
			if ( tempDelegate == null )
			{
				base.effectsCallAdded( tCall );
			}
			else
			{
				onEvent += tempDelegate;
			}
		}
		
		protected override void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action<A,B,C,D,E> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E>;
			if ( tempDelegate == null )
			{
				base.effectsCallRemoved( tCall, tIndex );
			}
			else
			{
				onEvent -= tempDelegate;
			}
		}
		
	

		//=======================
		// Publish
		//=======================
		/// <summary>Invokes the <see cref="VisualDelegate.onVoid"/> and <see cref="onEvent"/> events</summary>
		public void publish( A tA, B tB, C tC, D tD, E tE )
		{
			publish();
			
			Action<A,B,C,D,E> tempEvent = onEvent;
			if ( tempEvent != null )
			{
				tempEvent( tA, tB, tC, tD, tE );
			}
		}
	}
	
	//##########################
	// Class Declaration
	//##########################
	/// <summary>6-Parameter Publisher</summary>
	public class VisualDelegate<A,B,C,D,E,F> : VisualDelegate
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Event for 6-Parameter delegates</summary>
		public event Action<A,B,C,D,E,F> onEvent;
		
		
		//=======================
		// Destructor
		//=======================
		~VisualDelegate()
		{
			onEvent = null;
		}
		
		//=======================
		// Types
		//=======================
		public override Type[] types
		{
			get
			{
				return new Type[] { typeof( A ), typeof( B ), typeof( C ), typeof( D ), typeof( E ), typeof( F ) };
			}
		}
		
		//=======================
		// Call
		//=======================
		protected override void effectsCallAdded( RawDelegate tCall )
		{
			Action<A,B,C,D,E,F> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E,F>;
			if ( tempDelegate == null )
			{
				base.effectsCallAdded( tCall );
			}
			else
			{
				onEvent += tempDelegate;
			}
		}
		
		protected override void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action<A,B,C,D,E,F> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E,F>;
			if ( tempDelegate == null )
			{
				base.effectsCallRemoved( tCall, tIndex );
			}
			else
			{
				onEvent -= tempDelegate;
			}
		}
		
		

		//=======================
		// Publish
		//=======================
		/// <summary>Invokes the <see cref="VisualDelegate.onVoid"/> and <see cref="onEvent"/> events</summary>
		public void publish( A tA, B tB, C tC, D tD, E tE, F tF )
		{
			publish();
			
			Action<A,B,C,D,E,F> tempEvent = onEvent;
			if ( tempEvent != null )
			{
				tempEvent( tA, tB, tC, tD, tE, tF );
			}
		}
	}
	
	//##########################
	// Class Declaration
	//##########################
	/// <summary>7-Parameter Publisher</summary>
	public class VisualDelegate<A,B,C,D,E,F,G> : VisualDelegate
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Event for 7-Parameter delegates</summary>
		public event Action<A,B,C,D,E,F,G> onEvent;
		
		//=======================
		// Destructor
		//=======================
		~VisualDelegate()
		{
			onEvent = null;
		}
		
		//=======================
		// Types
		//=======================
		public override Type[] types
		{
			get
			{
				return new Type[] { typeof( A ), typeof( B ), typeof( C ), typeof( D ), typeof( E ), typeof( F ), typeof( G ) };
			}
		}
		
		//=======================
		// Call
		//=======================
		protected override void effectsCallAdded( RawDelegate tCall )
		{
			Action<A,B,C,D,E,F,G> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E,F,G>;
			if ( tempDelegate == null )
			{
				base.effectsCallAdded( tCall );
			}
			else
			{
				onEvent += tempDelegate;
			}
		}
		
		protected override void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action<A,B,C,D,E,F,G> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E,F,G>;
			if ( tempDelegate == null )
			{
				base.effectsCallRemoved( tCall, tIndex );
			}
			else
			{
				onEvent -= tempDelegate;
			}
		}
		
	

		//=======================
		// Publish
		//=======================
		/// <summary>Invokes the <see cref="VisualDelegate.onVoid"/> and <see cref="onEvent"/> events</summary>
		public void publish( A tA, B tB, C tC, D tD, E tE, F tF, G tG )
		{
			publish();
			
			Action<A,B,C,D,E,F,G> tempEvent = onEvent;
			if ( tempEvent != null )
			{
				tempEvent( tA, tB, tC, tD, tE, tF, tG );
			}
		}
	}
	
	//##########################
	// Class Declaration
	//##########################
	/// <summary>8-Parameter Publisher</summary>
	public class VisualDelegate<A,B,C,D,E,F,G,H> : VisualDelegate
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Event for 8-Parameter delegates</summary>
		public event Action<A,B,C,D,E,F,G,H> onEvent;
		
		
		//=======================
		// Destructor
		//=======================
		~VisualDelegate()
		{
			onEvent = null;
		}
		
		//=======================
		// Types
		//=======================
		public override Type[] types
		{
			get
			{
				return new Type[] { typeof( A ), typeof( B ), typeof( C ), typeof( D ), typeof( E ), typeof( F ), typeof( G ), typeof( H ) };
			}
		}
		
		//=======================
		// Call
		//=======================
		protected override void effectsCallAdded( RawDelegate tCall )
		{
			Action<A,B,C,D,E,F,G,H> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E,F,G,H>;
			if ( tempDelegate == null )
			{
				base.effectsCallAdded( tCall );
			}
			else
			{
				onEvent += tempDelegate;
			}
		}
		
		protected override void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action<A,B,C,D,E,F,G,H> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E,F,G,H>;
			if ( tempDelegate == null )
			{
				base.effectsCallRemoved( tCall, tIndex );
			}
			else
			{
				onEvent -= tempDelegate;
			}
		}
		
	

		//=======================
		// Publish
		//=======================
		/// <summary>Invokes the <see cref="VisualDelegate.onVoid"/> and <see cref="onEvent"/> events</summary>
		public void publish( A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH )
		{
			publish();
			
			Action<A,B,C,D,E,F,G,H> tempEvent = onEvent;
			if ( tempEvent != null )
			{
				tempEvent( tA, tB, tC, tD, tE, tF, tG, tH );
			}
		}
	}
	
	//##########################
	// Class Declaration
	//##########################
	/// <summary>9-Parameter Publisher</summary>
	public class VisualDelegate<A,B,C,D,E,F,G,H,I> : VisualDelegate
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Event for 9-Parameter delegates</summary>
		public event Action<A,B,C,D,E,F,G,H,I> onEvent;
		
		// Destructor
		//=======================
		~VisualDelegate()
		{
			onEvent = null;
		}
		
		//=======================
		// Types
		//=======================
		public override Type[] types
		{
			get
			{
				return new Type[] { typeof( A ), typeof( B ), typeof( C ), typeof( D ), typeof( E ), typeof( F ), typeof( G ), typeof( H ), typeof( I ) };
			}
		}
		
		//=======================
		// Call
		//=======================
		protected override void effectsCallAdded( RawDelegate tCall )
		{
			Action<A,B,C,D,E,F,G,H,I> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E,F,G,H,I>;
			if ( tempDelegate == null )
			{
				base.effectsCallAdded( tCall );
			}
			else
			{
				onEvent += tempDelegate;
			}
		}
		
		protected override void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action<A,B,C,D,E,F,G,H,I> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E,F,G,H,I>;
			if ( tempDelegate == null )
			{
				base.effectsCallRemoved( tCall, tIndex );
			}
			else
			{
				onEvent -= tempDelegate;
			}
		}
		
	
		//=======================
		// Publish
		//=======================
		/// <summary>Invokes the <see cref="VisualDelegate.onVoid"/> and <see cref="onEvent"/> events</summary>
		public void publish( A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH, I tI )
		{
			publish();
			
			Action<A,B,C,D,E,F,G,H,I> tempEvent = onEvent;
			if ( tempEvent != null )
			{
				tempEvent( tA, tB, tC, tD, tE, tF, tG, tH, tI );
			}
		}
	}
	
	//##########################
	// Class Declaration
	//##########################
	/// <summary>10-Parameter Publisher</summary>
	public class VisualDelegate<A,B,C,D,E,F,G,H,I,J> : VisualDelegate
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Event for 10-Parameter delegates</summary>
		public event Action<A,B,C,D,E,F,G,H,I,J> onEvent;
		
		//=======================
		// Destructor
		//=======================
		~VisualDelegate()
		{
			onEvent = null;
		}
		
		//=======================
		// Types
		//=======================
		public override Type[] types
		{
			get
			{
				return new Type[] { typeof( A ), typeof( B ), typeof( C ), typeof( D ), typeof( E ), typeof( F ), typeof( G ), typeof( H ), typeof( I ), typeof( J ) };
			}
		}
		
		//=======================
		// Call
		//=======================
		protected override void effectsCallAdded( RawDelegate tCall )
		{
			Action<A,B,C,D,E,F,G,H,I,J> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E,F,G,H,I,J>;
			if ( tempDelegate == null )
			{
				base.effectsCallAdded( tCall );
			}
			else
			{
				onEvent += tempDelegate;
			}
		}
		
		protected override void effectsCallRemoved( RawDelegate tCall, int tIndex )
		{
			Action<A,B,C,D,E,F,G,H,I,J> tempDelegate = tCall.delegateInstance as Action<A,B,C,D,E,F,G,H,I,J>;
			if ( tempDelegate == null )
			{
				base.effectsCallRemoved( tCall, tIndex );
			}
			else
			{
				onEvent -= tempDelegate;
			}
		}
	
		//=======================
		// Publish
		//=======================
		/// <summary>Invokes the <see cref="VisualDelegate.onVoid"/> and <see cref="onEvent"/> events</summary>
		public void publish( A tA, B tB, C tC, D tD, E tE, F tF, G tG, H tH, I tI, J tJ )
		{
			publish();
			
			Action<A,B,C,D,E,F,G,H,I,J> tempEvent = onEvent;
			if ( tempEvent != null )
			{
				tempEvent( tA, tB, tC, tD, tE, tF, tG, tH, tI, tJ );
			}
		}
	}
}