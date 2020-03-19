using System;
using System.Text;
using System.Reflection;

namespace VisualEvent
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Utility class for a cached <see cref="System.Reflection.MethodInfo"/></summary>
	public sealed class MemberMethod : Member<MethodInfo>
	{
		public MemberMethod( MethodInfo tInfo ) : base( tInfo) { }
        public override bool isvaluetype => m_info.ReturnType.IsValueType;
        public override string GetdisplayName()
		{
				StringBuilder tempName = new StringBuilder( m_info.ReturnType.GetKeyword() );
				tempName.Append( " " );
				tempName.Append( m_info.Name );
				
				ParameterInfo[] tempParameters = m_info.GetParameters();
				if ( tempParameters != null )
				{
					tempName.Append( "(" );
					int tempListLength = tempParameters.Length;
					for ( int i = 0; i < tempListLength; ++i )
					{
						tempName.Append( " " );
						tempName.Append( tempParameters[i].ParameterType.GetKeyword() );
						
						if ( i < ( tempListLength - 1 ) )
						{
							tempName.Append( "," );
						}
					}
					tempName.Append( " )" );
				}
				
				return tempName.ToString();
		}
	}
}