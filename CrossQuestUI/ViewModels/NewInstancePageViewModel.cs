using CommunityToolkit.Mvvm.Input;
using CrossQuestUI.Services;

namespace CrossQuestUI.ViewModels
{
    public partial class NewInstancePageViewModel : ViewModelBase
    {
        [RelayCommand]
        public void GoBack()
        {
            RoutingService.GoToDestination(RoutingService.RoutingDestination.Instances);
        }
    }
}