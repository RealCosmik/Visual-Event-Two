using UnityEngine;
using System;
using VisualDelegates;

namespace EventsPlusTest
{
	public class Singleton : MonoBehaviour
	{
		protected static Singleton _instance;
		protected static bool isExiting; // used to prevent creation of singleton when closing the game
		
		public void Awake()
		{
			// Deny new instances, set this one as persistent
			if ( ReferenceEquals( _instance, null ) )
			{
				_instance = this;
				DontDestroyOnLoad( gameObject );
			}
			else
			{
				Debug.Log( "Cannot have more than one instance of Singleton!" );
				Destroy( gameObject );
			}
		}
		
		public void OnDestroy()
		{
			// Clear instance
			if ( ReferenceEquals( _instance, this ) )
			{
				_instance = null;
			}
		}
		
		public void OnApplicationQuit()
		{
			isExiting = true;
		}
			
		public static Singleton instance
		{
			get
			{
				if ( isExiting )
				{
					return null;
				}
				else if ( ReferenceEquals( _instance, null ) )
				{
					Instantiate<GameObject>( Resources.Load<GameObject>( "Singleton" ) ); // this only works because engine is single-threaded (you don't need thread locks)
					if ( _instance == null )
					{
						Debug.LogWarning( "No \"Singleton\" prefab found in a \"Resources\" folder! Creating new instance!" );
						
						GameObject tempObject = new GameObject();
						tempObject.name = "Singleton";
						tempObject.AddComponent<Singleton>();
					}
				}
				
				return _instance;
			}
		}
		
		public void onLeftClick()
		{
			Debug.Log( "Left clicked!" );
		}
		
		public void onRightClick()
		{
			Debug.Log( "Right clicked!" );
		}
	}
}