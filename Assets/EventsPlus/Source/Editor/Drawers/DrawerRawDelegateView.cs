using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace EventsPlus
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Inspector class for rendering <see cref="RawDelegate"/>s in the inspector</summary>
	public class DrawerRawDelegateView<T> : PropertyDrawer where T : RawDelegateView,new()
	{

        //=======================
        // Variables
        //=======================
        /// <summary>Cached delegate drop-down data used for optimization</summary>
        public static Dictionary<string, T> cache { get; protected set; } = new Dictionary<string, T>();
		//=======================
		// Initialization
		//=======================
		/// <summary>Initializes the drawer and calculates the inspector height</summary>
		/// <param name="tProperty">Serialized delegate property</param>
		/// <param name="tLabel">GUI Label of the drawer</param>
		/// <returns>Height of the drawer</returns>
		public override float GetPropertyHeight( SerializedProperty tProperty, GUIContent tLabel )
		{ 
            // Initialize cache
            if (!cache.TryGetValue(tProperty.propertyPath, out T delegeateCache))
            {
                cache.Add(tProperty.propertyPath, createCache(tProperty));
            }
            AssemblyReloadEvents.afterAssemblyReload += AssemblyValidation;

            return base.GetPropertyHeight( tProperty, tLabel );

            void AssemblyValidation()
            { 
                Debug.LogWarning("HOPE THIS WORKS");
                AssemblyReloadEvents.afterAssemblyReload -= AssemblyValidation;
            }

        }


        /// <summary>Instantiates the delegate drop-down <see cref="cache"/></summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <returns>Delegate cache</returns>
        protected virtual T createCache( SerializedProperty tProperty ) 
		{
            Debug.Log("creatint cache type");
            //return new T();
            return Activator.CreateInstance<T>();
        }
        
		
		//=======================
		// Render
		//=======================
		/// <summary>Renders the individual delegate property</summary>
		/// <param name="tPosition">Inspector position and size of <paramref name="tProperty"/></param>
		/// <param name="tProperty">Serialized delegate property</param>
		/// <param name="tLabel">GUI Label of the drawer</param>
		public override void OnGUI( Rect tPosition, SerializedProperty tProperty, GUIContent tLabel )
		{
            // Validate cache
			T DelegateCache = cache[ tProperty.propertyPath ];
            if (DelegateCache == null)
                DelegateCache = createCache(tProperty);
			validate( tProperty, DelegateCache );
            // Target  
            float tempFieldWidth = ( tPosition.width - EditorGUIUtility.labelWidth ) * 0.5f;
			tPosition.height = base.GetPropertyHeight( tProperty, tLabel );
			if ( DelegateCache.CurrentTarget == null ) // empty field
			{   
				EditorGUI.BeginChangeCheck();
				UnityEngine.Object UserParentTarget = EditorGUI.ObjectField( tPosition, tLabel.text, null, typeof( UnityEngine.Object ), true );
                //on target change 
				if ( EditorGUI.EndChangeCheck() )
				{
                    DelegateCache.SetParentTarget(UserParentTarget);
					handleTargetUpdate( tProperty, DelegateCache );
                    DelegateCache.UpdateSelectedMember(DelegateCache.selectedMemberIndex);
                    handleMemberUpdate(tProperty, DelegateCache);
				}
			} 
			else // drop-down
			{
				tPosition.width = tempFieldWidth + EditorGUIUtility.labelWidth;
				EditorGUI.BeginChangeCheck();
				int UserSelectedTarget = EditorGUI.Popup( tPosition, tLabel.text, DelegateCache.CurrentTargetIndex, DelegateCache._targetNames );
				if ( EditorGUI.EndChangeCheck() )
				{
                    DelegateCache.UpdateSelectedTarget(UserSelectedTarget);
                    Debug.Log(DelegateCache._targetNames[UserSelectedTarget]);
                    handleTargetUpdate( tProperty, DelegateCache );
                    DelegateCache.UpdateSelectedMember(DelegateCache.selectedMemberIndex);
                    handleMemberUpdate(tProperty, DelegateCache);
				}
			}
			
			// Members
			if ( DelegateCache.CurrentTarget != null )
			{
				float tempIndentSize = ( EditorGUI.indentLevel - 1 ) * EditorUtility.IndentSize;
				tPosition.x += tPosition.width - 13 - tempIndentSize;
				tPosition.width = tempFieldWidth + 13 + tempIndentSize;
				
				EditorGUI.BeginChangeCheck();
				int tempSelectedMember = EditorGUI.Popup( tPosition, DelegateCache.selectedMemberIndex, DelegateCache.memberNames );
				if ( EditorGUI.EndChangeCheck() )
				{ 
                    DelegateCache.UpdateSelectedMember(tempSelectedMember);
					handleMemberUpdate( tProperty, DelegateCache );
				}
			}
		}
		
		/// <summary>Validates the delegate property against the <paramref name="tCache"/></summary>
		/// <param name="tProperty">Serialized delegate property</param>
		/// <param name="tCache">Cached delegate drop-down data</param>
		protected virtual void validate( SerializedProperty tProperty, T tCache )
		{
			SerializedProperty tempMemberProperty = tProperty.FindPropertyRelative( "_member" );
			if ( !tCache.validateTarget( tProperty.FindPropertyRelative( "_target" ), tempMemberProperty ) )
			{
				handleTargetUpdate( tProperty, tCache );
			}
			if ( !tCache.validateMember( tempMemberProperty ) )
			{
                Debug.Log("member update still");
				handleMemberUpdate( tProperty, tCache );
			}
		} 
		
		/// <summary>Applies the target property of the <see cref="RawDelegate"/></summary>
		/// <param name="tProperty">Serialized delegate property</param>
		/// <param name="tCache">Cached delegate drop-down data</param>
		protected virtual void handleTargetUpdate( SerializedProperty tProperty, T tCache )
		{
			tProperty.FindPropertyRelative( "_target" ).objectReferenceValue = tCache.CurrentTarget;
		//    handleMemberUpdate( tProperty, tCache );
		}
		
		/// <summary>Applies the member property of the <see cref="RawDelegate"/></summary>
		/// <param name="tProperty">Serialized delegate property</param>
		/// <param name="tCache">Cached delegate drop-down data</param>
		protected virtual void handleMemberUpdate( SerializedProperty tProperty, T tCache )
		{
            Debug.Log("UPDATING MEMBER INFO IMPLICITLY");
            tProperty.FindPropertyRelative( "_member" ).stringValue = tCache.SelectedMember.serializedName;
			tProperty.serializedObject.ApplyModifiedProperties();
		}
    }
}