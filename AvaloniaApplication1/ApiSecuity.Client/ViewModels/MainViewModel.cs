using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Net.Http.Headers;
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
    private const string HostUrl = "localhost:6767";
    private readonly HubConnection _connection;

    [ObservableProperty] private long _totalFileSize;
    [ObservableProperty] private long _downloadProgressSize;
    [ObservableProperty] private string? _connectedId;
    [ObservableProperty] private bool _isConnected;

    public MainViewModel()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"ws://{HostUrl}/chat")
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

    [RelayCommand]
    private async Task OnFileDownloadedAsync()
    {
        TotalFileSize = 1000000;
        var url = $"http://localhost:6767/api/file/download/08dc57cf-4ea4-4757-85f7-09ba2b463a99";
        //获取到文件总大小 通过head请求
        var processMessageHander = new ProgressMessageHandler(new HttpClientHandler());
        using var client = new HttpClient();
        processMessageHander.HttpReceiveProgress += OnHttpReceiveProgress;
        await using var fileStream =
            new FileStream("www/ssss.s", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        var res = await client.GetAsync(url);
    }

    [RelayCommand]
    private async Task OnFileUploadedAsync()
    {
        // if (!IsConnected)
        // {
        //     await NotificationHelper.ShowErrorAsync("清先连接服务器");
        //     return;
        // }

        //var url = $"http://{HostUrl}/api/file/upload";
        var url = $"http://localhost:6767/api/file/download/08dc57cf-4ea4-4757-85f7-09ba2b463a99";

        var memoryStream = new MemoryStream();
        for (var i = 0; i < 1024 * 1024; i++)
        {
            memoryStream.WriteByte((byte)Random.Shared.Next(0, 255));
        }

        memoryStream.Position = 0;
        var processMessageHander = new ProgressMessageHandler(new HttpClientHandler());
        processMessageHander.HttpSendProgress += OnHttpSendProgress;

        using var client = new HttpClient();
        var data = new MultipartFormDataContent();
        var content = new StreamContent(memoryStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        data.Add(content, "file-name", "text");
        TotalFileSize = memoryStream.Length;

        var resp = await client.PostAsync(url, data);
    }

    /// <summary>
    /// 上传进度
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnHttpSendProgress(object? sender, HttpProgressEventArgs e)
    {
        DownloadProgressSize += e.ProgressPercentage;
    }

    /// <summary>
    /// 下载回调
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnHttpReceiveProgress(object? sender, HttpProgressEventArgs e)
    {
        DownloadProgressSize += e.ProgressPercentage;
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
        TotalFileSize = arg.FileSize;
        await NotificationHelper.ShowInfoAsync("下载完成");

        var url = $"http://{HostUrl}/api/file/down/{arg.FileId}";
        //获取到文件总大小 通过head请求
        using var client = new HttpClient();
        await using var fileStream =
            new FileStream($"www/{arg.FileName}", FileMode.Create, FileAccess.Write, FileShare.Read);

        //开始分片下载
        while (DownloadProgressSize < TotalFileSize)
        {
            //组装range 0,1000 1000,2000 0,9999
            long start = DownloadProgressSize;
            long end = start + 9999;
            if (end > (TotalFileSize - 1))
            {
                end = TotalFileSize - 1;
            }

            client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);
            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
                break;

            byte[] bytes = await res.Content.ReadAsByteArrayAsync();
            await fileStream.WriteAsync(bytes);
            //更新UI的进度
            DownloadProgressSize += bytes.Length;
        }
    }

    #endregion
}