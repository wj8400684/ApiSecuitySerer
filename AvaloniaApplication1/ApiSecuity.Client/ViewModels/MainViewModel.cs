using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Threading;
using System.Threading.Tasks;
using ApiSecuity.Client.Helper;
using ApiSecuity.Client.Model;
using ApiSecuityServer.Message;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using NewLife;
using NewLife.Log;
using NewLife.Serialization;

namespace ApiSecuity.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private IStorageProvider _storageProvider = null!;
    private const string HostUrl = "192.168.124.86:6767";
    private HubConnection _connection = null!;
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly AsyncQueue<DownloadFileMessage> _downloadQueue = new();
    private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www");

    [ObservableProperty] private string? _targetFileId;
    [ObservableProperty] private string? _targetConnectionId;
    [ObservableProperty] private long _totalFileSize;
    [ObservableProperty] private double _downloadProgressSize;
    [ObservableProperty] private double _uploadProgressSize;
    [ObservableProperty] private string? _connectedId;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string? _downloadProgress;
    [ObservableProperty] private string? _downloadDescription;
    [ObservableProperty] private string? _uploadDescription;
    [ObservableProperty] private string? _nickName;
    [ObservableProperty] private ConnectionModel? _selectedConnection;
    [ObservableProperty] private ObservableCollection<ConnectionModel> _clientCollection = [];

    public MainViewModel()
    {
        if (!Directory.Exists(FilePath))
            Directory.CreateDirectory(FilePath);

        ThreadPool.QueueUserWorkItem(callBack => StartDownloadAsync());

        
        var info = new MachineInfo();
        info.Init();
        Console.WriteLine(info.ToJson(true));
        Console.WriteLine(
            $"MachineName {Environment.MachineName} UserDomainName{Environment.UserDomainName} version {Environment.Version} cpu usage {Environment.CpuUsage} ProcessorCount {Environment.ProcessorCount} UserName {Environment.UserName}");
    }

    //{
    //   "OSName": "Windows 11 专业版",
    //   "OSVersion": "10.0.26100",
    //   "Product": "FA880 PRO",
    //   "Vendor": "FEVM",
    //   "Processor": "AMD Ryzen 7 8845HS w/ Radeon 780M Graphics",
    //   "UUID": "33B39900-8E0A-11EF-B099-54AD601B7301",
    //   "Guid": "5c6fc885-83a9-425b-80cd-c28a7fa63179",
    //   "Serial": null,
    //   "Board": null,
    //   "DiskID": null,
    //   "Memory": 67433156608,
    //   "AvailableMemory": 51833106432,
    //   "CpuRate": 0.0744,
    //   "UplinkSpeed": 0,
    //   "DownlinkSpeed": 0,
    //   "Temperature": 0,
    //   "Battery": 1
    // }
    
    private void NewHubConnection()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(
                $"ws://{HostUrl}/api/chat?&platform={Random.Shared.Next(1, 3)}&groupName=local&user=wujun&nickName={NickName}")
            .AddJsonProtocol()
            .ConfigureJsonHubOptions()
            .Build();

        _connection.Closed += OnClosedHandlerAsync;
        _connection.RegisterHandler<ConnectionInfoMessage>(ServerClientApiCommand.SyncConnectionInfo,
            OnSyncConnectionInfoHandlerMessage);
        _connection.RegisterHandler<ConnectionEventMessage>(ServerClientApiCommand.ConnectionEvent,
            OnConnectionEventHandlerAsync);
        _connection.RegisterHandler<ConnectionEventMessage>(ServerClientApiCommand.DisConnectionEvent,
            OnDisconnectionEventHandlerAsync);
        _connection.RegisterHandler<DownloadFileMessage>(ServerClientApiCommand.DownloadFileEvent,
            OnDownloadFileHandlerAsync);
    }

    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    #region mvvmcommand

    [RelayCommand]
    private async Task OnConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(NickName))
        {
            await NotificationHelper.ShowErrorAsync("清先设置用户名");
            return;
        }

        NewHubConnection();

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

        if (SelectedConnection == null)
        {
            await NotificationHelper.ShowErrorAsync("请选择客户端");
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

            foreach (var folder in folders)
            {
                var stream = await folder.OpenReadAsync();
                stream.Position = 0;
                await NotificationHelper.ShowInfoAsync($"正在上传");

                long bufferLength = 1024 * 1024 * 1000; //100m

                if (stream.Length < bufferLength) //大于500m才需要分组上传
                    bufferLength = stream.Length;

                var chunks = stream.Length / bufferLength;

                if (chunks == 0)
                    chunks = 1;

                //var url = $"http://{HostUrl}/api/file/upload";
                var url1 =
                    $"http://{HostUrl}/api/file/upload?ConnectionId={SelectedConnection.Id}&FileName={folder.Name}&PartNumber={0}&Chunks={chunks}&Size=81960&Start={1}&End={2}&Total={stream.Length}";

                var partNumber = 0;
                var start = 0;
                var end = 0;

                var sizeDescription = string.Empty;

                if (stream.Length < 1024)
                    sizeDescription = $"{stream.Length} bit";
                else if (stream.Length > 1024 && stream.Length < 1048576) //小鱼1m
                    sizeDescription = $"{stream.Length / (double)1024} kb";
                else if (stream.Length > 1048576 && stream.Length < 1073741824) //小于1g
                    sizeDescription = $"{stream.Length / (double)1048576} mb";
                else
                    sizeDescription = $"{stream.Length / (double)1073741824} gb";

                UploadDescription = $"({folder.Name}-{sizeDescription})";

                while (true)
                {
                    UploadProgressSize = 0;

                    var buffer = new byte[bufferLength]; //每次上传10m
                    //var size = await stream.ReadAsync(buffer);

                    var size = 100;
                    if (partNumber == 0)
                        start = 0;
                    else
                        start += size;

                    end = start + size;

                    var url = string.Format(url1, partNumber.ToString(), start, end);

                    if (size == 0)
                        break;

                    var processMessageHander = new ProgressMessageHandler(new HttpClientHandler());
                    processMessageHander.HttpSendProgress += OnHttpSendProgress;
                    using var client = new HttpClient(processMessageHander);
                    var data = new MultipartFormDataContent();

                    var content = new StreamContent(stream, 4096);
                    data.Add(content, "file-name", folder.Name);
                    var resp = await client.PostAsync(url, data);
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine(resp.Content.ReadAsStringAsync().Result);
                    }

                    partNumber++;

                    break;
                }

                UploadDescription = "完成";

                await NotificationHelper.ShowInfoAsync($"上传完毕");
            }
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
        UploadProgressSize = e.ProgressPercentage;
        Console.WriteLine(e.ProgressPercentage);
    }

    /// <summary>
    /// 下载回调
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnHttpReceiveProgress(object? sender, HttpProgressEventArgs e)
    {
        Console.WriteLine(e.ProgressPercentage);
    }

    #endregion

    #region hubcallback

    private async Task OnClosedHandlerAsync(Exception? arg)
    {
        IsConnected = false;

        await NotificationHelper.ShowErrorAsync($"断开连接{arg?.Message}");

        await _semaphore.WaitAsync();

        try
        {
            for (var i = 0; i < ClientCollection.Count; i++)
            {
                ClientCollection.RemoveAt(i);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 同步其他客户端连接信息
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private async Task OnSyncConnectionInfoHandlerMessage(ConnectionInfoMessage arg)
    {
        await _semaphore.WaitAsync();

        try
        {
            for (var j = 0; j < ClientCollection.Count; j++)
            {
                ClientCollection.RemoveAt(j);
            }

            foreach (var connection in arg.Clients)
            {
                ClientCollection.Add(new ConnectionModel
                {
                    Id = connection.ConnectionId,
                    Host = connection.Host,
                    Port = connection.Port,
                    NickName = connection.NickName,
                    Platform = 1,
                });
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 下载通知
    /// </summary>
    /// <param name="arg"></param>
    private async Task OnDownloadFileHandlerAsync(DownloadFileMessage arg)
    {
        await NotificationHelper.ShowInfoAsync($"推送下载 {arg.FileName} 文件大小 {arg.FileSize}");

        _downloadQueue.Enqueue(arg);
    }

    /// <summary>
    /// 断开连接通知
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private async Task OnDisconnectionEventHandlerAsync(ConnectionEventMessage arg)
    {
        await NotificationHelper.ShowInfoAsync($"{arg.NickName} 离线");

        await _semaphore.WaitAsync();

        try
        {
            var connections = ClientCollection.ToList();

            foreach (var c in connections.Where(c => c.Id == arg.ConnectionId))
                ClientCollection.Remove(c);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 连接通知
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private async Task OnConnectionEventHandlerAsync(ConnectionEventMessage arg)
    {
        await NotificationHelper.ShowInfoAsync($"{arg.NickName} 上线");

        await _semaphore.WaitAsync();

        try
        {
            ClientCollection.Add(new ConnectionModel
            {
                Id = arg.ConnectionId,
                Host = arg.Host,
                Port = arg.Port,
                NickName = arg.NickName,
                Platform = 1,
            });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    #endregion

    private async ValueTask StartDownloadAsync()
    {
        await foreach (var arg in _downloadQueue.ReadAsync(CancellationToken.None))
        {
            try
            {
                var path = Path.Combine(FilePath, arg.FileName);

                var sizeDescription = string.Empty;

                if (arg.FileSize < 1024)
                    sizeDescription = $"{arg.FileSize} bit";
                else if (arg.FileSize > 1024 && arg.FileSize < 1048576) //小鱼1m
                    sizeDescription = $"{arg.FileSize / (double)1024} kb";
                else if (arg.FileSize > 1048576 && arg.FileSize < 1073741824) //小于1g
                    sizeDescription = $"{arg.FileSize / (double)1048576} mb";
                else
                    sizeDescription = $"{arg.FileSize / (double)1073741824} gb";

                DownloadDescription = $"({arg.FileName}-{sizeDescription})";

                DownloadProgressSize = 0;
                var url = $"http://{HostUrl}/api/file/download/{arg.FileId}";
                //获取到文件总大小 通过head请求
                var processMessageHander = new ProgressMessageHandler(new HttpClientHandler());
                using var client = new HttpClient(processMessageHander);
                processMessageHander.HttpReceiveProgress += OnHttpReceiveProgress;

                await using var fileStream =
                    new FileStream(path, FileMode.OpenOrCreate,
                        FileAccess.Write);
                using var httpResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                await using var responseStream = await httpResponse.Content.ReadAsStreamAsync();

                int bytesRead;
                double totalBytesRead = 0;
                var buffer = new byte[81920];
                while ((bytesRead = await responseStream.ReadAsync(buffer)) != 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalBytesRead += bytesRead;
                    DownloadProgressSize = (totalBytesRead / arg.FileSize) * 100;
                }

                DownloadDescription = $"{arg.FileName} 完成";
                await NotificationHelper.ShowInfoAsync($"下载完毕 {arg.FileName} 文件大小 {arg.FileSize}");
            }
            catch (Exception e)
            {
                await NotificationHelper.ShowErrorAsync($"下载错误 {arg.FileName} 文件大小 {arg.FileSize}-{e.Message}");
            }
        }
    }
}