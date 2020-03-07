﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace EventsPlus
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Inspector class for rendering <see cref="RawDelegate"/>s in the inspector</summary>
	public abstract class DrawerRawDelegateView<delegateType> : PropertyDrawer
	{
       
		//=======================
		// Initialization
		/// <summary>Initializes the drawer and calculates the inspector height</summary>
		/// <param name="tProperty">Serialized delegate property</param>
		/// <param name="tLabel">GUI Label of the drawer</param>
		/// <returns>Height of the drawer</returns>
		public override float GetPropertyHeight( SerializedProperty tProperty, GUIContent tLabel )
		{
            var publisherpath = tProperty.GetPublisherPath();
            var index = tProperty.GetRawCallIndex();
            if (!ViewCache.Cache.TryGetValue(publisherpath,out List<RawDelegateView> cachelist))
                ViewCache.Cache.Add(publisherpath, cachelist = new List<RawDelegateView>());
            if (index >= cachelist.Count)
                    cachelist.Add(createCache(tProperty));
            return base.GetPropertyHeight( tProperty, tLabel );
        }


        /// <summary>Instantiates the delegate drop-down <see cref="cache"/></summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <returns>Delegate cache</returns>
        protected abstract RawDelegateView createCache(SerializedProperty tProperty);
		//=======================
		// Render
		//=======================
		/// <summary>Renders the individual delegate property</summary>
		/// <param name="tPosition">Inspector position and size of <paramref name="tProperty"/></param>
		/// <param name="tProperty">Serialized delegate property</param>
		/// <param name="tLabel">GUI Label of the drawer</param>
		public override void OnGUI( Rect tPosition, SerializedProperty tProperty, GUIContent tLabel )
		{
            tLabel.text = null;

            var index = tProperty.GetRawCallIndex();
            var pubpath = tProperty.GetPublisherPath();
            RawDelegateView DelegateCache = ViewCache.Cache[pubpath][index];
          //  if (DelegateCache.propertypath != tProperty.propertyPath)
            //    DelegateCache.ClearViewCache(); 
            //else Debug.Log("thats okay");
			validate( tProperty, DelegateCache );
            // Target  
            float tempFieldWidth = ( tPosition.width - EditorGUIUtility.labelWidth ) * 0.5f;
			tPosition.height = base.GetPropertyHeight( tProperty, tLabel );
            tPosition.x -= 120;
            tPosition.y += 5;
            tPosition.width += 120;
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
                    EditorGUIUtility.PingObject(UserParentTarget);
                }
            }
            else // drop-down
			{
				tPosition.width = tempFieldWidth + EditorGUIUtility.labelWidth+30;
                EditorGUI.BeginChangeCheck();
                int previousIndex = DelegateCache.CurrentTargetIndex;
				int UserSelectedTarget = EditorGUI.Popup( tPosition, tLabel.text, DelegateCache.CurrentTargetIndex, DelegateCache._targetNames);
                // on memberchange 
                if (EditorGUI.EndChangeCheck() && previousIndex != UserSelectedTarget)
                {
                    EditorGUIUtility.PingObject(DelegateCache.GetObjectFromTree(UserSelectedTarget));
                    DelegateCache.UpdateSelectedTarget(UserSelectedTarget);
                    //Debug.Log(DelegateCache._targetNames[UserSelectedTarget]);
                    handleTargetUpdate(tProperty, DelegateCache);
                    DelegateCache.UpdateSelectedMember(DelegateCache.selectedMemberIndex);
                    handleMemberUpdate(tProperty, DelegateCache);
                }
            }
			
			// Members
			if ( DelegateCache.CurrentTarget != null )
			{
				float tempIndentSize = ( EditorGUI.indentLevel - 1 ) * EditorUtility.IndentSize;
				tPosition.x += tPosition.width - 13 - tempIndentSize+10;
				tPosition.width = tempFieldWidth + 13 + tempIndentSize+70;
				EditorGUI.BeginChangeCheck();
				int tempSelectedMember = EditorGUI.Popup( tPosition, DelegateCache.selectedMemberIndex, DelegateCache.memberNames);
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
		protected virtual void validate( SerializedProperty tProperty, RawDelegateView tCache )
		{
			SerializedProperty tempMemberProperty = tProperty.FindPropertyRelative( "methodData" );
			if ( !tCache.validateTarget( tProperty.FindPropertyRelative( "m_target" ), tempMemberProperty) )
			{
				handleTargetUpdate( tProperty, tCache );
			}
			 if ( !tCache.validateMember(tempMemberProperty) )
			{
                Debug.Log("member update still");
				handleMemberUpdate( tProperty, tCache );
			}
		} 
		
		/// <summary>Applies the target property of the <see cref="RawDelegate"/></summary>
		/// <param name="tProperty">Serialized delegate property</param>
		/// <param name="tCache">Cached delegate drop-down data</param>
		protected virtual void handleTargetUpdate( SerializedProperty tProperty, RawDelegateView tCache )
		{
			tProperty.FindPropertyRelative( "m_target" ).objectReferenceValue = tCache.CurrentTarget;
            tProperty.serializedObject.ApplyModifiedProperties();
            //    handleMemberUpdate( tProperty, tCache );
        }

        /// <summary>Applies the member property of the <see cref="RawDelegate"/></summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tCache">Cached delegate drop-down data</param>
        protected virtual void handleMemberUpdate( SerializedProperty tProperty, RawDelegateView tCache )
		{
            var methodData_prop = tProperty.FindPropertyRelative("methodData");
            if (tCache.SelectedMember == null)
                methodData_prop.arraySize = 0;
            else
            {
                methodData_prop.arraySize = tCache.SelectedMember.SeralizedData.Length;
                for (int i = 0; i < tCache.SelectedMember.SeralizedData.Length; i++)
                {
                    methodData_prop.GetArrayElementAtIndex(i).stringValue = tCache.SelectedMember.SeralizedData[i];
                }
            }
			tProperty.serializedObject.ApplyModifiedProperties();
		}
        
    
    }
}