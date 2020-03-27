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
        static GUIContent Dynamic_Content = new GUIContent("Is Dyanmic");
        static GUIContent Yield_Content = new GUIContent("Yield");

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
                if (tempCache.isDynamicable||tempCache.isYieldable)
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
            var isruntime = tProperty.FindPropertyRelative("m_runtime").boolValue;
            if (isruntime)
            {
                DrawRunTimeCall(tPosition, tProperty);
                return;
            }
            // Inheritance
            base.OnGUI(tPosition, tProperty, tLabel);

            if (ViewCache.GetDelegateView(tProperty, out RawCallView delegateCache))
            {
                SetInitialHeight(delegateCache, tProperty);
                if (delegateCache.CurrentTarget != null)
                {
                    tPosition.height = base.GetPropertyHeight(tProperty, tLabel);

                    if (delegateCache.isDynamicable)
                    {
                        SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");
                        VisualEdiotrUtility.StandardStyle.CalcMinMaxWidth(Dynamic_Content, out float min, out float max);
                        tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing + 5;
                        tPosition.height = EditorGUI.GetPropertyHeight(tempDynamicProperty);
                        var dynamic_rect = tPosition; 
                        dynamic_rect.width = max*1.5f;
                        EditorGUI.BeginChangeCheck();
                        bool tempIsDynamic = EditorGUI.ToggleLeft(dynamic_rect, Dynamic_Content, delegateCache.isDynamic);
                        if (EditorGUI.EndChangeCheck())
                        {
                            delegateCache.isDynamic = tempIsDynamic;
                            handleDynamicUpdate(tProperty, delegateCache);
                            delegateCache.RequiresRecalculation = true;
                        }
                    }  
                    if (delegateCache.isYieldable)
                    {
                        SerializedProperty isYieldCall_prop = tProperty.FindPropertyRelative("m_isYieldableCall");
                        VisualEdiotrUtility.StandardStyle.CalcMinMaxWidth(Yield_Content, out float min, out float max);
                        Rect yield_rect;
                        if (delegateCache.isDynamicable)
                        {
                            yield_rect = tPosition;
                            yield_rect.x += 100;
                        }
                        else
                        {
                            tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing + 5;
                            tPosition.height = EditorGUI.GetPropertyHeight(isYieldCall_prop);
                            yield_rect = tPosition;
                        }
                        yield_rect.width = max * 3;
                        EditorGUI.BeginChangeCheck();
                        isYieldCall_prop.boolValue = EditorGUI.ToggleLeft(yield_rect, Yield_Content, isYieldCall_prop.boolValue);
                        if (EditorGUI.EndChangeCheck())
                        {
                            tProperty.serializedObject.ApplyModifiedProperties();
                            if (EditorApplication.isPlaying)
                                tProperty.GetVisualDelegateObject()?.ReInitialize();
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
            }
        }
        private void DrawRunTimeCall(Rect position, SerializedProperty property)
        {
            if (EditorApplication.isPlaying && ViewCache.GetDelegateView(property, out RawDynamicDelegateView cache))
            {
                var methodData_prop = property.FindPropertyRelative("methodData");
                if (methodData_prop.arraySize == 0)
                {
                    var dynamicdelgate = property.GetTarget<RawCall>();
                    AddTarget(property, cache, dynamicdelgate.delegateInstance.Target);
                    var seralizedMethodData = Utility.QuickSeralizer(dynamicdelgate.delegateInstance.Method);
                    VisualEdiotrUtility.CopySeralizedMethodDataToProp(methodData_prop, seralizedMethodData);
                    cache.MethodName = ParseMethodName(seralizedMethodData[1]);
                    cache.CalcHeight();
                    property.serializedObject.ApplyModifiedProperties();
                }
                Rect targetRect = position;
                targetRect.height *= .5f;
                Rect methodRect = targetRect;
                methodRect.y += methodRect.height;
                EditorGUI.LabelField(targetRect, cache.TargetContent);
                EditorGUI.LabelField(methodRect, cache.MethodContent);
            }
        }
        private void AddTarget(SerializedProperty delegateprop, RawDynamicDelegateView cache, object target)
        {
            var target_type = target.GetType();
            if (target is UnityEngine.Object unity_object)
                delegateprop.FindPropertyRelative("m_target").objectReferenceValue = unity_object;
            delegateprop.FindPropertyRelative("TargetType").stringValue = target_type.AssemblyQualifiedName;
            cache.TargetName = ParseTargetName(target_type.FullName);
        }
        private string ParseMethodName(string methodname)
        {
            if (methodname[0] == '<')//anon method
            {
                return $@"Method: Anonymous method created in ""{methodname.Substring(1, methodname.IndexOf('>') - 1)}""";
            }
            else return $@"Method: ""{methodname}""";
        }
        private string ParseTargetName(string TargetType)
        {
            if (TargetType.Contains("+"))
            {
                return $@"Target: Anonymous Type Created in ""{TargetType.Substring(0, TargetType.IndexOf('+'))}""";
            }
            return $@"Target: ""{TargetType}""";
        }
        protected override void validate(SerializedProperty tProperty, RawCallView delegatecache)
        {
            if (!delegatecache.isvalidated)
            {
                SerializedProperty tempMemberProperty = tProperty.FindPropertyRelative("methodData");
                SerializedProperty isStatic_prop = tProperty.FindPropertyRelative("isStatic");
                SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");
                if (!delegatecache.validateTarget(tProperty.FindPropertyRelative("m_target"), isStatic_prop, tempDynamicProperty))
                {
                    handleTargetUpdate(tProperty, delegatecache);
                    isStatic_prop.boolValue = delegatecache.hasStaticTarget;
                    delegatecache.RequiresRecalculation = true;
                }
                if (!delegatecache.validateMember(tempMemberProperty, tempDynamicProperty))
                {
                    handleMemberUpdate(tProperty, delegatecache);
                }

                delegatecache.isvalidated = true;
            }
            else if (delegatecache.CurrentTarget == null && delegatecache.memberNames != null)
                delegatecache.isvalidated = false;

            if (delegatecache.CurrentTarget != null)
            {
                var methodName = tProperty.FindPropertyRelative("methodData").GetArrayElementAtIndex(1).stringValue;
                if (delegatecache.HasDelegateError || !methodName.Equals(delegatecache.SelectedMember.SeralizedData[1]))
                {
                    HandleDelegeateError(tProperty, delegatecache);
                }
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

        protected override void handleMemberUpdate(SerializedProperty tProperty, RawCallView tCache)
        {
            base.handleMemberUpdate(tProperty, tCache);
            if (!tCache.isYieldable)
                tProperty.FindPropertyRelative("m_isYieldableCall").boolValue = false;
            handleDynamicUpdate(tProperty, tCache as RawCallView);
        }
        private void HandleDelegeateError(SerializedProperty delegeateProp, RawCallView delegatecache)
        {
            Debug.Log("handle error");
            var methoData_prop = delegeateProp.FindPropertyRelative("methodData");
            var contextobj = delegeateProp.FindPropertyRelative("m_target").serializedObject.targetObject;
            string errormessage = VisualEdiotrUtility.CreateErrorMessage(methoData_prop, contextobj);
            delegeateProp.FindPropertyRelative("m_target").objectReferenceValue = delegatecache.CurrentTarget;
            var argumentArray = delegeateProp.FindPropertyRelative("m_arguments");
            argumentArray.ClearArray();
            argumentArray.arraySize = 3;
            argumentArray.GetArrayElementAtIndex(0).FindPropertyRelative("stringValue").stringValue = errormessage;
            argumentArray.GetArrayElementAtIndex(1).FindPropertyRelative("_x1").floatValue = (int)LogType.Error;
            argumentArray.GetArrayElementAtIndex(2).FindPropertyRelative("objectValue").objectReferenceValue = contextobj;

            VisualEdiotrUtility.CopySeralizedMethodDataToProp(methoData_prop, delegatecache.SelectedMember.SeralizedData);
            handleDynamicUpdate(delegeateProp, delegatecache);
            if (EditorApplication.isPlaying)
                delegeateProp.GetVisualDelegateObject()?.ReInitialize();
            delegatecache.HasDelegateError = false;
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
                tempArgumentsProperty.ClearArray();
            }
            // Not dynamic, generate arguments if not void
            else
            {
                tempDynamicProperty.boolValue = false;
                Argument[] tempArguments = delegateCache.arguments;
                if (tempArguments == null)
                {
                    tempArgumentsProperty.ClearArray();
                }
                else
                {
                    tempArgumentsProperty.arraySize = tempArguments.Length;
                    delegateCache.RequiresRecalculation = true;
                    for (int i = (tempArguments.Length - 1); i >= 0; --i)
                    {
                        var assemblytypename = tempArguments[i].type.AssemblyQualifiedName;
                        var fulltypename = tempArguments[i].type.FullName;
                        var currentargumentproperty = tempArgumentsProperty.GetArrayElementAtIndex(i);
                        currentargumentproperty.FindPropertyRelative("assemblyQualifiedArgumentName").stringValue = assemblytypename;
                        currentargumentproperty.FindPropertyRelative("FullArgumentName").stringValue = fulltypename;
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