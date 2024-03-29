﻿using UnityEditor;
using UnityEngine;
using System.IO;

namespace VisualDelegates.Editor
{
    class DelegateEditorSettings : ScriptableObject
    {
        static DelegateEditorSettings m_instance;
        public bool visualDebugging = false;
        public Color EvenColor, OddColor, Selectedcolor, YieldColor, InvocationColor, ErrorColor;
        public static DelegateEditorSettings instance
        {
            get
            {
                return m_instance = m_instance ?? GetOrCreateSingleton();
            }
        }

        [MenuItem("VisualDelegate/Editor Settings")]
        private static void OpenDelegateEditorSettings()
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = GetOrCreateSingleton();
        }
        private static DelegateEditorSettings GetOrCreateSingleton()
        {
            var assetpath = AssetDatabase.FindAssets("t:DelegateEditorSettings");
            if (assetpath.Length == 0)
            {
                string tempPath = "Assets/Editor/DelegateSettings.asset";
                if (!AssetDatabase.IsValidFolder("Assets/Editor"))
                {
                    Directory.CreateDirectory(Path.Combine(Application.dataPath, "Editor"));
                }
                m_instance = CreateInstance<DelegateEditorSettings>();
                AssetDatabase.CreateAsset(m_instance, AssetDatabase.GenerateUniqueAssetPath(tempPath));
                SetDefaultColors();
                EditorUtility.SetDirty(m_instance);
                AssetDatabase.SaveAssets();
                return m_instance;
            }
            else return AssetDatabase.LoadAssetAtPath<DelegateEditorSettings>(AssetDatabase.GUIDToAssetPath(assetpath[0]));
        }
        private static void SetDefaultColors()
        {
            m_instance.EvenColor = new Color(.3f, .3f, .3f, 1f);
            m_instance.OddColor = new Color(.18f, .18f, .18f, 1f);
            m_instance.Selectedcolor = new Color(.13f, .26f, .27f, 1f);
            m_instance.YieldColor = new Color(.6f, .6f, .3f, 1f);
            m_instance.InvocationColor = new Color(.6f, .8f, .35f, 1f);
            m_instance.ErrorColor = new Color(.8f, .3f, .3f, 1f);
        }
    }
}