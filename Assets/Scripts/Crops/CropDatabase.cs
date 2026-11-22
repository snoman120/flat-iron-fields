using System.Collections.Generic;
using System.Linq;
using FlatIronFields.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FlatIronFields.Crops
{
    public static class CropDatabase
    {
        private static readonly Dictionary<string, CropDefinition> Crops = new Dictionary<string, CropDefinition>();

        public static int Count { get { return Crops.Count; } }

        public static void Initialize()
        {
            Crops.Clear();
            CropDefinition[] loaded = Resources.LoadAll<CropDefinition>("Crops");
            if (loaded == null || loaded.Length == 0)
            {
#if UNITY_EDITOR
                loaded = LoadFromAssetDatabase();
#endif
            }
            if (loaded != null)
            {
                foreach (CropDefinition crop in loaded)
                {
                    if (crop != null && !string.IsNullOrEmpty(crop.Id)) { AddCrop(crop); }
                }
            }
            if (Crops.Count == 0) { LoadFallbackCrops(); }
        }

        public static void Initialize(IEnumerable<CropDefinition> definitions)
        {
            Crops.Clear();
            if (definitions == null) { return; }
            foreach (CropDefinition crop in definitions)
            {
                if (crop != null && !string.IsNullOrEmpty(crop.Id)) { AddCrop(crop); }
            }
        }

        public static void AddCrop(CropDefinition crop)
        {
            if (crop == null || string.IsNullOrEmpty(crop.Id)) { return; }
            Crops[crop.Id] = crop;
        }

        public static void RemoveCrop(string id)
        {
            if (!string.IsNullOrEmpty(id)) { Crops.Remove(id); }
        }

        public static CropDefinition GetCrop(string id)
        {
            CropDefinition crop;
            if (string.IsNullOrEmpty(id)) { return null; }
            return Crops.TryGetValue(id, out crop) ? crop : null;
        }

        public static bool TryGetCrop(string id, out CropDefinition crop)
        {
            if (string.IsNullOrEmpty(id)) { crop = null; return false; }
            return Crops.TryGetValue(id, out crop);
        }

        public static IReadOnlyList<CropDefinition> All()
        {
            return Crops.Values.ToList();
        }

        public static void Clear()
        {
            Crops.Clear();
        }

        private static void LoadFallbackCrops()
        {
            Season[] all = { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };
            Season[] warm = { Season.Spring, Season.Summer };
            Season[] cool = { Season.Autumn, Season.Spring };

            AddFallback("grass", "Grass", all, 2, 1, 2, 2, 4, true, 1);
            AddFallback("cabbage", "Cabbage", cool, 4, 1, 3, 8, 18, false, 1);
            AddFallback("wheat", "Wheat", warm, 5, 2, 4, 10, 22, false, 1);
            AddFallback("carrot", "Carrot", cool, 4, 2, 4, 9, 20, false, 1);
            AddFallback("sweetpotato", "Sweet Potato", warm, 5, 2, 4, 12, 26, false, 1);
            AddFallback("potato", "Potato", cool, 5, 2, 5, 12, 26, false, 1);
            AddFallback("corn", "Corn", warm, 6, 2, 4, 14, 30, false, 2);
            AddFallback("pepper", "Pepper", warm, 6, 2, 4, 15, 32, false, 2);
            AddFallback("tomato", "Tomato", warm, 6, 3, 5, 16, 34, true, 2);
            AddFallback("watermelon", "Watermelon", new[] { Season.Summer }, 8, 1, 2, 24, 60, false, 2);
            AddFallback("pumpkin", "Pumpkin", new[] { Season.Autumn }, 8, 1, 3, 22, 56, false, 2);
            AddFallback("onion", "Onion", cool, 5, 2, 4, 11, 24, false, 2);
            AddFallback("garlic", "Garlic", new[] { Season.Spring }, 5, 2, 3, 13, 28, false, 2);
            AddFallback("strawberry", "Strawberry", new[] { Season.Spring }, 7, 2, 4, 18, 40, true, 3);
            AddFallback("apple", "Apple", new[] { Season.Autumn }, 10, 2, 4, 26, 52, true, 3);
            AddFallback("peach", "Peach", new[] { Season.Summer }, 10, 2, 4, 28, 56, true, 3);
            AddFallback("plum", "Plum", new[] { Season.Autumn }, 9, 2, 4, 25, 50, true, 3);
            AddFallback("grape", "Grape", new[] { Season.Summer, Season.Autumn }, 10, 2, 5, 30, 60, true, 3);
            AddFallback("cherry", "Cherry", new[] { Season.Spring }, 11, 2, 4, 32, 66, true, 4);
            AddFallback("orange", "Orange", new[] { Season.Winter }, 11, 2, 4, 30, 62, true, 4);
            AddFallback("lemon", "Lemon", new[] { Season.Winter }, 11, 2, 4, 30, 62, true, 4);
            AddFallback("blueberry", "Blueberry", new[] { Season.Summer }, 8, 2, 4, 22, 48, true, 4);
            AddFallback("blackberry", "Blackberry", new[] { Season.Summer, Season.Autumn }, 8, 2, 5, 24, 50, true, 4);
            AddFallback("raspberry", "Raspberry", new[] { Season.Summer }, 8, 2, 4, 23, 48, true, 4);
            AddFallback("goldenapple", "Golden Apple", all, 14, 1, 2, 60, 180, true, 5);
            AddFallback("moonberry", "Moonberry", new[] { Season.Winter }, 16, 1, 2, 80, 240, true, 6);
        }

        private static void AddFallback(string id, string name, Season[] seasons, int days, int min, int max,
                                        int seedCost, int salePrice, bool multiple, int unlock)
        {
            var crop = ScriptableObject.CreateInstance<CropDefinition>();
            crop.Id = id;
            crop.DisplayName = name;
            crop.Seasons = seasons;
            crop.GrowthDays = days;
            crop.YieldMin = min;
            crop.YieldMax = max;
            crop.SeedCost = seedCost;
            crop.SalePrice = salePrice;
            crop.MultipleHarvest = multiple;
            crop.UnlockedAtLevel = unlock;
            AddCrop(crop);
        }

#if UNITY_EDITOR
        private static CropDefinition[] LoadFromAssetDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:CropDefinition", new[] { "Assets/ScriptableObjects/Crops" });
            var result = new List<CropDefinition>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CropDefinition crop = AssetDatabase.LoadAssetAtPath<CropDefinition>(path);
                if (crop != null) { result.Add(crop); }
            }
            return result.ToArray();
        }
#endif
    }
}
