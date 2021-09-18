using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Commands;
using Dynamo.Utilities;

namespace Dynamo.ViewModels
{
    public partial class InPortViewModel : PortViewModel
    {
        #region Properties/Fields

        private DelegateCommand _useLevelsCommand;
        private DelegateCommand _keepListStructureCommand;

        /// <summary>
        /// Returns whether this port has a default value that can be used.
        /// </summary>
        public bool DefaultValueEnabled
        {
            get { return _port.DefaultValue != null; }
        }
        
        /// <summary>
        /// Returns whether the port is using its default value, or whether this been disabled
        /// </summary>
        public bool UsingDefaultValue
        {
            get { return _port.UsingDefaultValue; }
            set
            {
                _port.UsingDefaultValue = value;
                RaisePropertyChanged(nameof(UsingDefaultValueMarkerVisibile));
            }
        }

        /// <summary>
        /// If UseLevel is enabled on this port.
        /// </summary>
        public bool UseLevels
        {
            get { return _port.UseLevels; }
        }

        /// <summary>
        /// Determines whether or not the UseLevelsSpinner is visible on the port.
        /// </summary>
        public Visibility UseLevelSpinnerVisible
        {
            get
            {
                if (PortType == PortType.Output) return Visibility.Collapsed;
                if (UseLevels) return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        /// <summary>
        /// Determines whether the blue marker appears beside an input port, indicating
        /// the default value for this port is being used.
        /// </summary>
        public bool UsingDefaultValueMarkerVisibile
        {
            get => PortType == PortType.Input && UsingDefaultValue && DefaultValueEnabled;
        }

        /// <summary>
        /// If should keep list structure on this port.
        /// </summary>
        public bool ShouldKeepListStructure
        {
            get { return _port.KeepListStructure; }
        }

        /// <summary>
        /// Levle of list.
        /// </summary>
        public int Level
        {
            get { return _port.Level; }
            set
            {
                ChangeLevel(value);
            }
        }

        /// <summary>
        /// The visibility of Use Levels menu.
        /// </summary>
        public Visibility UseLevelVisibility
        {
            get
            {
                if (_node.ArgumentLacing != LacingStrategy.Disabled)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Shows or hides the Use Levels and Keep List Structure checkboxes
        /// in the node chevron popup menu.
        /// </summary>
        public bool UseLevelCheckBoxVisibility
        {
            get => _port.PortType == PortType.Input;
        }

        /// <summary>
        /// Shows or hides the Use Default Value checkbox in the node chevron popup menu.
        /// </summary>
        public bool UseDefaultValueCheckBoxVisibility
        {
            get => _port.PortType == PortType.Input && DefaultValueEnabled;
        }

        #endregion

        public InPortViewModel(NodeViewModel node, PortModel port) : base(node, port)
        {
        }

        /// <summary>
        /// UseLevels command
        /// </summary>
        public DelegateCommand UseLevelsCommand
        {
            get
            {
                if (_useLevelsCommand == null)
                {
                    _useLevelsCommand = new DelegateCommand(UseLevel, p => true);
                }
                return _useLevelsCommand;
            }
        }

        private void UseLevel(object parameter)
        {
            var useLevel = (bool)parameter;
            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty, _node.NodeLogic.GUID, "UseLevels", string.Format("{0}:{1}", _port.Index, useLevel));

            _node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
            RaisePropertyChanged(nameof(UseLevelSpinnerVisible));
        }

        /// <summary>
        /// ShouldKeepListStructure command
        /// </summary>
        public DelegateCommand KeepListStructureCommand
        {
            get
            {
                if (_keepListStructureCommand == null)
                {
                    _keepListStructureCommand = new DelegateCommand(KeepListStructure, p => true);
                }
                return _keepListStructureCommand;
            }
        }

        private void KeepListStructure(object parameter)
        {
            bool keepListStructure = (bool)parameter;
            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty, _node.NodeLogic.GUID, "KeepListStructure", string.Format("{0}:{1}", _port.Index, keepListStructure));

            _node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
        }

        private void ChangeLevel(int level)
        {
            var command = new DynamoModel.UpdateModelValueCommand(
                            Guid.Empty, _node.NodeLogic.GUID, "ChangeLevel", string.Format("{0}:{1}", _port.Index, level));

            _node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
        }

        /// <summary>
        /// Handles the logic for updating the PortBackgroundColor and PortBackgroundBrushColor
        /// </summary>
        internal override void RefreshPortColors()
        {
            // The visual appearance of ports can be affected by many different logical states
            // Inputs have more display styles than outputs
            // Special case for keeping list structure visual appearance
            if (_port.UseLevels && _port.KeepListStructure && _port.IsConnected)
            {
                PortBackgroundColor = new SolidColorBrush(Color.FromRgb(94, 165, 196));
                PortBorderBrushColor = new SolidColorBrush(Color.FromRgb(106, 192, 231));
            }

            // Port has a default value, shows blue marker
            else if (UsingDefaultValue && DefaultValueEnabled)
            {
                PortBackgroundColor = new SolidColorBrush(Color.FromRgb(70, 90, 99));
                PortBorderBrushColor = new SolidColorBrush(Color.FromRgb(106, 192, 231));
            }
            else
            {
                // Port isn't connected and has no default value (or isn't using it)
                if (!_port.IsConnected)
                {
                    PortBackgroundColor = new SolidColorBrush(Color.FromRgb(107, 67, 67));
                    PortBorderBrushColor = new SolidColorBrush(Color.FromRgb(244, 134, 134));
                }
                // Port is connected and has no default value
                else
                {
                    PortBackgroundColor = new SolidColorBrush(Color.FromRgb(70, 90, 99));
                    PortBorderBrushColor = new SolidColorBrush(Color.FromRgb(106, 192, 231));
                }
            }
        }
    }
}
