using UnityEditor;
using System;
using System.Collections.Generic;

namespace VisualEvent
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Stores cached data for filter drop-downs; used by the <see cref="DrawerFilter"/></summary>
	public sealed class CacheFilter
	{
		//=======================
		// Variables
		//=======================
		/// <summary>Cached <see cref="Namespace"/>s</summary>
		private List<Namespace> _namespaces;
		/// <summary>Display names of each namespace</summary>
		private string[] _namespaceNames;
		/// <summary>Index of the currently selected namespace</summary>
		private int _selectedNamespace;
		/// <summary>Display names of each of the classes belonging to the selected namespace</summary>
		private string[] _classNames;
		/// <summary>Index of the currently selected class</summary>
		private int _selectedClass;
		/// <summary>Cached members belonging to the selected class</summary>
		private List<IMember> _members;
		/// <summary>Display names of each of the members belonging to the selected class</summary>
		private string[] _memberNames;
		
		//=======================
		// Constructor
		//=======================
		/// <summary>Constructor</summary>
		/// <param name="tNamespaces">Cached namespaces</param>
		/// <param name="tNamespaceNames">Cached namespace display names</param>
		public CacheFilter( List<Namespace> tNamespaces, string[] tNamespaceNames )
		{
			_namespaces = tNamespaces;
			_namespaceNames = tNamespaceNames;
		}
		
		//=======================
		// Namespace
		//=======================
		/// <summary>Gets the display name of the currently selected namespace</summary>
		public string namespaceName
		{
			get
			{
				return _namespaceNames[ _selectedNamespace ];
			}
		}
		
		/// <summary>Gets/Sets the <see cref="_selectedNamespace"/>; if set, will regenerate the class names</summary>
		public int selectedNamespace
		{
			get
			{
				return _selectedNamespace;
			}
			set
			{
				_selectedNamespace = value < 0 ? 0 : value;
				
				// Generate class names
				List<Type> tempClasses = _namespaces[ _selectedNamespace ].classes;
				int tempListLength = tempClasses.Count;
				_classNames = new string[ tempListLength ];
				for ( int i = ( tempListLength - 1 ); i >= 0; --i )
				{
					_classNames[i] = tempClasses[i].Name;
				}
				
				// Default class
				selectedClass = 0;
			}
		}
		
		/// <summary>Checks for discrepancies between the <see cref="UnityEditor.SerializedProperty"/>s and the cached data; tries to match the cache to the properties</summary>
		/// <param name="tNamespace">Selected namespace property</param>
		/// <param name="tClass">Selected class property</param>
		/// <returns>True if the data matches</returns>
		public bool validateNamespace( SerializedProperty tNamespace, SerializedProperty tClass )
		{
			if ( tNamespace.stringValue != _namespaceNames[ _selectedNamespace ] )
			{
				selectedNamespace = Array.IndexOf( _namespaceNames, tNamespace.stringValue );
				selectedClass = Array.IndexOf( _classNames, tClass.stringValue );
				
				return false;
			}
			
			return true;
		}
		
		//=======================
		// Class
		//=======================
		/// <summary>Gets the <see cref="_classNames"/> of the selected namespace</summary>
		public string[] classNames
		{
			get
			{
				return _classNames;
			}
		}
		
		/// <summary>Gets the selected class name</summary>
		public string className
		{
			get
			{
				return _classNames[ _selectedClass ];
			}
		}
		
		/// <summary>Gets/Sets the <see cref="_selectedClass"/>; if set, will regenerate the members</summary>
		public int selectedClass
		{
			get
			{
				return _selectedClass;
			}
			set
			{
				_selectedClass = value < 0 ? 0 : value;

                // Generate members
                _members = _namespaces[_selectedNamespace].classes[_selectedClass].GetMemberList(false);
				int tempListLength = _members.Count;
				_memberNames = new string[ tempListLength ];
				for ( int i = ( tempListLength - 1 ); i >= 0; --i )
				{
					_memberNames[i] = _members[i].GetdisplayName();
				}
			}
		}

		/// <summary>Checks for discrepancies between the <see cref="UnityEditor.SerializedProperty"/> and the cached data; tries to match the cache to the property</summary>
		/// <param name="tClass">Selected class property</param>
		/// <returns>True if the data matches</returns>
		public bool validateClass( SerializedProperty tClass )
		{
			if ( tClass.stringValue != _classNames[ _selectedClass ] )
			{
				selectedClass = Array.IndexOf( _classNames, tClass.stringValue );
				
				return false;
			}
			
			return true;
		}
		
		//=======================
		// Members
		//=======================
		/// <summary>Gets the <see cref="_members"/> of the selected class</summary>
		public List<IMember> members
		{
			get
			{
				return _members;
			}
		}
		
		/// <summary>Gets the <see cref="_memberNames"/> of the selected class</summary>
		public string[] memberNames
		{
			get
			{
				return _memberNames;
			}
		}
		
		/// <summary>Finds the index of a serialized member name within the <see cref="_members"/> list</summary>
		/// <param name="tSerializedName">Serialized name of the member being searched</param>
		/// <returns>Index if found, -1 if not</returns>
		public int findMember( string tSerializedName )
		{
			for ( int i = ( _members.Count - 1 ); i >= 0; --i )
			{
				if ( _members[i].serializedName == tSerializedName )
				{
					return i;
				}
			}
			
			return -1;
		}
	}
}