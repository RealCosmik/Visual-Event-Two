using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
namespace EventsPlus
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
        public override float GetPropertyHeight(SerializedProperty tProperty, GUIContent tLabel)
        {
            float tempHeight = base.GetPropertyHeight(tProperty, tLabel);

            // Dynamic toggle height
            SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");
            var path = tProperty.propertyPath;
            var index = tProperty.GetRawCallIndex();
            var pubpath = tProperty.GetPublisherPath();
            RawCallView tempCache = listcache[pubpath][index];
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
                    tempHeight += EditorGUI.GetPropertyHeight(tempArgumentsProperty,false) + EditorGUIUtility.standardVerticalSpacing;
                    if (tempArgumentsProperty.isExpanded)
                    {
                        for (int i = 0; i < tempArgumentsProperty.arraySize; i++)
                        {
                           tempHeight += EditorGUI.GetPropertyHeight(tempArgumentsProperty.GetArrayElementAtIndex(i))+EditorGUIUtility.standardVerticalSpacing;
                        }
                      // tempHeight += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * tempArgumentsProperty.arraySize;
                    } 
                }
            }
            return tempHeight+10;
        }

        protected override RawCallView createCache(SerializedProperty tProperty)
        {
            Publisher tempPublisher = tProperty.GetPublisher();
            var view= new RawCallView(tempPublisher == null ? null : tempPublisher.types);
            view.propertypath = tProperty.propertyPath;
            return view;
        }

        //=======================
        // Render
        //=======================
        public override void OnGUI(Rect tPosition, SerializedProperty tProperty, GUIContent tLabel)
        {
            // Inheritance
            base.OnGUI(tPosition, tProperty, tLabel);
            var index = tProperty.GetRawCallIndex();
            var path = tProperty.GetPublisherPath();
            RawCallView tempCache = listcache[path][index];
            if (tempCache.HasDelegateError)
                HandleDelegeateError(tProperty, tempCache);
           if(tempCache.CurrentTarget!=null)
            EditorGUI.indentLevel += 1;
            if (tempCache.CurrentTarget != null)
            {
                tPosition.height = base.GetPropertyHeight(tProperty, tLabel);
                if (tempCache.isDynamicable)
                { 
                    SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");

                    tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing;
                    tPosition.height = EditorGUI.GetPropertyHeight(tempDynamicProperty);

                    EditorGUI.BeginChangeCheck();
                    bool tempIsDynamic = EditorGUI.Toggle(tPosition, tempDynamicProperty.displayName, tempCache.isDynamic);
                    if (EditorGUI.EndChangeCheck())
                    {
                        tempCache.isDynamic = tempIsDynamic;
                        handleDynamicUpdate(tProperty, tempCache);
                    }
                }
                // Arguments
                if (!tempCache.isDynamic)
                {
                    SerializedProperty tempArgumentsProperty = tProperty.FindPropertyRelative("m_arguments");

                    if (tempArgumentsProperty.arraySize > 0)
                    {
                        tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing;
                        tPosition.height = EditorGUI.GetPropertyHeight(tempArgumentsProperty, false);

                        tempArgumentsProperty.isExpanded = EditorGUI.Foldout(tPosition, tempArgumentsProperty.isExpanded, tempArgumentsProperty.displayName);
                        if (tempArgumentsProperty.isExpanded)
                        {
                           ++EditorGUI.indentLevel;
                            int tempListLength = tempArgumentsProperty.arraySize;
                            for (int i = 0; i < tempListLength; ++i)
                            {
                                DrawArgument(ref tPosition, tempArgumentsProperty.GetArrayElementAtIndex(i), i,tempCache);
                            }
                           --EditorGUI.indentLevel;
                        }
                    }
                 }
                if (tempCache.CurrentTarget != null)
                    EditorGUI.indentLevel -= 1;
            }
        }
        protected override void validate(SerializedProperty tProperty, RawCallView tCache)
        {
            SerializedProperty tempMemberProperty = tProperty.FindPropertyRelative("methodData");
            SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");
            if (!tCache.validateTarget(tProperty.FindPropertyRelative("m_target"), tempMemberProperty, tempDynamicProperty))
            {
                handleTargetUpdate(tProperty, tCache);
            }
            if (!tCache.validateMember(tempMemberProperty, tempDynamicProperty))
            {
                handleMemberUpdate(tProperty, tCache);
            }
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
            EditorGUI.PropertyField(tPosition, tArgument,new GUIContent(tCache.arguments[tIndex].name));
        }

        protected override void handleMemberUpdate(SerializedProperty tProperty, RawCallView tCache)
        {
            base.handleMemberUpdate(tProperty, tCache);
            handleDynamicUpdate(tProperty, tCache);
        }
        private void HandleDelegeateError(SerializedProperty delegeateProp, RawCallView delegatecall)
        { 
            Debug.Log("handle error");
            var methoData_prop = delegeateProp.FindPropertyRelative("methodData");
            var contextobj = delegeateProp.FindPropertyRelative("m_target").serializedObject.targetObject;
            string errormessage = CreateErrorMessage(methoData_prop, contextobj);
            delegeateProp.FindPropertyRelative("m_target").objectReferenceValue = delegatecall.CurrentTarget;
            var argumentArray = delegeateProp.FindPropertyRelative("m_arguments");
            argumentArray.ClearArray();
            argumentArray.arraySize = 3;
            argumentArray.GetArrayElementAtIndex(0).FindPropertyRelative("stringValue").stringValue = errormessage;
            argumentArray.GetArrayElementAtIndex(1).FindPropertyRelative("_x1").floatValue = (int)LogType.Error;
            argumentArray.GetArrayElementAtIndex(2).FindPropertyRelative("objectValue").objectReferenceValue = contextobj;
             
            SetMethodData(methoData_prop, delegatecall.SelectedMember.SeralizedData);
            delegatecall.HasDelegateError = false;
            handleDynamicUpdate(delegeateProp, delegatecall);
        }
        private void SetMethodData(SerializedProperty methodDataProp,string[] methodData)
        {
            methodDataProp.arraySize = methodData.Length;
            for (int i = 0; i < methodData.Length; i++)
            {
                methodDataProp.GetArrayElementAtIndex(i).stringValue = methodData[i];
            }
        }
        string CreateErrorMessage(SerializedProperty methodData_prop, UnityEngine.Object ErrorObject)
        {
            var member_type = (MemberTypes)int.Parse(methodData_prop.GetArrayElementAtIndex(0).stringValue);
            var member_name = methodData_prop.GetArrayElementAtIndex(1).stringValue;
            return $@"{member_type}: ""{member_name}"" was removed or renamed in type: ""{ErrorObject.GetType()}""";
        }

        /// <summary>Applies the dynamic toggle and arguments properties of the <see cref="RawCall"/></summary>
        /// <param name="tProperty">Serialized call property</param>
        /// <param name="tCache">Cached call drop-down data</param>
        protected virtual void handleDynamicUpdate(SerializedProperty tProperty, RawCallView tCache)
        {
            // Properties
            SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");
            SerializedProperty tempArgumentsProperty = tProperty.FindPropertyRelative("m_arguments");

            // Dynamic, erase arguments
            if (tCache.isDynamic)
            {
                tempDynamicProperty.boolValue = true;
                tempArgumentsProperty.ClearArray();
            }
            // Not dynamic, generate arguments if not void
            else
            {
                tempDynamicProperty.boolValue = false;
                Argument[] tempArguments = tCache.arguments;
                if (tempArguments == null)
                {
                }
                else
                {
                    if (tCache.CurrentTarget != null)
                        Debug.Log(tempArguments.Length);
                    tempArgumentsProperty.arraySize = tempArguments.Length;
                    for (int i = (tempArguments.Length - 1); i >= 0; --i)
                    {
                        var assemblytypename = tempArguments[i].type.AssemblyQualifiedName;
                        var fulltypename = tempArguments[i].type.FullName;
                        tempArgumentsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("assemblyQualifiedArgumentName").stringValue = assemblytypename;
                        tempArgumentsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("FullArgumentName").stringValue = fulltypename;
                    }
                }
            }
            tProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}