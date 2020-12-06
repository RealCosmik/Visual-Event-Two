﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Reflection;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace VisualDelegates.Editor
{
    internal sealed class DelegateAOT : ScriptableObject
    {
        [SerializeField] bool GenerateOnBuild;
        private static DelegateAOT instance;
        [MenuItem("VisualDelegate/Open AOT Solver", false)]
        static void OpenDelegateMenu()
        {

            Selection.activeObject = GetSingleGenerator();
        }
        private static DelegateAOT GetSingleGenerator()
        {
            var assetGUIDS = AssetDatabase.FindAssets("t:DelegateAOT");
            if (assetGUIDS.Length == 0)
            {
                string tempPath = "Assets/Editor/Delegate AOT.asset";
                if (!AssetDatabase.IsValidFolder("Assets/Editor"))
                {
                    Directory.CreateDirectory(Path.Combine(Application.dataPath, "Editor"));
                }
                instance = CreateInstance<DelegateAOT>();
                AssetDatabase.CreateAsset(instance, AssetDatabase.GenerateUniqueAssetPath(tempPath));
                AssetDatabase.SaveAssets();
                UnityEditor.EditorUtility.FocusProjectWindow();
                return instance;
            }
            else return AssetDatabase.LoadAssetAtPath<DelegateAOT>(AssetDatabase.GUIDToAssetPath(assetGUIDS[0]));
        }
        /// <summary>
        /// Opens and closes all scenes included in build to deseralize all delegates
        /// </summary>
        private static void SeralizeScenes()
        {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var included_scenes = EditorBuildSettings.scenes.Where(s => s.enabled);
            var scene_count = included_scenes.Count();
            for (int i = 0; i < scene_count; i++)
            {
                var currentscene = EditorSceneManager.OpenScene(included_scenes.ElementAt(i).path);
            }
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
        private static int DisplayWarningMessage()
        {
            return EditorUtility.DisplayDialogComplex("Unsaved Changes", "Unsaved Scene Changes will be lost during generation",
                "Save And Generate",
                "Cancel Generation",
                "Dont Save And Generate");
        }
        [MenuItem("VisualDelegate/Generate", false)]
        internal static void AOTGeneration()
        {
            bool cangenerate = true;
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                int option = DisplayWarningMessage();
                if (option == 0)
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                cangenerate = option != 1;
            }
            if (cangenerate)
            {
                SeralizeScenes();
                GenerateAOTFiles();
            }
        }

        /// <summary>
        /// Creates a list of all the seralization cache in the build project
        /// </summary>
        /// <returns></returns>
        private static List<KeyValuePair<Type[], MethodInfo>> GetAllSerailizationCache()
        {
            var count = Utility.delegateFieldSetterCreationMethods?.Count ?? 0 +
                        Utility.DelegateFieldGetterCreationMethods?.Count ?? 0 +
                        Utility.delegateDynamicFieldSetterCreationMethods?.Count ?? 0 +
                        Utility.delegateMethodCreationMethods?.Count ?? 0 +
                        Utility.DelegateDynamicMethodCreationMethods?.Count ?? 0 +
                        Utility.delegatePropertySetterCreationMethods?.Count ?? 0 +
                        Utility.delegatePropertyGetterCreationMethod?.Count ?? 0 +
                        Utility.delegateDynamicPropertySetterCreationMethod?.Count ?? 0;

            var info_cache = new List<KeyValuePair<Type[], MethodInfo>>(count);
            info_cache.AddRange(Utility.delegateFieldSetterCreationMethods ?? null);
            info_cache.AddRange(Utility.DelegateFieldGetterCreationMethods ?? null);
            info_cache.AddRange(Utility.delegateDynamicFieldSetterCreationMethods ?? null);
            info_cache.AddRange(Utility.delegatePropertySetterCreationMethods ?? null);
            info_cache.AddRange(Utility.delegateDynamicPropertySetterCreationMethod ?? null);
            info_cache.AddRange(Utility.delegatePropertyGetterCreationMethod ?? null);
            info_cache.AddRange(Utility.delegateMethodCreationMethods ?? null);
            info_cache.AddRange(Utility.DelegateDynamicMethodCreationMethods ?? null);
            Debug.Log(info_cache.Count);
            return info_cache;
        }
        /// <summary>
        /// Generates AOT file to player project
        /// </summary>
        private static void GenerateAOTFiles()
        {
            var workspace = new AdhocWorkspace(MefHostServices.DefaultHost);
            var generator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);
            var all_cache = GetAllSerailizationCache();
            int invocation_count = all_cache.Count;
            List<SyntaxNode> invocations = new List<SyntaxNode>(invocation_count);
            if (invocation_count > 0)
            {
                for (int i = 0; i < invocation_count; i++)
                {
                    invocations.Add(CreateMethodInocation(generator, all_cache[i].Value, all_cache[i].Key));
                    EditorUtility.DisplayProgressBar("test", "more info", i / invocation_count * 100f);
                }
                EditorUtility.ClearProgressBar();
                var method = CreateAOTFixDeclaration(generator, invocations);
                var classconstruct = ConstructClass(generator, method);
                GenerateFile(classconstruct);
            }
            else Debug.LogWarning("NO AOT REQUIRED");
        }
        /// <summary>
        /// Creates a roslyn invocation out of the method info
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="info">method to creation exppression</param>
        /// <param name="Generics">generics for method</param>
        /// <returns></returns>
        private static SyntaxNode CreateMethodInocation(SyntaxGenerator generator, MethodInfo info, Type[] Generics = null)
        {
            SyntaxNode methodNameNode = generator.IdentifierName(info.Name);
            if (Generics != null)
            {
                var typenodes = new SyntaxNode[Generics.Length];
                for (int i = 0; i < typenodes.Length; i++)
                {
                    if (Generics[i].IsSubclassOf(typeof(Delegate)))
                        typenodes[i] = GetDelegateInocation(generator, Generics[i]);
                    else typenodes[i] = generator.IdentifierName(Generics[i].FullName);
                }
                methodNameNode = generator.WithTypeArguments(methodNameNode, typenodes);
            }
            var argumentnodes = GetMethodArguments(info, generator);
            return generator.InvocationExpression(methodNameNode, argumentnodes);
        }
        private static SyntaxNode GetDelegateInocation(SyntaxGenerator generator, Type delegatetype)
        {

            string delegatename = delegatetype.Name;
            if (delegatetype.IsGenericType)
            {
                delegatename = delegatename.Substring(0, delegatename.Length - 2);
                var generic_type = delegatetype.GetGenericArguments()[0];
                return generator.WithTypeArguments(generator.IdentifierName(delegatename), generator.IdentifierName(generic_type.FullName));
            }
            else return generator.IdentifierName(delegatename);
        }
        /// <summary>
        /// Creates roslyn nodes for all all arguments inside a method info
        /// </summary>
        /// <param name="info"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        private static SyntaxNode[] GetMethodArguments(MethodInfo info, SyntaxGenerator generator)
        {
            int param_count = info.GetParameters().Length;
            var arguments = new SyntaxNode[param_count];
            for (int i = 0; i < param_count; i++)
            {
                arguments[i] = generator.NullLiteralExpression();
            }
            return arguments;
        }
        /// <summary>
        /// Creates method Declaration For roslyn class
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="methodbody">all the incocations that exist inside method</param>
        /// <returns></returns>
        private static SyntaxNode CreateAOTFixDeclaration(SyntaxGenerator generator, List<SyntaxNode> methodbody)
        {
            methodbody.Add(GetThrowExpression());
            return generator.MethodDeclaration("AOTFIX", accessibility: Accessibility.Private, statements: methodbody.ToArray());
        }
        /// <summary>
        /// Creates a roslyn throw expression
        /// </summary>
        /// <returns></returns>
        private static SyntaxNode GetThrowExpression()
        {
            return SyntaxFactory.ThrowStatement(
          SyntaxFactory.ObjectCreationExpression(
              SyntaxFactory.QualifiedName(
                  SyntaxFactory.IdentifierName("System"),
                  SyntaxFactory.IdentifierName("AccessViolationException")))
          .WithArgumentList(SyntaxFactory.ArgumentList()).NormalizeWhitespace());
        }
        /// <summary>
        /// Creates a roslyn class construct
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="members">all the members that exist in the class</param>
        /// <returns></returns>
        private static SyntaxNode ConstructClass(SyntaxGenerator generator, params SyntaxNode[] members)
        {

            var base_type = generator.IdentifierName("RawCall");
            var visualEventNameSpace = generator.NamespaceImportDeclaration("VisualDelegates");
            var SystemNamespace = generator.NamespaceImportDeclaration("System");
            var classConstruction = generator.ClassDeclaration("fixerclass", accessibility: Accessibility.Internal, baseType: base_type, members: members);
            var FullClass = generator.CompilationUnit(SystemNamespace, visualEventNameSpace, classConstruction).NormalizeWhitespace();
            return FullClass;
        }
        /// <summary>
        /// Creates a .cs file from the roslyn node
        /// </summary>
        /// <param name="AOTNode"></param>
        private static void GenerateFile(SyntaxNode AOTNode)
        {
            var writeDirectory = Path.Combine(Application.dataPath, "Scripts", "Generated");
            if (!Directory.Exists(writeDirectory))
                Directory.CreateDirectory(writeDirectory);

            string writePath = Path.Combine(writeDirectory, "AOT.cs");
            using (StreamWriter writer = new StreamWriter(writePath))
            {
                writer.WriteLine(AOTNode.GetText().ToString());
            }
            AssetDatabase.ImportAsset(Path.Combine("Assets", "Scripts", "Generated", "AOT.cs"), ImportAssetOptions.ForceUpdate);
        }
        internal class AOTsolver : IPreprocessBuildWithReport, IPostprocessBuildWithReport
        {
            public int callbackOrder => 1;
            bool success;
            DelegateAOT generator;
            public AOTsolver()
            {
                generator = GetSingleGenerator();
            }
            public void OnPreprocessBuild(BuildReport report)
            {
                if (generator.GenerateOnBuild)
                {
#if !UNITY_CLOUD_BUILD

                    try
                    {
                        AOTGeneration();
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("there was an error in the build!");
                        success = false;
                        throw new BuildFailedException(ex);
                    }
#else
                Debug.LogError("AOT code generation is not allowed in cloud builds");

#endif

                }
            }
            public void OnPostprocessBuild(BuildReport report)
            {
#if !UNITY_CLOUD_BUILD
                if (generator.GenerateOnBuild)
                {
                    if (success)
                        Debug.Log("<color=green> Delegate AOT Generation was Succesful</color>");
                    else Debug.Log("<color=red> Delegate AOT Generation failed </color>");
                }
#endif
            }
        }
    }
}