// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using Windows.Devices.Radios;

namespace BlueShield.Bluetooth;

internal static class BluetoothRadio
{
    internal static async Task<Radio?> GetAsync()
    {
        if (await Radio.RequestAccessAsync() != RadioAccessStatus.Allowed)
            return null;
        var radios = await Radio.GetRadiosAsync();
        return radios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
    }
}
