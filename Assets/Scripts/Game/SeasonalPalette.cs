using FlatIronFields.Core;
using UnityEngine;

namespace FlatIronFields.Game
{
    /// <summary>
    /// Static seasonal color tuning. Kept separate from the MonoBehaviour so the
    /// values can be covered by deterministic EditMode tests.
    /// </summary>
    public static class SeasonalPalette
    {
        public sealed class Scheme
        {
            public Color Background;
            public Color TileTint;
            public Color HudTint;
            public float PostExposure;
            public float Contrast;
            public float Saturation;
            public Color ColorFilter;
        }

        public static Scheme For(Season season)
        {
            switch (season)
            {
                case Season.Summer:
                    return new Scheme
                    {
                        Background = new Color(0.62f, 0.76f, 0.36f),
                        TileTint = new Color(1.04f, 1.02f, 0.90f),
                        HudTint = new Color(1.00f, 0.94f, 0.62f),
                        PostExposure = 0.05f,
                        Contrast = 4f,
                        Saturation = 10f,
                        ColorFilter = new Color(1.00f, 0.98f, 0.86f)
                    };
                case Season.Autumn:
                    return new Scheme
                    {
                        Background = new Color(0.58f, 0.45f, 0.25f),
                        TileTint = new Color(1.08f, 0.88f, 0.68f),
                        HudTint = new Color(1.00f, 0.72f, 0.42f),
                        PostExposure = -0.02f,
                        Contrast = 6f,
                        Saturation = -5f,
                        ColorFilter = new Color(1.00f, 0.86f, 0.68f)
                    };
                case Season.Winter:
                    return new Scheme
                    {
                        Background = new Color(0.70f, 0.78f, 0.85f),
                        TileTint = new Color(0.82f, 0.90f, 1.00f),
                        HudTint = new Color(0.74f, 0.88f, 1.00f),
                        PostExposure = -0.05f,
                        Contrast = 2f,
                        Saturation = -20f,
                        ColorFilter = new Color(0.82f, 0.90f, 1.00f)
                    };
                default:
                    return new Scheme
                    {
                        Background = new Color(0.50f, 0.68f, 0.35f),
                        TileTint = Color.white,
                        HudTint = new Color(0.72f, 1.00f, 0.68f),
                        PostExposure = 0f,
                        Contrast = 2f,
                        Saturation = 5f,
                        ColorFilter = new Color(0.92f, 1.00f, 0.90f)
                    };
            }
        }
    }
}
