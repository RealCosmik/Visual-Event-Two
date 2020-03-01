using UnityEngine;
using System;
using EventsPlus;

namespace EventsPlusTest
{
	public class InputTest : MonoBehaviour
	{
		public Publisher onLeftClick;
		public Publisher onRightClick;
		public KeyCode leftClickButton = KeyCode.Mouse0;
		public KeyCode rightClickButton = KeyCode.Mouse1;
		
		public void Awake()
		{
			// Initialize Singleton
			Singleton tempSingleton = Singleton.instance;
			
			// Initialize Publishers
			onLeftClick.initialize();
			onRightClick.initialize();
		}
		
		public void Update()
		{
			if ( Input.GetKeyDown( leftClickButton ) )
			{
				onLeftClick.publish();
			}
			else if ( Input.GetKeyDown( rightClickButton ) )
			{
				onRightClick.publish();
			}
		}
	}
}