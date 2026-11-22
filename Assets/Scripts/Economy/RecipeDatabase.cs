using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FlatIronFields.Economy
{
    public static class RecipeDatabase
    {
        private static readonly Dictionary<string, RecipeDefinition> Recipes = new Dictionary<string, RecipeDefinition>();

        public static void Initialize()
        {
            Recipes.Clear();
            RecipeDefinition[] loaded = Resources.LoadAll<RecipeDefinition>("Recipes");
            if (loaded == null || loaded.Length == 0)
            {
#if UNITY_EDITOR
                loaded = LoadFromAssetDatabase();
#endif
            }
            if (loaded == null) { return; }
            foreach (RecipeDefinition recipe in loaded)
            {
                if (recipe != null && !string.IsNullOrEmpty(recipe.Id)) { Recipes[recipe.Id] = recipe; }
            }
        }

        public static void Initialize(IEnumerable<RecipeDefinition> definitions)
        {
            Recipes.Clear();
            if (definitions == null) { return; }
            foreach (RecipeDefinition recipe in definitions)
            {
                if (recipe != null && !string.IsNullOrEmpty(recipe.Id)) { Recipes[recipe.Id] = recipe; }
            }
        }

        public static RecipeDefinition GetRecipe(string id)
        {
            RecipeDefinition recipe;
            if (string.IsNullOrEmpty(id)) { return null; }
            return Recipes.TryGetValue(id, out recipe) ? recipe : null;
        }

        public static void Clear()
        {
            Recipes.Clear();
        }

        public static IReadOnlyList<RecipeDefinition> All()
        {
            return Recipes.Values.ToList();
        }

#if UNITY_EDITOR
        private static RecipeDefinition[] LoadFromAssetDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:RecipeDefinition", new[] { "Assets/ScriptableObjects/Recipes" });
            var result = new List<RecipeDefinition>();
            foreach (string guid in guids)
            {
                RecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<RecipeDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (recipe != null) { result.Add(recipe); }
            }
            return result.ToArray();
        }
#endif
    }
}
