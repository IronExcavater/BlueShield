// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using BlueShield.Core.Bluetooth;
using BlueShield.Core.Settings;
using BlueShield.Tray;
using Microsoft.UI.Dispatching;
using Windows.UI.Notifications;

namespace BlueShield.Notifications;

internal sealed class ProtectionNotificationHandler
{
    private const string TurnOffAction = "turnoff";

    private readonly IProtectionService _protection;
    private readonly TrayService _tray;
    private readonly AppSettings _settings;
    private readonly DispatcherQueue _dispatcher;
    private readonly NotificationService _notifications;

    public ProtectionNotificationHandler(
        IProtectionService protection,
        TrayService tray,
        AppSettings settings,
        DispatcherQueue dispatcher,
        NotificationService notifications)
    {
        _protection = protection;
        _tray = tray;
        _settings = settings;
        _dispatcher = dispatcher;
        _notifications = notifications;

        _protection.BluetoothRestored += OnRestored;
    }

    private async void OnRestored(object? _, BluetoothRestoredEventArgs e)
    {
        var notification = await _notifications.ShowTemplate(
            e.Success ? "BluetoothRestored" : "BluetoothRestoredFailed");

        if (e.Success)
            notification.Activated += OnTurnOff;
    }

    private void OnTurnOff(ToastNotification _, object args)
    {
        if (args is not ToastActivatedEventArgs { Arguments: TurnOffAction }) return;

        _dispatcher.TryEnqueue(async () =>
        {
            await _protection.DisableAsync();
            _tray.SetProtectionEnabled(false);
            _settings.ProtectionEnabled = false;
        });
    }
}
