using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ApiSecuity.Client.Helper;
using ApiSecuityServer.Message;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ApiSecuity.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly HubConnection _connection;

    [ObservableProperty] private string? _connectedId;
    [ObservableProperty] private bool _isConnected;

    public MainViewModel()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("ws://127.0.0.1:5224/chat")
            .AddJsonProtocol()
            .ConfigureJsonHubOptions()
            .Build();

        _connection.Closed += OnClosedAsync;
        _connection.RegisterHandler<PublishDownloadMessage>(ServerClientApiCommand.PublishDownload,
            OnPublishDownloadAsync);
    }

    #region mvvmcommand

    [RelayCommand]
    private async Task OnConnectAsync()
    {
        IsConnected = false;

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception e)
        {
            await NotificationHelper.ShowErrorAsync($"连接服务器失败{e.Message}");
            return;
        }

        await NotificationHelper.ShowInfoAsync($"连接成功");
        IsConnected = true;
        ConnectedId = _connection.ConnectionId;
    }

    #endregion

    #region hubcallback

    private async Task OnClosedAsync(Exception? arg)
    {
        await NotificationHelper.ShowErrorAsync($"断开连接{arg?.Message}");
    }

    private async Task OnPublishDownloadAsync(PublishDownloadMessage arg)
    {
        await NotificationHelper.ShowInfoAsync($"正在下载文件{arg.FileName}");
        
        Task.Factory.StartNew(async () => await DownloadFileAsync(arg));
    }

    private async ValueTask DownloadFileAsync(PublishDownloadMessage arg)
    {
        using var http = new HttpClient();
        var response = await http.GetAsync(new Uri($"http://localhost:5224/api/file/download?fileId={arg.FileId}"));
        await using var fs = System.IO.File.Create(arg.FileName);
        await response.Content.CopyToAsync(fs);
        await NotificationHelper.ShowInfoAsync("下载完成");
    }

    #endregion
}