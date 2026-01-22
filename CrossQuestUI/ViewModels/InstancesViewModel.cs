using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrossQuestUI.Models;
using CrossQuestUI.Services;

namespace CrossQuestUI.ViewModels
{
    public partial class InstancesViewModel(IModdingInstanceService moddingInstanceService) : PageViewModelBase
    {
        public static string Title => "CrossQuest";
        public ObservableCollection<ModdingInstance> ModdingInstances { get; set; } = new ObservableCollection<ModdingInstance>(new List<ModdingInstance>());

        [ObservableProperty] private CreateInstanceViewModel _createInstanceViewModel = new CreateInstanceViewModel();
        
        
        public async Task OnLoad()
        {
            var instances = await moddingInstanceService.GetInstanceList();

            foreach (var instance in instances)
            {
                ModdingInstances.Add(instance);
            }
        }

        [RelayCommand]
        public void CreateInstance()
        {
            CreateInstanceViewModel.OnCreate += (sender, args) =>
            {
                 Dispatcher.UIThread.Invoke(async () =>
                 {
                    var instance = await  moddingInstanceService.CreateInstance(CreateInstanceViewModel.EditorPath,
                         CreateInstanceViewModel.Version, CreateInstanceViewModel.QuestGame,
                         CreateInstanceViewModel.GamePath);
                    ModdingInstances.Add(instance);
                 });
                
            };
            CreateInstanceViewModel.Show();
        }
        
        public override bool HasNavigation
        {
            get => true;
            protected set => throw new NotSupportedException();
        }
        
        public override bool CanNavigateNext
        {
            get => true;
            protected set => throw new NotSupportedException();
        }

        // You cannot go back from this page
        public override bool CanNavigatePrevious
        {
            get => false;
            protected set => throw new NotSupportedException();
        }
    }
}