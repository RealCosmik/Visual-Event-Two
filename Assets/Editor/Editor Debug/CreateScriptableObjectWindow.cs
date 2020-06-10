using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;

public class CreateScriptableObjectWindow : EditorWindow
{

    string search = "";
    List<Type> results = new List<Type>();
    Vector2 scroll;

    [MenuItem("Assets/Create/Scriptable Object", false, -1)]
    private static void ShowWindow()
    {
        var window = GetWindow<CreateScriptableObjectWindow>();
        window.titleContent = new GUIContent("Scriptable Object");
        window.Show();
    }

    void OnEnable()
    {
        results = new List<Type>();
        UpdateSearch(search);
    }

    void UpdateSearch(string searchStr)
    {

        if (searchStr == "") return;
        searchStr = searchStr.ToLower();
        results.Clear();
        var allTypes = TypeCache.GetTypesDerivedFrom<ScriptableObject>().Where(t => t.Name.ToLower().Contains(searchStr));
        results.AddRange(allTypes);
    }

    string CurrentSelectionFolderPath() => AssetDatabase.GetAssetPath(Selection.activeObject);
    void CreateInstanceBytype(Type instancetype)
    {
        var inst = CreateInstance(instancetype);
        var path = CurrentSelectionFolderPath() + "/" + instancetype.Name + ".asset";
        ProjectWindowUtil.CreateAsset(inst, path);
    }
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.toolbar);
        EditorGUI.BeginChangeCheck();
        GUI.SetNextControlName("SearchBar");
        search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);
        GUI.FocusControl("SearchBar");

        if (EditorGUI.EndChangeCheck())
        {
            UpdateSearch(search);
        }

        if (results != null)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(position.height));
            foreach (var t in results)
            {
                if (GUILayout.Button(t.Name, EditorStyles.toolbarButton))
                {
                    CreateInstanceBytype(t);
                }
            }
            EditorGUILayout.EndScrollView();

        }
        EditorGUILayout.EndVertical();
    }
}