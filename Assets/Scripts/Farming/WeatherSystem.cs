using FlatIronFields.Core;
using FlatIronFields.Crops;
using UnityEngine;

namespace FlatIronFields.Farming
{
    /// <summary>
    /// Applies deterministic weather and season modifiers to daily farming growth.
    /// No frame-rate dependence: every effect is keyed off the explicit day advance.
    /// </summary>
    public sealed class WeatherSystem
    {
        /// <summary>Daily water change applied to planted plots before growth.</summary>
        public int DailyWaterDelta(Weather weather)
        {
            switch (weather)
            {
                case Weather.Rainy: return 2;
                case Weather.Cloudy: return 0;
                default: return -1;
            }
        }

        /// <summary>
        /// Deterministic winter dormancy: crops only grow on even days in winter.
        /// Other seasons grow every day.
        /// </summary>
        public bool CanGrow(Season season, int day)
        {
            if (season != Season.Winter) { return true; }
            return day % 2 == 0;
        }

        /// <summary>
        /// Quality nudge from favourable weather. Rainy days keep foliage healthy,
        /// cloud shields against scorching sun.
        /// </summary>
        public int QualityModifier(Weather weather)
        {
            if (weather == Weather.Rainy || weather == Weather.Cloudy) { return 1; }
            return 0;
        }

        /// <summary>
        /// Apply water changes to all planted plots and advance those that can grow.
        /// </summary>
        public void ApplyDay(GameState state, FarmGrid grid, CropSystem crops)
        {
            if (state == null || state.Farm == null || state.Farm.Plots == null) { return; }

            int delta = DailyWaterDelta(state.Weather);
            bool grow = CanGrow(state.Season, state.Day);

            foreach (Plot plot in state.Farm.Plots)
            {
                if (plot == null || string.IsNullOrEmpty(plot.CropId) || plot.Ready) { continue; }

                if (delta > 0)
                {
                    plot.Water = Mathf.Clamp(plot.Water + delta, 0, 4);
                }
                else if (delta < 0 && plot.State != PlotState.Wild)
                {
                    plot.Water = Mathf.Max(0, plot.Water + delta);
                }

                if (grid != null && crops != null && grow && plot.Water > 0)
                {
                    if (crops.IsInSeason(plot.CropId, state.Season)) { crops.Advance(plot, 1); }
                }
            }
        }
    }
}
