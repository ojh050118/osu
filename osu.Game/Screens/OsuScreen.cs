﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Microsoft.EntityFrameworkCore.Internal;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using osu.Game.Screens.Menu;
using osu.Game.Overlays;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens
{
    public abstract class OsuScreen : Screen, IOsuScreen, IKeyBindingHandler<GlobalAction>, IHasDescription
    {
        public BackgroundScreen Background { get; }

        /// <summary>
        /// A user-facing title for this screen.
        /// </summary>
        public virtual string Title => GetType().ShortDisplayName();

        public string Description => Title;

        protected virtual bool AllowBackButton => true;

        public virtual bool AllowExternalScreenChange => false;

        /// <summary>
        /// Override to create a BackgroundMode for the current screen.
        /// Note that the instance created may not be the used instance if it matches the BackgroundMode equality clause.
        /// </summary>
        protected virtual BackgroundScreen CreateBackground() => null;

        private Action updateOverlayStates;

        /// <summary>
        /// Whether all overlays should be hidden when this screen is entered or resumed.
        /// </summary>
        protected virtual bool HideOverlaysOnEnter => false;

        protected readonly Bindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>();

        /// <summary>
        /// Whether overlays should be able to be opened once this screen is entered or resumed.
        /// </summary>
        protected virtual OverlayActivation InitialOverlayActivationMode => OverlayActivation.All;

        public virtual bool CursorVisible => true;

        protected new OsuGameBase Game => base.Game as OsuGameBase;

        [Resolved]
        private OsuLogo logo { get; set; }

        public virtual bool AllowBeatmapRulesetChange => true;

        protected readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        public virtual float BackgroundParallaxAmount => 1;

        protected readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        private SampleChannel sampleExit;

        protected OsuScreen()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Background = CreateBackground();
        }

        [BackgroundDependencyLoader(true)]
        private void load(BindableBeatmap beatmap, OsuGame osu, AudioManager audio, Bindable<RulesetInfo> ruleset)
        {
            Beatmap.BindTo(beatmap);
            Ruleset.BindTo(ruleset);

            if (osu != null)
            {
                OverlayActivationMode.BindTo(osu.OverlayActivationMode);

                updateOverlayStates = () =>
                {
                    if (HideOverlaysOnEnter)
                        osu.CloseAllOverlays();
                    else
                        osu.Toolbar.State = Visibility.Visible;
                };
            }

            sampleExit = audio.Sample.Get(@"UI/screen-back");
        }

        public virtual bool OnPressed(GlobalAction action)
        {
            if (!this.IsCurrentScreen()) return false;

            if (action == GlobalAction.Back && AllowBackButton)
            {
                this.Exit();
                return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => action == GlobalAction.Back && AllowBackButton;

        public override void OnResuming(IScreen last)
        {
            sampleExit?.Play();
            applyArrivingDefaults(true);

            base.OnResuming(last);
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);
            onSuspendingLogo();
        }

        public override void OnEntering(IScreen last)
        {
            applyArrivingDefaults(false);

            base.OnEntering(last);
        }

        public override bool OnExiting(IScreen next)
        {
            if (ValidForResume && logo != null)
                onExitingLogo();

            if (base.OnExiting(next))
                return true;

            Beatmap.UnbindAll();
            return false;
        }

        /// <summary>
        /// Fired when this screen was entered or resumed and the logo state is required to be adjusted.
        /// </summary>
        protected virtual void LogoArriving(OsuLogo logo, bool resuming)
        {
            logo.Action = null;
            logo.FadeOut(300, Easing.OutQuint);
            logo.Anchor = Anchor.TopLeft;
            logo.Origin = Anchor.Centre;
            logo.RelativePositionAxes = Axes.None;
            logo.BeatMatching = true;
            logo.Triangles = true;
            logo.Ripple = true;
        }

        private void applyArrivingDefaults(bool isResuming)
        {
            logo.AppendAnimatingAction(() =>
            {
                if (this.IsCurrentScreen()) LogoArriving(logo, isResuming);
            }, true);

            OverlayActivationMode.Value = InitialOverlayActivationMode;

            updateOverlayStates?.Invoke();
        }

        private void onExitingLogo()
        {
            logo.AppendAnimatingAction(() => LogoExiting(logo), false);
        }

        /// <summary>
        /// Fired when this screen was exited to add any outwards transition to the logo.
        /// </summary>
        protected virtual void LogoExiting(OsuLogo logo)
        {
        }

        private void onSuspendingLogo()
        {
            logo.AppendAnimatingAction(() => LogoSuspending(logo), false);
        }

        /// <summary>
        /// Fired when this screen was suspended to add any outwards transition to the logo.
        /// </summary>
        protected virtual void LogoSuspending(OsuLogo logo)
        {
        }
    }
}
