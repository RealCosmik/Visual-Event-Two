using System;
using System.Reflection;

namespace EventsPlus
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Abstract utility class for cached member information</summary>
	public abstract class Member<T> : IMember where T : MemberInfo
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Reflection info</summary>
		protected T m_info;
        /// <summary>Cached serialized name of the member</summary>
        public string serializedName { get; protected set; }

        /// <summary>Gets the reflection <see cref="m_info"/></summary>
        public MemberInfo info => m_info;

        /// <summary>Gets the display name of the member for use in drop-downs</summary>
        public abstract string GetdisplayName();

        //=======================
        // Constructor
        //=======================
        /// <summary>Constructor</summary>
        /// <param name="tInfo">Reflection info</param>
        public Member( T tInfo )
		{
			m_info = tInfo;
            //Custom seralize it here so that at run time we can decode it and retrieve the member data
			serializedName = Utility.Serialize( m_info );
		}
	}
}