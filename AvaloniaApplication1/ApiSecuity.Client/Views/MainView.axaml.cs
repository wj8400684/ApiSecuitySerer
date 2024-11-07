using System;
using ApiSecuity.Client.Helper;
using ApiSecuity.Client.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace ApiSecuity.Client.Views;

public partial class MainView : UserControl
{
    private readonly MainViewModel _mainViewModel;

    public MainView()
    {
        InitializeComponent();
        DataContext = _mainViewModel = new MainViewModel();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var topLevel = TopLevel.GetTopLevel(this);

        ArgumentNullException.ThrowIfNull(topLevel);

        _mainViewModel.SetStorageProvider(topLevel.StorageProvider);
        
        NotificationHelper.Notification = new WindowNotificationManager(topLevel)
            { MaxItems = 10, Position = NotificationPosition.TopRight };
    }
}