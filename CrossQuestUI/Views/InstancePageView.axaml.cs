using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CrossQuestUI.Models;
using CrossQuestUI.ViewModels;

namespace CrossQuestUI.Views
{
    public partial class InstancePageView : UserControl
    {
        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            var vm = (InstancePageViewModel)DataContext;
            vm.OnUnload();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            var vm = (InstancePageViewModel)DataContext;
            vm.OnLoad();
        }

        public InstancePageView()
        {
            InitializeComponent();
        }
    }
}