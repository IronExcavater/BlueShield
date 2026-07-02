// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using Windows.ApplicationModel.Resources;

namespace BlueShield.Resources;

// Typed accessor for Resources.resw. Property names must match resource keys exactly —
// nameof() is used throughout so any mismatch is caught at compile time.
// GetForViewIndependentUse is required because some callers (e.g. notification events)
// run on background threads where no CoreWindow is available.
internal static class Strings
{
    private static readonly ResourceLoader Loader = ResourceLoader.GetForViewIndependentUse();

    internal static string AppName => Loader.GetString(nameof(AppName));

    internal static string TrayMenuProtection => Loader.GetString(nameof(TrayMenuProtection));
    internal static string TrayMenuStartup => Loader.GetString(nameof(TrayMenuStartup));
    internal static string TrayMenuExit => Loader.GetString(nameof(TrayMenuExit));

    internal static string NotificationRestoredBody => Loader.GetString(nameof(NotificationRestoredBody));
    internal static string NotificationRestoredBodyFailed => Loader.GetString(nameof(NotificationRestoredBodyFailed));
    internal static string NotificationRestoredActionTurnOff => Loader.GetString(nameof(NotificationRestoredActionTurnOff));
}
