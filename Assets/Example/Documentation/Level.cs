using UnityEngine;
using VisualDelegates;

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
			onReset.Invoke();
		}
	}
}