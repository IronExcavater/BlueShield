// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;

namespace BlueShield.Windows;

internal static partial class AppWindowExtensions
{
    // Called once from frame.Loaded (fires after H.NotifyIcon's frame.Loaded adds WS_BORDER via
    // SetWindowStyleAsPopupWindow). Stripping here ensures the flyout.ShowAt that follows in
    // window.Activated measures against the full client area — no spurious ScrollViewer.
    public static void StripBorder(this AppWindow window)
    {
        var hwnd = Win32Interop.GetWindowFromWindowId(window.Id);
        var style = GetWindowLong(hwnd, GwlStyle);
        if ((style & WsBorder) == 0) return;
        SetWindowLong(hwnd, GwlStyle, style & ~WsBorder);
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
            SwpFrameChanged | SwpNoMove | SwpNoSize | SwpNozOrder | SwpNoActivate);
    }

    // Called on every window.Activated (non-Deactivated): clamps Y above the taskbar and
    // asserts HWND_TOPMOST so the menu sits above the always-on-top Shell window.
    public static void FixContextMenuWindow(this AppWindow window)
    {
        var hwnd = Win32Interop.GetWindowFromWindowId(window.Id);

        if (!Taskbar.TryGetBounds(out var taskbar)) return;

        var right = window.Position.X + window.Size.Width;
        if (window.Position.X >= taskbar.Right || right <= taskbar.Left) return;

        var maxY = taskbar.Top - window.Size.Height;
        SetWindowPos(hwnd, HwndTopmost, window.Position.X, Math.Min(window.Position.Y, maxY),
            0, 0, SwpNoSize | SwpNoActivate);
    }

    private const int GwlStyle = -16;
    private const int WsBorder = 0x00800000;
    private static readonly IntPtr HwndTopmost = new(-1);
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNozOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpFrameChanged = 0x0020;

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static partial int GetWindowLong(IntPtr hWnd, int nIndex);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static partial int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
}
