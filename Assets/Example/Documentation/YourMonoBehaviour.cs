using UnityEngine;
using System;
using VisualDelegates;

namespace EventsPlusTest
{
	[Serializable]
	public class PublisherInt : VisualDelegate<int>
	{
	}

	public class YourMonoBehaviour : MonoBehaviour
	{
		public PublisherInt publisher;
		
		public void Awake()
		{
			publisher.initialize();
		}
		
		public void Start()
		{
			publisher.Invoke( 5 );
		}
	}
}