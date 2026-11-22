using FlatIronFields.Core;
using FlatIronFields.Game;
using NUnit.Framework;

namespace FlatIronFields.Tests
{
    public sealed class Phase2PolishTests
    {
        [Test]
        public void SeasonalPalette_AllSeasonsHaveDistinctReadableSchemes()
        {
            SeasonalPalette.Scheme spring = SeasonalPalette.For(Season.Spring);
            SeasonalPalette.Scheme summer = SeasonalPalette.For(Season.Summer);
            SeasonalPalette.Scheme autumn = SeasonalPalette.For(Season.Autumn);
            SeasonalPalette.Scheme winter = SeasonalPalette.For(Season.Winter);

            Assert.IsNotNull(spring);
            Assert.IsNotNull(summer);
            Assert.IsNotNull(autumn);
            Assert.IsNotNull(winter);
            Assert.AreNotEqual(spring.Background, summer.Background);
            Assert.AreNotEqual(summer.Background, autumn.Background);
            Assert.AreNotEqual(autumn.Background, winter.Background);
            Assert.Greater(spring.Saturation, winter.Saturation);
            Assert.Less(winter.PostExposure, summer.PostExposure);
        }

        [Test]
        public void SeasonalPalette_UnknownSeasonFallsBackToSpring()
        {
            SeasonalPalette.Scheme scheme = SeasonalPalette.For((Season)99);
            Assert.IsNotNull(scheme);
            Assert.Greater(scheme.Saturation, 0f);
        }
    }
}
