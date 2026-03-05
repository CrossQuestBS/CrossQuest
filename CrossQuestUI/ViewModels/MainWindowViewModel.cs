using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrossQuestUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CrossQuestUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    [ObservableProperty]
    private bool _showNavigation;
    
    
    public MainWindowViewModel()
    {
        RoutingService.OnChangedPage += (sender, args) =>
        {
            CurrentPage = args.Page;
        };
        RoutingService.GoToDestination(RoutingService.RoutingDestination.Instances);
    }
    
    [ObservableProperty]
    private ViewModelBase _currentPage;
}