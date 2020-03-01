using System;
using UnityEngine;

namespace EventsPlus
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Helper component used to automatically hookup delegates to Publishers</summary>
	public class Subscriber : MonoBehaviour
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Global event that gets fired when a Subscriber is instantiated</summary>
		public static event Action<Subscriber> OnLoaded;
		/// <summary>Global event that gets fired when a Subscriber is destroyed</summary>
		public static event Action<Subscriber> OnDestroyed;
		/// <summary>List of raw <see cref="RawRequest"/> objects used for binding to <see cref="Publisher"/>s</summary>
		[SerializeField]
		protected RawRequest[] _requests;
		
		//=======================
		// Constructor
		//=======================
		/// <summary>Initializes <see cref="_requests"/> and registers with any potential <see cref="Publisher"/> instances</summary>
		protected virtual void Awake()
		{
			// Initialize requests
			if ( _requests != null )
			{
				int tempListLength = _requests.Length;
				for ( int i = 0; i < tempListLength; ++i )
				{
					_requests[i].initialize();
				}
			}
			
			// Subscribe to Publisher
			Publisher.OnLoaded += onPublisherLoaded;
			
			// Fire load event
			Action<Subscriber> tempEvent = OnLoaded;
			if ( tempEvent != null )
			{
				tempEvent( this );
			}
		}
		
		//=======================
		// Destructor
		//=======================
		/// <summary>Unregisters from <see cref="Publisher"/> instances and clears memory usage</summary>
		protected virtual void OnDestroy()
		{
			// Unsubscribe from Publisher
			Publisher.OnLoaded -= onPublisherLoaded;
			
			// Fire destroyed event
			Action<Subscriber> tempEvent = OnDestroyed;
			if ( tempEvent != null )
			{
				tempEvent( this );
			}
			
			// Clear memory
			_requests = null;
		}
		
		//=======================
		// Publisher
		//=======================
		/// <summary>Callback for when a <see cref="Publisher"/> is loaded; this will back a confirmation so the Subscriber can be processed</summary>
		protected virtual void onPublisherLoaded( Publisher tPublisher )
		{
			// Directly notify Publisher
			tPublisher.onSubscriberLoaded( this );
		}
		
		//=======================
		// Requests
		//=======================
		/// <summary>Gets the <see cref="_requests"/> array</summary>
		public RawRequest[] requests
		{
			get
			{
				return _requests;
			}
		}
	}
}