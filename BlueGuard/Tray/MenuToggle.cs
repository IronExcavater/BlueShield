// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using Microsoft.UI.Xaml.Controls;

namespace BlueGuard.Tray;

// A checkable MenuFlyoutItem. Uses the Icon slot for the checkmark so it lines up with
// sibling items that have their own icon, like Exit.
// The Icon is always set — only its Opacity toggles — because MenuFlyoutItem collapses
// the icon column's reserved space entirely when Icon is null, which shifts the text
// sideways as items are checked/unchecked.
// Fires Toggled only on user clicks — programmatic IsChecked sets are silent, which
// prevents feedback loops when reverting a failed toggle.
internal sealed class MenuToggle
{
    private readonly MenuFlyoutItem _item;
    private readonly SymbolIcon _checkIcon = new(Symbol.Accept) { Opacity = 0 };
    private bool _isChecked;

    public MenuToggle(string text, double minWidth)
    {
        _item = new MenuFlyoutItem { Text = text, MinWidth = minWidth, Icon = _checkIcon };
        _item.Click += (_, _) =>
        {
            IsChecked = !IsChecked;
            Toggled?.Invoke(this, IsChecked);
        };
    }

    public MenuFlyoutItem Item => _item;

    public event EventHandler<bool>? Toggled;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            _isChecked = value;
            _checkIcon.Opacity = value ? 1 : 0;
        }
    }
}
