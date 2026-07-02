// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using Microsoft.UI.Xaml.Controls;

namespace BlueShield.Tray;

// A checkable MenuFlyoutItem. Uses the Icon slot for the checkmark so it lines up with
// sibling items that have their own icon, like Exit.
// Fires Toggled only on user clicks — programmatic IsEnabled sets are silent, which
// prevents feedback loops when reverting a failed toggle.
internal sealed partial class MenuToggle
{
    private readonly MenuFlyoutItem _item;
    private bool _isEnabled;

    public MenuToggle(string text, double minWidth)
    {
        _item = new MenuFlyoutItem { Text = text, MinWidth = minWidth };
        _item.Click += (_, _) =>
        {
            IsEnabled = !IsEnabled;
            Toggled?.Invoke(this, IsEnabled);
        };
    }

    public MenuFlyoutItem Item => _item;

    public event EventHandler<bool>? Toggled;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            _item.Icon = value ? new SymbolIcon(Symbol.Accept) : null;
        }
    }
}
