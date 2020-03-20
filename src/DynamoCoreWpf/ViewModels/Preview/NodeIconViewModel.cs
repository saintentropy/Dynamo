using System;
using System.Windows;
using System.Windows.Media;
using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public class NodeIconViewModel : ViewModelBase
    {

        #region Properties

        public DynamoViewModel DynamoViewModel { get; private set; }

        private double zIndex;
        public double ZIndex
        {
            get { return zIndex; }
            set { zIndex = value; RaisePropertyChanged("ZIndex"); }
        }

        public Point targetTopLeft;
        public Point TargetTopLeft
        {
            get { return targetTopLeft; }
            set { targetTopLeft = value; RaisePropertyChanged("TargetTopLeft"); }
        }
        public Point targetBotRight;
        public Point TargetBotRight
        {
            get { return targetBotRight; }
            set { targetBotRight = value; RaisePropertyChanged("TargetBotRight"); }
        }

        private ImageSource largeIcon;
        public ImageSource LargeIcon
        {
            get
            {
                if (largeIcon == null)
                {
                    var svm = DynamoViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel?.FindViewModelForNode(nodeName);  //.CurrentSpaceViewModel.InCanvasSearchViewModel.FindViewModelForNode(nodeName);
                    if (svm != null)
                    {
                        largeIcon = svm.LargeIcon;
                    }

                }

                return largeIcon;
            }

            private set 
            {
                largeIcon = value;
            }
        }

        private String nodeName;


        #endregion

        #region Public Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamoViewModel"></param>
        public NodeIconViewModel(DynamoViewModel dynamoViewModel, String nodeName, ImageSource icon)
        {
            this.DynamoViewModel = dynamoViewModel;
            this.nodeName = nodeName;
            this.LargeIcon = icon;
            ZIndex = 500; //Todo make reactive to the node highest Z index.
        }

        #endregion

        #region Command Methods

        private void UpdatePosition(object parameter)
        {
            NodeIconDataPacket data = (NodeIconDataPacket)parameter;

            TargetTopLeft = data.TopLeft;
            TargetBotRight = data.BotRight;
        }

        private bool CanUpdatePosition(object parameter)
        {
            return true;
        }

        #endregion

        private DelegateCommand updatePositionCommand;

        public DelegateCommand UpdatePositionCommand
        {
            get
            {
                if (updatePositionCommand == null)
                    updatePositionCommand = new DelegateCommand(UpdatePosition, CanUpdatePosition);
                return updatePositionCommand;
            }
        }

    }

    public struct NodeIconDataPacket
    {

        public Point TopLeft;
        public Point BotRight;

        public NodeIconDataPacket(Point topLeft, Point botRight)
        {

            TopLeft = topLeft;
            BotRight = botRight;
        }
    }

}
