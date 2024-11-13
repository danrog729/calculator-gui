using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace calculator_gui
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            foreach (Theme theme in App.MainApp.themes)
            {
                ThemeSelector.Items.Add(new ComboBoxItem() { Content=theme.Name });
            }
            ThemeSelector.SelectedIndex = App.MainApp.themes.IndexOf(App.MainApp.CurrentTheme);
            BSPMaxDepth.Text = App.MainApp.maxBSPDepth.ToString();
        }

        public void ChangeTheme(object sender, RoutedEventArgs e)
        {
            App.MainApp.CurrentTheme = App.MainApp.themes[ThemeSelector.SelectedIndex];
        }

        public void ChangeAngleType(object sender, RoutedEventArgs e)
        {
            App.MainApp.angleType = AngleSelector.SelectedIndex;
        }

        public void GraphBSPChanged(object sender, RoutedEventArgs e)
        {
            App.MainApp.viewGraphBSP = !App.MainApp.viewGraphBSP;
        }

        public void AutoBSPChanged(object sender, RoutedEventArgs e)
        {
            App.MainApp.useAutoBSPDepth = !App.MainApp.useAutoBSPDepth;
            if (BSPMaxDepth != null)
            {
                BSPMaxDepth.IsEnabled = !BSPMaxDepth.IsEnabled;
            }
        }

        public void MaxBSPDepthChanged(object sender, RoutedEventArgs e)
        {
            if (Int32.TryParse(BSPMaxDepth.Text, out int output))
            {
                if (output >= 0)
                {
                    App.MainApp.maxBSPDepth = output;
                }
            }
        }

        public void TitleBar_Dragged(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void CloseApplication(object sender, RoutedEventArgs e)
        {
            Hide();
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
    }
}
