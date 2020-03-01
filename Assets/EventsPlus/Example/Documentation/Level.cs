using UnityEngine;
using EventsPlus;

namespace EventsPlusTest
{
	public class Level : MonoBehaviour
	{
		public Publisher onReset;
	
		public void Awake()
		{
			onReset.initialize();
		}

		public void reset()
		{
			onReset.publish();
		}
	}
}