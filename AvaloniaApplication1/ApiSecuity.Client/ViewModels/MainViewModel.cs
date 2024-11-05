using System;
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

    [ObservableProperty] private bool _isConnected;

    public MainViewModel()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("ws://127.0.0.1:6767/order")
            .AddMessagePackProtocol()
            .Build();

        _connection.Closed += OnClosedAsync;
        _connection.RegisterHandler<RequestRefreshMessage>(ServerClientApiCommand.RefreshAll, OnRefreshAllAsync);
        _connection.RegisterHandler<RequestRefreshMessage, RequestRefreshReplyMessage>(
            ServerClientApiCommand.RequestRefresh, OnRequestRefreshAsync);
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
    }

    #endregion


    #region hubcallback

    private async Task OnClosedAsync(Exception? arg)
    {
        await NotificationHelper.ShowErrorAsync($"断开连接{arg?.Message}");
    }

    private async Task<RequestRefreshReplyMessage> OnRequestRefreshAsync(RequestRefreshMessage arg)
    {
        await NotificationHelper.ShowInfoAsync("请求刷新返回结果");

        return new RequestRefreshReplyMessage(true);
    }

    private async Task OnRefreshAllAsync(RequestRefreshMessage arg)
    {
        await NotificationHelper.ShowInfoAsync("请求刷新");
    }

    #endregion
}