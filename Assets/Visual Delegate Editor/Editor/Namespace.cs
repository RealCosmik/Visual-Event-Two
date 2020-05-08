using System;
using System.Collections.Generic;

namespace VisualDelegates.Editor
{
	//##########################
	// Struct Declaration
	//##########################
	/// <summary>Utility container for cached namespace data</summary>
	public struct Namespace
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Namespace name</summary>
		private string _name;
		/// <summary>Immutable list of class types associated with the namespace</summary>
		private List<Type> _classes;
		
		//=======================
		// Constructor
		//=======================
		/// <summary>Constructor</summary>
		/// <param name="tName">Namespace name</param>
		/// <param name="tClasses">List of associated class names</param>
		public Namespace( string tName, List<Type> tClasses )
		{
			_name = tName;
			_classes = tClasses;
		}
		
		//=======================
		// Accessors
		//=======================
		/// <summary>Gets the <see cref="_name"/> of the namespace</summary>
		public string name
		{
			get
			{
				return _name;
			}
		}
		
		/// <summary>Gets the namespace <see cref="_classes"/></summary>
		public List<Type> classes
		{
			get
			{
				return _classes;
			}
		}
	}
}