using UnityEngine;
using System.Collections.Generic;

namespace VisualEvent
{
	//##########################
	// Struct Declaration
	//##########################
	/// <summary>Utility container used to represent a hashed version of the <see cref="Filter"/> structure</summary>
	public struct HashedFilter
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Override to expose filtered members in the inspector even if they are blacklisted by a base class</summary>
		private HashSet<string> _whiteList;
		/// <summary>Prevents any contained members from being exposed in the inspector</summary>
		private HashSet<string> _blackList;
		
		//=======================
		// Constructor
		//=======================
		/// <summary>Constructor</summary>
		/// <param name="tFilter"><see cref="Filter"/> to ingest and convert</param>
		public HashedFilter( Filter tFilter )
		{
			_whiteList = tFilter.whiteList == null ? null : new HashSet<string>( tFilter.whiteList );
			_blackList = tFilter.blackList == null ? null : new HashSet<string>( tFilter.blackList );
		}
		
		//=======================
		// Whitelist
		//=======================
		/// <summary>Gets the member <see cref="_whiteList"/></summary>
		public HashSet<string> whiteList
		{
			get
			{
				return _whiteList;
			}
		}
		
		/// <summary>Checks if a <see cref="IMember"/> is whitelisted</summary>
		/// <param name="tMember">Member to check</param>
		/// <returns>True if whitelisted</returns>
		public bool isWhiteListed( IMember tMember )
		{
			return _whiteList != null && _whiteList.Contains( tMember.serializedName );
		}
		
		//=======================
		// Blacklist
		//=======================
		/// <summary>Gets the member <see cref="_blackList"/></summary>
		public HashSet<string> blackList
		{
			get
			{
				return _blackList;
			}
		}
		
		/// <summary>Checks if a <see cref="IMember"/> is blacklisted</summary>
		/// <param name="tMember">Member to check</param>
		/// <returns>True if blacklisted</returns>
		public bool isBlackListed( IMember tMember )
		{
			return _blackList != null && _blackList.Contains( tMember.serializedName );
		}
	}
}