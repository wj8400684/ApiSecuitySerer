using System;
using ApiSecuity.Client.Helper;
using ApiSecuity.Client.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace ApiSecuity.Client.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var topLevel = TopLevel.GetTopLevel(this);

        ArgumentNullException.ThrowIfNull(topLevel);

        NotificationHelper.Notification = new WindowNotificationManager(topLevel)
            { MaxItems = 10, Position = NotificationPosition.TopRight };
    }
}