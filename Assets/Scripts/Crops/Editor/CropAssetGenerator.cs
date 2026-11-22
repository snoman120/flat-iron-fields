using System.Collections.Generic;
using FlatIronFields.Core;
using UnityEditor;
using UnityEngine;

namespace FlatIronFields.Crops
{
    public static class CropAssetGenerator
    {
        private sealed class CropData
        {
            public string Id;
            public string DisplayName;
            public Season[] Seasons;
            public int GrowthDays;
            public int YieldMin;
            public int YieldMax;
            public int SeedCost;
            public int SalePrice;
            public bool MultipleHarvest;
            public int UnlockedAtLevel;
        }

        private static readonly Season[] AllSeasons = { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };
        private static readonly Season[] WarmSeasons = { Season.Spring, Season.Summer };
        private static readonly Season[] CoolSeasons = { Season.Autumn, Season.Spring };

        private static readonly List<CropData> Data = new List<CropData>
        {
            new CropData { Id="grass", DisplayName="Grass", Seasons=AllSeasons, GrowthDays=2, YieldMin=1, YieldMax=2, SeedCost=2, SalePrice=4, MultipleHarvest=true, UnlockedAtLevel=1 },
            new CropData { Id="cabbage", DisplayName="Cabbage", Seasons=CoolSeasons, GrowthDays=4, YieldMin=1, YieldMax=3, SeedCost=8, SalePrice=18, UnlockedAtLevel=1 },
            new CropData { Id="wheat", DisplayName="Wheat", Seasons=WarmSeasons, GrowthDays=5, YieldMin=2, YieldMax=4, SeedCost=10, SalePrice=22, UnlockedAtLevel=1 },
            new CropData { Id="carrot", DisplayName="Carrot", Seasons=CoolSeasons, GrowthDays=4, YieldMin=2, YieldMax=4, SeedCost=9, SalePrice=20, UnlockedAtLevel=1 },
            new CropData { Id="sweetpotato", DisplayName="Sweet Potato", Seasons=WarmSeasons, GrowthDays=5, YieldMin=2, YieldMax=4, SeedCost=12, SalePrice=26, UnlockedAtLevel=1 },
            new CropData { Id="potato", DisplayName="Potato", Seasons=CoolSeasons, GrowthDays=5, YieldMin=2, YieldMax=5, SeedCost=12, SalePrice=26, UnlockedAtLevel=1 },
            new CropData { Id="corn", DisplayName="Corn", Seasons=WarmSeasons, GrowthDays=6, YieldMin=2, YieldMax=4, SeedCost=14, SalePrice=30, UnlockedAtLevel=2 },
            new CropData { Id="pepper", DisplayName="Pepper", Seasons=WarmSeasons, GrowthDays=6, YieldMin=2, YieldMax=4, SeedCost=15, SalePrice=32, UnlockedAtLevel=2 },
            new CropData { Id="tomato", DisplayName="Tomato", Seasons=WarmSeasons, GrowthDays=6, YieldMin=3, YieldMax=5, SeedCost=16, SalePrice=34, MultipleHarvest=true, UnlockedAtLevel=2 },
            new CropData { Id="watermelon", DisplayName="Watermelon", Seasons=new[]{Season.Summer}, GrowthDays=8, YieldMin=1, YieldMax=2, SeedCost=24, SalePrice=60, UnlockedAtLevel=2 },
            new CropData { Id="pumpkin", DisplayName="Pumpkin", Seasons=new[]{Season.Autumn}, GrowthDays=8, YieldMin=1, YieldMax=3, SeedCost=22, SalePrice=56, UnlockedAtLevel=2 },
            new CropData { Id="onion", DisplayName="Onion", Seasons=CoolSeasons, GrowthDays=5, YieldMin=2, YieldMax=4, SeedCost=11, SalePrice=24, UnlockedAtLevel=2 },
            new CropData { Id="garlic", DisplayName="Garlic", Seasons=new[]{Season.Spring}, GrowthDays=5, YieldMin=2, YieldMax=3, SeedCost=13, SalePrice=28, UnlockedAtLevel=2 },
            new CropData { Id="strawberry", DisplayName="Strawberry", Seasons=new[]{Season.Spring}, GrowthDays=7, YieldMin=2, YieldMax=4, SeedCost=18, SalePrice=40, MultipleHarvest=true, UnlockedAtLevel=3 },
            new CropData { Id="apple", DisplayName="Apple", Seasons=new[]{Season.Autumn}, GrowthDays=10, YieldMin=2, YieldMax=4, SeedCost=26, SalePrice=52, MultipleHarvest=true, UnlockedAtLevel=3 },
            new CropData { Id="peach", DisplayName="Peach", Seasons=new[]{Season.Summer}, GrowthDays=10, YieldMin=2, YieldMax=4, SeedCost=28, SalePrice=56, MultipleHarvest=true, UnlockedAtLevel=3 },
            new CropData { Id="plum", DisplayName="Plum", Seasons=new[]{Season.Autumn}, GrowthDays=9, YieldMin=2, YieldMax=4, SeedCost=25, SalePrice=50, MultipleHarvest=true, UnlockedAtLevel=3 },
            new CropData { Id="grape", DisplayName="Grape", Seasons=new[]{Season.Summer, Season.Autumn}, GrowthDays=10, YieldMin=2, YieldMax=5, SeedCost=30, SalePrice=60, MultipleHarvest=true, UnlockedAtLevel=3 },
            new CropData { Id="cherry", DisplayName="Cherry", Seasons=new[]{Season.Spring}, GrowthDays=11, YieldMin=2, YieldMax=4, SeedCost=32, SalePrice=66, MultipleHarvest=true, UnlockedAtLevel=4 },
            new CropData { Id="orange", DisplayName="Orange", Seasons=new[]{Season.Winter}, GrowthDays=11, YieldMin=2, YieldMax=4, SeedCost=30, SalePrice=62, MultipleHarvest=true, UnlockedAtLevel=4 },
            new CropData { Id="lemon", DisplayName="Lemon", Seasons=new[]{Season.Winter}, GrowthDays=11, YieldMin=2, YieldMax=4, SeedCost=30, SalePrice=62, MultipleHarvest=true, UnlockedAtLevel=4 },
            new CropData { Id="blueberry", DisplayName="Blueberry", Seasons=new[]{Season.Summer}, GrowthDays=8, YieldMin=2, YieldMax=4, SeedCost=22, SalePrice=48, MultipleHarvest=true, UnlockedAtLevel=4 },
            new CropData { Id="blackberry", DisplayName="Blackberry", Seasons=new[]{Season.Summer, Season.Autumn}, GrowthDays=8, YieldMin=2, YieldMax=5, SeedCost=24, SalePrice=50, MultipleHarvest=true, UnlockedAtLevel=4 },
            new CropData { Id="raspberry", DisplayName="Raspberry", Seasons=new[]{Season.Summer}, GrowthDays=8, YieldMin=2, YieldMax=4, SeedCost=23, SalePrice=48, MultipleHarvest=true, UnlockedAtLevel=4 },
            new CropData { Id="goldenapple", DisplayName="Golden Apple", Seasons=AllSeasons, GrowthDays=14, YieldMin=1, YieldMax=2, SeedCost=60, SalePrice=180, MultipleHarvest=true, UnlockedAtLevel=5 },
            new CropData { Id="moonberry", DisplayName="Moonberry", Seasons=new[]{Season.Winter}, GrowthDays=16, YieldMin=1, YieldMax=2, SeedCost=80, SalePrice=240, MultipleHarvest=true, UnlockedAtLevel=6 }
        };

        [MenuItem("Flat Iron Fields/Generate Crop Assets")]
        public static void GenerateAll()
        {
            EnsureFolder("Assets/ScriptableObjects");
            EnsureFolder("Assets/ScriptableObjects/Crops");

            foreach (CropData data in Data)
            {
                string path = "Assets/ScriptableObjects/Crops/" + data.Id + ".asset";
                CropDefinition asset = AssetDatabase.LoadAssetAtPath<CropDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<CropDefinition>();
                    AssetDatabase.CreateAsset(asset, path);
                }

                asset.Id = data.Id;
                asset.DisplayName = data.DisplayName;
                asset.Seasons = data.Seasons;
                asset.GrowthDays = data.GrowthDays;
                asset.YieldMin = data.YieldMin;
                asset.YieldMax = data.YieldMax;
                asset.SeedCost = data.SeedCost;
                asset.SalePrice = data.SalePrice;
                asset.MultipleHarvest = data.MultipleHarvest;
                asset.UnlockedAtLevel = data.UnlockedAtLevel;
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Flat Iron Fields: generated " + Data.Count + " crop assets.");
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
