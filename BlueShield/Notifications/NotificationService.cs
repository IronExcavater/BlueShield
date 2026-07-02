// Copyright (C) 2026 Niclas Rogulski. All rights reserved.
// SPDX-License-Identifier: MPL-2.0

using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace BlueShield.Notifications;

internal sealed class NotificationService
{
    private readonly ToastNotifier _notifier = ToastNotificationManager.CreateToastNotifier();

    public async Task<ToastNotification> ShowTemplate(string name)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(
            new Uri($"ms-appx:///Notifications/Templates/{name}.xml"));
        var xml = await FileIO.ReadTextAsync(file);
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var notification = new ToastNotification(doc);
        _notifier.Show(notification);
        return notification;
    }
}
