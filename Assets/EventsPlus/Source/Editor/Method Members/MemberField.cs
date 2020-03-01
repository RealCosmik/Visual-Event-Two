using System;
using System.Reflection;

namespace EventsPlus
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Utility class for a cached <see cref="System.Reflection.FieldInfo"/></summary>
	public sealed class MemberField : Member<FieldInfo>
	{		
		public MemberField( FieldInfo tInfo ) : base( tInfo) { }
		
		public override string GetdisplayName()=>$"{m_info.FieldType.GetKeyword()} {m_info.Name}";
	}
}