using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Linq;
using System.Reflection;
namespace VisualEvent
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Inspector class for rendering the <see cref="VisualDelegate"/> in the inspector</summary>
    [CustomPropertyDrawer(typeof(VisualDelegateBase), true)]
    public class DrawerVisualDelegate : PropertyDrawer
    {

        private static Texture2D eventexture;
        private static Texture2D oddtexture;
        private static GUIStyle evenstyle;
        private static GUIStyle oddstyle;
        private static Color evencolor = new Color(.21f, .21f, .21f);
        private static Color oddColor = new Color(.5f, .5f, .5f);
        private static RawCallView copiedcache;
        private static int copyIndex;
        private static GenericMenu rightclick_menu;
        private const string Copy = "Copy";
        private const string Paste = "Paste";
        private const string Cut = "Cut";
        private const string Delete = "SoftDelete";

        //=======================
        // Variables
        //=======================
        /// <summary>Cached <see cref="ReorderableList"/> dictionary used for optimization</summary>
        private Dictionary<string, ReorderableList> cache = new Dictionary<string, ReorderableList>();

        //=======================
        // Initialization
        //=======================
        /// <summary>Initializes the drawer and calculates the inspector height</summary>
        /// <param name="tProperty">Serialized <see cref="VisualDelegate"/> property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        /// <returns>Height of the drawer</returns>
        public override float GetPropertyHeight(SerializedProperty tProperty, GUIContent tLabel)
        {
            CreateVisualDelegateCache(tProperty);
            // Initialize reorderable list
            SerializedProperty callsProperty = tProperty.FindPropertyRelative("m_calls");
            ReorderableList tempList;
            var pubcache = ViewCache.GetVisualDelegateInstanceCache(tProperty);
            if (!cache.TryGetValue(tProperty.propertyPath, out tempList))
            {
                tempList = new ReorderableList(tProperty.serializedObject, callsProperty, true, true, true, true);
                tempList.footerHeight = EditorGUIUtility.singleLineHeight;
                tempList.drawHeaderCallback += rect =>
                {
                    // we leave this delegate empty so that nothing draws in the list header
                };
                tempList.drawElementCallback += (Rect rect, int index, bool tIsActive, bool tIsFocused) =>
                {
                    var prop = tempList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.BeginProperty(rect, GUIContent.none, prop);
                    EditorGUI.PropertyField(rect, prop,GUIContent.none, true);
                    EditorGUI.EndProperty();
                };
                tempList.elementHeightCallback += (int index) =>
                {
                    if (index < pubcache.RawCallCache.Count)
                    {
                        return pubcache.RawCallCache[index].delegateView.Height + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else return 67;
                };
                cache.Add(tProperty.propertyPath, tempList);

                tempList.onReorderCallbackWithDetails += (ReorderableList list, int oldindex, int newindex) =>
                  OnReorder(list, oldindex, newindex, tProperty);

                tempList.onSelectCallback += list =>
                {
                    if (Event.current.button == 1)  // right click
                        ShowRightClickMenu(tempList, callsProperty);
                };
                tempList.onRemoveCallback += onElementDelete;
                tempList.onAddCallback += onAddElement;
            }
            // Calculate height 
            float tempHeight = base.GetPropertyHeight(tProperty, tLabel);
            if (tProperty.isExpanded)
            {
                tempHeight += tempList.GetHeight() + (callsProperty.arraySize * EditorGUIUtility.standardVerticalSpacing);
            }
            return tempHeight;
        }
        private void CreateVisualDelegateCache(SerializedProperty VisualDelegateprop)
        {
            int Id = VisualDelegateprop.serializedObject.targetObject.GetInstanceID();
            ViewCache.GetVisualDelegateCacheFromObject(Id);
        }
        //=======================
        // Render
        //=======================
        /// <summary>Renders the individual <see cref="VisualDelegate"/> property</summary>
        /// <param name="tPosition">Inspector position and size of <paramref name="tProperty"/></param>
        /// <param name="tProperty">Serialized <see cref="VisualDelegate"/> property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        public override void OnGUI(Rect tPosition, SerializedProperty tProperty, GUIContent tLabel)
        {
            EditorGUI.BeginChangeCheck();
            ClearOldCache(tProperty);
            tPosition.height = base.GetPropertyHeight(tProperty, tLabel);
            tProperty.isExpanded = EditorGUI.Foldout(tPosition, tProperty.isExpanded, tProperty.displayName);
            if (tProperty.isExpanded)
            {
                ReorderableList tempList = cache[tProperty.propertyPath];
                tempList.draggable = true;
                ++EditorGUI.indentLevel;
                // Calls
                int tempIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                float tempIndentSize = tempIndentLevel * VisualEdiotrUtility.IndentSize;
                tPosition.x += tempIndentSize;
                tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing;
                tPosition.x -= tempIndentSize;
                tPosition.height = tempList.GetHeight();
                tempList.DoList(tPosition);
                EditorGUI.indentLevel = tempIndentLevel - 1;
                CopyPasteKeyboard(tempList, tempList.serializedProperty);

            }
            if (EditorGUI.EndChangeCheck())
            {
                tProperty.serializedObject.ApplyModifiedProperties();
                PrefabUtility.RecordPrefabInstancePropertyModifications(tProperty.serializedObject.targetObject);
                if (EditorApplication.isPlaying)
                {
                    Debug.Log("change");
                    tProperty.GetVisualDelegateObject()?.ReInitialize();
                } 
                else
                {
                    var AllDelegateCaches = ViewCache.GetVisualDelegateInstanceCache(tProperty).RawCallCache;
                    var length = AllDelegateCaches.Count;
                    bool hasyeild = false;
                    for (int i = 0; i < length; i++)
                    {
                        var delegate_view = AllDelegateCaches[i].delegateView;
                        if (delegate_view is RawCallView rawDelegateview && rawDelegateview.isYieldable)
                        {
                            hasyeild = true;
                            break;
                        }
                    }
                    Debug.Log(hasyeild);
                    tProperty.FindPropertyRelative("hasyield").boolValue = hasyeild;
                    if (hasyeild)
                        tProperty.FindPropertyRelative("Yield_target").objectReferenceValue = tProperty.serializedObject.targetObject;
                    else tProperty.FindPropertyRelative("Yield_target").objectReferenceValue = null;
                    tProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }
        /// <summary>
        /// remove RawDelegate to visualDelgate list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="arrayprop"></param>
        private void onElementDelete(ReorderableList list)
        {
            Debug.Log("delete");
            var removedindex = list.index;
            var arrayprop = list.serializedProperty;
            if (list.index < arrayprop.arraySize)
            {
                var delegateprop = list.serializedProperty.GetArrayElementAtIndex(removedindex);
                var cache = ViewCache.GetRawCallCache(delegateprop);
                arrayprop.DeleteArrayElementAtIndex(removedindex);
                arrayprop.serializedObject.ApplyModifiedProperties();
                if (cache?.CurrentTarget != null)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(arrayprop.serializedObject.targetObject);
                    //PrefabUtility.ApplyPrefabInstance((arrayprop.serializedObject.targetObject as Component).gameObject,
                    //    InteractionMode.UserAction);
                }
                cache?.ClearViewCache();
            }
        }
        /// <summary>
        /// On Add element list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="arrayprop"></param>
        private void onAddElement(ReorderableList list)
        {
          //  Undo.RegisterCompleteObjectUndo(list.serializedProperty.serializedObject.targetObject, "add");
            var arrayprop = list.serializedProperty;
            var size = arrayprop.arraySize;
            arrayprop.InsertArrayElementAtIndex(size);
            arrayprop.GetArrayElementAtIndex(size).managedReferenceValue = new RawCall();
            var delegateprop = arrayprop.GetArrayElementAtIndex(size);
            delegateprop.FindPropertyRelative("m_target").objectReferenceValue = null;
            delegateprop.FindPropertyRelative("m_runtime").boolValue = false;
          //  PrefabUtility.RecordPrefabInstancePropertyModifications(arrayprop.serializedObject.targetObject);
            arrayprop.serializedObject.ApplyModifiedProperties();
        } 

        private void OnReorder(ReorderableList list, int oldindex, int newindex, SerializedProperty ViewDelegateProp)
        {
            var cache_list = ViewCache.GetVisualDelegateInstanceCache(ViewDelegateProp).RawCallCache;
            var elementCache = cache_list[oldindex]; //cache before reorder
            if (newindex < oldindex) // moved element higher up
            {
                //Debug.Log("shifted up");
                for (int i = oldindex; i > newindex; i--) //2
                {
                    var index_shitDown = i - 1 == -1 ? cache_list.Count - 1 : i - 1; //shift other indicies up or wrap to bottom
                                                                                     // Debug.Log(index_shitDown + "turns to " + i);
                    cache_list[i] = cache_list[index_shitDown];
                }
            }
            else
            {
                for (int i = oldindex; i < newindex; i++)
                {
                    var index_shiftUp = i + 1 == cache_list.Count ? 0 : i + 1;  // shift indidces down or wrap
                    cache_list[i] = cache_list[index_shiftUp];
                }
            }
            cache_list[newindex] = elementCache; // place cache
            if (!EditorApplication.isPlaying)
                ViewDelegateProp.serializedObject.ApplyModifiedProperties();
            if (EditorApplication.isPlaying)
                ViewDelegateProp.GetVisualDelegateObject()?.ReInitialize();
        }
        /// <summary>
        /// Clears Cache if the ViewDelegates list is empty
        /// </summary>
        /// <param name="ViewDelegateprop"></param>
        private void ClearOldCache(SerializedProperty ViewDelegateprop)
        {
            var Array_size = ViewDelegateprop.FindPropertyRelative("m_calls").arraySize;
            var call_list = ViewCache.GetVisualDelegateInstanceCache(ViewDelegateprop).RawCallCache;
            if (Array_size == 0 && call_list.Count != 0)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(ViewDelegateprop.serializedObject.targetObject);
                ViewDelegateprop.FindPropertyRelative("m_calls").ClearArray();
                ViewDelegateprop.serializedObject.ApplyModifiedProperties();
                call_list.Clear();
            }
        }
        /// <summary>
        /// Copies call to static fields
        /// </summary>
        /// <param name="list"></param>
        /// <param name="arrayprop"></param>
        private void CopyDelegate(ReorderableList list, SerializedProperty arrayprop)
        {
            if (list.index != -1)
            {
                Debug.Log("Copy");
                copiedcache = ViewCache.GetRawCallCache(arrayprop.GetArrayElementAtIndex(list.index)) as RawCallView;
                copyIndex = list.index;
            }

        }
        private void PasteDelegate(ReorderableList list, SerializedProperty arrayprop)
        {
            if (copiedcache?.CurrentTarget != null)
            {
                SerializedProperty currentdelegateprop;
                if (list.index == copyIndex)
                {
                    onAddElement(list);
                    currentdelegateprop = arrayprop.GetArrayElementAtIndex(arrayprop.arraySize - 1);
                }
                else currentdelegateprop = arrayprop.GetArrayElementAtIndex(list.index);

                SerializedProperty copiedprop = arrayprop.GetArrayElementAtIndex(copyIndex);
                VisualEdiotrUtility.CopySeralizedMethodDataToProp(currentdelegateprop.FindPropertyRelative("methodData"), copiedcache.SelectedMember.SeralizedData);
                currentdelegateprop.FindPropertyRelative("m_target").objectReferenceValue = copiedcache.CurrentTarget;
                var arguments = currentdelegateprop.FindPropertyRelative("m_arguments");
                var argument_size = copiedcache.arguments.Length;
                arguments.arraySize = argument_size;

                var copied_arguments = copiedprop.FindPropertyRelative("m_arguments");
                for (int i = 0; i < argument_size; i++)
                {
                    arguments.GetArrayElementAtIndex(i).FindPropertyRelative("assemblyQualifiedArgumentName").stringValue = copiedcache.arguments[i].type.AssemblyQualifiedName;
                    arguments.GetArrayElementAtIndex(i).FindPropertyRelative("FullArgumentName").stringValue = copiedcache.arguments[i].type.FullName;
                    VisualEdiotrUtility.CopyDelegateArguments(arguments.GetArrayElementAtIndex(i), copied_arguments.GetArrayElementAtIndex(i));

                }
                currentdelegateprop.FindPropertyRelative("m_isDynamic").boolValue = copiedprop.FindPropertyRelative("m_isDynamic").boolValue;
                var currentcache = ViewCache.GetRawCallCache(currentdelegateprop) as RawCallView;
                currentcache.SetParentTarget(copiedcache.CurrentTarget);
                currentcache.CurrentTargetIndex = copiedcache.CurrentTargetIndex;
                currentcache.UpdateSelectedTarget(copiedcache.CurrentTargetIndex);
                currentcache.UpdateSelectedMember(copiedcache.selectedMemberIndex);
                currentcache.isDynamic = copiedcache.isDynamic;
                currentcache.CopyDynamicTypes(copiedcache);
                currentcache.RequiresRecalculation = true;

                currentdelegateprop.serializedObject.ApplyModifiedProperties();
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
            }
        }
        private void ShowRightClickMenu(ReorderableList list, SerializedProperty calls)
        {
            if (rightclick_menu == null)
            {
                rightclick_menu = new GenericMenu();
                rightclick_menu.AddItem(new GUIContent("copy delegate"), false, () => CopyDelegate(list, calls));
                rightclick_menu.AddItem(new GUIContent("paste delegate"), false, () => PasteDelegate(list, calls));
            }
            rightclick_menu.ShowAsContext();
        }
        private void CopyPasteKeyboard(ReorderableList list, SerializedProperty arrayprop)
        {
            if (list.HasKeyboardControl())
            {
                if (Event.current.commandName == Copy)
                    CopyDelegate(list, arrayprop);
                else if (Event.current.commandName == Paste)
                    PasteDelegate(list, arrayprop);
                else if (Event.current.commandName == Cut || Event.current.commandName == Delete)
                    onElementDelete(list);
                Event.current.commandName = null; // explicitly set to null to clear command 
            }
        }
    }
}
