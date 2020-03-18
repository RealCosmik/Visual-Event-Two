using UnityEngine;
using System;
using VisualEvent;

namespace EventsPlusTest
{
	public class InputTest : MonoBehaviour
	{
		public VisualDelegate onLeftClick;
		public VisualDelegate onRightClick;
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
				onLeftClick.Invoke();
			}
			else if ( Input.GetKeyDown( rightClickButton ) )
			{
				onRightClick.Invoke();
			}
		}
	}
}