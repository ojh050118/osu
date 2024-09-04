// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Framework.Logging;
using osu.Framework.Platform.SDL2;
using osu.Framework.Utils;

namespace osu.Desktop.Windows
{
    internal partial class BossKeyManager : Component, IKeyBindingHandler<GlobalAction>, IHandleGlobalKeyboardInput
    {
        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private VolumeOverlay volumeOverlay { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            host.Window.CreateNotificationTrayIcon("osu!", () => Schedule(() => onShow()));
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            var window = host.Window as SDL2Window;

            if (e.Action == GlobalAction.BossKey && !e.Repeat)
            {
                host.Window.CreateNotificationTrayIcon("osu!", () => Schedule(() => onShow()));
                WindowHelper.SetParentToDesktop(window.WindowHandle, WindowParent.WorkerW, window.Position.X, window.Position.Y, window.Size.Width, window.Size.Height);
                Logger.Log($"Created notification tray icon.");

                return true;
            }

            return false;
        }

        private void onShow()
        {
            var window = host.Window as SDL2Window;

            Logger.Log($"Notification tray icon clicked.");
            WindowHelper.SetParentToDesktop(window.WindowHandle, WindowParent.SysListView32, window.Position.X, window.Position.Y, window.Size.Width, window.Size.Height);
            host.Window.RemoveNotificationTrayIcon();
            Logger.Log($"Notification tray icon removed.");
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
