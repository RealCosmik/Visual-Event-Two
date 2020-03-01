using UnityEngine;
using EventsPlus;

namespace EventsPlusTest
{
	public class Explosion : MonoBehaviour
	{
		public Publisher onExplode;
	
		public void Awake()
		{
			onExplode.initialize();
		}
		
		public void Start()
		{
			onExplode.publish();
		}
	}
}