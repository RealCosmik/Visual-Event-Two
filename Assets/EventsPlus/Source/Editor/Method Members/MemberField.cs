using System;
using System.Reflection;

namespace VisualEvent
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Utility class for a cached <see cref="System.Reflection.FieldInfo"/></summary>
	public sealed class MemberField : Member<FieldInfo>
	{
        public MemberField(FieldInfo tInfo, bool Setter = true ) : base(tInfo)
        {
            if (Setter)
                m_seralizeData[2] = "SET";
            else m_seralizeData[2] = "GET";
        }
        public override bool isvaluetype => m_info.FieldType.IsValueType;
        public override string GetdisplayName()=>$"{m_info.FieldType.GetKeyword()} {m_seralizeData[2]} {m_info.Name}";
	}
}