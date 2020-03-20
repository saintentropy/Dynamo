using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Dynamo.Configuration;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.UI;
using Dynamo.Wpf.Utilities;
using NodeIconViewModel = Dynamo.ViewModels.NodeIconViewModel;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for PreviewInfoBubble.xaml
    /// </summary>
    public partial class NodeIconView : UserControl
    {
        #region Properties

        private NodeIconViewModel viewModel = null;

        public NodeIconViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                if (viewModel == null)
                {
                    viewModel = value;
                    viewModel.PropertyChanged += ViewModel_PropertyChanged;
                }
            }
        }

        private bool IsDisconnected { get { return (this.ViewModel == null); } }


        #endregion

        public NodeIconView()
        {
            InitializeComponent();

            // Make sure to unsubscribe to the event handlers
            Unloaded += (s, e) =>
            {
                Dispose();
            };

            this.DataContextChanged += NodeIconView_DataContextChanged;
        }

        private void NodeIconWindowUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                UpdatePosition();
            }
        }

        private void NodeIconView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = e.NewValue as NodeIconViewModel;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // The fix for the following issue was previously performed in 
            // NodeModel. This is shifted over to infobubble to centralize 
            // the issue until code restructuring is completed.
            //
            // This is a temporarily measure, it work by dispatching the 
            // work to UI thread when info bubble UI values need to be 
            // modified by background evaluation thread.
            // To completely solve this, changes affecting UI values should be 
            // restructured into UI Binding in order for things to be thread 
            // safe. 
            // The above mentioned issue is being documented in:
            //
            //      http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-847
            //
            Action propertyChanged = (() =>
            {
                switch (e.PropertyName)
                {
                    case "TargetTopLeft":
                    case "TargetBotRight":
                        UpdatePosition();
                        break;
                }
            });

            if (this.ViewModel.DynamoViewModel.UIDispatcher != null &&
                this.ViewModel.DynamoViewModel.UIDispatcher != null)
            {
                if (this.ViewModel.DynamoViewModel.UIDispatcher.CheckAccess())
                    propertyChanged();
                else
                    this.ViewModel.DynamoViewModel.UIDispatcher.BeginInvoke(propertyChanged);
            }
        }

        #region Update Position

        private void UpdatePosition()
        {
            IconGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double estimatedHeight = IconGrid.DesiredSize.Height;
            double estimatedWidth = IconGrid.DesiredSize.Width;
            
            IconGrid.Margin = GetMargin(estimatedHeight, estimatedWidth);
        }

        private System.Windows.Thickness GetMargin(double estimatedHeight, double estimatedWidth)
        {
            System.Windows.Thickness margin = new System.Windows.Thickness();
            double nodeCenterX = (ViewModel.TargetBotRight.X - ViewModel.TargetTopLeft.X)/2 + ViewModel.TargetTopLeft.X;
            double nodeCenterY = (ViewModel.TargetBotRight.Y - ViewModel.TargetTopLeft.Y)/2 + ViewModel.TargetTopLeft.Y;
            margin.Left = -IconGrid.ActualWidth/2 + nodeCenterX;
            margin.Top = -IconGrid.ActualHeight/2 + nodeCenterY + 5;
            return margin;
        }
        #endregion

        /// <summary>
        /// Dispose function adding resubscribe logic
        /// </summary>
        public void Dispose()
        {
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
    }
}