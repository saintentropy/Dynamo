using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
using NodeIconViewModel = NodeIcon.NodeIconViewModel;

namespace NodeIcon
{
    /// <summary>
    /// Interaction logic for NodeIcon.xaml
    /// </summary>
    public partial class NodeIconView : UserControl
    {
        public NodeIconView()
        {
            InitializeComponent();
        }
    }

    

    public class NullValueToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}