using UnityEditor;
using UnityEngine;
namespace VisualEvent.Editor
{
    class DelegateEditorSettings : ScriptableSingleton<DelegateEditorSettings>
    {
        public Color EvenColor, OddColor, Selectedcolor, YieldColor, InvocationColor, ErrorColor;
        [MenuItem("VisualDelegate/Editor Settings")]
        private static void OpenDelegateEditorSettings()
        {
            var assetpath = AssetDatabase.FindAssets("t:DelegateEditorSettings");
            if (assetpath.Length == 0)
            {
                string tempPath = "Assets/Editor/DelegateSettings.asset";
                var newsettings = CreateInstance<DelegateEditorSettings>();
                AssetDatabase.CreateAsset(newsettings, AssetDatabase.GenerateUniqueAssetPath(tempPath));
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
            }
            Selection.activeObject = instance;
        }
    }
}