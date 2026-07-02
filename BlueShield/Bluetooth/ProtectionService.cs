// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using BlueShield.Core.Bluetooth;
using Windows.Devices.Radios;

namespace BlueShield.Bluetooth;

internal sealed class ProtectionService : IProtectionService
{
    private Radio? _radio;

    public bool IsEnabled { get; private set; }

    public event EventHandler<BluetoothRestoredEventArgs>? BluetoothRestored;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsEnabled) return;

        _radio = await BluetoothRadio.GetAsync();

        if (_radio is null) return;

        _radio.StateChanged += OnStateChanged;
        IsEnabled = true;
    }

    public Task StopAsync()
    {
        _radio?.StateChanged -= OnStateChanged;
        _radio = null;

        IsEnabled = false;
        return Task.CompletedTask;
    }

    public async Task DisableAsync()
    {
        if (_radio is not null)
        {
            _radio.StateChanged -= OnStateChanged;
            await _radio.SetStateAsync(RadioState.Off);
            _radio = null;
        }
        else
        {
            // Protection may have been stopped externally; get a fresh reference to turn off.
            var radio = await BluetoothRadio.GetAsync();
            if (radio is not null)
                await radio.SetStateAsync(RadioState.Off);
        }

        IsEnabled = false;
    }

    private async void OnStateChanged(Radio sender, object args)
    {
        if (!IsEnabled || sender.State != RadioState.Off || _radio is null) return;

        var result = await _radio.SetStateAsync(RadioState.On);

        BluetoothRestored?.Invoke(this, new BluetoothRestoredEventArgs(
            success: result == RadioAccessStatus.Allowed,
            timestamp: DateTimeOffset.Now));
    }
}
