using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FlatIronFields.Tractors
{
    public static class TractorDatabase
    {
        private static readonly Dictionary<string, TractorDefinition> Tractors = new Dictionary<string, TractorDefinition>();

        public static void Initialize()
        {
            Tractors.Clear();
            TractorDefinition[] loaded = Resources.LoadAll<TractorDefinition>("Tractors");
            if (loaded == null || loaded.Length == 0)
            {
#if UNITY_EDITOR
                loaded = LoadFromAssetDatabase();
#endif
            }
            if (loaded == null) { return; }
            foreach (TractorDefinition tractor in loaded)
            {
                if (tractor != null && !string.IsNullOrEmpty(tractor.Id)) { Tractors[tractor.Id] = tractor; }
            }
        }

        public static void Initialize(IEnumerable<TractorDefinition> definitions)
        {
            Tractors.Clear();
            if (definitions == null) { return; }
            foreach (TractorDefinition tractor in definitions)
            {
                if (tractor != null && !string.IsNullOrEmpty(tractor.Id)) { Tractors[tractor.Id] = tractor; }
            }
        }

        public static TractorDefinition GetTractor(string id)
        {
            TractorDefinition tractor;
            if (string.IsNullOrEmpty(id)) { return null; }
            return Tractors.TryGetValue(id, out tractor) ? tractor : null;
        }

        public static void Clear()
        {
            Tractors.Clear();
        }

        public static IReadOnlyList<TractorDefinition> All()
        {
            return Tractors.Values.ToList();
        }

#if UNITY_EDITOR
        private static TractorDefinition[] LoadFromAssetDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:TractorDefinition", new[] { "Assets/ScriptableObjects/Tractors" });
            var result = new List<TractorDefinition>();
            foreach (string guid in guids)
            {
                TractorDefinition tractor = AssetDatabase.LoadAssetAtPath<TractorDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (tractor != null) { result.Add(tractor); }
            }
            return result.ToArray();
        }
#endif
    }
}
