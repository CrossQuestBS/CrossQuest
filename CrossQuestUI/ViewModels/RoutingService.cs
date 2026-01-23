using System;
using CrossQuestUI.ViewModels;

namespace CrossQuestUI.Services
{
    public static class RoutingService
    {
        public class DestinationEventArgs(ViewModelBase page) : EventArgs
        {
            public ViewModelBase Page { get; } = page;
        }
        
        public enum RoutingDestination
        {
            Instances,
            NewInstance,
            
        }

        private static InstancesPageViewModel _instancesPageViewModel = new (new ModdingInstanceService());
        private static NewInstancePageViewModel _newInstancePageViewModel = new();
        
        public static event EventHandler<DestinationEventArgs> OnChangedPage;

        public static void GoToDestination(RoutingDestination destination)
        {
            switch (destination)
            {
                case RoutingDestination.Instances:
                    OnChangedPage(null, new DestinationEventArgs(_instancesPageViewModel));
                    break;
                case RoutingDestination.NewInstance:
                    OnChangedPage(null, new DestinationEventArgs(_newInstancePageViewModel));
                    break;
            }
        }
        

    }
}