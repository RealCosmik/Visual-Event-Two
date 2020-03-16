using UnityEngine;
using System;
using EventsPlus;

namespace EventsPlusTest
{
	[Serializable]
	public class PublisherFloat : VisualDelegate<float>
	{
	}
	
	public class CustomInput : MonoBehaviour
	{
		public PublisherFloat onJumpInput;
		public float jumpHeight = 5.5f;
		public KeyCode jumpKey = KeyCode.Space;
		
		public void Awake()
		{
			onJumpInput.initialize();
		}
		
		public void Update()
		{
			if ( Input.GetKeyDown( jumpKey ) )
			{
				onJumpInput.publish( jumpHeight );
			}
		}
	}
}