// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using BlueGuard.Resources;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BlueGuard.Tray;

internal sealed class TrayService : IDisposable
{
    // H.NotifyIcon's SecondWindow context menu measures each item's DesiredSize directly
    // (MenuFlyoutPresenterStyle on the flyout is not applied), so width must be set per-item.
    private const double MenuItemMinWidth = 200;

    // TaskbarIcon.IconSource converts via System.Drawing.Icon, which requires .ico not PNG.
    private static readonly Uri IconOnUri = new("ms-appx:///Assets/ToggleOn.ico");
    private static readonly Uri IconOffUri = new("ms-appx:///Assets/ToggleOff.ico");

    private readonly TaskbarIcon _icon;

    public MenuToggle Protection { get; }
    public MenuToggle Startup { get; }
    public MenuToggle ReopenAfterToggle { get; }

    public event EventHandler? ExitRequested;

    public TrayService()
    {
        Protection = new MenuToggle(Strings.TrayMenuProtection, MenuItemMinWidth);
        Startup = new MenuToggle(Strings.TrayMenuStartup, MenuItemMinWidth);
        ReopenAfterToggle = new MenuToggle(Strings.TrayMenuReopenAfterToggle, MenuItemMinWidth);

        Protection.Toggled += (_, enabled) => UpdateTrayIcon(enabled);

        var settings = new MenuFlyoutSubItem
        {
            Text = Strings.TrayMenuSettings,
            MinWidth = MenuItemMinWidth,
            Items = { ReopenAfterToggle.Item },
        };

        var exit = new MenuFlyoutItem { Text = Strings.TrayMenuExit, MinWidth = MenuItemMinWidth };
        exit.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        // IsHitTestVisible = false prevents the separator from receiving pointer events.
        // Without it, the separator's default touch margins can overlap the item above it.
        var separator = new MenuFlyoutSeparator { IsHitTestVisible = false };

        var flyout = new MenuFlyout
        {
            Items = { Protection.Item, Startup.Item, settings, separator, exit },
        };

        _icon = new TaskbarIcon
        {
            ToolTipText = Strings.AppName,
            NoLeftClickDelay = true,
            MenuActivation = PopupActivationMode.LeftOrRightClick,
            ContextMenuMode = ContextMenuMode.SecondWindow,
            ContextFlyout = flyout,
        };

        // CloseContextMenuOnItemClick (added in 2.5.0-beta.1) replaces the old close-then-
        // reopen dance: when false, an item click simply never closes the menu (the library
        // cancels its own flyout.Closing internally), so "stay open" needs no window handles,
        // P/Invoke, or dispatcher timing at all.
        ReopenAfterToggle.Toggled += (_, enabled) => _icon.CloseContextMenuOnItemClick = !enabled;
    }

    public void Create()
    {
        _icon.CloseContextMenuOnItemClick = !ReopenAfterToggle.IsChecked;
        _icon.ForceCreate();
    }

    public void SetProtectionEnabled(bool enabled)
    {
        Protection.IsChecked = enabled;
        UpdateTrayIcon(enabled);
    }

    private void UpdateTrayIcon(bool enabled) =>
        _icon.IconSource = new BitmapImage(enabled ? IconOnUri : IconOffUri);

    public void Dispose() => _icon.Dispose();
}
