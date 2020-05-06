﻿using System;
using System.Reflection;

namespace VisualEvent.Editor
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Utility class for a cached <see cref="System.Reflection.FieldInfo"/></summary>
	public sealed class MemberField : Member<FieldInfo>
	{
        public MemberField(FieldInfo tInfo) : base(tInfo)
        {
        }
        public override bool isvaluetype => m_info.FieldType.IsValueType;
        public override string GetdisplayName()=>$"{m_info.FieldType.GetKeyword()} {m_info.Name}";
	}
}