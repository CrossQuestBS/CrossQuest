using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using CrossQuestUI.Models;
using CrossQuestUI.Services;
using CrossQuestUI.ViewModels;
using CrossQuestUI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CrossQuestUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();


        services.AddCommonServices();


        Services = services.BuildServiceProvider();


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var applicationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var crossQuestAppPath = Path.Join(applicationPath, "CrossQuestBS");


            // Creats a path if it does not exist!
            Directory.CreateDirectory(crossQuestAppPath);

            ApplicationPath = crossQuestAppPath;

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var vm = Services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };

            StorageProvider = desktop.MainWindow.StorageProvider;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }


    public new static App? Current => Application.Current as App;

    public IServiceProvider? Services { get; private set; }

    public IStorageProvider? StorageProvider { get; private set; }


    public ModdingConfig ModdingConfig { get; set; }

    public string ApplicationPath { get; private set; }
}