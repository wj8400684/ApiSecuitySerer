using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using ApiSecuity.Client.ViewModels;
using ApiSecuity.Client.Views;
using ApiSecuityServer.Model;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using NewLife;

namespace ApiSecuity.Client;

public partial class App : Application
{
    internal const string HostUrl = "localhost:6767";

    internal static readonly HttpClient Http = new()
    {
        BaseAddress = new Uri($"http://{HostUrl}")
    };

    internal static System.Text.Json.JsonSerializerOptions HttpSerializerOptions = new()
    {
        TypeInfoResolverChain = { AppJsonSerializerContext.Default },
        IgnoreReadOnlyProperties = true,
        DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
    };

    public override void Initialize()
    {
        RegDevices();
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// 注册设备信息
    /// </summary>
    private async void RegDevices()
    {
        var machine = new MachineInfo();
        machine.Init();

        var register = new ClientRegisterRequest(
            OSName: machine.OSName,
            OSVersion: machine.OSVersion,
            Product: machine.Product,
            Vendor: machine.Vendor,
            Processor: machine.Processor,
            UUID: machine.UUID,
            Guid: machine.Guid,
            Memory: (long)machine.Memory);

        var jsonContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(register, HttpSerializerOptions),
            Encoding.UTF8,
            "application/json"
        );

        var response = await Http.PostAsync("api/client/register", jsonContent);

        if (response.IsSuccessStatusCode)
            return;

        await response.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}