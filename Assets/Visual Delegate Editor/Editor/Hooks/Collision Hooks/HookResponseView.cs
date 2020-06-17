using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;
namespace VisualDelegates.Editor
{
   public class HookResponseView
    {
        List<MonoScript> currentScripts;
        SerializedProperty hookresponse;
        public ReorderableList scriptviews;
        [SerializeField] bool noteExpanded,headerExpanded;
        Vector2 scroll;
        const string TYPE_INFOS = "typeInfos";
        const string COLLISION_DELEGATE = "onCollision";
        const string NOTE = "collisionNote";
        string header;
        GUIContent Listheadercontent = new GUIContent("Collision Types");
        public HookResponseView(SerializedProperty collisionresponse,string headername, MonoScript[] avaialbeScripts)
        {
            header = headername;
            hookresponse = collisionresponse;
            var typenames = hookresponse.FindPropertyRelative(TYPE_INFOS);
            var size = typenames.arraySize;
            currentScripts = new List<MonoScript>(size);
            for (int i = 0; i < size; i++)
            {
                var current_type = Type.GetType(typenames.GetArrayElementAtIndex(i).stringValue);
                var script = current_type == null ? null : avaialbeScripts.FirstOrDefault(m => m.GetClass() == current_type);
                currentScripts.Add(script);
            }
            initReorderableList();
        }
        private void initReorderableList()
        {
            scriptviews = new ReorderableList(currentScripts, typeof(MonoScript), true, true, true, true);
            scriptviews.drawHeaderCallback += DrawHeader;
            scriptviews.drawElementCallback += DrawMonoscript;
            scriptviews.elementHeightCallback += val => EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);
            scriptviews.onAddCallback += OnScriptAdded;
            scriptviews.onRemoveCallback += OnScriptRemoved;
        }
        private void DrawMonoscript(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            currentScripts[index] = EditorGUI.ObjectField(rect, currentScripts[index], typeof(MonoScript), false) as MonoScript;
            if (EditorGUI.EndChangeCheck())
            {
                hookresponse.FindPropertyRelative(TYPE_INFOS).GetArrayElementAtIndex(index).stringValue =
                    currentScripts[index].GetClass().AssemblyQualifiedName;
                hookresponse.serializedObject.ApplyModifiedProperties();
            }
        }
        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, Listheadercontent);
        }
        private void OnScriptAdded(ReorderableList typelist)
        {
            var serializedtypes = hookresponse.FindPropertyRelative(TYPE_INFOS);
            serializedtypes.InsertArrayElementAtIndex(serializedtypes.arraySize);
            hookresponse.serializedObject.ApplyModifiedProperties();
            currentScripts.Add(null);
        }
        private void OnScriptRemoved(ReorderableList typelist)
        {
            var index = typelist.index;
            if (index != -1)
            {
                currentScripts.RemoveAt(index);
                hookresponse.FindPropertyRelative(TYPE_INFOS).DeleteArrayElementAtIndex(index);
                hookresponse.serializedObject.ApplyModifiedProperties();
            }
        }
        public void LayoutHookResponse()
        {
            headerExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(headerExpanded, header);
            if (headerExpanded)
            {
                DrawNote();
                EditorGUILayout.BeginHorizontal();
                var rect = GUILayoutUtility.GetRect(1, scriptviews.GetHeight());
                scriptviews.DoList(rect);
                EditorGUILayout.Space(10, false);
                var response = hookresponse.FindPropertyRelative(COLLISION_DELEGATE);
                response.isExpanded = true;
                EditorGUILayout.PropertyField(response);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Separator();
        }
        private void DrawNote()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Hook Note");
            var noteprop = hookresponse.FindPropertyRelative(NOTE);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(50), GUILayout.ExpandWidth(false));
            EditorGUI.BeginChangeCheck();
            noteprop.stringValue = EditorGUILayout.TextArea(noteprop.stringValue, EditorStyles.textArea, GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
                noteprop.serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}