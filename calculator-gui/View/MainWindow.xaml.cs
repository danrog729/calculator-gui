﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace calculator_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void Load_SimpleCalculator(object sender, RoutedEventArgs e)
        {
            Frame_Calculator.Source = new Uri("SimpleCalculator.xaml", UriKind.Relative);
        }

        public void Load_ScientificCalculator(object sender, RoutedEventArgs e)
        {
            Frame_Calculator.Source = new Uri("ScientificCalculator.xaml", UriKind.Relative);
        }

        public void Load_Settings(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.Owner = this;
            settings.Show();
        }

        public void CloseApplication(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void MaximiseApplication(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this);
                MaximiseButton.Content = "1"; // maximise in webdings
            }
            else
            {
                SystemCommands.MaximizeWindow(this);
                MaximiseButton.Content = "2"; // un-maximise in webdings
            }
        }

        public void MinimiseApplication(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        public void TitleBar_Dragged(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}