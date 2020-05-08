using System;
using System.Text;
using System.Reflection;

namespace VisualDelegates.Editor
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Utility class for a cached <see cref="System.Reflection.MethodInfo"/></summary>
    public sealed class MemberMethod : Member<MethodInfo>
    {
        ParameterInfo[] paraminfo;
        string m_methodDisplayName;
        public MemberMethod(MethodInfo tInfo) : base(tInfo) { }
        public override bool isvaluetype => m_info.ReturnType.IsValueType;
        public override string GetdisplayName()
        {
            return m_methodDisplayName ?? GenerateDisplayName();
        }
        private string GenerateDisplayName()
        {
            StringBuilder tempName = new StringBuilder(m_info.ReturnType.GetKeyword());
            tempName.Append(" ");
            tempName.Append(m_info.Name);

            paraminfo = paraminfo ?? m_info.GetParameters();
            if (paraminfo != null)
            {
                tempName.Append("(");
                int tempListLength = paraminfo.Length;
                for (int i = 0; i < tempListLength; ++i)
                {
                    tempName.Append(" ");
                    tempName.Append(paraminfo[i].ParameterType.GetKeyword());

                    if (i < (tempListLength - 1))
                    {
                        tempName.Append(",");
                    }
                }
                tempName.Append(" )");
            }

           m_methodDisplayName= tempName.ToString();
            return m_methodDisplayName;
        }
    }
}