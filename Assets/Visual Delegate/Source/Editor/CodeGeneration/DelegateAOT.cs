using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using VisualEvent;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Reflection;
using UnityEditor.SceneManagement;
using System.Linq;
public sealed class DelegateAOT :ScriptableObject,IPreprocessBuildWithReport
{
    public bool GenerateOnBuild;
    public int callbackOrder => 0;
    [MenuItem("VisualDelegate/AOT Solver", false)]
    static void OpenDelegateMenu()
    {
        string tempPath = "Assets/Editor/Delegate AOT.asset";
        DelegateAOT resolver = AssetDatabase.LoadMainAssetAtPath(tempPath) as DelegateAOT;
        if (resolver == null)
        {
            resolver = CreateInstance<DelegateAOT>();
            AssetDatabase.CreateAsset(resolver, AssetDatabase.GenerateUniqueAssetPath(tempPath));
            AssetDatabase.SaveAssets();
            UnityEditor.EditorUtility.FocusProjectWindow();
        }
        Selection.activeObject = resolver;
    }
    public void Trythis()
    {
        Debug.Log("invoke try");
    }
    [MenuItem("AOT/traversal")]
    public static void Search()
    {
    }
 
    [MenuItem("AOT/test")]
    public static void TestThis()
    {
        Type[] type_names = new Type[1];
        MethodInfo info = null;
        Debug.Log(Utility.DelegatePropertyCreationMethod.Count);
        int iteration = 0;
        foreach (var p in Utility.DelegatePropertyCreationMethod)
        {
            if (iteration == 0)
            {
                Debug.Log(p.Key[0].Name);
                type_names[0] = p.Key[0];
                info = p.Value;
                Debug.Log(info.Name);
            }
            iteration++;
        }
        var filepath = Path.Combine(Application.dataPath, "testerfile.cs");
        var workspace = new AdhocWorkspace(MefHostServices.DefaultHost);
        var generator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);
        var visualimport = generator.NamespaceImportDeclaration("VisualEvent");
        var base_type = generator.IdentifierName("RawCall");
        var invocationName = generator.WithTypeArguments(generator.IdentifierName(info.Name), generator.IdentifierName(type_names[0].FullName));
        var body = generator.InvocationExpression(invocationName, GetArguments(info, generator));
        var fixer = generator.MethodDeclaration("AOTFIXER", null, null, null, Accessibility.Private, DeclarationModifiers.None, new SyntaxNode[] { body });
        var classdecleartion = generator.ClassDeclaration("fixerclass", null, Accessibility.Internal, DeclarationModifiers.Sealed,
            base_type, null, new SyntaxNode[] { fixer });
        var classdec = generator.CompilationUnit(visualimport, classdecleartion).NormalizeWhitespace();
        GenerateCodeFile(classdec.GetText().ToString(), filepath);
        AssetDatabase.ImportAsset(Path.Combine("Assets", "testerfile.cs"), ImportAssetOptions.ForceUpdate);
        //var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        //var result = CSharpCompilation.Create("tester.dll", new Microsoft.CodeAnalysis.SyntaxTree[] { classdec.SyntaxTree },
        //  Getassemblymeta(), options).Emit(filepath);
        //Debug.Log(result.Success);
        //for (int i = 0; i < result.Diagnostics.Length; i++)
        //{
        //    Debug.Log(result.Diagnostics[i]);
        //}

    }
    [MenuItem("AOT/clear")]
    public static void Clear() => Utility.DelegatePropertyCreationMethod.Clear();
    private static SyntaxNode[] GetArguments(MethodInfo info, SyntaxGenerator generator)
    {
        var paramargs = info.GetParameters();
        for (int i = 0; i < paramargs.Length; i++)
        {
            Debug.Log(paramargs[i].ParameterType.FullName);
        }
        int param_count = info.GetParameters().Length;
        var arguments = new SyntaxNode[param_count];
        for (int i = 0; i < param_count; i++)
        {
            arguments[i] = generator.NullLiteralExpression();
        }
        return arguments;
    }
    private static void GenerateDLL(CSharpCompilation compiledDll)
    {

    }
    private static void GenerateCodeFile(string code, string filepath)
    {
        using (StreamWriter writer = new StreamWriter(filepath))
        {
            writer.WriteLine(code);
        }
    }
    public void OnPreprocessBuild(BuildReport report)
    {
        if (GenerateOnBuild)
        {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var included_scenes = EditorBuildSettings.scenes.Where(s => s.enabled);
            var scene_count = included_scenes.Count();
            for (int i = 0; i < scene_count; i++)
            {
                var currentscene = EditorSceneManager.OpenScene(included_scenes.ElementAt(i).path);
            }
            Debug.Log(Utility.DelegatePropertyCreationMethod.Count);
            TestThis();
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
    }
}
