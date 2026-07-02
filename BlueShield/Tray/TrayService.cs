// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;
using BlueShield.Resources;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BlueShield.Tray;

internal sealed partial class TrayService : IDisposable
{
    // H.NotifyIcon's SecondWindow context menu measures each item's DesiredSize directly
    // (MenuFlyoutPresenterStyle on the flyout is not applied), so width must be set per-item.
    private const double MenuItemMinWidth = 200;

    // TaskbarIcon.IconSource converts via System.Drawing.Icon, which requires .ico not PNG.
    private static readonly Uri IconOnUri = new("ms-appx:///Assets/ToggleOn.ico");
    private static readonly Uri IconOffUri = new("ms-appx:///Assets/ToggleOff.ico");

    private readonly DispatcherQueue _dispatcher = DispatcherQueue.GetForCurrentThread();
    private readonly TaskbarIcon _icon;

    public MenuToggle Protection { get; }
    public MenuToggle Startup { get; }

    public event EventHandler? ExitRequested;

    public TrayService()
    {
        Protection = new MenuToggle(Strings.TrayMenuProtection, MenuItemMinWidth);
        Startup = new MenuToggle(Strings.TrayMenuStartup, MenuItemMinWidth);

        // H.NotifyIcon closes the menu on every item tap. Re-show the second-window directly
        // at Low priority so it runs after the full close sequence (flyout.Closed, deactivation).
        Protection.Toggled += (_, enabled) => { UpdateTrayIcon(enabled); ReopenMenu(); };
        Startup.Toggled += (_, _) => ReopenMenu();

        var exit = new MenuFlyoutItem
        {
            Text = Strings.TrayMenuExit,
            Icon = new SymbolIcon(Symbol.Cancel),
            MinWidth = MenuItemMinWidth,
        };
        exit.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        // IsHitTestVisible = false prevents the separator from receiving pointer events.
        // Without it, the separator's default touch margins can overlap the item above it.
        var separator = new MenuFlyoutSeparator { IsHitTestVisible = false };

        // Disable animations so flyout.Closed fires synchronously (no animation delay).
        // H.NotifyIcon's flyout.Closed handler checks AreOpenCloseAnimationsEnabled: when false
        // it always calls HideWindow immediately, which lets our Low-priority ReopenMenu callback
        // run after the close is fully complete — not mid-animation where ShowAt is a no-op.
        var flyout = new MenuFlyout
        {
            AreOpenCloseAnimationsEnabled = false,
            Items = { Protection.Item, Startup.Item, separator, exit },
        };

        _icon = new TaskbarIcon
        {
            ToolTipText = Strings.AppName,
            NoLeftClickDelay = true,
            MenuActivation = PopupActivationMode.LeftOrRightClick,
            ContextMenuMode = ContextMenuMode.SecondWindow,
            ContextFlyout = flyout,
        };

        _icon.FixContextMenuOnEachOpen();
    }

    public void Create() => _icon.ForceCreate();

    public void SetProtectionEnabled(bool enabled)
    {
        Protection.IsEnabled = enabled;
        UpdateTrayIcon(enabled);
    }

    private void UpdateTrayIcon(bool enabled) =>
        _icon.IconSource = new BitmapImage(enabled ? IconOnUri : IconOffUri);

    // Bypasses _icon.ShowContextMenu entirely: its internal SetForegroundWindow can fail when
    // the foreground-lock window has timed out or the window is deactivating. Instead, show
    // the second-window HWND directly — ShowWindow makes it visible, SetForegroundWindow
    // activates it, which fires window.Activated and causes H.NotifyIcon to re-show the flyout.
    private void ReopenMenu()
    {
        var hwnd = (IntPtr)_icon.GetContextMenuWindowHandle();
        if (hwnd == IntPtr.Zero) return;

        _dispatcher.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            ShowWindow(hwnd, SwShow);
            SetForegroundWindow(hwnd);
        });
    }

    private const int SwShow = 5;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);

    public void Dispose() => _icon.Dispose();
}
