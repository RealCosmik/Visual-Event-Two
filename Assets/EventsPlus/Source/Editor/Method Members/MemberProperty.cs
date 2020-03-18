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
		public override string GetdisplayName()=> m_seralizeData[2]+" prop. " + m_info.PropertyType.GetKeyword() + " " + m_info.Name;
        public MemberProperty( PropertyInfo tInfo,bool Setter=true ) : base(tInfo)
        {
            if (Setter)
                m_seralizeData[2] = "set";
            else m_seralizeData[2] = "get";

        }
	}
}