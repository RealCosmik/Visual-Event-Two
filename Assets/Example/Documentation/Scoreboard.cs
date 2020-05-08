using UnityEngine;
using System;
using VisualDelegates;

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