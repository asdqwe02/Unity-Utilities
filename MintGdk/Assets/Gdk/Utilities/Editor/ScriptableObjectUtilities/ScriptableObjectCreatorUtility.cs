using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Mep.Gdk.Utilities.Editor
{
    /// <summary>
    /// Utility for creating ScriptableObject assets from script files via right-click context menu
    /// </summary>
    public static class ScriptableObjectCreatorUtility
    {
        private const string MENU_ITEM_NAME = "Create ScriptableObject Asset";
        private const int MENU_PRIORITY = 81; // After "Reimport" (80)

        [MenuItem("Assets/" + MENU_ITEM_NAME, true)]
        private static bool ValidateCreateScriptableObjectAsset()
        {
            // Only show menu item if a single script file is selected
            if (Selection.objects.Length != 1)
                return false;

            var selectedObject = Selection.activeObject;
            if (selectedObject == null)
                return false;

            // Check if it's a script file
            var assetPath = AssetDatabase.GetAssetPath(selectedObject);
            if (!assetPath.EndsWith(".cs"))
                return false;

            // Check if the script has CreateAssetMenu attribute and inherits from ScriptableObject
            var scriptInfo = GetScriptableObjectInfo(assetPath);
            return scriptInfo != null;
        }

        [MenuItem("Assets/" + MENU_ITEM_NAME, false, MENU_PRIORITY)]
        private static void CreateScriptableObjectAsset()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null)
                return;

            var assetPath = AssetDatabase.GetAssetPath(selectedObject);
            var scriptInfo = GetScriptableObjectInfo(assetPath);

            if (scriptInfo == null)
            {
                EditorUtility.DisplayDialog("Error", "Selected script does not inherit from ScriptableObject or does not have CreateAssetMenu attribute.", "OK");
                return;
            }

            CreateAssetFromScriptInfo(scriptInfo, assetPath);
        }

        private static ScriptableObjectInfo GetScriptableObjectInfo(string scriptPath)
        {
            try
            {
                // Get the script asset
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
                if (script == null)
                    return null;

                // Get the type from the script
                var scriptClass = script.GetClass();
                if (scriptClass == null)
                    return null;

                // Check if it inherits from ScriptableObject
                if (!typeof(ScriptableObject).IsAssignableFrom(scriptClass))
                    return null;

                // Check for CreateAssetMenu attribute
                var createAssetMenuAttribute = scriptClass.GetCustomAttribute<CreateAssetMenuAttribute>();
                if (createAssetMenuAttribute == null)
                    return null;

                return new ScriptableObjectInfo
                {
                    ScriptClass = scriptClass,
                    CreateAssetMenuAttribute = createAssetMenuAttribute,
                    ScriptPath = scriptPath
                };
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to analyze script {scriptPath}: {ex.Message}");
                return null;
            }
        }

        private static void CreateAssetFromScriptInfo(ScriptableObjectInfo scriptInfo, string scriptPath)
        {
            try
            {
                // Create the ScriptableObject instance
                var instance = ScriptableObject.CreateInstance(scriptInfo.ScriptClass);
                if (instance == null)
                {
                    EditorUtility.DisplayDialog("Error", "Failed to create ScriptableObject instance.", "OK");
                    return;
                }

                // Determine the asset name and path
                var fileName = GetAssetFileName(scriptInfo);
                var assetPath = GetAssetPath(scriptPath, fileName);

                // Ensure the asset path is unique
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

                // Create the asset
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Select the created asset
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = instance;

                Debug.Log($"Created ScriptableObject asset: {assetPath}");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create ScriptableObject asset: {ex.Message}", "OK");
                Debug.LogError($"Failed to create ScriptableObject asset: {ex}");
            }
        }

        private static string GetAssetFileName(ScriptableObjectInfo scriptInfo)
        {
            var attribute = scriptInfo.CreateAssetMenuAttribute;

            // Use fileName from attribute if specified
            if (!string.IsNullOrEmpty(attribute.fileName))
            {
                var fileName = attribute.fileName;
                // Ensure .asset extension
                if (!fileName.EndsWith(".asset"))
                    fileName += ".asset";
                return fileName;
            }

            // Fallback to class name if no fileName specified
            var className = scriptInfo.ScriptClass.Name;
            return $"{className}.asset";
        }

        private static string GetAssetPath(string scriptPath, string fileName)
        {
            // Try to place the asset in the same directory as the script
            var scriptDirectory = Path.GetDirectoryName(scriptPath);

            // If script is in an Editor folder, try to place asset in parent or appropriate folder
            if (scriptDirectory.Contains("Editor"))
            {
                // Try to find a non-Editor folder
                var parentDirectory = Directory.GetParent(scriptDirectory)?.FullName;
                if (parentDirectory != null)
                {
                    var relativePath = GetRelativeAssetPath(parentDirectory);
                    if (!string.IsNullOrEmpty(relativePath))
                    {
                        return Path.Combine(relativePath, fileName).Replace('\\', '/');
                    }
                }

                // Fallback to Assets root
                return $"Assets/{fileName}";
            }

            return Path.Combine(scriptDirectory, fileName).Replace('\\', '/');
        }

        private static string GetRelativeAssetPath(string absolutePath)
        {
            var dataPath = Application.dataPath;
            if (absolutePath.StartsWith(dataPath))
            {
                return "Assets" + absolutePath.Substring(dataPath.Length).Replace('\\', '/');
            }
            return null;
        }

        #region Context Menu Enhancement

        [MenuItem("Assets/Create ScriptableObject from Selected Script", true)]
        private static bool ValidateCreateScriptableObjectFromScript()
        {
            return ValidateCreateScriptableObjectAsset();
        }

        [MenuItem("Assets/Create ScriptableObject from Selected Script", false, 19)]
        private static void CreateScriptableObjectFromScript()
        {
            CreateScriptableObjectAsset();
        }

        #endregion

        #region Asset Creation Info Dialog

        [MenuItem("Assets/ScriptableObject Info", true)]
        private static bool ValidateShowScriptableObjectInfo()
        {
            return ValidateCreateScriptableObjectAsset();
        }

        [MenuItem("Assets/ScriptableObject Info", false, MENU_PRIORITY + 1)]
        private static void ShowScriptableObjectInfo()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null)
                return;

            var assetPath = AssetDatabase.GetAssetPath(selectedObject);
            var scriptInfo = GetScriptableObjectInfo(assetPath);

            if (scriptInfo == null)
                return;

            var attribute = scriptInfo.CreateAssetMenuAttribute;
            var info = $"ScriptableObject Info:\n\n" +
                      $"Class: {scriptInfo.ScriptClass.Name}\n" +
                      $"Namespace: {scriptInfo.ScriptClass.Namespace ?? "None"}\n" +
                      $"Menu Name: {attribute.menuName ?? "None"}\n" +
                      $"File Name: {attribute.fileName ?? "Auto-generated"}\n" +
                      $"Order: {attribute.order}\n\n" +
                      $"Asset will be created as: {GetAssetFileName(scriptInfo)}";

            EditorUtility.DisplayDialog("ScriptableObject Info", info, "OK");
        }

        #endregion

        #region Data Structures

        private class ScriptableObjectInfo
        {
            public Type ScriptClass { get; set; }
            public CreateAssetMenuAttribute CreateAssetMenuAttribute { get; set; }
            public string ScriptPath { get; set; }
        }

        #endregion
    }
}