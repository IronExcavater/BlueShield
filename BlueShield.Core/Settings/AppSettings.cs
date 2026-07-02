// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

namespace BlueShield.Core.Settings;

public sealed class AppSettings(ISettingsStore store)
{
    public bool ProtectionEnabled
    {
        get => store.Get(nameof(ProtectionEnabled), defaultValue: true);
        set => store.Set(nameof(ProtectionEnabled), value);
    }

    public bool LaunchAtStartup
    {
        get => store.Get(nameof(LaunchAtStartup), defaultValue: false);
        set => store.Set(nameof(LaunchAtStartup), value);
    }
}
