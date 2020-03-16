using UnityEngine;
using System;
using EventsPlus;

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
			publisher.publish( 5 );
		}
	}
}