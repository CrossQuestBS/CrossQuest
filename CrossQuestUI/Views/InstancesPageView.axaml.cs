using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CrossQuestUI.Models;
using CrossQuestUI.Services;
using CrossQuestUI.ViewModels;

namespace CrossQuestUI.Views
{
    public partial class InstancesPageView : UserControl
    {
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            var vm = (InstancesPageViewModel)DataContext;
            vm.OnLoad();
        }

        public InstancesPageView()
        {
            InitializeComponent();
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var moddingInstance = (ModdingInstance)button.DataContext;
            ModdingInstanceService.SelectedInstance = moddingInstance;
            RoutingService.GoToDestination(RoutingService.RoutingDestination.Instance);
        }
    }
}