using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CrossQuestUI.Models;
using CrossQuestUI.ViewModels;

namespace CrossQuestUI.Views
{
    public partial class NewInstancePageView : UserControl
    {
        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            var vm = (NewInstancePageViewModel)DataContext;
            vm.OnUnload();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            var vm = (NewInstancePageViewModel)DataContext;
            vm.OnLoad();
        }

        public NewInstancePageView()
        {
            InitializeComponent();
        }

        private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var vm = (NewInstancePageViewModel)DataContext;
            var checkbox = (ComboBox)sender;

            if (checkbox.SelectedItem is ModdableGame game)
            {
                vm.ChangedGame(game);
            }
        }

        private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            var vm = (NewInstancePageViewModel)DataContext;
            var checkbox = (CheckBox)sender;
            
            vm.CheckedMod(checkbox.Content as string, checkbox.IsChecked ?? false);
        }
    }
}