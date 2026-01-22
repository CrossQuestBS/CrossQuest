using Avalonia.Platform.Storage;
using CrossQuestUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CrossQuestUI.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDesignServices(this IServiceCollection collection)
        {
            collection.AddSingleton<IModdingInstanceService, ModdingInstanceServiceMock>();
            collection.AddTransient<InstancesViewModel>();

        }
        
        public static void AddCommonServices(this IServiceCollection collection)
        {
            collection.AddSingleton<IModdingService, ModdingService>();
            collection.AddSingleton<IStreamLogger, StreamLogger>();
            collection.AddSingleton<IDownloader, Downloader>();
            collection.AddSingleton<IGithubDownloader, GithubDownloader>();
            collection.AddSingleton<IAndroidService, AndroidService>();
            collection.AddSingleton<IFilesService, FilesService>();
            collection.AddSingleton<IUnityEditor, UnityEditor>();
            collection.AddSingleton<IProcessCaller, ProcessCaller>();
            collection.AddSingleton<IModdingInstanceService, ModdingInstanceService>();

            collection.AddTransient<MainWindowViewModel>();
            collection.AddTransient<InstancesViewModel>();
            collection.AddTransient<ConfigSetupViewModel>();
            collection.AddTransient<VerificationViewModel>();
            collection.AddTransient<BaseFileDownloadViewModel>();
            collection.AddTransient<InitialModSetupViewModel>();
            collection.AddTransient<ModdingProcessViewModel>();
            collection.AddTransient<DashboardViewModel>();
        }
    }
}