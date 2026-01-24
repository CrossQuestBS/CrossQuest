using Avalonia.Platform.Storage;
using CrossQuestUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CrossQuestUI.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDesignServices(this IServiceCollection collection)
        {
            collection.AddTransient<InstancesPageViewModel>();

        }
        
        public static void AddCommonServices(this IServiceCollection collection)
        {
            collection.AddTransient<MainWindowViewModel>();
            collection.AddTransient<InstancesPageViewModel>();
            collection.AddTransient<DashboardViewModel>();
        }
    }
}