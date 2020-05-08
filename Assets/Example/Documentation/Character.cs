using UnityEngine;
using System;
using VisualDelegates;

namespace EventsPlusTest
{
	public class Character : MonoBehaviour
	{
		public Rigidbody body;
		
		public void onJumpInput( float tStrength )
		{
			body.AddForce( Vector3.up * tStrength );
		}
	}
}