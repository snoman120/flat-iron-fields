using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Economy;
using FlatIronFields.Progression;
using UnityEditor;
using UnityEngine;

namespace FlatIronFields.Tractors
{
    public static class SystemAssetGenerator
    {
        [MenuItem("Flat Iron Fields/Generate System Assets")]
        public static void GenerateAll()
        {
            EnsureFolder("Assets/ScriptableObjects/Tractors");
            EnsureFolder("Assets/ScriptableObjects/Tractors/Attachments");
            EnsureFolder("Assets/ScriptableObjects/Skills");
            EnsureFolder("Assets/ScriptableObjects/Recipes");

            GenerateTractors();
            GenerateAttachments();
            GenerateSkills();
            GenerateRecipes();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Flat Iron Fields: generated tractor, attachment, skill, and recipe assets.");
        }

        private static void GenerateTractors()
        {
            var tractors = new List<TractorDefinition>
            {
                MakeTractor("basic", "Basic Tractor", 3f, 10, 0, 1),
                MakeTractor("crop", "Crop Tractor", 4f, 20, 300, 2),
                MakeTractor("heavyduty", "Heavy Duty", 2.5f, 40, 800, 3),
                MakeTractor("specialized", "Specialized", 5f, 15, 1200, 4)
            };
            foreach (TractorDefinition tractor in tractors)
            {
                Save("Assets/ScriptableObjects/Tractors/" + tractor.Id + ".asset", tractor);
            }
        }

        private static TractorDefinition MakeTractor(string id, string name, float speed, int capacity, int cost, int unlock)
        {
            var tractor = ScriptableObject.CreateInstance<TractorDefinition>();
            tractor.Id = id;
            tractor.DisplayName = name;
            tractor.Speed = speed;
            tractor.CargoCapacity = capacity;
            tractor.Cost = cost;
            tractor.UnlockedAtLevel = unlock;
            tractor.FuelCapacity = 100;
            return tractor;
        }

        private static void GenerateAttachments()
        {
            var ids = new[] { "tillage", "planter", "harvester", "transporter", "watering", "fertilizer" };
            var names = new[] { "Tillage", "Planter", "Harvester", "Transporter", "Watering", "Fertilizer" };
            var costs = new[] { 1, 1, 2, 1, 1, 2 };
            for (int i = 0; i < ids.Length; i++)
            {
                var attachment = ScriptableObject.CreateInstance<AttachmentDefinition>();
                attachment.Id = ids[i];
                attachment.DisplayName = names[i];
                attachment.FuelCost = costs[i];
                Save("Assets/ScriptableObjects/Tractors/Attachments/" + ids[i] + ".asset", attachment);
            }
        }

        private static void GenerateSkills()
        {
            var ids = new[] { "planting", "harvesting", "cropSelection", "resourceManagement", "tractorOperation", "farmExpansion" };
            var names = new[] { "Planting", "Harvesting", "Crop Selection", "Resource Management", "Tractor Operation", "Farm Expansion" };
            for (int i = 0; i < ids.Length; i++)
            {
                var skill = ScriptableObject.CreateInstance<SkillDefinition>();
                skill.Id = ids[i];
                skill.DisplayName = names[i];
                skill.Description = "Improves " + names[i].ToLowerInvariant() + " performance.";
                skill.UnlockedAtLevel = 1;
                Save("Assets/ScriptableObjects/Skills/" + ids[i] + ".asset", skill);
            }
        }

        private static void GenerateRecipes()
        {
            MakeRecipe("cabbage_good", "Better Cabbage", "cabbage", Quality.Normal, 3, "cabbage", Quality.Good, 1, 5);
            MakeRecipe("wheat_good", "Better Wheat", "wheat", Quality.Normal, 3, "wheat", Quality.Good, 1, 5);
        }

        private static void MakeRecipe(string id, string name, string inputCrop, Quality inputQuality, int inputQty, string outputCrop, Quality outputQuality, int outputQty, int cost)
        {
            var recipe = ScriptableObject.CreateInstance<RecipeDefinition>();
            recipe.Id = id;
            recipe.DisplayName = name;
            recipe.InputCropId = inputCrop;
            recipe.InputQuality = inputQuality;
            recipe.InputQuantity = inputQty;
            recipe.OutputCropId = outputCrop;
            recipe.OutputQuality = outputQuality;
            recipe.OutputQuantity = outputQty;
            recipe.CoinCost = cost;
            Save("Assets/ScriptableObjects/Recipes/" + id + ".asset", recipe);
        }

        private static void Save<T>(string path, T asset) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                EditorUtility.CopySerialized(asset, existing);
                Object.DestroyImmediate(asset);
                EditorUtility.SetDirty(existing);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) { return; }
            string parent = path.Substring(0, path.LastIndexOf('/'));
            string name = path.Substring(path.LastIndexOf('/') + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
