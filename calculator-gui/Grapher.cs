using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace calculator_gui
{
    public class Graph
    {
        private int pixelWidth;
        private int pixelHeight;
        private float minX;
        private float maxX;
        private float minY;
        private float maxY;

        private float majorGridXStep = 5;
        private float majorGridYStep = 5;
        private float minorGridXStep = 1;
        private float minorGridYStep = 1;

        private Canvas canvas;

        public Graph(ref Canvas newCanvas)
        {
            canvas = newCanvas;
            canvas.Children.Clear();
            minX = -10;
            maxX = 10;
            minY = -10;
            maxY = 10;
        }

        public void SizeChanged(int newWidth, int newHeight)
        {
            if (pixelWidth != 0 && pixelHeight != 0)
            {
                float middleX = (minX + maxX) / 2;
                float xSpan = maxX - middleX;
                minX = middleX - xSpan * ((float)newWidth / pixelWidth);
                maxX = middleX + xSpan * ((float)newWidth / pixelWidth);

                float middleY = (minY + maxY) / 2;
                float ySpan = maxY - middleY;
                minY = middleY - ySpan * ((float)newHeight / pixelHeight);
                maxY = middleY + ySpan * ((float)newHeight / pixelHeight);
            }
            else
            {
                // initialisation
                if (newWidth <= newHeight)
                {
                    minX = -5;
                    maxX = 5;
                    minY = -5 * ((float)newHeight / newWidth);
                    maxY = 5 * ((float)newHeight / newWidth);
                }
                else
                {
                    minX = -5 * ((float)newWidth / newHeight);
                    maxX = 5 * ((float)newWidth / newHeight);
                    minY = -5;
                    maxY = 5;
                }
            }
            pixelWidth = newWidth;
            pixelHeight= newHeight;
            UpdateFrame();
        }

        public void Zoom(int xPos, int yPos, int amount)
        {
            float factor;
            if (amount > 0)
            {
                // zoom in by 10%, centred on (xPos, yPos)
                factor = 0.9f;
            }
            else
            {
                factor = 1.11111111f;
            }
            (float, float) virtualised = VirtualiseCoords(xPos, yPos);
            float virtualX = virtualised.Item1;
            float virtualY = virtualised.Item2;
            minX = virtualX - factor * (virtualX - minX);
            maxX = virtualX + factor * (maxX - virtualX);
            minY = virtualY - factor * (virtualY - minY);
            maxY = virtualY + factor * (maxY - virtualY);

            // update the grid steps
            if (maxX - minX <= majorGridXStep || maxY - minY <= majorGridYStep)
            {
                minorGridXStep /= 5;
                majorGridXStep /= 5;
                minorGridYStep /= 5;
                majorGridYStep /= 5;
            }
            else if (maxX - minX >= majorGridXStep * 5 && maxY - minY >= majorGridYStep * 5)
            {
                minorGridXStep *= 5;
                majorGridXStep *= 5;
                minorGridYStep *= 5;
                majorGridYStep *= 5;
            }
            UpdateFrame();
        }

        private void UpdateFrame()
        {
            canvas.Children.Clear();
            DrawAxes(
                new SolidColorBrush() { Color = Color.FromRgb(64, 64, 64) },
                new SolidColorBrush() { Color = Color.FromRgb(160, 160, 160) },
                new SolidColorBrush() { Color = Color.FromRgb(192, 192, 192) } );
        }

        private void DrawAxes(SolidColorBrush axesColour, SolidColorBrush majorGridColour, SolidColorBrush minorGridColour)
        {
            DrawGrid(minorGridXStep, minorGridYStep, minorGridColour, 1);

            DrawGrid(majorGridXStep, majorGridYStep, majorGridColour, 1);

            DrawVerticalVirtualLine(0, axesColour, 2);
            DrawHorizontalVirtualLine(0, axesColour, 2);
        }

        private void DrawGrid(float xStep, float yStep, SolidColorBrush colour, int thickness)
        {
            float x = xStep * (float)Math.Floor(minX / xStep);
            while (x < maxX)
            {
                DrawVerticalVirtualLine(x, colour, thickness);
                x += xStep;
            }
            float y = yStep * (float)Math.Floor(minY / yStep);
            while (y < maxY)
            {
                DrawHorizontalVirtualLine(y, colour, thickness);
                y += yStep;
            }
        }

        private void DrawVirtualLine(float x1, float y1, float x2, float y2, SolidColorBrush colour)
        {
            Line line = new Line();
            line.X1 = (x1 - minX) / (maxX - minX) * pixelWidth;
            line.Y1 = pixelHeight - (y1 - minY) / (maxY - minY) * pixelHeight;
            line.X2 = (x2 - minX) / (maxX - minX) * pixelWidth;
            line.Y2 = pixelHeight - (y2 - minY) / (maxY - minY) * pixelHeight;
            line.StrokeThickness = 1;
            line.Stroke = colour;
            line.SnapsToDevicePixels = false;
            canvas.Children.Add(line);
        }

        private void DrawVerticalVirtualLine(float x, SolidColorBrush colour, int thickness)
        {
            Line line = new Line();
            line.X1 = (x - minX) / (maxX - minX) * pixelWidth;
            line.Y1 = 0;
            line.X2 = (x - minX) / (maxX - minX) * pixelWidth;
            line.Y2 = pixelHeight;
            line.StrokeThickness = thickness;
            line.Stroke = colour;
            line.SnapsToDevicePixels = false;
            canvas.Children.Add(line);
        }

        private void DrawHorizontalVirtualLine(float y, SolidColorBrush colour, int thickness)
        {
            Line line = new Line();
            line.X1 = 0;
            line.Y1 = pixelHeight - (y - minY) / (maxY - minY) * pixelHeight;
            line.X2 = pixelWidth;
            line.Y2 = pixelHeight - (y - minY) / (maxY - minY) * pixelHeight;
            line.StrokeThickness = thickness;
            line.Stroke = colour;
            line.SnapsToDevicePixels = false;
            canvas.Children.Add(line);
        }

        private (float, float) VirtualiseCoords(int x, int y)
        {
            return (minX + (maxX - minX) * ((float)x / pixelWidth), minY + (maxY - minY) * (1-((float)y / pixelHeight)));
        }
    }
}
