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
using System.Threading;

namespace calculator_gui
{
    /// <summary>
    /// Interaction logic for ScientificCalculator.xaml
    /// </summary>
    public partial class ScientificCalculator : Page
    {
        Grapher grapher;

        bool mouseWasDown;
        int lastMouseX;
        int lastMouseY;
        Point mousePos;

        public ScientificCalculator()
        {
            InitializeComponent();
            grapher = new Grapher(ref OutputImage);
        }

        public void EquationTextChanged(object sender, EventArgs e)
        {
            grapher.NewEquation(((EquationBox)sender).Text);
        }

        public void ImageSize_Changed(object sender, SizeChangedEventArgs e)
        {
            grapher.SizeChanged((int)ImageBorder.ActualWidth, (int)ImageBorder.ActualHeight);
        }

        public void ImageZoom(object sender, MouseWheelEventArgs e)
        {
            mousePos = e.GetPosition(OutputImage);
            grapher.Zoom((int)mousePos.X, (int)mousePos.Y, e.Delta);
        }

        public void ImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            mousePos = e.GetPosition(OutputImage);
            lastMouseX = (int)mousePos.X;
            lastMouseY = (int)mousePos.Y;
            mouseWasDown = true;
        }

        public void ImageMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mousePos = e.GetPosition(OutputImage);
                if (!mouseWasDown)
                {
                    lastMouseX = (int)mousePos.X;
                    lastMouseY = (int)mousePos.Y;
                    mouseWasDown = true;
                }
                int deltaX = (int)mousePos.X - lastMouseX;
                int deltaY = (int)mousePos.Y - lastMouseY;

                grapher.Pan(deltaX, deltaY);

                lastMouseX = (int)mousePos.X;
                lastMouseY = (int)mousePos.Y;
            }
            else
            {
                mouseWasDown = false;
            }
        }

        public void AddNewEquation(object sender, RoutedEventArgs e)
        {
            EquationBox equationBox = new EquationBox();
            equationBox.Margin = new Thickness(0, 5, 0, 5);
            equationBox.TextChanged += EquationTextChanged;
            App.MainApp.clickSound.Play();
            EquationList.Children.Insert(
                EquationList.Children.Count - 1, equationBox);
        }
    }
}
