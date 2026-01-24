using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrossQuestUI.Models;
using CrossQuestUI.Services;

namespace CrossQuestUI.ViewModels
{
    public partial class InstancePageViewModel : ViewModelBase
    {

        public ObservableCollection<ModInfo> SelectedMods { get; set; } = new ObservableCollection<ModInfo>(new List<ModInfo>());

        
        [ObservableProperty]
        private ModdingInstance _moddingInstance;

        [ObservableProperty]
        private bool _readOnlyMods;

        public void OnUnload()
        {
            SelectedMods.Clear();
        }
        
        public async Task OnLoad()
        {
            ReadOnlyMods = true;
            if (ModdingInstanceService.SelectedInstance != null)
                ModdingInstance = ModdingInstanceService.SelectedInstance;
            else
                ModdingInstance = (await ModdingInstanceService.GetInstanceList())[0];

            foreach (var mod in ModdingInstance.Mods)
            {
                SelectedMods.Add(mod);
            }
        }
        
        
        [RelayCommand]
        public void GoBack()
        {
            RoutingService.GoToDestination(RoutingService.RoutingDestination.Instances);
        }

    }
}