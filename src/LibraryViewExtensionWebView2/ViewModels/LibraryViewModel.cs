﻿using System.Windows;
using Dynamo.Core;

namespace Dynamo.LibraryViewExtensionWebView2.ViewModels
{
    /// <summary>
    /// Library View Loader
    /// </summary>
    public class LibraryViewModel : NotificationObject
    {
        /// <summary>
        /// Checks and updates the visibility property
        /// </summary>
        public bool IsVisible
        {
            get { return Visibility == Visibility.Visible; }
            set { Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary>
        /// Controls visibilty of the view
        /// </summary>
        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                RaisePropertyChanged("Visibility");
            }
        }

        private Visibility visibility = Visibility.Visible;
    }
}
