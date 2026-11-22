using FlatIronFields.Core;
using FlatIronFields.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace FlatIronFields.Game
{
    /// <summary>
    /// Applies the seasonal palette to the camera, farm tilemaps, HUD, and the
    /// 2D URP global volume. All changes are driven by explicit day/season
    /// transitions, not frame rate.
    /// </summary>
    public sealed class SeasonalPaletteSystem : MonoBehaviour
    {
        public Camera TargetCamera;
        public Tilemap[] Tilemaps;
        public Volume GlobalVolume;
        public UIManager UI;

        public Season AppliedSeason { get; private set; } = Season.Spring;

        private ColorAdjustments colorAdjustments;

        private void Awake()
        {
            CacheReferences();
        }

        public void Initialize(GameState state)
        {
            CacheReferences();
            Apply(state != null ? state.Season : Season.Spring);
        }

        public void Apply(Season season)
        {
            AppliedSeason = season;
            SeasonalPalette.Scheme scheme = SeasonalPalette.For(season);
            if (scheme == null) { return; }

            Camera camera = TargetCamera != null ? TargetCamera : Camera.main;
            if (camera != null) { camera.backgroundColor = scheme.Background; }

            if (Tilemaps != null)
            {
                foreach (Tilemap tilemap in Tilemaps)
                {
                    if (tilemap != null) { tilemap.color = scheme.TileTint; }
                }
            }

            if (UI != null && UI.SeasonText != null) { UI.SeasonText.color = scheme.HudTint; }

            if (colorAdjustments == null) { return; }
            colorAdjustments.postExposure.Override(scheme.PostExposure);
            colorAdjustments.contrast.Override(scheme.Contrast);
            colorAdjustments.saturation.Override(scheme.Saturation);
            colorAdjustments.colorFilter.Override(scheme.ColorFilter);
        }

        private void CacheReferences()
        {
            if (colorAdjustments != null) { return; }
            if (GlobalVolume == null || GlobalVolume.profile == null) { return; }
            if (GlobalVolume.profile.TryGet(out colorAdjustments)) { return; }
            colorAdjustments = null;
        }
    }
}
