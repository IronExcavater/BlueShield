// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using BlueShield.Bluetooth;
using BlueShield.Core.Bluetooth;
using BlueShield.Core.Settings;
using BlueShield.Core.Startup;
using BlueShield.Notifications;
using BlueShield.Settings;
using BlueShield.Startup;
using BlueShield.Tray;
using BlueShield.Windows;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace BlueShield;

public partial class App : Application
{
    private readonly DispatcherQueue _dispatcher = DispatcherQueue.GetForCurrentThread();

    private HostWindow? _host;
    private NotificationService? _notifications;
    private ProtectionNotificationHandler? _protectionNotifications;
    private AppSettings _settings = null!;
    private IProtectionService _protection = null!;
    private IStartupService _startup = null!;
    private TrayService _tray = null!;

    public App() => InitializeComponent();

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        _settings = new AppSettings(new LocalSettingsStore());
        _protection = new ProtectionService();
        _startup = new StartupService();
        _tray = new TrayService();

        _host = new HostWindow();
        _host.Activate();

        _notifications = new NotificationService();
        _protectionNotifications = new ProtectionNotificationHandler(_protection, _tray, _settings, _dispatcher, _notifications);

        _tray.Protection.Toggled += OnProtectionToggled;
        _tray.Startup.Toggled += OnStartupToggled;
        _tray.ExitRequested += OnExitRequested;

        _tray.Startup.IsEnabled = await _startup.IsEnabledAsync();
        _tray.SetProtectionEnabled(_settings.ProtectionEnabled);
        _tray.Create();

        if (_settings.ProtectionEnabled)
            await _protection.StartAsync();
    }

    private async void OnProtectionToggled(object? sender, bool enabled)
    {
        _settings.ProtectionEnabled = enabled;
        if (enabled) await _protection.StartAsync();
        else await _protection.StopAsync();
    }

    private async void OnStartupToggled(object? sender, bool enabled)
    {
        if (!await _startup.SetEnabledAsync(enabled))
            _tray.Startup.IsEnabled = !enabled;
        else
            _settings.LaunchAtStartup = enabled;
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        _tray.Dispose();
        Exit();
    }
}
