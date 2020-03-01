using UnityEngine;
using System;
using EventsPlus;

namespace EventsPlusTest
{
	public class Scoreboard : MonoBehaviour
	{
		public int score = 0;
		
		public void addScore( int tScore )
		{
			score += tScore;
		}
		
		public void addScore()
		{
			++score;
		}
	}
}