// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

namespace BlueShield.Core.Startup;

public interface IStartupService
{
    Task<bool> IsEnabledAsync();

    // Returns false if the OS denied the request (e.g. user dismissed the consent dialog).
    Task<bool> SetEnabledAsync(bool enabled);
}
