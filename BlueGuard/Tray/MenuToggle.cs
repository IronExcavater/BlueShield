// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using Microsoft.UI.Xaml.Controls;

namespace BlueGuard.Tray;

// Checkable MenuFlyoutItem — the checkmark lives in the Icon slot so it aligns with
// sibling items that have real icons, like Exit.
// Icon stays assigned; only Opacity toggles. A null Icon collapses the column entirely,
// shifting the text sideways whenever checked state changes.
// Toggled fires only on clicks — programmatic IsChecked sets stay silent, so reverting a
// failed toggle doesn't loop back into the handler.
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
