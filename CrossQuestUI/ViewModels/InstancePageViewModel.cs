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
        private string _moddingStatus;
        
        [ObservableProperty]
        private ModdingInstance _moddingInstance;

        [ObservableProperty]
        private bool _readOnlyMods;

        public void OnUnload()
        {
            SelectedMods.Clear();
        }

        [RelayCommand]
        public async Task Compile()
        {
            ModdingStatus = "Compiling, this will take up to 10-30+ minutes.";
            if (await ModdingInstance.CompileProject())
            {
                ModdingStatus = "Done compiling!";
            }
            else
            {
                ModdingStatus = "Failed to compile :(";
            }
            ModdingStatus = "Building APK...";
            await ModdingInstance.BuildModdedApk();
            ModdingStatus = "Done compiling and building apk!";
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