using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mint.Gdk.Utilities.Editor
{
    [InitializeOnLoad]
    public class AutoReserialize
    {
        static AutoReserialize()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterReloadDomain;
        }

        private static void OnAfterReloadDomain()
        {
            // Find all MonoBehaviour with ReserializeAttribute
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(MonoBehaviour)) &&
                               Attribute.IsDefined(type, typeof(ReserializeAttribute)));

            foreach (var type in types)
            {
                Debug.Log("Reserialize " + type.ToString());
                ReserializePrefab(type);
            }
        }

        private static void ReserializePrefab(Type monoType)
        {
            // Find all MonoBehaviour types with ReserializeAttribute
            Type targetType = monoType;

            List<GameObject> prefabNeedToReSerialize = null;

            if (targetType == null) return;
            prefabNeedToReSerialize = FindPrefabsWithSelectedComponent(targetType);

            if (prefabNeedToReSerialize == null) return;
            IEnumerable<string> ieReserialize = prefabNeedToReSerialize.Select(x => AssetDatabase.GetAssetPath(x));
            AssetDatabase.ForceReserializeAssets(ieReserialize);
        }

        private static List<GameObject> FindPrefabsWithSelectedComponent(Type type)
        {
            string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            List<GameObject> prefabsWithComponent = new List<GameObject>();

            foreach (var prefabGUID in allPrefabs)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab.GetComponentInChildren(type) != null)
                {
                    prefabsWithComponent.Add(prefab);
                }
            }
            return prefabsWithComponent;
        }
    }
}
