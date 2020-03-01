using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Linq;
using System.Reflection;
namespace EventsPlus
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Inspector class for rendering the <see cref="Publisher"/> in the inspector</summary>
	[CustomPropertyDrawer( typeof( Publisher ), true )]
	public class DrawerPublisher : PropertyDrawer
	{
        
		//=======================
		// Variables
		//=======================
		/// <summary>Cached <see cref="UnityEditorInternal.ReorderableList"/> dictionary used for optimization</summary>
		private Dictionary<string,ReorderableList> cache = new Dictionary<string,ReorderableList>();

		//=======================
		// Initialization
		//=======================
		/// <summary>Initializes the drawer and calculates the inspector height</summary>
		/// <param name="tProperty">Serialized <see cref="Publisher"/> property</param>
		/// <param name="tLabel">GUI Label of the drawer</param>
		/// <returns>Height of the drawer</returns>
		public override float GetPropertyHeight( SerializedProperty tProperty, GUIContent tLabel )
		{
            // Initialize reorderable list
			SerializedProperty tempCallsProperty = tProperty.FindPropertyRelative( "_calls" );
			ReorderableList tempList;
			if ( !cache.TryGetValue( tProperty.propertyPath, out tempList ) )
			{
				tempList = new ReorderableList( tProperty.serializedObject, tempCallsProperty, true, true, true, true );
				tempList.footerHeight = EditorGUIUtility.singleLineHeight;

                tempList.drawHeaderCallback += rect =>
                {
                };
				tempList.drawElementCallback += ( Rect tPosition, int tIndex, bool tIsActive, bool tIsFocused ) =>
				{
					EditorGUI.PropertyField( tPosition, tempList.serializedProperty.GetArrayElementAtIndex( tIndex ), true );
				};
				tempList.elementHeightCallback += ( int tIndex ) =>
				{
					return EditorGUI.GetPropertyHeight( tempList.serializedProperty.GetArrayElementAtIndex( tIndex ) ) + EditorGUIUtility.standardVerticalSpacing;
				};
				cache.Add( tProperty.propertyPath, tempList );
              tempList.onRemoveCallback += list => onElementDelete(list, tempCallsProperty);
             //   tempList.onAddCallback += list => onAddElement(list, tempCallsProperty);
			}
			
			// Calculate height
			float tempHeight = base.GetPropertyHeight( tProperty, tLabel );
			if ( tProperty.isExpanded )
			{
				tempHeight += EditorGUI.GetPropertyHeight( tProperty.FindPropertyRelative( "_tag" ), true ) + EditorGUIUtility.standardVerticalSpacing;
				tempHeight += tempList.GetHeight() + ( tempCallsProperty.arraySize * EditorGUIUtility.standardVerticalSpacing  );
			}
			return tempHeight;
		}
		
		//=======================
		// Render
		//=======================
		/// <summary>Renders the individual <see cref="Publisher"/> property</summary>
		/// <param name="tPosition">Inspector position and size of <paramref name="tProperty"/></param>
		/// <param name="tProperty">Serialized <see cref="Publisher"/> property</param>
		/// <param name="tLabel">GUI Label of the drawer</param>
		public override void OnGUI( Rect tPosition, SerializedProperty tProperty, GUIContent tLabel )
		{
            // Draw
            tPosition.height = base.GetPropertyHeight( tProperty, tLabel );
			tProperty.isExpanded = EditorGUI.Foldout( tPosition, tProperty.isExpanded, tProperty.displayName );
			if ( tProperty.isExpanded )
			{
				++EditorGUI.indentLevel;
				
				// Tag
				SerializedProperty tempTagProperty = tProperty.FindPropertyRelative( "_tag" );
				
				tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing;
				tPosition.height = EditorGUI.GetPropertyHeight( tempTagProperty );
				
				EditorGUI.BeginChangeCheck();
				string tempTag = EditorGUI.TextField( tPosition, tempTagProperty.displayName, tempTagProperty.stringValue );
				if ( EditorGUI.EndChangeCheck() )
				{
					tempTagProperty.stringValue = tempTag;
					tProperty.serializedObject.ApplyModifiedProperties();
				} 
				// Calls
				ReorderableList tempList = cache[ tProperty.propertyPath ];
                int tempIndentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				
				float tempIndentSize = tempIndentLevel * EditorUtility.IndentSize;
				tPosition.x += tempIndentSize;
				tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing;
				tPosition.width -= tempIndentSize;
				tPosition.height = tempList.GetHeight();
				
				tempList.DoList( tPosition );
				
				EditorGUI.indentLevel = tempIndentLevel - 1;
			}
		}

        private void SetManagedPublisherReference(SerializedProperty publiserproperty)
        {
            Debug.Log(publiserproperty.managedReferenceFieldTypename);
            int StartTypeName_index = publiserproperty.managedReferenceFullTypename.IndexOf(' ')+1;
            Debug.Log(StartTypeName_index);
            string TypeName = publiserproperty.managedReferenceFieldTypename.Substring(StartTypeName_index);
            Debug.Log(TypeName);
            var publisher = Utility.CreatePublisherFromTypeName(TypeName);
            publiserproperty.managedReferenceValue = publisher;
        }
        private void onElementDelete(ReorderableList list,SerializedProperty arrayprop)
        {
            var delegateprop = arrayprop.GetArrayElementAtIndex(list.index);
            string delegatepath = delegateprop.propertyPath;
            delegateprop.FindPropertyRelative("_arguments").ClearArray();
            delegateprop.serializedObject.ApplyModifiedProperties();
            DrawerRawCallView.cache[delegatepath].ClearViewCache();
            arrayprop.DeleteArrayElementAtIndex(list.index);
            arrayprop.serializedObject.ApplyModifiedProperties();
            Debug.Log("aye");
        } 
    }
}