using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        private int majorGridCoefficient = 5;
        private int majorGridXPowerOfTen = 0;
        private int majorGridYPowerOfTen = 0;

        public SolidColorBrush axesColour;
        public SolidColorBrush majorGridColour;
        public SolidColorBrush minorGridColour;

        private Canvas canvas;

        public Graph(ref Canvas newCanvas)
        {
            canvas = newCanvas;
            canvas.Children.Clear();
            minX = -10;
            maxX = 10;
            minY = -10;
            maxY = 10;
            axesColour = new SolidColorBrush() { Color = Color.FromRgb(64, 64, 64) };
            majorGridColour = new SolidColorBrush() { Color = Color.FromRgb(192, 192, 192) };
            minorGridColour = new SolidColorBrush() { Color = Color.FromRgb(224, 224, 224) };
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
            float virtualX = VirtualiseX(xPos);
            float virtualY = VirtualiseY(yPos);
            minX = virtualX - factor * (virtualX - minX);
            maxX = virtualX + factor * (maxX - virtualX);
            minY = virtualY - factor * (virtualY - minY);
            maxY = virtualY + factor * (maxY - virtualY);

            // update the grid steps
            if (majorGridXStep * (pixelWidth / (maxX - minX)) < 75 && majorGridYStep * (pixelHeight / (maxY - minY)) < 75)
            {
                // zoom out the grids
                if (majorGridCoefficient == 1)
                {
                    majorGridCoefficient = 2;
                }
                else if (majorGridCoefficient == 2)
                {
                    majorGridCoefficient = 5;
                }
                else
                {
                    majorGridCoefficient = 1;
                    majorGridXPowerOfTen++;
                    majorGridYPowerOfTen++;
                }
                majorGridXStep = majorGridCoefficient * (float)Math.Pow(10, majorGridXPowerOfTen);
                majorGridYStep = majorGridCoefficient * (float)Math.Pow(10, majorGridYPowerOfTen);
                if (majorGridCoefficient == 2)
                {
                    minorGridXStep = majorGridXStep / 4;
                    minorGridYStep = majorGridYStep / 4;
                }
                else
                {
                    minorGridXStep = majorGridXStep / 5;
                    minorGridYStep = majorGridYStep / 5;
                }
            }
            else if (majorGridXStep * (pixelWidth / (maxX - minX)) > 200 && majorGridYStep * (pixelWidth / (maxY - minY)) > 200)
            {
                // zoom in the grids
                if (majorGridCoefficient == 1)
                {
                    majorGridCoefficient = 5;
                    majorGridXPowerOfTen--;
                    majorGridYPowerOfTen--;
                }
                else if (majorGridCoefficient == 5)
                {
                    majorGridCoefficient = 2;
                }
                else
                {
                    majorGridCoefficient = 1;
                }
                majorGridXStep = majorGridCoefficient * (float)Math.Pow(10, majorGridXPowerOfTen);
                majorGridYStep = majorGridCoefficient * (float)Math.Pow(10, majorGridYPowerOfTen);
                if (majorGridCoefficient == 2)
                {
                    minorGridXStep = majorGridXStep / 4;
                    minorGridYStep = majorGridYStep / 4;
                }
                else
                {
                    minorGridXStep = majorGridXStep / 5;
                    minorGridYStep = majorGridYStep / 5;
                }
            }
            UpdateFrame();
        }

        public void Pan(int realDeltaX, int realDeltaY)
        {
            float deltaX = minX - VirtualiseX(realDeltaX);
            float deltaY = maxY - VirtualiseY(realDeltaY);

            minX += deltaX;
            maxX += deltaX;
            minY += deltaY;
            maxY += deltaY;
            UpdateFrame();
        }

        private void UpdateFrame()
        {
            canvas.Children.Clear();
            DrawAxes();
        }

        private void DrawAxes()
        {
            DrawGrid(minorGridXStep, minorGridYStep, ref minorGridColour, 1);

            DrawGrid(majorGridXStep, majorGridYStep, ref majorGridColour, 1);

            DrawVerticalVirtualLine(0, ref axesColour, 2);
            DrawHorizontalVirtualLine(0, ref axesColour, 2);

            DrawAxesLabels(ref axesColour);
        }

        private void DrawGrid(float xStep, float yStep, ref SolidColorBrush colour, int thickness)
        {
            float x = xStep * (float)Math.Floor(minX / xStep);
            while (x < maxX)
            {
                DrawVerticalVirtualLine(x, ref colour, thickness);
                x += xStep;
            }
            float y = yStep * (float)Math.Floor(minY / yStep);
            while (y < maxY)
            {
                DrawHorizontalVirtualLine(y, ref colour, thickness);
                y += yStep;
            }
        }

        private void DrawRealBorder(int minX, int maxX, int minY, int maxY, ref SolidColorBrush border, ref SolidColorBrush fill)
        {
            Border newBorder = new Border();
            newBorder.Width = maxX - minX;
            newBorder.Height = maxY - minY;
            newBorder.BorderBrush = border;
            newBorder.Background = fill;
        }

        private void DrawVirtualLabel(float x, float y, int realOffsetX, int realOffsetY, string text, ref SolidColorBrush colour)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = colour;
            canvas.Children.Add(textBlock);
            textBlock.Measure(new Size(Single.PositiveInfinity, Single.PositiveInfinity));
            Size textSize = textBlock.DesiredSize;
            Canvas.SetTop(textBlock, RealiseY(y) - realOffsetY - textSize.Height / 2);
            Canvas.SetLeft(textBlock, RealiseX(x) + realOffsetX - textSize.Width / 2);
        }

        private void DrawAxesLabels(ref SolidColorBrush colour)
        {
            DrawVirtualLabel(0, 0, -10, -10, "0", ref colour);
            string decimalPlaces = "n0";
            if (majorGridXPowerOfTen < 0)
            {
                decimalPlaces = "n" + (-majorGridXPowerOfTen).ToString();
            }
            float x = majorGridXStep * (float)Math.Floor(minX / majorGridXStep);
            while (x < maxX)
            {
                if (x - majorGridXStep / 2 > 0 || x + majorGridXStep / 2 < 0)
                {
                    DrawVirtualLabel(x, 0, 0, -10, x.ToString(decimalPlaces), ref colour);
                }
                x += majorGridXStep;
            }
            decimalPlaces = "n0";
            if (majorGridYPowerOfTen < 0)
            {
                decimalPlaces = "n" + (-majorGridYPowerOfTen).ToString();
            }
            float y = majorGridYStep * (float)Math.Floor(minY / majorGridYStep);
            while (y < maxY)
            {
                if (y - majorGridYStep / 2 > 0 || y + majorGridYStep / 2 < 0)
                {
                    DrawVirtualLabel(0, y, -10, 0, y.ToString(decimalPlaces), ref colour);
                }
                y += majorGridYStep;
            }
        }

        private void DrawVirtualLine(float x1, float y1, float x2, float y2, ref SolidColorBrush colour)
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

        private void DrawVerticalVirtualLine(float x, ref SolidColorBrush colour, int thickness)
        {
            if (minX > x || maxX < x)
            {
                return;
            }
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

        private void DrawHorizontalVirtualLine(float y, ref SolidColorBrush colour, int thickness)
        {
            if (minY > y || maxY < y)
            {
                return;
            }
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

        private float VirtualiseX(int x)
        {
            return minX + (maxX - minX) * ((float)x / pixelWidth);
        }
        private float VirtualiseY(int y)
        {
            return minY + (maxY - minY) * (1 - ((float)y / pixelHeight));
        }

        private int RealiseX(float x)
        {
            return (int)((x - minX) / (maxX - minX) * pixelWidth);
        }
        private int RealiseY(float y)
        {
            return (int)((1 - (y - minY) / (maxY - minY)) * pixelHeight);
        }

    }
}
