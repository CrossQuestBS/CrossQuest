using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrossQuestUI.Models;
using CrossQuestUI.Services;

namespace CrossQuestUI.ViewModels
{
    public partial class NewInstancePageViewModel : ViewModelBase
    {
        public ObservableCollection<ModdableGame> ModdableGameList { get; set; } = new ObservableCollection<ModdableGame>(new List<ModdableGame>());

        public ObservableCollection<ModInfo> AvailableMods { get; set; } = new ObservableCollection<ModInfo>(new List<ModInfo>());

        
        [ObservableProperty]
        private ModdableGame? _selectedGame;
        
        [ObservableProperty]
        private string _editorPath = "";
        
        [ObservableProperty]
        private string _questGame = "";
        
        [ObservableProperty]
        private string _gamePath = "";
        
        [ObservableProperty] private bool _hasFilledOut;

        private HashSet<string> SelectModsId { get; set; } = new ();
        
        [RelayCommand]
        public void GoBack()
        {
            RoutingService.GoToDestination(RoutingService.RoutingDestination.Instances);
        }
        
        public NewInstancePageViewModel()
        {
            PropertyChanged += (sender, args) =>
            {
                Update();
            };
        }

        public bool Update()
        {
            HasFilledOut = Directory.Exists(GamePath) && File.Exists(QuestGame) && Directory.Exists(EditorPath);
            return HasFilledOut;
        }


        public void OnUnload()
        {
            ModdableGameList.Clear();
            AvailableMods.Clear();
            SelectedGame = null;
        }

        [RelayCommand]
        public async Task CreateInstance()
        {
            var modsToInstall = AvailableMods.Where(it => SelectModsId.Contains(it.Id)).ToArray();

            await ModdingInstanceService.CreateInstance(EditorPath, SelectedGame.Value.Version, QuestGame, GamePath,
                modsToInstall);
            
            RoutingService.GoToDestination(RoutingService.RoutingDestination.Instances);
        }

        public void CheckedMod(string name, bool isChecked)
        {
            var mod = AvailableMods.FirstOrDefault(it => it.Name == name);
            if (isChecked)
            {
                SelectModsId.Add(mod.Id);
                return;
            }
        
            SelectModsId.Remove(mod.Id);
        }
        
        public void ChangedGame(ModdableGame game)
        {
            AvailableMods.Clear();
            foreach (var mod in game.Mods)
            {
                AvailableMods.Add(mod);
            }

            var unityEditor = App.Current.UnityEditors.FirstOrDefault(it => it.Version == game.UnityVersion);
            
            if (unityEditor is not null)
            {
                EditorPath = unityEditor.UnityPath;
            }

            var requiredMods = game.Mods.Where(it => it.Required).ToList();

            SelectModsId.Clear();
            foreach (var requiredMod in requiredMods)
            {
                SelectModsId.Add(requiredMod.Id);
            }

            SelectedGame = game;
        }

        private ModdableGame[]? GetGamesForDesign()
        {
            var client = App.Current?.Client;        
            var request = new HttpRequestMessage(HttpMethod.Get, "https://github.com/CrossQuestBS/Mods/raw/refs/heads/main/ModdableGames.json");
            var response = client?.Send(request);

            var responseStream = response?.Content.ReadAsStream();       
            using var memoryStream = new MemoryStream();
            responseStream?.CopyTo(memoryStream);
            var bytes = memoryStream.ToArray();
            var result = JsonSerializer.Deserialize<ModdableGame[]>(bytes);

            return result;
        }
        
        [RelayCommand]
        public async Task SetEditorPath()
        {
            var folder = await FileService.OpenFolderAsync();

            if (folder is not null)
            {
                EditorPath = Uri.UnescapeDataString(folder.Path.AbsolutePath);
            }
        }
        
        [RelayCommand]
        public async Task SetQuestGamePath()
        {
            var folder = await FileService.OpenFileAsync();

            if (folder is not null)
            {
                QuestGame = Uri.UnescapeDataString(folder.Path.AbsolutePath);
            }
        }
        
        [RelayCommand]
        public async Task SetGamePath()
        {
            var folder = await FileService.OpenFolderAsync();

            if (folder is not null)
            {
                GamePath = Uri.UnescapeDataString(folder.Path.AbsolutePath);
            }
        }

        public void OnLoad()
        {
            var moddableGames = Design.IsDesignMode ? GetGamesForDesign() : App.Current.ModdableGames;
            foreach (var game in moddableGames)
            {
                ModdableGameList.Add(game);
            }

            if (ModdableGameList.Count > 0)
            {
                SelectedGame = ModdableGameList[0];
            }
        }
    }
}