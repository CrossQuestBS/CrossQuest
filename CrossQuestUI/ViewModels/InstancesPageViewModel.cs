using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrossQuestUI.Models;
using CrossQuestUI.Services;

namespace CrossQuestUI.ViewModels
{
    public partial class InstancesPageViewModel() : ViewModelBase
    {
        public static string Title => "CrossQuest";
        public ObservableCollection<ModdingInstance> ModdingInstances { get; set; } = new ObservableCollection<ModdingInstance>(new List<ModdingInstance>());
        
        public async Task OnLoad()
        {
            ModdingInstances.Clear();
            var instances = await ModdingInstanceService.GetInstanceList();

            foreach (var instance in instances)
            {
                ModdingInstances.Add(instance);
            }
        }

        [RelayCommand]
        public void CreateInstance()
        {
            RoutingService.GoToDestination(RoutingService.RoutingDestination.NewInstance);
        }
    }
}