using System;
using System.Reflection;

namespace EventsPlus
{
	/// <summary>
    /// Interface for seralized members that reside in class fields <see cref="MemberField"/> ,props<see cref="MemberProperty"/> 
    /// or methods <see cref="MemberMethod"/>
    /// </summary>
	public interface IMember
	{
		MemberInfo info { get; }
		string serializedName { get; }
        string GetdisplayName();
	}
}