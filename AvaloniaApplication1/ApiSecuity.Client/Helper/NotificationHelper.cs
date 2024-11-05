using System.Threading.Tasks;
using Avalonia.Controls.Notifications;

namespace ApiSecuity.Client.Helper;

public static class NotificationHelper
{
    public static WindowNotificationManager? Notification;

    public static async Task ShowAsync(Notification notification)
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => Notification?.Show(notification));
    }

    public static async Task ShowInfoAsync(string message)
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            Notification?.Show(new Notification(
                title: "通知",
                message: message)));
    }

    public static async Task ShowErrorAsync(string message)
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            Notification?.Show(new Notification(
                title: "异常",
                message: message,
                type: NotificationType.Error)));
    }
}