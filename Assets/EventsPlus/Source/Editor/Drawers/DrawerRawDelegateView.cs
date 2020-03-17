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
    public abstract class DrawerRawDelegateView<ViewType> : PropertyDrawer where ViewType : RawDelegateView
    {
        //=======================
        // Initialization
        /// <summary>Initializes the drawer and calculates the inspector height</summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        /// <returns>Height of the drawer</returns>
        public override float GetPropertyHeight(SerializedProperty tProperty, GUIContent tLabel)
        {
            return base.GetPropertyHeight(tProperty, tLabel);
        }


        //=======================
        // Render
        //=======================
        /// <summary>Renders the individual delegate property</summary>
        /// <param name="tPosition">Inspector position and size of <paramref name="tProperty"/></param>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        public override void OnGUI(Rect tPosition, SerializedProperty tProperty, GUIContent tLabel)
        {
            tLabel.text = null;

            if (ViewCache.GetDelegateView(tProperty, out ViewType DelegateCache))
            {
                validate(tProperty, DelegateCache);
                // Target  
                float tempFieldWidth = (tPosition.width - EditorGUIUtility.labelWidth) * 0.5f;
                tPosition.height = base.GetPropertyHeight(tProperty, tLabel);
                tPosition.x -= 120;
                tPosition.y += 5;
                tPosition.width += 120;
                if (DelegateCache.CurrentTarget == null) // empty field
                {
                    DelegateCache.Height = tPosition.height + 10;
                    EditorGUI.BeginChangeCheck();
                    UnityEngine.Object UserParentTarget = EditorGUI.ObjectField(tPosition, tLabel.text, null, typeof(UnityEngine.Object), true);
                    //on target change 
                    if (EditorGUI.EndChangeCheck())
                    {
                        DelegateCache.SetParentTarget(UserParentTarget);
                        handleTargetUpdate(tProperty, DelegateCache);
                        DelegateCache.UpdateSelectedMember(DelegateCache.selectedMemberIndex);
                        handleMemberUpdate(tProperty, DelegateCache);
                        EditorGUIUtility.PingObject(UserParentTarget);
                    }
                }
                else // Target drop-down
                {
                    tPosition.width = tempFieldWidth + EditorGUIUtility.labelWidth + 30;
                    EditorGUI.BeginChangeCheck();
                    int UserSelectedTarget = EditorGUI.Popup(tPosition, tLabel.text, DelegateCache.CurrentTargetIndex, DelegateCache._targetNames);
                    // on memberchange 
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorGUIUtility.PingObject(DelegateCache.GetObjectFromTree(UserSelectedTarget));
                        DelegateCache.UpdateSelectedTarget(UserSelectedTarget);
                        handleTargetUpdate(tProperty, DelegateCache);
                        DelegateCache.UpdateSelectedMember(DelegateCache.selectedMemberIndex);
                        handleMemberUpdate(tProperty, DelegateCache);
                    }
                }

                // Members
                if (DelegateCache.CurrentTarget != null)
                {
                    float tempIndentSize = (EditorGUI.indentLevel - 1) * VisualEdiotrUtility.IndentSize;
                    tPosition.x += tPosition.width - 13 - tempIndentSize + 10;
                    tPosition.width = tempFieldWidth + 13 + tempIndentSize + 70;
                    EditorGUI.BeginChangeCheck();
                    int tempSelectedMember = EditorGUI.Popup(tPosition, DelegateCache.selectedMemberIndex, DelegateCache.memberNames);
                    if (EditorGUI.EndChangeCheck())
                    {
                        DelegateCache.UpdateSelectedMember(tempSelectedMember);
                        handleMemberUpdate(tProperty, DelegateCache);
                    }
                }
            }
        }


        /// <summary>Validates the delegate property against the <paramref name="tCache"/></summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tCache">Cached delegate drop-down data</param>
        protected virtual void validate(SerializedProperty tProperty, RawDelegateView tCache)
        {
         
            //this method is only used because the cache gets deleted on editor recompiles so we have to reconstruct the cache
            //using the data from the seralized property to make recompilation seem seemless on the front end

            if (!tCache.isvalidated) //validation only needs to occur once in a caches lifecycle
            {
                SerializedProperty tempMemberProperty = tProperty.FindPropertyRelative("methodData");
                if (!tCache.validateTarget(tProperty.FindPropertyRelative("m_target"), tempMemberProperty))
                {
                    handleTargetUpdate(tProperty, tCache);
                }
                if (!tCache.validateMember(tempMemberProperty))
                {
                    handleMemberUpdate(tProperty, tCache);
                }
                tCache.isvalidated = true;
            }
            tCache.ValidateComponentTree();
        }

        /// <summary>Applies the target property of the <see cref="RawDelegate"/></summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tCache">Cached delegate drop-down data</param>
        protected virtual void handleTargetUpdate(SerializedProperty tProperty, RawDelegateView tCache)
        {
            var targetobject = tProperty.serializedObject.targetObject;
            Undo.RegisterCompleteObjectUndo(targetobject, "Delegate Target Change");
            tProperty.FindPropertyRelative("m_target").objectReferenceValue = tCache.CurrentTarget;
            if (tProperty.serializedObject.hasModifiedProperties)
            {
               
                if (PrefabUtility.IsPartOfAnyPrefab(targetobject))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(targetobject);
                tProperty.serializedObject.ApplyModifiedProperties();

            }

            //    handleMemberUpdate( tProperty, tCache );
        }

        /// <summary>Applies the member property of the <see cref="RawDelegate"/></summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tCache">Cached delegate drop-down data</param>
        protected virtual void handleMemberUpdate(SerializedProperty tProperty, RawDelegateView tCache)
        {
            var targetobject = tProperty.serializedObject.targetObject;
            Undo.RegisterCompleteObjectUndo(targetobject, "DelegateMemberChange");
            var methodData_prop = tProperty.FindPropertyRelative("methodData");
            if (tCache.SelectedMember == null)
                methodData_prop.arraySize = 0;
            else
                VisualEdiotrUtility.CopySeralizedMethodDataToProp(methodData_prop, tCache.SelectedMember.SeralizedData);

            if (tProperty.serializedObject.hasModifiedProperties)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(targetobject))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(targetobject);
                tProperty.serializedObject.ApplyModifiedProperties();
            }
        }


    }
}