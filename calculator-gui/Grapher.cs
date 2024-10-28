using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace calculator_gui
{
    public class Grapher
    {
        private Viewport viewport;

        public Grapher(int height, int width)
        {
            viewport = new Viewport(-10, 10, -10, 10, height, width);
        }

        public void UpdateFrame(GeometryDrawing drawing)
        {
            drawing.Brush = Brushes.White;
            drawing.Pen = new Pen(Brushes.Gray, 0.1);
            GeometryGroup axes = new GeometryGroup();
            axes.Children.Add(new RectangleGeometry(new System.Windows.Rect(0, 0, 1, 1)));
            axes.Children.Add(new LineGeometry(new System.Windows.Point(0.5, 0), new System.Windows.Point(0.5, 1)));
            drawing.Geometry = axes;
        }
    }

    internal class Viewport
    {
        public float MinimumX;
        public float MaximumX;
        public float MinimumY;
        public float MaximumY;

        public int RealHeight;
        public int RealWidth;

        public Viewport(float minimumX, float maximumX, float minimumY, float maximumY, int realHeight, int realWidth)
        {
            MinimumX = minimumX;
            MaximumX = maximumX;
            MinimumY = minimumY;
            MaximumY = maximumY;

            RealHeight = realHeight;
            RealWidth = realWidth;
        }
    }
}
