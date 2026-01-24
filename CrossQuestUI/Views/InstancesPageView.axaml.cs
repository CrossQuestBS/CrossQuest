using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
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
    }
}