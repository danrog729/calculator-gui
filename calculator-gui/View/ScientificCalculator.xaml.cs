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
    /// Interaction logic for ScientificCalculator.xaml
    /// </summary>
    public partial class ScientificCalculator : Page
    {
        Graph grapher;
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
            Point zoomPoint = e.GetPosition(OutputCanvas);
            grapher.Zoom((int)zoomPoint.X, (int)zoomPoint.Y, e.Delta);
        }
    }
}
