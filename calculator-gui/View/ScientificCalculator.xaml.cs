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
            App.MainApp.colourList = new ColourList();
        }

        public void EquationTextChanged(object sender, EventArgs e)
        {
            int index = EquationList.Children.IndexOf(sender as EquationBox);
            grapher.UpdateEquation(index, ((EquationBox)sender).Text);
        }

        public void EquationColourChanged(object sender, EventArgs e)
        {
            int index = EquationList.Children.IndexOf(sender as EquationBox);
            grapher.UpdateColour(index, ((EquationBox)sender).colour);
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

        public void EquationDeleted(object sender, EventArgs e)
        {
            int index = EquationList.Children.IndexOf(sender as EquationBox);
            grapher.DeleteEquation(index);
            EquationList.Children.Remove(sender as EquationBox);
        }

        public void EquationHidden(object sender, EventArgs e)
        {
            int index = EquationList.Children.IndexOf(sender as EquationBox);
            grapher.ToggleEquationVisibility(index);
        }

        public void AddNewEquation(object sender, RoutedEventArgs e)
        {
            EquationBox equationBox = new EquationBox();
            equationBox.Margin = new Thickness(0, 5, 0, 5);
            equationBox.TextChanged += EquationTextChanged;
            equationBox.ColourChanged += EquationColourChanged;
            equationBox.DeleteEquation += EquationDeleted;
            equationBox.VisiblilityChanged += EquationHidden;
            App.MainApp.clickSound.Play();
            EquationList.Children.Insert(
                EquationList.Children.Count - 1, equationBox);
            grapher.NewEquation("", equationBox.colour);
        }
    }

    public class ColourList
    {
        private Color[] colours;
        private int index;

        public ColourList()
        {
            colours = new Color[8];
            colours[0] = Colors.Red;
            colours[1] = Colors.Green;
            colours[2] = Colors.Blue;
            colours[3] = Colors.Yellow;
            colours[4] = Colors.Cyan;
            colours[5] = Colors.Magenta;
            colours[6] = Colors.Brown;
            colours[7] = Colors.Black;
            index = 0;
        }

        public Color NextColour()
        {
            Color colour = colours[index++];
            if (index >= colours.Length)
            {
                index = 0;
            }
            return colour;
        }
    }
}
