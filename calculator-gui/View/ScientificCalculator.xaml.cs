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
        Graph grapher;

        bool mouseWasDown;
        int lastMouseX;
        int lastMouseY;
        Point mousePos;

        public ScientificCalculator()
        {
            InitializeComponent();
            grapher = new Graph(ref OutputCanvas);
        }

        public void CanvasSize_Changed(object sender, SizeChangedEventArgs e)
        {
            grapher.SizeChanged((int)e.NewSize.Width, (int)e.NewSize.Height);
        }

        public void CanvasZoom(object sender, MouseWheelEventArgs e)
        {
            mousePos = e.GetPosition(OutputCanvas);
            grapher.Zoom((int)mousePos.X, (int)mousePos.Y, e.Delta);
        }

        public void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            mousePos = e.GetPosition(OutputCanvas);
            lastMouseX = (int)mousePos.X;
            lastMouseY = (int)mousePos.Y;
            mouseWasDown = true;
        }

        public void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mousePos = e.GetPosition(OutputCanvas);
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
    }
}
