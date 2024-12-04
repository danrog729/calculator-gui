using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace calculator_gui
{
    /// <summary>
    /// Interaction logic for EquationBox.xaml
    /// </summary>
    public partial class EquationBox : UserControl
    {
        public event EventHandler TextChanged;
        public event EventHandler VisiblilityChanged;
        public string Text;
        public bool GraphHidden;

        public EquationBox()
        {
            InitializeComponent();
        }

        private void EquationTextChanged(object sender, RoutedEventArgs e)
        {
            Text = Equation.Text;
            if (TextChanged != null)
            {
                TextChanged(this, e);
            }
        }
        private void VisibilityClicked(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
            GraphHidden = !GraphHidden;
            if (VisiblilityChanged != null)
            {
                VisiblilityChanged(this, e);
            }
            if (GraphHidden)
            {
                VisibilityButton.Content = "○";
            }
            else
            {
                VisibilityButton.Content = "👁";
            }
        }
        private void ColourButtonClicked(object sender, RoutedEventArgs e)
        {
            App.MainApp.clickSound.Play();
        }
    }
}
