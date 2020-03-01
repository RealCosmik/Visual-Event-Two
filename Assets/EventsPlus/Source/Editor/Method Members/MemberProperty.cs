using System;
using System.Reflection;

namespace EventsPlus
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Utility class for a cached <see cref="System.Reflection.PropertyInfo"/></summary>
	public sealed class MemberProperty : Member<PropertyInfo>
	{
		public override string GetdisplayName()=> "set " + m_info.PropertyType.GetKeyword() + " " + m_info.Name;
        public MemberProperty( PropertyInfo tInfo ) : base( tInfo) { }
	}
}