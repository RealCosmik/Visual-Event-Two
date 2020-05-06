using System;
using System.Reflection;

namespace VisualEvent.Editor
{
	//##########################
	// Struct Declaration
	//##########################
	/// <summary>Utility container for cached method argument data</summary>
	public class Argument
	{
        //=======================
        // Variables
        //=======================
        /// <summary>Parameter name/label</summary>
        public string name { get; protected set; }
        /// <summary>Argument type</summary>
        public Type type { get; protected set; }

        //=======================
        // Constructor
        //=======================
        public Argument()
        {

        }
		/// <summary>Constructor</summary>
		/// <param name="tName">Name of the parameter</param>
		/// <param name="tType">Type of argument</param>
		public Argument( string tName, Type tType )
		{
			name = tName;
			type = tType;
		}
		
		/// <summary>Constructor</summary>
		/// <param name="tField"><see cref="System.Reflection.FieldInfo"/> to instantiate the argument information</param>
		public Argument( FieldInfo tField )
		{
			name = tField.Name;
			type = tField.FieldType;
		}
		
		/// <summary>Constructor</summary>
		/// <param name="tProperty"><see cref="System.Reflection.PropertyInfo"/> to instantiate the argument information</param>
		public Argument( PropertyInfo tProperty )
		{
			name = tProperty.Name;
			type = tProperty.PropertyType;
		}
		
		/// <summary>Constructor</summary>
		/// <param name="tParameter"><see cref="System.Reflection.ParameterInfo"/> to instantiate the argument information</param>
		public Argument( ParameterInfo tParameter )
		{
			name = tParameter.Name;
			type = tParameter.ParameterType;
		}
	}
}