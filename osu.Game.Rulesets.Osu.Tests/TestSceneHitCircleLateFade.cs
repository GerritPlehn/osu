// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneHitCircleLateFade : OsuTestScene
    {
        private float? alphaAtMiss;

        [Test]
        public void TestHitCircleClassicModMiss()
        {
            AddStep("Create hit circle", () =>
            {
                SelectedMods.Value = new Mod[] { new OsuModClassic() };
                createCircle();
            });

            AddUntilStep("Wait until circle is missed", () => alphaAtMiss.IsNotNull());
            AddAssert("Transparent when missed", () => alphaAtMiss == 0);
        }

        [Test]
        public void TestHitCircleClassicAndFullHiddenMods()
        {
            AddStep("Create hit circle", () =>
            {
                SelectedMods.Value = new Mod[] { new OsuModHidden(), new OsuModClassic() };
                createCircle();
            });

            AddUntilStep("Wait until circle is missed", () => alphaAtMiss.IsNotNull());
            AddAssert("Transparent when missed", () => alphaAtMiss == 0);
        }

        [Test]
        public void TestHitCircleClassicAndApproachCircleOnlyHiddenMods()
        {
            AddStep("Create hit circle", () =>
            {
                SelectedMods.Value = new Mod[] { new OsuModHidden { OnlyFadeApproachCircles = { Value = true } }, new OsuModClassic() };
                createCircle();
            });

            AddUntilStep("Wait until circle is missed", () => alphaAtMiss.IsNotNull());
            AddAssert("Transparent when missed", () => alphaAtMiss == 0);
        }

        /// <summary>
        /// No early fade is expected to be applied if the hit circle has been hit.
        /// </summary>
        [Test]
        public void TestHitCircleClassicModHit()
        {
            AddStep("Create hit circle", () =>
            {
                SelectedMods.Value = new Mod[] { new OsuModClassic() };
                createCircle(true);
            });
        }

        [Test]
        public void TestHitCircleNoModMiss()
        {
            AddStep("Create hit circle", () =>
            {
                SelectedMods.Value = Array.Empty<Mod>();
                createCircle();
            });

            AddUntilStep("Wait until circle is missed", () => alphaAtMiss.IsNotNull());
            AddAssert("Opaque when missed", () => alphaAtMiss == 1);
        }

        [Test]
        public void TestHitCircleNoModHit()
        {
            AddStep("Create hit circle", () =>
            {
                SelectedMods.Value = Array.Empty<Mod>();
                createCircle(true);
            });
        }

        [Test]
        public void TestSliderClassicMod()
        {
            AddStep("Create slider", () =>
            {
                SelectedMods.Value = new Mod[] { new OsuModClassic() };
                createSlider();
            });

            AddUntilStep("Wait until head circle is missed", () => alphaAtMiss.IsNotNull());
            AddAssert("Head circle transparent when missed", () => alphaAtMiss == 0);
        }

        [Test]
        public void TestSliderNoMod()
        {
            AddStep("Create slider", () =>
            {
                SelectedMods.Value = Array.Empty<Mod>();
                createSlider();
            });

            AddUntilStep("Wait until head circle is missed", () => alphaAtMiss.IsNotNull());
            AddAssert("Head circle opaque when missed", () => alphaAtMiss == 1);
        }

        private void createCircle(bool auto = false)
        {
            alphaAtMiss = null;

            TestDrawableHitCircle drawableHitCircle = new TestDrawableHitCircle(new HitCircle
            {
                StartTime = Time.Current + 500,
                Position = new Vector2(250),
            }, auto);

            drawableHitCircle.Scale = new Vector2(2f);

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObject>())
                mod.ApplyToDrawableHitObject(drawableHitCircle);

            drawableHitCircle.HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            drawableHitCircle.OnNewResult += (_, result) =>
            {
                if (!result.IsHit)
                    alphaAtMiss = drawableHitCircle.Alpha;
            };

            Child = drawableHitCircle;
        }

        private void createSlider()
        {
            alphaAtMiss = null;

            DrawableSlider drawableSlider = new DrawableSlider(new Slider
            {
                StartTime = Time.Current + 500,
                Position = new Vector2(250),
                Path = new SliderPath(PathType.Linear, new[]
                {
                    Vector2.Zero,
                    new Vector2(0, 100),
                })
            });

            drawableSlider.Scale = new Vector2(2f);

            drawableSlider.HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            drawableSlider.OnLoadComplete += _ =>
            {
                foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObject>())
                    mod.ApplyToDrawableHitObject(drawableSlider.HeadCircle);

                drawableSlider.HeadCircle.OnNewResult += (_, result) =>
                {
                    if (!result.IsHit)
                        alphaAtMiss = drawableSlider.HeadCircle.Alpha;
                };
            };
            Child = drawableSlider;
        }

        protected partial class TestDrawableHitCircle : DrawableHitCircle
        {
            private readonly bool auto;

            public TestDrawableHitCircle(HitCircle h, bool auto)
                : base(h)
            {
                this.auto = auto;
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (auto && !userTriggered && timeOffset >= 0 && CheckHittable?.Invoke(this, Time.Current) != false)
                {
                    // force success
                    ApplyResult(r => r.Type = HitResult.Great);
                }
                else
                    base.CheckForResult(userTriggered, timeOffset);
            }
        }
    }
}
