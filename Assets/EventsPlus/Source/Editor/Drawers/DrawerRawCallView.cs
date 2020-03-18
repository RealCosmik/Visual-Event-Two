using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
namespace VisualEvent
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Inspector class for rendering <see cref="RawCall"/>s in the inspector</summary>
    [CustomPropertyDrawer(typeof(RawCall), true)]
    public class DrawerRawCallView : DrawerRawDelegateView<RawCallView>
    {
        //=======================
        // Initialization
        //=======================
        private void calculateTotalHeight(SerializedProperty tProperty)
        {
            float tempHeight = 0f;
            SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");
            RawCallView tempCache = ViewCache.GetRawCallCache(tProperty) as RawCallView;
            if (tempCache.CurrentTarget != null)
            {
                if (tempCache.isDynamicable)
                {
                    tempHeight += EditorGUI.GetPropertyHeight(tempDynamicProperty) + EditorGUIUtility.standardVerticalSpacing;
                }
                // Arguments height
                SerializedProperty tempArgumentsProperty = tProperty.FindPropertyRelative("m_arguments");
                if (tempArgumentsProperty.arraySize > 0 && !tempDynamicProperty.boolValue)
                {
                    tempHeight += EditorGUI.GetPropertyHeight(tempArgumentsProperty, false) + EditorGUIUtility.standardVerticalSpacing;
                    if (tempArgumentsProperty.isExpanded)
                    {
                        for (int i = 0; i < tempArgumentsProperty.arraySize; i++)
                        {
                            tempHeight += EditorGUI.GetPropertyHeight(tempArgumentsProperty.GetArrayElementAtIndex(i)) + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                }
            }
            tempCache.Height = tempHeight + base.GetPropertyHeight(tProperty, null) + 10;
        }
        private void SetInitialHeight(RawCallView cache, SerializedProperty rawcallprop)
        {
            if (cache.RequiresRecalculation)
            {
                calculateTotalHeight(rawcallprop);
                cache.RequiresRecalculation = false;
            }
            if (cache.CurrentTarget != null)
                EditorGUI.indentLevel += 1;
        }
        //=======================
        // Render
        //=======================
        public override void OnGUI(Rect tPosition, SerializedProperty tProperty, GUIContent tLabel)
        {
            // Inheritance
            EditorGUI.BeginChangeCheck();
            base.OnGUI(tPosition, tProperty, tLabel);

            if (ViewCache.GetDelegateView(tProperty, out RawCallView delegateCache))
            {
                EditorGUI.BeginProperty(tPosition, tLabel, tProperty);
                SetInitialHeight(delegateCache, tProperty);
                if (delegateCache.HasDelegateError)
                    HandleDelegeateError(tProperty, delegateCache);
                if (delegateCache.CurrentTarget != null)
                {
                    tPosition.height = base.GetPropertyHeight(tProperty, tLabel);
                    if (delegateCache.isDynamicable)
                    {
                        SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");

                        tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing + 5;
                        tPosition.height = EditorGUI.GetPropertyHeight(tempDynamicProperty);

                        EditorGUI.BeginChangeCheck();
                        bool tempIsDynamic = EditorGUI.ToggleLeft(tPosition, tempDynamicProperty.displayName, delegateCache.isDynamic);
                        if (EditorGUI.EndChangeCheck())
                        {
                            delegateCache.isDynamic = tempIsDynamic;
                            handleDynamicUpdate(tProperty, delegateCache);
                            delegateCache.RequiresRecalculation = true;
                        }
                    }
                    // Arguments
                    if (!delegateCache.isDynamic)
                    {

                        SerializedProperty tempArgumentsProperty = tProperty.FindPropertyRelative("m_arguments");

                        if (tempArgumentsProperty.arraySize > 0)
                        {
                            if (tempArgumentsProperty.isExpanded != delegateCache.isexpanded)
                            {
                                delegateCache.RequiresRecalculation = true;
                                delegateCache.isexpanded = tempArgumentsProperty.isExpanded;
                            }
                            tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing + 5;
                            tPosition.height = EditorGUI.GetPropertyHeight(tempArgumentsProperty, false);

                            tempArgumentsProperty.isExpanded = EditorGUI.Foldout(tPosition, tempArgumentsProperty.isExpanded, tempArgumentsProperty.displayName);
                            if (tempArgumentsProperty.isExpanded)
                            {
                                ++EditorGUI.indentLevel;
                                int tempListLength = tempArgumentsProperty.arraySize;
                                for (int i = 0; i < tempListLength; ++i)
                                {
                                    DrawArgument(ref tPosition, tempArgumentsProperty.GetArrayElementAtIndex(i), i, delegateCache);
                                }
                                --EditorGUI.indentLevel;
                            }
                        }
                    }
                    if (delegateCache.CurrentTarget != null)
                        EditorGUI.indentLevel -= 1;
                }
                EditorGUI.EndProperty();
                if (EditorGUI.EndChangeCheck())
                {
                    Debug.Log("change here");
                }
            }
        }
        protected override void validate(SerializedProperty tProperty, RawCallView delegatecache)
        {
            if (!delegatecache.isvalidated)
            {
                SerializedProperty tempMemberProperty = tProperty.FindPropertyRelative("methodData");
                SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");
                if (!delegatecache.validateTarget(tProperty.FindPropertyRelative("m_target"), tempMemberProperty, tempDynamicProperty))
                {
                    handleTargetUpdate(tProperty, delegatecache);
                    delegatecache.RequiresRecalculation = true;
                }
                if (!delegatecache.validateMember(tempMemberProperty, tempDynamicProperty))
                {
                    handleMemberUpdate(tProperty, delegatecache);
                }
                delegatecache.isvalidated = true;
            } 
            else if (delegatecache.CurrentTarget == null)
            {
                delegatecache.isvalidated = false;
            }
            delegatecache.ValidateComponentTree();
        }

        /// <summary>Utility function for rendering a <see cref="RawArgument"/> property</summary>
        /// <param name="tPosition">Inspector position and size of <paramref name="tArgument"/></param>
        /// <param name="tArgument">Serialized argument property</param>
        /// <param name="tIndex">Index of the <paramref name="tArgument"/></param>
        /// <param name="tCache">Cached call drop-down data</param>
        protected static void DrawArgument(ref Rect tPosition, SerializedProperty tArgument, int tIndex, RawCallView tCache)
        {
            tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing;
            tPosition.height = EditorGUI.GetPropertyHeight(tArgument);
            EditorGUI.PropertyField(tPosition, tArgument, new GUIContent(tCache.arguments[tIndex].name));
        }

        protected override void handleMemberUpdate(SerializedProperty tProperty, RawDelegateView tCache)
        {
            base.handleMemberUpdate(tProperty, tCache);
            handleDynamicUpdate(tProperty, tCache as RawCallView);
        }
        private void HandleDelegeateError(SerializedProperty delegeateProp, RawCallView delegatecall)
        { 
            Debug.Log("handle error");
            var methoData_prop = delegeateProp.FindPropertyRelative("methodData");
            var contextobj = delegeateProp.FindPropertyRelative("m_target").serializedObject.targetObject;
            string errormessage = VisualEdiotrUtility.CreateErrorMessage(methoData_prop, contextobj);
            delegeateProp.FindPropertyRelative("m_target").objectReferenceValue = delegatecall.CurrentTarget;
            var argumentArray = delegeateProp.FindPropertyRelative("m_arguments");
            argumentArray.ClearArray();
            argumentArray.arraySize = 3;
            argumentArray.GetArrayElementAtIndex(0).FindPropertyRelative("stringValue").stringValue = errormessage;
            argumentArray.GetArrayElementAtIndex(1).FindPropertyRelative("_x1").floatValue = (int)LogType.Error;
            argumentArray.GetArrayElementAtIndex(2).FindPropertyRelative("objectValue").objectReferenceValue = contextobj;

            VisualEdiotrUtility.CopySeralizedMethodDataToProp(methoData_prop, delegatecall.SelectedMember.SeralizedData);
            delegatecall.HasDelegateError = false;
            handleDynamicUpdate(delegeateProp, delegatecall);
        }

        /// <summary>Applies the dynamic toggle and arguments properties of the <see cref="RawCall"/></summary>
        /// <param name="tProperty">Serialized call property</param>
        /// <param name="delegateCache">Cached call drop-down data</param>
        protected virtual void handleDynamicUpdate(SerializedProperty tProperty, RawCallView delegateCache)
        {
            // Properties
            Undo.RegisterCompleteObjectUndo(tProperty.serializedObject.targetObject, "DynamicStateChange");
            SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");
            SerializedProperty tempArgumentsProperty = tProperty.FindPropertyRelative("m_arguments");

            // Dynamic, erase arguments
            if (delegateCache.isDynamic)
            {
                tempDynamicProperty.boolValue = true;
                // tempArgumentsProperty.ClearArray();
            }
            // Not dynamic, generate arguments if not void
            else
            {
                tempDynamicProperty.boolValue = false;
                Argument[] tempArguments = delegateCache.arguments;
                if (tempArguments == null)
                {
                }
                else
                {
                    if (delegateCache.CurrentTarget != null)
                    {
                        tempArgumentsProperty.arraySize = tempArguments.Length;
                        delegateCache.RequiresRecalculation = true;
                    }
                    for (int i = (tempArguments.Length - 1); i >= 0; --i)
                    {
                        var assemblytypename = tempArguments[i].type.AssemblyQualifiedName;
                        var fulltypename = tempArguments[i].type.FullName;
                        var currentargumentproperty = tempArgumentsProperty.GetArrayElementAtIndex(i);
                        currentargumentproperty.FindPropertyRelative("assemblyQualifiedArgumentName").stringValue = assemblytypename;
                        currentargumentproperty.FindPropertyRelative("FullArgumentName").stringValue = fulltypename;
                        if (currentargumentproperty.FindPropertyRelative("UseReference").boolValue)
                        {
                        }
                    }
                }
            }
                tProperty.serializedObject.ApplyModifiedProperties();
        }
        /// <summary>
        /// TODO: needs implementation
        /// </summary>
        /// <param name="tprop"></param>
        private void HandleDrag(SerializedProperty tprop)
        {
            //TODO: implment this later
        }
    }
}