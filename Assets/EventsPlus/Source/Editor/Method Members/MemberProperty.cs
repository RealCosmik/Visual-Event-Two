using System;
using System.Reflection;

namespace VisualEvent
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Utility class for a cached <see cref="System.Reflection.PropertyInfo"/></summary>
	public sealed class MemberProperty : Member<PropertyInfo>
	{
		public override string GetdisplayName()=>"prop. " + m_info.PropertyType.GetKeyword() + " " + m_info.Name;
        public MemberProperty( PropertyInfo tInfo) : base(tInfo)
        {
        }
        public override bool isvaluetype => m_info.PropertyType.IsValueType;
    }
}