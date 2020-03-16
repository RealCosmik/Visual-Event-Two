using UnityEngine;
using EventsPlus;

namespace EventsPlusTest
{
	public class Level : MonoBehaviour
	{
		public VisualDelegate onReset;
	
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