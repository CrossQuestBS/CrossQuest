using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace CrossQuestUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    [ObservableProperty]
    private bool _showNavigation;

    private void OnPageChange()
    {
        NavigateNextCommand.NotifyCanExecuteChanged();
        NavigateBackCommand.NotifyCanExecuteChanged();
        CurrentPage.PropertyChanged += OnPropertyChanged;
    }

    void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        NavigateNextCommand.NotifyCanExecuteChanged();
        NavigateBackCommand.NotifyCanExecuteChanged();
    }

    private PageViewModelBase? GetPageAtIndex(int index)
    {
        var page = _pages[index];
        
        if (_pages.Count > index)
            return page;
        
        return null;
    }
    
    public MainWindowViewModel()
    {
        _pageTypes =
        [
            typeof(InstancesViewModel)
        ];
        
        foreach (var type in _pageTypes)
        {
            _pages.Add(App.Current.Services.GetRequiredService(type) as PageViewModelBase);
        }
        
        var page = GetPageAtIndex(0);
        
        if (page is null)
            return;

        CurrentPage = page;
        ShowNavigation = CurrentPage.HasNavigation;
        NavigateNextCommand = new RelayCommand(NavigateNext, () => CurrentPage.CanNavigateNext);
        NavigateBackCommand = new RelayCommand(NavigateBack, () => CurrentPage.CanNavigatePrevious);
        OnPageChange();
    }
    
    public IRelayCommand NavigateNextCommand { get; }
    
    public IRelayCommand NavigateBackCommand { get; }
    
    private void NavigateNext()
    {
        // get the current index and add 1
        var index = _pages.IndexOf(CurrentPage) + 1;

        var page = GetPageAtIndex(index);
        
        if (page is null)
            return;
        
        CurrentPage = page;
        ShowNavigation = CurrentPage.HasNavigation;
        OnPageChange();
    }
    
    private void NavigateBack()
    {
        // get the current index and subtract 1
        var index = _pages.IndexOf(CurrentPage) - 1;

        if (index < 0)
        {
            return;
        }

        var page = GetPageAtIndex(index);
        
        if (page is null)
            return;

        CurrentPage = page;
        ShowNavigation = CurrentPage.HasNavigation;
        OnPageChange();
    }

    
    [ObservableProperty]
    private PageViewModelBase _currentPage;

    [ObservableProperty] private List<PageViewModelBase> _visitedPages = [];

    private readonly List<PageViewModelBase> _pages = new List<PageViewModelBase>();
    private readonly Type[] _pageTypes;
}