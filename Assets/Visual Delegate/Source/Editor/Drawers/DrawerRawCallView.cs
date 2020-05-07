using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
namespace VisualEvent.Editor
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
            if (!tempCache.serializationError)
            {
                if (tempCache.CurrentTarget != null)
                {
                    if (tempCache.isDynamicable || tempCache.isYieldable)
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
            else tempCache.Height = 30f;
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
            // Debug.Log(tProperty.FindPropertyRelative("m_isYieldableCall").boolValue);
            // Inheritance

            if (ViewCache.GetDelegateView(tProperty, out RawCallView delegateCache))
            {
                SetInitialHeight(delegateCache, tProperty);
                if (HasSeralizationError(tProperty, delegateCache))
                {
                    GUIStyle style = new GUIStyle();
                    style.fontStyle = FontStyle.Bold;
                    style.fontSize = 12;
                    string message = HandleDelegateErrormessage(tProperty, delegateCache);
                    GUI.Label(tPosition, message, style);
                    return;
                }
                base.OnGUI(tPosition, tProperty, tLabel);
                if (delegateCache.CurrentTarget != null)
                {
                    tPosition.height = delegateCache.Height;
                    if (delegateCache.isDynamicable)
                    {
                        SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");
                        VisualEditorUtility.StandardStyle.CalcMinMaxWidth(Dynamic_Content, out float min, out float max);
                        tPosition.height = EditorGUI.GetPropertyHeight(tempDynamicProperty);
                        tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing;
                        var dynamic_rect = tPosition;
                        dynamic_rect.width = max * 1.5f;
                        EditorGUI.BeginChangeCheck();
                        bool tempIsDynamic = EditorGUI.ToggleLeft(dynamic_rect, Dynamic_Content, delegateCache.isDynamic);
                        if (EditorGUI.EndChangeCheck())
                        {
                            delegateCache.isDynamic = tempIsDynamic;
                            UpdateMethodName(tProperty, delegateCache);
                            handleDynamicUpdate(tProperty, delegateCache);
                            delegateCache.RequiresRecalculation = true;
                        }
                    }
                    if (delegateCache.isYieldable)
                    {
                        SerializedProperty isYieldCall_prop = tProperty.FindPropertyRelative("m_isYieldableCall");
                        VisualEditorUtility.StandardStyle.CalcMinMaxWidth(Yield_Content, out float min, out float max);
                        Rect yield_rect;
                        if (delegateCache.isDynamicable)
                        {
                            yield_rect = tPosition;
                            yield_rect.x += 100;
                        }
                        else
                        {
                            tPosition.height = EditorGUI.GetPropertyHeight(isYieldCall_prop);
                            tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing + 5;
                            yield_rect = tPosition;
                        }
                        yield_rect.width = max * 3;
                        EditorGUI.BeginChangeCheck();
                        isYieldCall_prop.boolValue = EditorGUI.ToggleLeft(yield_rect, Yield_Content, isYieldCall_prop.boolValue);
                        if (EditorGUI.EndChangeCheck())
                        {
                            tProperty.serializedObject.ApplyModifiedProperties();
                            if (EditorApplication.isPlaying)
                                //tProperty.GetVisualDelegateObject()?.ReInitialize();
                                Debug.LogWarning("might have to change this");
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
                            // tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing + 5;
                            tPosition.height = EditorGUI.GetPropertyHeight(tempArgumentsProperty, false);
                            tPosition.y += 25f;
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

        protected override void validate(SerializedProperty tProperty, RawCallView delegatecache)
        {
            if (!delegatecache.isvalidated)
            {
                SerializedProperty tempMemberProperty = tProperty.FindPropertyRelative("methodData");
                SerializedProperty tempDynamicProperty = tProperty.FindPropertyRelative("m_isDynamic");
                if (!delegatecache.validateTarget(tProperty.FindPropertyRelative("m_target"), tempDynamicProperty))
                {
                    //  handleTargetUpdate(tProperty, delegatecache);
                    delegatecache.RequiresRecalculation = true;
                }
                if (!delegatecache.validateMember(tempMemberProperty, tempDynamicProperty))
                {
                    // handleMemberUpdate(tProperty, delegatecache);
                }
                tProperty.FindPropertyRelative("serializationError").boolValue = delegatecache.serializationError;
                if (!delegatecache.serializationError)
                { 
                    UpdateMethodName(tProperty, delegatecache);
                }
                delegatecache.isvalidated = true;
            }
            else if (delegatecache.CurrentTarget == null && delegatecache.memberNames != null)
                delegatecache.isvalidated = false;
            delegatecache.ValidateComponentTree();
        }
        bool HasSeralizationError(SerializedProperty tProperty, RawCallView delegatecache)
        {
            return delegatecache.serializationError;
        }

        /// <summary>Utility function for rendering a <see cref="RawArgument"/> property</summary>
        /// <param name="tPosition">Inspector position and size of <paramref name="tArgument"/></param>
        /// <param name="tArgument">Serialized argument property</param>
        /// <param name="tIndex">Index of the <paramref name="tArgument"/></param>
        /// <param name="tCache">Cached call drop-down data</param>
        protected static void DrawArgument(ref Rect tPosition, SerializedProperty tArgument, int tIndex, RawCallView tCache)
        {
            var argument_height = EditorGUI.GetPropertyHeight(tArgument);
            tPosition.y += argument_height + EditorGUIUtility.standardVerticalSpacing;
            var pos = tPosition;
            // pos.y += 10f;
            //tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing;
            // pos.height = argument_height;
            EditorGUI.PropertyField(pos, tArgument, new GUIContent(tCache.arguments[tIndex].name));
        }

        protected override void handleMemberUpdate(SerializedProperty tProperty, RawCallView tCache)
        {
            UpdateMethodName(tProperty, tCache);
            if (!tCache.isYieldable)
                tProperty.FindPropertyRelative("m_isYieldableCall").boolValue = false;
            base.handleMemberUpdate(tProperty, tCache);
            handleDynamicUpdate(tProperty, tCache as RawCallView);
        }
        private string HandleDelegateErrormessage(SerializedProperty delegeateProp, RawCallView delegatecache)
        {
            UnityEngine.Object intededObject = delegeateProp.FindPropertyRelative("m_target").objectReferenceValue;
            string message= VisualEditorUtility.CreateErrorMessage(delegeateProp.FindPropertyRelative("methodData"), intededObject);
            delegeateProp.FindPropertyRelative("creationMethod").stringValue = message;
            delegeateProp.FindPropertyRelative("serializationError").boolValue = true;
            PrefabUtility.RecordPrefabInstancePropertyModifications(delegeateProp.serializedObject.targetObject);
            delegeateProp.serializedObject.ApplyModifiedProperties();
            return message;

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
            PrefabUtility.RecordPrefabInstancePropertyModifications(tProperty.serializedObject.targetObject);
            tProperty.serializedObject.ApplyModifiedProperties();
        }
        /// <summary>
        /// used populate string with method name that will be called at runtime
        /// </summary>
        /// <param name="tproperty"></param>
        /// <param name="rawcallview"></param>
        private void UpdateMethodName(SerializedProperty tproperty, RawCallView rawcallview)
        {
            SerializedProperty creationmethodprop = tproperty.FindPropertyRelative("creationMethod");
            if (rawcallview.arguments != null)
            {
                var argumentlength = rawcallview.arguments.Length;
                MemberInfo info = rawcallview.SelectedMember.info;
                string NewmethodName = null;
                if (info is MethodInfo method_info)
                {
                    if (method_info.ReturnType == typeof(void))
                    {
                        if (rawcallview.isDynamic)
                        {
                            Debug.Log("should go in here");
                            NewmethodName = "createAction" + argumentlength;
                        }
                        else NewmethodName = "createActionCall" + argumentlength;
                    }
                    else
                    {
                        if (rawcallview.isDynamic)
                            NewmethodName = "createFunc" + argumentlength;
                        else NewmethodName = "createFuncCall" + argumentlength;
                    }
                }
                else if (info is FieldInfo)
                {
                    if (rawcallview.isDynamic)
                        NewmethodName = "createFieldAction";
                    else NewmethodName = "createFieldCall";
                }
                else
                {
                    if (rawcallview.isDynamic)
                        NewmethodName = "createPropertyAction";
                    else NewmethodName = "createPropertyCall";
                }
                if (NewmethodName != null && !NewmethodName.Equals(creationmethodprop.stringValue, StringComparison.OrdinalIgnoreCase))
                {
                    creationmethodprop.stringValue = NewmethodName;
                }
            }
        }
    }
}