using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
namespace VisualDelegates.Events.Editor
{
    [CustomPropertyDrawer(typeof(IVisualVariable), true)]
    class VisualVariablePropertyDrawer : PropertyDrawer
    {
        static Dictionary<int, SerializedObject> variableCache = new Dictionary<int, SerializedObject>();
        static readonly float objectHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);
        const string INITIAL_PROPERTY = "initialValue";
        const string CURRENT_PROPERTY = "currentValue";
        private string GetVariableName() => EditorApplication.isPlaying ? CURRENT_PROPERTY : INITIAL_PROPERTY;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue != null)
                return objectHeight + GetVariableHeight(GetSeriazliedVariable(property.objectReferenceValue));
            else return objectHeight;
        }
        private float GetVariableHeight(SerializedObject serializedvar)
        {
            float variableheight;
            if (EditorApplication.isPlaying)
                variableheight = EditorGUI.GetPropertyHeight(serializedvar.FindProperty(CURRENT_PROPERTY));
            else variableheight = EditorGUI.GetPropertyHeight(serializedvar.FindProperty(INITIAL_PROPERTY));
            return variableheight == objectHeight ? 0 : variableheight;
        }
        private SerializedObject GetSeriazliedVariable(UnityEngine.Object variable)
        {
            int key = variable.GetInstanceID();
            if (!variableCache.TryGetValue(key, out SerializedObject serializedvar))
            {
                serializedvar = new SerializedObject(variable);
                variableCache.Add(key, serializedvar);
            }
            return serializedvar;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var variableobject = property.objectReferenceValue;
            var objectrect = position;
            objectrect.width = variableobject == null ? position.width : position.width * .7f;
            objectrect.height = objectHeight;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(objectrect, property);
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
            if (variableobject != null)
            {
                GUI.enabled = false;
                var variablerect = objectrect;
                variablerect.x += objectrect.width;
                variablerect.width = position.width * .3f;
                var serializedvar = GetSeriazliedVariable(variableobject);
                EditorGUI.PropertyField(variablerect, serializedvar.FindProperty(GetVariableName()), GUIContent.none);
                GUI.enabled = true;
                if (EditorApplication.isPlaying)
                    serializedvar.UpdateIfRequiredOrScript();
            }
        }
    }
}
