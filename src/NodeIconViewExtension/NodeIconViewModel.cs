using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

namespace NodeIcon
{
    public class NodeIconViewModel : ViewModelBase
    {

        #region Properties

        private readonly DynamoViewModel dynamoViewModel;
        private readonly NodeIconViewExtension viewExtension;

        public NodeModel NodeModel { get; }

        private string nodeName;

        public string NodeName
        {
            get
            {
                if (nodeName == null)
                {
                    nodeName = NodeModel.CreationName;
                    if (nodeName == "")
                    {
                        nodeName = NodeModel.GetType().FullName;
                    }

                }

                return nodeName;
            }

            private set => nodeName = value;
        }

        private double zIndex;
        
        public double ZIndex
        {
            get { return zIndex; }
            set { zIndex = value; RaisePropertyChanged("ZIndex"); }
        }

        public double Left
        {
            get
            {
                return NodeModel.CenterX - (120 / 2);
            }
        }

        public double Top
        {
            get
            {
                return NodeModel.CenterY + 5 - (120 / 2);
            }
        }

        public Visibility Visible
        {
            get 
            {
                return LargeIcon == null || !viewExtension.IconsVisible ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private ImageSource largeIcon;
        
        public ImageSource LargeIcon
        {
            get
            {
                if (largeIcon == null)
                {
                    var svm = dynamoViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel?.FindViewModelForNode(NodeName); 
                    if (svm != null)
                    {
                        largeIcon = svm.LargeIcon;
                    }

                }

                return largeIcon;
            }

            private set => largeIcon = value;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dynamoViewModel"></param>
        /// <param name="nodeModel"></param>
        /// <param name="nodeIconViewExtension"></param>
        public NodeIconViewModel(DynamoViewModel dynamoViewModel, NodeModel nodeModel, NodeIconViewExtension nodeIconViewExtension, ImageSource largeIcon = null)
        {
            this.dynamoViewModel = dynamoViewModel;
            this.NodeModel = nodeModel;
            this.viewExtension = nodeIconViewExtension;
            this.largeIcon = largeIcon;
            nodeIconViewExtension.PropertyChanged += PropertyChangedHandler;
            nodeModel.PropertyChanged += PropertyChangedHandler;
            ZIndex = 500; //Todo make reactive to the node highest Z index.
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                case "Width":
                    RaisePropertyChanged("Left");
                    break;
                case "Y":
                case "Height":
                    RaisePropertyChanged("Top");
                    break;
                case "Position":
                    RaisePropertyChanged("Left");
                    RaisePropertyChanged("Top");
                    break;
                case "Visible":
                    RaisePropertyChanged("Visible");
                    break;
            }
        }

        public override void Dispose()
        {
            NodeModel.PropertyChanged -= PropertyChangedHandler;
            viewExtension.PropertyChanged -= PropertyChangedHandler;
            base.Dispose();
        }
    }
}
