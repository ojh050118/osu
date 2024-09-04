// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Platform.SDL2;
using osu.Framework.Utils;
using osu.Game.Overlays.Settings.Sections.Maintenance;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public partial class WallpaperSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Desktop wallpaper";

        [BackgroundDependencyLoader]
        private void load(GameHost host, IDialogOverlay? dialogOverlay, FrameworkConfigManager frameworkConfig)
        {
            var window = host.Window as SDL2Window;

            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = "Change window parent to Desktop",
                    Action = () => frameworkConfig.GetBindable<WindowMode>(FrameworkSetting.WindowMode).Value = WindowMode.Windowed
                },
                new SettingsButton
                {
                    Text = "Change window parent to SysListView32",
                    Action = () => WindowHelper.SetParentToDesktop(window.WindowHandle, WindowParent.SysListView32, window.Position.X, window.Position.Y, window.Size.Width, window.Size.Height)
                },
                new DangerousSettingsButton
                {
                    Text = "Change window parent to WorkerW",
                    Action = () =>
                    {
                        dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
                        {
                            WindowHelper.SetParentToDesktop(window.WindowHandle, WindowParent.WorkerW, window.Position.X, window.Position.Y, window.Size.Width, window.Size.Height);
                        }, "Do you want to switch Window's parent to WorkerW? This action cannot be undone!"));
                    }
                },
            };
        }
    }
}
