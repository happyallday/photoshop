using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotoshopApp.Core.History;
using PhotoshopApp.Core.ImageProcessing;
using PhotoshopApp.Core.Layers;
using PhotoshopApp.Core.Tools;
using PhotoshopApp.Services;
using PhotoshopApp.UI.ViewModels;
using System.Windows;

namespace PhotoshopApp;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Core services
                services.AddSingleton<ILayerManager, LayerManager>();
                services.AddSingleton<IEditHistory, EditHistory>();
                services.AddSingleton<IImageProcessor, ImageProcessor>();
                services.AddSingleton<ToolManager>();
                services.AddSingleton<ImageAdjuster>();
                
                // UI services
                services.AddSingleton<IFileDialogService, FileDialogService>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<ToolsViewModel>();
                services.AddTransient<ImageAdjustmentsViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}