// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using System.Reflection;
using System.Runtime.InteropServices;
using BlueShield.Windows;
using H.NotifyIcon;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace BlueShield.Tray;

internal static partial class TaskbarIconExtensions
{
    private static readonly PropertyInfo? ContextMenuAppWindowProperty =
        typeof(TaskbarIcon).GetProperty("ContextMenuAppWindow", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly PropertyInfo? ContextMenuWindowProperty =
        typeof(TaskbarIcon).GetProperty("ContextMenuWindow", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly PropertyInfo? ContextMenuWindowHandleProperty =
        typeof(TaskbarIcon).GetProperty("ContextMenuWindowHandle", BindingFlags.NonPublic | BindingFlags.Instance);

    // Sets up two hooks on H.NotifyIcon's internal second-window:
    //
    // 1. frame.Loaded — fires once after H.NotifyIcon's own frame.Loaded, which:
    //      (a) adds WS_BORDER via SetWindowStyleAsPopupWindow
    //      (b) calls flyout.ShowAt + flyout.Hide → flyout.Closed → HideWindow (hides the window)
    //    We strip the border then re-show the window, which fires window.Activated and causes
    //    H.NotifyIcon to call flyout.ShowAt against the now-correct client area (no scroll).
    //    This only works reliably when AreOpenCloseAnimationsEnabled = false on the flyout,
    //    so that flyout.Closed fires synchronously (no animation delay).
    //
    // 2. window.Activated — fires on every menu open. Clamps Y above the taskbar and asserts
    //    HWND_TOPMOST so the menu sits above the always-on-top Shell window.
    public static void FixContextMenuOnEachOpen(this TaskbarIcon icon)
    {
        var appWindow = ContextMenuAppWindowProperty?.GetValue(icon) as AppWindow;
        var window = ContextMenuWindowProperty?.GetValue(icon) as Window;

        if (appWindow is null || window is null) return;

        if (window.Content is FrameworkElement frame)
        {
            frame.Loaded += (_, _) =>
            {
                appWindow.StripBorder();
                // H.NotifyIcon's frame.Loaded handler hid the window via flyout.Closed → HideWindow.
                // Re-show and re-activate so window.Activated fires and flyout re-opens without WS_BORDER.
                var hwnd = Win32Interop.GetWindowFromWindowId(appWindow.Id);
                ShowWindow(hwnd, SwShow);
                SetForegroundWindow(hwnd);
            };
        }

        window.Activated += (_, args) =>
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated) return;
            appWindow.FixContextMenuWindow();
        };
    }

    public static nint GetContextMenuWindowHandle(this TaskbarIcon icon)
    {
        var val = ContextMenuWindowHandleProperty?.GetValue(icon);
        if (val is nint h) return h;
        if (val is IntPtr p) return (nint)p;
        return 0;
    }

    public static AppWindow? GetContextMenuAppWindow(this TaskbarIcon icon) =>
        ContextMenuAppWindowProperty?.GetValue(icon) as AppWindow;

    private const int SwShow = 5;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);
}
