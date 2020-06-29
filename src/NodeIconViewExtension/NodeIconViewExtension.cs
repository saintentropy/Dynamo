using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Wpf.Extensions;
using Microsoft.Practices.Prism.ViewModel;

namespace NodeIcon
{
    public class NodeIconViewExtension : NotificationObject, IViewExtension
    {
        private const String extensionName = "Node Icon View";
        private ViewLoadedParams viewLoadedParams;
        private WorkspaceModel currentWorkspace;
        private WorkspaceViewModel currentWorkspaceViewModel;
        private DynamoViewModel dynamoViewModel;
        private ObservableCollection<NodeIconViewModel> Icons { get; } = new ObservableCollection<NodeIconViewModel>();
        private ResourceDictionary dataTemplatesDictionary;
        internal MenuItem nodeIconMenuItem;
        private bool isInitialized;
        private bool iconsVisible;

        public bool IconsVisible
        {
            get { return iconsVisible; }
            set
            {
                if (iconsVisible != value)
                {
                    iconsVisible = value;
                    RaisePropertyChanged("Visible");
                }
            }
        }

        /// <summary>
        /// Extension Name
        /// </summary>
        public string Name
        {
            get { return extensionName; }
        }

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public string UniqueId
        {
            get { return "AFD8D738-3324-46BE-95AA-51F403B4D75F"; }
        }

        public void Startup(ViewStartupParams viewLoadedParams)
        {

        }

        public void Loaded(ViewLoadedParams p)
        {
            viewLoadedParams = p;
            currentWorkspace = p.CurrentWorkspaceModel as WorkspaceModel;
            dynamoViewModel = (DynamoViewModel)p.DynamoWindow.DataContext;

            p.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnCurrentWorkspaceCleared;

            // Location DataTemplate.xaml file so the resource can be injected into the WorkspaceViewModel
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var uri = new Uri(Path.Combine(path, "DataTemplates.xaml"));
            dataTemplatesDictionary = new ResourceDictionary() {Source = uri};

            // Adding a button in view menu to refresh and show node icons manually
            nodeIconMenuItem = new MenuItem { Header = "Show Node Icons", IsCheckable = true, IsChecked = false };
            nodeIconMenuItem.Click += (sender, args) =>
            {
                if (nodeIconMenuItem.IsChecked)
                {
                    // Refresh dependency data
                    AddDataTemplate();
                    InitializeGraphIconViews();
                    IconsVisible = true;
                    nodeIconMenuItem.IsChecked = true;
                }
                else
                {
                    IconsVisible = false;
                    nodeIconMenuItem.IsChecked = false;
                }

            };
            
            viewLoadedParams.AddMenuItem(MenuBarType.View, nodeIconMenuItem);
        }

        public void Shutdown()
        {
            viewLoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            viewLoadedParams.CurrentWorkspaceCleared -= OnCurrentWorkspaceCleared;
        }

        public void Dispose()
        {
        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel workspaceModel)
        {
            if (workspaceModel is WorkspaceModel)
            {
                // Unsubscribe from previous workspace
                if (currentWorkspace != null)
                {
                    currentWorkspace.NodeAdded -= OnNodeAdded;
                    currentWorkspace.NodeRemoved -= OnNodeRemoved;
                    currentWorkspace.NodesCleared -= OnNodesCleared;
                    Icons.Clear();
                    currentWorkspace = null;
                    iconsVisible = false;
                    nodeIconMenuItem.IsChecked = false;
                    isInitialized = false;
                }

                // Update to current workspace
                currentWorkspace = workspaceModel as WorkspaceModel;
                currentWorkspace.NodeAdded += OnNodeAdded;
                currentWorkspace.NodeRemoved += OnNodeRemoved;
                currentWorkspace.NodesCleared += OnNodesCleared;
                currentWorkspaceViewModel = dynamoViewModel.CurrentSpaceViewModel;

                var iconsColl = new CollectionContainer { Collection = Icons };
                currentWorkspaceViewModel.WorkspaceElements.Add(iconsColl);
            }
        }

        private void OnCurrentWorkspaceCleared(IWorkspaceModel workspaceModel)
        {
            OnCurrentWorkspaceChanged(workspaceModel);
        }

        private void OnNodeAdded(NodeModel node)
        {
            if (isInitialized)
            {
                var nodeIconViewModel = new NodeIconViewModel(dynamoViewModel, node, this);
                Icons.Add(nodeIconViewModel);
            }
        }

        private void InitializeGraphIconViews()
        {
            if (!isInitialized)
            {
                isInitialized = true;

                foreach (var node in currentWorkspace.Nodes)
                {
                    OnNodeAdded(node);
                }
            }
        }

        private void OnNodeRemoved(NodeModel node)
        {
            var iconViewModel = Icons.FirstOrDefault(x => x.NodeModel == node);
            if (iconViewModel != null)
            {
                Icons.Remove(iconViewModel);
                iconViewModel.Dispose();
            }
        }

        private void OnNodesCleared()
        {
            foreach (var iconViewModel in Icons)
            {
                iconViewModel.Dispose();
            }
            Icons.Clear();
        }

        private void AddDataTemplate()
        {
            var views = FindVisualChildren<WorkspaceView>(viewLoadedParams.DynamoWindow);

            foreach (var view in views)
            {
                view.Resources.MergedDictionaries.Add(dataTemplatesDictionary);
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
