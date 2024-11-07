using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ApiSecuity.Client.Helper;
using ApiSecuityServer.Message;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ApiSecuity.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private IStorageProvider _storageProvider = null!;
    private const string HostUrl = "193.112.192.177:6767";
    private readonly HubConnection _connection;

    [ObservableProperty] private string? _targetFileId;
    [ObservableProperty] private string? _targetConnectionId;
    [ObservableProperty] private long _totalFileSize;
    [ObservableProperty] private double _downloadProgressSize;
    [ObservableProperty] private double _uploadProgressSize;
    [ObservableProperty] private string? _connectedId;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string? _downloadProgress;

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

    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
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
        await Task.Factory.StartNew(async () =>
        {
            await NotificationHelper.ShowInfoAsync($"开始下载");
            TotalFileSize = 1000000;
            var url = $"http://{HostUrl}/api/file/download/{TargetFileId}";
            //获取到文件总大小 通过head请求
            var processMessageHander = new ProgressMessageHandler(new HttpClientHandler());
            using var client = new HttpClient(processMessageHander);
            processMessageHander.HttpReceiveProgress += OnHttpReceiveProgress;
            await using var fileStream =
                new FileStream("www/ssss.s", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            var res = await client.GetAsync(url);
            await res.Content.CopyToAsync(fileStream);
            await NotificationHelper.ShowInfoAsync($"下载成功");
        });
    }

    [RelayCommand]
    private async Task OnFileUploadedAsync()
    {
        if (!IsConnected)
        {
            await NotificationHelper.ShowErrorAsync("清先连接服务器");
            return;
        }


        try
        {
            var folders = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择文件",
                AllowMultiple = true,
                FileTypeFilter = [FilePickerFileTypes.All]
            });

            if (!folders.Any())
                return;

            var folder = folders[0];

            var stream = await folder.OpenReadAsync();
        
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", "归档.zip");

            await NotificationHelper.ShowInfoAsync($"正在上传");
            var file = stream;//File.OpenRead(path);

            //var url = $"http://{HostUrl}/api/file/upload";
            var url =
                $"http://{HostUrl}/api/file/upload?ConnectionId={TargetConnectionId}&FileName=sssssss&PartNumber=1&Chunks=1&Start=1&Size=81960&End=1&Total={file.Length}";
            UploadProgressSize = 0;
            file.Position = 0;
        
            var processMessageHander = new ProgressMessageHandler(new HttpClientHandler());
            processMessageHander.HttpSendProgress += OnHttpSendProgress;
            using var client = new HttpClient(processMessageHander);
            var data = new MultipartFormDataContent();
            var content = new StreamContent(file, 1024 * 1024);
            data.Add(content, "file-name", folder.Name);
            var resp = await client.PostAsync(url, data);
            await NotificationHelper.ShowInfoAsync($"上传完毕");
        }
        catch (Exception e)
        {
            await NotificationHelper.ShowErrorAsync(e.Message);
        }
        
    }

    /// <summary>
    /// 上传进度
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnHttpSendProgress(object? sender, HttpProgressEventArgs e)
    {
        DownloadProgress = $"{e.ProgressPercentage}%";
        UploadProgressSize = e.ProgressPercentage;
    }

    /// <summary>
    /// 下载回调
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnHttpReceiveProgress(object? sender, HttpProgressEventArgs e)
    {
    }

    #endregion

    #region hubcallback

    private async Task OnClosedAsync(Exception? arg)
    {
        await NotificationHelper.ShowErrorAsync($"断开连接{arg?.Message}");
    }

    private async Task OnPublishDownloadAsync(PublishDownloadMessage arg)
    {
        DownloadProgressSize = 0;

        await NotificationHelper.ShowInfoAsync($"推送下载 {arg.FileName} 文件大小 {arg.FileSize}");

        Task.Factory.StartNew(async () => await DownloadFileAsync(arg));
    }

    private async ValueTask DownloadFileAsync(PublishDownloadMessage arg)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", Path.GetRandomFileName() + arg.FileName);

        try
        {
            DownloadProgressSize = 0;
            var url = $"http://{HostUrl}/api/file/download/{arg.FileId}";
            //获取到文件总大小 通过head请求
            var processMessageHander = new ProgressMessageHandler(new HttpClientHandler());
            using var client = new HttpClient(processMessageHander);
            processMessageHander.HttpReceiveProgress += OnHttpReceiveProgress;

            await using var fileStream =
                new FileStream(path, FileMode.OpenOrCreate,
                    FileAccess.Write);
            var res = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            await using var file = await res.Content.ReadAsStreamAsync();
            var buffer = new byte[81920];
            double totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await file.ReadAsync(buffer)) != 0)
            {
                await Task.Delay(1);
                //await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalBytesRead += bytesRead;
                DownloadProgressSize = (totalBytesRead / arg.FileSize) * 100;
            }

            await NotificationHelper.ShowInfoAsync($"下载完毕 {arg.FileName} 文件大小 {arg.FileSize}");
        }
        catch (Exception e)
        {
            await NotificationHelper.ShowErrorAsync($"下载错误 {arg.FileName} 文件大小 {arg.FileSize}-{e.Message}");
        }
    }

    #endregion
}