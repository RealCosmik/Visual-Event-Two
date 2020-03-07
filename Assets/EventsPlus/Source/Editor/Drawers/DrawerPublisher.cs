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
    [CustomPropertyDrawer(typeof(Publisher), true)]
    public class DrawerPublisher : PropertyDrawer
    {

        private static Texture2D eventexture;
        private static Texture2D oddtexture;
        private static GUIStyle evenstyle;
        private static GUIStyle oddstyle;
        private static Color evencolor = new Color(.21f, .21f, .21f);
        private static Color oddColor = new Color(.5f, .5f, .5f);
        private static RawCallView copiedcache;
        private static SerializedProperty copiedprop;
        private static GenericMenu rightclick_menu;
        private const string Copy = "Copy";
        private const string Paste = "Paste";
        // Note that this function is only meant to be called from OnGUI() functions.
        public static void GUIDrawRect(Rect position, int index)
        {
            if (eventexture == null)
            {
                eventexture = new Texture2D(1, 1);
                eventexture.SetPixel(0, 0, evencolor);
                eventexture.Apply();
            }
            if (oddtexture == null)
            {
                oddtexture = new Texture2D(1, 1);
                oddtexture.SetPixel(0, 0, oddColor);
                oddtexture.Apply();
            }

            if (evenstyle == null)
            {
                evenstyle = new GUIStyle();
                evenstyle.normal.background = eventexture;
            }
            if (oddstyle == null)
            {
                oddstyle = new GUIStyle();
                oddstyle.normal.background = oddtexture;
            }
            if (index % 2 == 0)
                GUI.Box(position, GUIContent.none, evenstyle);
            else GUI.Box(position, GUIContent.none, oddstyle);


        }

        static Texture2D t = Texture2D.whiteTexture;
        //=======================
        // Variables
        //=======================
        /// <summary>Cached <see cref="ReorderableList"/> dictionary used for optimization</summary>
        private Dictionary<string, ReorderableList> cache = new Dictionary<string, ReorderableList>();

        //=======================
        // Initialization
        //=======================
        /// <summary>Initializes the drawer and calculates the inspector height</summary>
        /// <param name="tProperty">Serialized <see cref="Publisher"/> property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        /// <returns>Height of the drawer</returns>
        public override float GetPropertyHeight(SerializedProperty tProperty, GUIContent tLabel)
        {
            // Initialize reorderable list
            SerializedProperty tempCallsProperty = tProperty.FindPropertyRelative("_calls");
            ReorderableList tempList;
            if (!cache.TryGetValue(tProperty.propertyPath, out tempList))
            {
                tempList = new ReorderableList(tProperty.serializedObject, tempCallsProperty, true, true, true, true);
                tempList.footerHeight = EditorGUIUtility.singleLineHeight;
                tempList.drawHeaderCallback += rect =>
                {
                };
                tempList.drawElementCallback += (Rect rect, int tindex, bool tIsActive, bool tIsFocused) =>
                {
                    EditorGUI.PropertyField(rect, tempList.serializedProperty.GetArrayElementAtIndex(tindex), true);
                };
                tempList.elementHeightCallback += (int tIndex) =>
                {

                    return EditorGUI.GetPropertyHeight(tempList.serializedProperty.GetArrayElementAtIndex(tIndex), false) + EditorGUIUtility.standardVerticalSpacing;
                };
                cache.Add(tProperty.propertyPath, tempList);
                tempList.onRemoveCallback += list => onElementDelete(list, tempCallsProperty);
                tempList.onAddCallback += list => onAddElement(list, tempCallsProperty);
                tempList.onReorderCallbackWithDetails += (ReorderableList list, int oldindex, int newindex) =>
                  OnReorder(list, oldindex, newindex, tProperty);

                tempList.onSelectCallback += list =>
                {
                    if (Event.current.button == 1)
                    {
                        ShowRightClickMenu(tempList, tempCallsProperty);
                    }
                };

            }

            // Calculate height
            float tempHeight = base.GetPropertyHeight(tProperty, tLabel);
            if (tProperty.isExpanded)
            {
                tempHeight += tempList.GetHeight() + (tempCallsProperty.arraySize * EditorGUIUtility.standardVerticalSpacing);
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
        public override void OnGUI(Rect tPosition, SerializedProperty tProperty, GUIContent tLabel)
        {
            ClearOldCache(tProperty);
            tPosition.height = base.GetPropertyHeight(tProperty, tLabel);
            var calls = tProperty.FindPropertyRelative("_calls");
            tProperty.isExpanded = EditorGUI.Foldout(tPosition, tProperty.isExpanded, tProperty.displayName);
            if (tProperty.isExpanded)
            {
                ++EditorGUI.indentLevel;
                // Calls
                ReorderableList tempList = cache[tProperty.propertyPath];
                int tempIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                float tempIndentSize = tempIndentLevel * EditorUtility.IndentSize;
                tPosition.x += tempIndentSize;
                tPosition.y += tPosition.height + EditorGUIUtility.standardVerticalSpacing;
                tPosition.x -= tempIndentSize;
                tPosition.height = tempList.GetHeight();
                EditorGUI.BeginChangeCheck();
                tempList.DoList(tPosition);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    tProperty.serializedObject.ApplyModifiedProperties();
                    tProperty.GetPublisher()?.ReInitialize();
                }
                EditorGUI.indentLevel = tempIndentLevel - 1;
                CopyPasteKeyboard(tempList, calls);
            }
        }

        private void SetManagedPublisherReference(SerializedProperty publiserproperty)
        {
            Debug.Log(publiserproperty.managedReferenceFieldTypename);
            int StartTypeName_index = publiserproperty.managedReferenceFullTypename.IndexOf(' ') + 1;
            Debug.Log(StartTypeName_index);
            string TypeName = publiserproperty.managedReferenceFieldTypename.Substring(StartTypeName_index);
            Debug.Log(TypeName);
            var publisher = Utility.CreatePublisherFromTypeName(TypeName);
            publiserproperty.managedReferenceValue = publisher;
        }
        private void onElementDelete(ReorderableList list, SerializedProperty arrayprop)
        {
            var removedindex = list.index;
            var delegateprop = arrayprop.GetArrayElementAtIndex(removedindex);
            string pubpath = delegateprop.GetPublisherPath();
            delegateprop.FindPropertyRelative("m_arguments").ClearArray();
            delegateprop.FindPropertyRelative("m_target").objectReferenceValue = null;
            ViewCache.Cache[pubpath].RemoveAt(removedindex);
            arrayprop.DeleteArrayElementAtIndex(removedindex);
            arrayprop.serializedObject.ApplyModifiedProperties();
        }
        private void onAddElement(ReorderableList list, SerializedProperty arrayprop)
        {
            int size = arrayprop.arraySize;
            arrayprop.InsertArrayElementAtIndex(size);
            var newcall = arrayprop.GetArrayElementAtIndex(size);
            newcall.FindPropertyRelative("m_arguments").ClearArray();
            newcall.FindPropertyRelative("m_target").objectReferenceValue = null;
            arrayprop.serializedObject.ApplyModifiedProperties();
        }

        private void OnReorder(ReorderableList list, int oldindex, int newindex, SerializedProperty publisherprop)
        {
            string pubpath = publisherprop.propertyPath;
            var cache_list =ViewCache.Cache[pubpath];
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
                    var index_shiftUp = i + 1 == cache_list.Count ? 0 : i + 1;
                    cache_list[i] = cache_list[index_shiftUp];
                }
            }
            cache_list[newindex] = elementCache;
            publisherprop.serializedObject.ApplyModifiedProperties();
            publisherprop.GetPublisher()?.ReInitialize();
        }

        private void ClearOldCache(SerializedProperty publisherprop)
        {
            var Array_size = publisherprop.FindPropertyRelative("_calls").arraySize;
            string publisherpath = publisherprop.propertyPath;
            if (Array_size == 0)
            { 
                if (ViewCache.Cache.TryGetValue(publisherpath, out List<RawDelegateView> viewcache))
                {
                        var OldCache = viewcache.Where(k => k!=null&& k.propertypath.StartsWith(publisherpath, StringComparison.OrdinalIgnoreCase));
                        for (int i = 0; i < OldCache.Count(); i++)
                        {
                            var currentCache = OldCache.ElementAt(i);
                            if (currentCache.CurrentTarget != null)
                                currentCache.ClearViewCache();
                        }
                    }
                }
        }
        /// <summary>
        /// Copies call to static fields
        /// </summary>
        /// <param name="list"></param>
        /// <param name="arrayprop"></param>
        private void CopyDelegate(ReorderableList list,SerializedProperty arrayprop)
        {
            var currentdelegate = arrayprop.GetArrayElementAtIndex(list.index);
            var cachelist = ViewCache.Cache[currentdelegate.GetPublisherPath()];
            copiedcache = cachelist[currentdelegate.GetRawCallIndex()] as RawCallView;
            copiedprop = currentdelegate;
        }
        private void PasteDelegate(ReorderableList list,SerializedProperty arrayprop)
        {
            if (copiedcache?.CurrentTarget != null)
            {
                var currentdelegateprop = arrayprop.GetArrayElementAtIndex(list.index);
                EditorUtility.CopySeralizedMethodDataToProp(currentdelegateprop.FindPropertyRelative("methodData"), copiedcache.SelectedMember.SeralizedData);
                currentdelegateprop.FindPropertyRelative("m_target").objectReferenceValue = copiedcache.CurrentTarget;
                var arguments = currentdelegateprop.FindPropertyRelative("m_arguments");
                var argument_size = copiedcache.arguments.Length;
                arguments.arraySize = argument_size;

                var copied_arguments = copiedprop.FindPropertyRelative("m_arguments");
                for (int i = 0; i < argument_size; i++)
                {
                    arguments.GetArrayElementAtIndex(i).FindPropertyRelative("assemblyQualifiedArgumentName").stringValue = copiedcache.arguments[i].type.AssemblyQualifiedName;
                    arguments.GetArrayElementAtIndex(i).FindPropertyRelative("FullArgumentName").stringValue = copiedcache.arguments[i].type.FullName;
                    EditorUtility.CopyDelegateArguments(arguments.GetArrayElementAtIndex(i), copied_arguments.GetArrayElementAtIndex(i));

                }

                currentdelegateprop.serializedObject.ApplyModifiedProperties();
                arrayprop.serializedObject.ApplyModifiedProperties();

                //updating cache 
                var currentcache = ViewCache.Cache[currentdelegateprop.GetPublisherPath()][currentdelegateprop.GetRawCallIndex()];
                currentcache.SetParentTarget(copiedcache.CurrentTarget);
                currentcache.CurrentTargetIndex = copiedcache.CurrentTargetIndex;
                currentcache.UpdateSelectedTarget(copiedcache.CurrentTargetIndex);
                currentcache.UpdateSelectedMember(copiedcache.selectedMemberIndex);
            } 
        }
        private void ShowRightClickMenu(ReorderableList list,SerializedProperty calls)
        {
            if (rightclick_menu == null)
            {
                rightclick_menu = new GenericMenu();
                rightclick_menu.AddItem(new GUIContent("copy delegate"), false, () => CopyDelegate(list, calls));
                rightclick_menu.AddItem(new GUIContent("paste delegate"), false, () => PasteDelegate(list, calls));
            }
            rightclick_menu.ShowAsContext();
        }
        private void CopyPasteKeyboard(ReorderableList list,SerializedProperty arrayprop)
        {
            if (Event.current.commandName == Copy)
                CopyDelegate(list, arrayprop);
            else if (Event.current.commandName == Paste)
                PasteDelegate(list, arrayprop);
        }
    }
}
