// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;

namespace BlueShield.Windows;

// Queries the Shell taskbar's screen rect via Win32. DisplayArea.WorkArea is not used because
// it returns the full monitor rect when the taskbar is set to auto-hide, so the only reliable
// source of truth is the taskbar window itself.
internal static partial class Taskbar
{
    public static bool TryGetBounds(out Bounds bounds)
    {
        var hwnd = FindWindow("Shell_TrayWnd", null);
        if (hwnd != IntPtr.Zero && GetWindowRect(hwnd, out var rect))
        {
            bounds = new Bounds(rect.Left, rect.Top, rect.Right, rect.Bottom);
            return true;
        }

        bounds = default;
        return false;
    }

    public readonly record struct Bounds(int Left, int Top, int Right, int Bottom);

    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
