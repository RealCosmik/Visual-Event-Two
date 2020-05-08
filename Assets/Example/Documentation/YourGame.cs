using UnityEngine;
using VisualDelegates;

namespace EventsPlusTest
{
	public class YourGame : MonoBehaviour
	{
		public int score;

		public void addScore()
		{
			score += 1;
		}
		
		public void addScore( int tAmount )
		{
			score += tAmount;
		}
	}
}