using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

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
        public SolidColorBrush axesLabelsOutOfRange;
        public SolidColorBrush majorGridColour;
        public SolidColorBrush minorGridColour;
        public SolidColorBrush transparent = new SolidColorBrush(Colors.Transparent);
        public SolidColorBrush BSPLineInvalid;
        public SolidColorBrush BSPLineValid;

        private Canvas canvas;
        private FreeformCalculator calculator;

        Stopwatch calculationStopwatch;
        Stopwatch renderingStopwatch;
        Stopwatch totalStopwatch;

        BSPNode bspRoot;
        private int absoluteMaxDepth = 0;
        private int maxDepth = 5;

        public Graph(ref Canvas newCanvas)
        {
            canvas = newCanvas;
            canvas.Children.Clear();
            minX = -10;
            maxX = 10;
            minY = -10;
            maxY = 10;

            axesColour = new SolidColorBrush() { Color = System.Windows.Media.Color.FromRgb(64, 64, 64) };
            axesLabelsOutOfRange = new SolidColorBrush() { Color = System.Windows.Media.Color.FromRgb(128,128, 128) };
            majorGridColour = new SolidColorBrush() { Color = System.Windows.Media.Color.FromRgb(192, 192, 192) };
            minorGridColour = new SolidColorBrush() { Color = System.Windows.Media.Color.FromRgb(224, 224, 224) };
            BSPLineInvalid = new SolidColorBrush() { Color = System.Windows.Media.Color.FromRgb(255, 0, 0) };
            BSPLineValid = new SolidColorBrush() { Color = System.Windows.Media.Color.FromRgb(0, 255, 0) };

            calculator = new FreeformCalculator();
            calculator.Input = "";

            absoluteMaxDepth =
                (int)Math.Max(
                    Math.Ceiling(Math.Log(pixelWidth, 2)),
                    Math.Ceiling(Math.Log(pixelHeight, 2))) - 1;
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
            absoluteMaxDepth = 
                (int)Math.Max(
                    Math.Ceiling(Math.Log(pixelWidth, 2)),
                    Math.Ceiling(Math.Log(pixelHeight, 2))) - 1;
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

        public void NewEquation(string equation)
        {
            calculator.Input = equation;
            UpdateFrame();
        }

        private void UpdateFrame()
        {
            if (App.MainApp.performanceStatsEnabled)
            {
                calculationStopwatch = new Stopwatch();
                renderingStopwatch = new Stopwatch();
                totalStopwatch = new Stopwatch();
                totalStopwatch.Start();
            }
            if (App.MainApp.useAutoBSPDepth)
            {
                maxDepth = absoluteMaxDepth;
            }
            else
            {
                maxDepth = App.MainApp.maxBSPDepth;
            }

            if (App.MainApp.performanceStatsEnabled)
            {
                renderingStopwatch.Start();
            }
            canvas.Children.Clear();
            DrawAxes();
            if (App.MainApp.performanceStatsEnabled)
            {
                renderingStopwatch.Stop();
            }

            if (calculator.isValidExpression)
            {
                if (App.MainApp.performanceStatsEnabled)
                {
                    calculationStopwatch.Start();
                }
                bspRoot = BSP();
                if (App.MainApp.performanceStatsEnabled)
                {
                    calculationStopwatch.Stop();
                }

                if (App.MainApp.performanceStatsEnabled)
                {
                    renderingStopwatch.Start();
                }
                DrawGraph(bspRoot);
                if (App.MainApp.viewGraphBSP)
                {
                    DrawBSP(bspRoot);
                }
                if (App.MainApp.performanceStatsEnabled)
                {
                    renderingStopwatch.Stop();
                }
            }

            if (App.MainApp.performanceStatsEnabled)
            {
                totalStopwatch.Stop();
                TextBlock textBlock = new TextBlock();
                textBlock.Text = "Calculation: " + calculationStopwatch.ElapsedMilliseconds.ToString() + "ms\n"
                    + "Rendering: " + renderingStopwatch.ElapsedMilliseconds.ToString() + "ms\n"
                    + "Total: " + totalStopwatch.ElapsedMilliseconds.ToString() + "ms";
                textBlock.Foreground = axesColour;
                canvas.Children.Add(textBlock);
                textBlock.Measure(new System.Windows.Size(Single.PositiveInfinity, Single.PositiveInfinity));
                System.Windows.Size textSize = textBlock.DesiredSize;
                Canvas.SetTop(textBlock, 5);
                Canvas.SetLeft(textBlock, 5);
            }
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

        private void DrawRealBorder(int x, int y, int width, int height, ref SolidColorBrush border, ref SolidColorBrush fill, int thickness)
        {
            Border newBorder = new Border();
            newBorder.Width = width;
            newBorder.Height = height;
            newBorder.BorderBrush = border;
            newBorder.BorderThickness = new Thickness(thickness);
            newBorder.Background = fill;
            canvas.Children.Add(newBorder);
            Canvas.SetTop(newBorder, y);
            Canvas.SetLeft(newBorder, x);
        }

        private void DrawVirtualLabel(float x, float y, int realOffsetX, int realOffsetY, string text, ref SolidColorBrush colour)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = colour;
            canvas.Children.Add(textBlock);
            textBlock.Measure(new System.Windows.Size(Single.PositiveInfinity, Single.PositiveInfinity));
            System.Windows.Size textSize = textBlock.DesiredSize;
            Canvas.SetTop(textBlock, RealiseY(y) - realOffsetY - textSize.Height / 2);
            Canvas.SetLeft(textBlock, RealiseX(x) + realOffsetX - textSize.Width / 2);
        }

        private void DrawAxesLabels(ref SolidColorBrush visibleColour)
        {
            int realOriginX = RealiseX(0);
            int realOriginY = RealiseY(0);


            float originX = 0;
            float originY = 0;
            int originOffsetX = -10;
            int originOffsetY = -10;
            ref SolidColorBrush colour = ref visibleColour;
            if (realOriginX < 0)
            {
                originX = minX;
                originOffsetX = 10;
                colour = ref axesLabelsOutOfRange;
            }
            else if (realOriginX > pixelWidth)
            {
                originX = maxX;
                originOffsetX = -10;
                colour = ref axesLabelsOutOfRange;
            }
            if (realOriginY < 0)
            {
                originY = maxY;
                originOffsetY = -10;
                colour = ref axesLabelsOutOfRange;
            }
            else if (realOriginY > pixelHeight)
            {
                originY = minY;
                originOffsetY = 10;
                colour = ref axesLabelsOutOfRange;
            }
            DrawVirtualLabel(originX, originY, originOffsetX, originOffsetY, "0", ref colour);


            colour = ref visibleColour;
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
                    float yValue = 0;
                    int offset = -10;
                    if (realOriginY < 0)
                    {
                        yValue = maxY;
                        offset = -10;
                        colour = ref axesLabelsOutOfRange;
                    }
                    else if (realOriginY > pixelHeight)
                    {
                        yValue = minY;
                        offset = 10;
                        colour = ref axesLabelsOutOfRange;
                    }
                    DrawVirtualLabel(x, yValue, 0, offset, x.ToString(decimalPlaces), ref colour);
                }
                x += majorGridXStep;
            }

            colour = ref visibleColour;
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
                    float xValue = 0;
                    int offset = -10;
                    if (realOriginX < 0)
                    {
                        xValue = minX;
                        offset = 10;
                        colour = ref axesLabelsOutOfRange;
                    }
                    else if (realOriginX > pixelWidth)
                    {
                        xValue = maxX;
                        offset = -10;
                        colour = ref axesLabelsOutOfRange;
                    }
                    DrawVirtualLabel(xValue, y, offset, 0, y.ToString(decimalPlaces), ref colour);
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

        private void DrawBSPNode(ref BSPNode node)
        {
            int realXMin = RealiseX(node.xMin);
            int realXMax = RealiseX(node.xMax);
            int realYMin = RealiseY(node.yMin);
            int realYMax = RealiseY(node.yMax);
            if (node.containsGraph)
            {
                DrawRealBorder(realXMin, realYMax, realXMax - realXMin, realYMin - realYMax, ref BSPLineValid, ref transparent, 1);
            }
            else
            {
                DrawRealBorder(realXMin, realYMax, realXMax - realXMin, realYMin - realYMax, ref BSPLineInvalid, ref transparent, 1);
            }
        }

        private void DrawCell(ref BSPNode node)
        {
            int realXMin = RealiseX(node.xMin);
            int realXMax = RealiseX(node.xMax);
            int realYMin = RealiseY(node.yMin);
            int realYMax = RealiseY(node.yMax);
            if (node.containsGraph && node.children.Count == 0)
            {
                DrawRealBorder(realXMin, realYMax, realXMax - realXMin, realYMin - realYMax, ref axesColour, ref axesColour, 0);
            }
        }

        private BSPNode BSP()
        {
            BSPNode root = new BSPNode() { xMin = minX, xMax = maxX, yMin = minY, yMax = maxY, containsGraph = false };
            if (calculator.InsideBounds((root.xMin, root.xMax), (root.yMin, root.yMax)))
            {
                root.containsGraph = true;
                if (maxDepth != 0)
                {
                    BSPDescend(root, 1);
                }
            }
            return root;
        }

        private void BSPDescend(BSPNode root, int depth)
        {
            root.children.Add(new BSPNode()
            {
                xMin = root.xMin,
                xMax = (root.xMax + root.xMin) / 2,
                yMin = root.yMin,
                yMax = (root.yMax + root.yMin) / 2,
                containsGraph = false
            });
            root.children.Add(new BSPNode()
            {
                xMin = (root.xMax + root.xMin) / 2,
                xMax = root.xMax,
                yMin = root.yMin,
                yMax = (root.yMax + root.yMin) / 2,
                containsGraph = false
            });
            root.children.Add(new BSPNode()
            {
                xMin = root.xMin,
                xMax = (root.xMax + root.xMin) / 2,
                yMin = (root.yMax + root.yMin) / 2,
                yMax = root.yMax,
                containsGraph = false
            });
            root.children.Add(new BSPNode()
            {
                xMin = (root.xMax + root.xMin) / 2,
                xMax = root.xMax,
                yMin = (root.yMax + root.yMin) / 2,
                yMax = root.yMax,
                containsGraph = false
            });
            foreach (BSPNode child in root.children)
            {
                if (calculator.InsideBounds((child.xMin, child.xMax), (child.yMin, child.yMax)))
                {
                    child.containsGraph = true;
                }
            }
            foreach (BSPNode child in root.children)
            {
                if (child.containsGraph)
                {
                    if (depth < maxDepth && depth < absoluteMaxDepth)
                    {
                        BSPDescend(child, depth + 1);
                    }
                }
            }
        }

        private void DrawBSP(BSPNode root)
        {
            DrawBSPNode(ref root);
            for (int index = 0; index < root.children.Count; index++)
            {
                DrawBSP(root.children[index]);
            }
        }

        private void DrawGraph(BSPNode root)
        {
            DrawCell(ref root);
            for (int index = 0; index < root.children.Count; index++)
            {
                DrawGraph(root.children[index]);
            }
        }
    }

    internal class BSPNode
    {
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;
        public bool containsGraph = true;
        public List<BSPNode> children = new List<BSPNode>();
    }
}
