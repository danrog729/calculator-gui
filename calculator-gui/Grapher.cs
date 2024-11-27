using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.ComponentModel;

namespace calculator_gui
{
    public class Grapher
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

        private Color transparent = Color.Transparent;

        private System.Windows.Controls.Image image;
        private BitmapRenderer renderer;
        private FreeformCalculator calculator;

        private Stopwatch calculationStopwatch;
        private Stopwatch renderingStopwatch;
        private Stopwatch axesStopwatch;
        private Stopwatch curveStopwatch;
        private Stopwatch BSPLineStopwatch;
        private Stopwatch compositingStopwatch;
        private Stopwatch totalStopwatch;

        private BSPNode bspRoot;
        private int absoluteMaxDepth = 0;
        private int maxDepth = 5;

        private BackgroundWorker axesWorker;
        private BitmapRenderer axesRendererOutput;

        private BackgroundWorker graphRenderWorker;
        private BitmapRenderer graphRendererOutput;

        private BackgroundWorker BSPRenderWorker;
        private BitmapRenderer BSPRendererOutput;

        private BackgroundWorker calculationWorker;

        public Grapher(ref System.Windows.Controls.Image newImage)
        {
            image = newImage;
            renderer = new BitmapRenderer(100, 100);
            axesRendererOutput = new BitmapRenderer(100, 100);
            graphRendererOutput = new BitmapRenderer(100, 100);
            BSPRendererOutput = new BitmapRenderer(100, 100);
            image.Source = renderer.bitmap;

            minX = -10;
            maxX = 10;
            minY = -10;
            maxY = 10;

            calculator = new FreeformCalculator();
            calculator.Input = "";

            absoluteMaxDepth =
                (int)Math.Max(
                    Math.Ceiling(Math.Log(pixelWidth, 2)),
                    Math.Ceiling(Math.Log(pixelHeight, 2))) - 1;

            bspRoot = new BSPNode();

            axesWorker = new BackgroundWorker();
            axesWorker.DoWork += DrawAxes;
            axesWorker.RunWorkerCompleted += DrawAxesCompleted;

            graphRenderWorker = new BackgroundWorker();
            graphRenderWorker.DoWork += DrawGraph;
            graphRenderWorker.RunWorkerCompleted += DrawGraphCompleted;

            BSPRenderWorker = new BackgroundWorker();
            BSPRenderWorker.DoWork += DrawBSP;
            BSPRenderWorker.RunWorkerCompleted += DrawBSPCompleted;

            calculationWorker = new BackgroundWorker();
            calculationWorker.DoWork += StartBSP;
            calculationWorker.RunWorkerCompleted += EndBSP;
        }

        public void SizeChanged(int newWidth, int newHeight)
        {
            renderer = new BitmapRenderer(newWidth, newHeight);
            renderer.Fill(App.MainApp.graphingColours.background);
            image.Source = renderer.bitmap;
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
            pixelHeight = newHeight;
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
            calculationStopwatch = new Stopwatch();
            renderingStopwatch = new Stopwatch();
            axesStopwatch = new Stopwatch();
            curveStopwatch = new Stopwatch();
            BSPLineStopwatch = new Stopwatch();
            compositingStopwatch = new Stopwatch();
            totalStopwatch = new Stopwatch();
            totalStopwatch.Start();

            if (App.MainApp.useAutoBSPDepth)
            {
                maxDepth = absoluteMaxDepth;
            }
            else
            {
                maxDepth = App.MainApp.maxBSPDepth;
            }

            if (!axesWorker.IsBusy)
            {
                axesWorker.RunWorkerAsync();
            }

            if (calculator.isValidExpression)
            {
                if (!calculationWorker.IsBusy)
                {
                    calculationWorker.RunWorkerAsync();
                }

                if (!graphRenderWorker.IsBusy)
                {
                    graphRenderWorker.RunWorkerAsync(bspRoot);
                }

                if (App.MainApp.viewGraphBSP)
                {
                    if (!BSPRenderWorker.IsBusy)
                    {
                        BSPRenderWorker.RunWorkerAsync(bspRoot);
                    }
                }
            }
            totalStopwatch.Stop();
        }

        private void CompositeImage()
        {
            totalStopwatch.Start();
            renderingStopwatch.Start();
            compositingStopwatch.Start();

            renderer.OverlayBitmapRenderer(ref axesRendererOutput);
            if (calculator.isValidExpression)
            {
                renderer.OverlayBitmapRenderer(ref graphRendererOutput);
            }
            if (App.MainApp.viewGraphBSP)
            {
                renderer.OverlayBitmapRenderer(ref BSPRendererOutput);
            }

            compositingStopwatch.Stop();
            renderingStopwatch.Stop();
            totalStopwatch.Stop();

            if (App.MainApp.performanceStatsEnabled)
            {
                DrawPerformanceStats();
            }
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

        private void StartBSP(object sender, DoWorkEventArgs e)
        {
            calculationStopwatch.Start();
            e.Result = BSP();
            calculationStopwatch.Stop();
        }

        private void EndBSP(object sender, RunWorkerCompletedEventArgs e)
        {
            bspRoot = (BSPNode)(e.Result);
            if (!graphRenderWorker.IsBusy)
            {
                graphRenderWorker.RunWorkerAsync(bspRoot);
            }

            if (App.MainApp.viewGraphBSP)
            {
                if (!BSPRenderWorker.IsBusy)
                {
                    BSPRenderWorker.RunWorkerAsync(bspRoot);
                }
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

        private void DrawAxes(object sender, DoWorkEventArgs e)
        {
            renderingStopwatch.Start();
            axesStopwatch.Start();
            BitmapRenderer axesRenderer = new BitmapRenderer(pixelWidth, pixelHeight);
            axesRenderer.Fill(App.MainApp.graphingColours.background);

            AxesRenderer axes = new AxesRenderer(ref axesRenderer);

            axes.minX = minX;
            axes.minY = minY;
            axes.maxX = maxX;
            axes.maxY = maxY;
            axes.pixelWidth = pixelWidth;
            axes.pixelHeight = pixelHeight;
            axes.minorGridXStep = minorGridXStep;
            axes.majorGridXStep = majorGridXStep;
            axes.minorGridYStep = minorGridYStep;
            axes.majorGridYStep = majorGridYStep;
            axes.majorGridCoefficient = majorGridCoefficient;
            axes.majorGridXPowerOfTen = majorGridXPowerOfTen;
            axes.majorGridYPowerOfTen = majorGridYPowerOfTen;

            axes.DrawGrid(minorGridXStep, minorGridYStep, ref App.MainApp.graphingColours.minorGridColour, 1);
            axes.DrawGrid(majorGridXStep, majorGridYStep, ref App.MainApp.graphingColours.majorGridColour, 1);

            axes.DrawVerticalVirtualLine(0, ref App.MainApp.graphingColours.axesColour, 2);
            axes.DrawHorizontalVirtualLine(0, ref App.MainApp.graphingColours.axesColour, 2);

            axes.DrawAxesLabels();
            e.Result = axesRenderer;
            axesStopwatch.Stop();
            renderingStopwatch.Stop();
        }

        private void DrawAxesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            renderingStopwatch.Start();
            if (e.Cancelled == false)
            {
                axesRendererOutput = (BitmapRenderer)(e.Result);
                CompositeImage();
            }
            renderingStopwatch.Stop();
        }

        private void DrawGraph(object sender, DoWorkEventArgs e)
        {
            renderingStopwatch.Start();
            curveStopwatch.Start();
            BitmapRenderer bitmapRenderer = new BitmapRenderer(pixelWidth, pixelHeight);
            BSPNode root = e.Argument as BSPNode;

            GraphRenderer graph = new GraphRenderer(ref bitmapRenderer);
            graph.minX = minX;
            graph.maxX = maxX;
            graph.minY = minY;
            graph.maxY = maxY;
            graph.pixelWidth = pixelWidth;
            graph.pixelHeight = pixelHeight;
            graph.DrawGraph(root);

            e.Result = bitmapRenderer;
            curveStopwatch.Stop();
            renderingStopwatch.Stop();
        }

        private void DrawGraphCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            renderingStopwatch.Start();
            if (e.Cancelled == false)
            {
                graphRendererOutput = (BitmapRenderer)(e.Result);
                CompositeImage();
            }
            renderingStopwatch.Stop();
        }

        private void DrawBSP(object sender, DoWorkEventArgs e)
        {
            renderingStopwatch.Start();
            BSPLineStopwatch.Start();
            BitmapRenderer bitmapRenderer = new BitmapRenderer(pixelWidth, pixelHeight);
            BSPNode root = e.Argument as BSPNode;

            BSPRenderer bsp = new BSPRenderer(ref bitmapRenderer);
            bsp.minX = minX;
            bsp.maxX = maxX;
            bsp.minY = minY;
            bsp.maxY = maxY;
            bsp.pixelWidth = pixelWidth;
            bsp.pixelHeight = pixelHeight;
            bsp.DrawBSP(root);

            e.Result = bitmapRenderer;
            BSPLineStopwatch.Stop();
            renderingStopwatch.Stop();
        }

        private void DrawBSPCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            renderingStopwatch.Start();
            if (e.Cancelled == false)
            {
                BSPRendererOutput = (BitmapRenderer)(e.Result);
                CompositeImage();
            }
            renderingStopwatch.Stop();
        }

        private void DrawPerformanceStats()
        {
            renderer.DrawText(5, 5, "Calculation: " + calculationStopwatch.ElapsedMilliseconds.ToString() + "ms (" + Math.Round(((double)calculationStopwatch.ElapsedMilliseconds / totalStopwatch.ElapsedMilliseconds) * 100, 1).ToString() + "%)\n"
                + "Rendering: " + renderingStopwatch.ElapsedMilliseconds.ToString() + "ms ( " + Math.Round(((double)renderingStopwatch.ElapsedMilliseconds / totalStopwatch.ElapsedMilliseconds) * 100, 1).ToString() + "%)\n"
                + "    Axes: " + axesStopwatch.ElapsedMilliseconds.ToString() + "ms ( " + Math.Round(((double)axesStopwatch.ElapsedMilliseconds / totalStopwatch.ElapsedMilliseconds) * 100, 1).ToString() + "%)\n"
                + "    Curve: " + curveStopwatch.ElapsedMilliseconds.ToString() + "ms ( " + Math.Round(((double)curveStopwatch.ElapsedMilliseconds / totalStopwatch.ElapsedMilliseconds) * 100, 1).ToString() + "%)\n"
                + "    BSP Lines: " + BSPLineStopwatch.ElapsedMilliseconds.ToString() + "ms ( " + Math.Round(((double)BSPLineStopwatch.ElapsedMilliseconds / totalStopwatch.ElapsedMilliseconds) * 100, 1).ToString() + "%)\n"
                + "    Compositing: " + compositingStopwatch.ElapsedMilliseconds.ToString() + "ms (" + Math.Round(((double)compositingStopwatch.ElapsedMilliseconds / totalStopwatch.ElapsedMilliseconds) * 100, 1).ToString() + "%)\n"
                + "Total: " + totalStopwatch.ElapsedMilliseconds.ToString() + "ms",
                ref App.MainApp.graphingColours.performanceStats);
        }
    }

    internal class Renderer
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
        public int pixelWidth;
        public int pixelHeight;
        public BitmapRenderer renderer;

        protected float VirtualiseX(int x)
        {
            return minX + (maxX - minX) * ((float)x / pixelWidth);
        }
        protected float VirtualiseY(int y)
        {
            return minY + (maxY - minY) * (1 - ((float)y / pixelHeight));
        }

        protected int RealiseX(float x)
        {
            return (int)((x - minX) / (maxX - minX) * pixelWidth);
        }
        protected int RealiseY(float y)
        {
            return (int)((1 - (y - minY) / (maxY - minY)) * pixelHeight);
        }
    }
    
    internal class AxesRenderer : Renderer
    {
        public float majorGridXStep = 5;
        public float majorGridYStep = 5;
        public float minorGridXStep = 1;
        public float minorGridYStep = 1;
        public int majorGridCoefficient = 5;
        public int majorGridXPowerOfTen = 0;
        public int majorGridYPowerOfTen = 0;

        public AxesRenderer(ref BitmapRenderer newRenderer)
        {
            renderer = newRenderer;
        }

        public void DrawGrid(float xStep, float yStep, ref Color colour, int thickness)
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

        public void DrawVerticalVirtualLine(float x, ref Color colour, int thickness)
        {
            if (minX > x || maxX < x)
            {
                return;
            }
            renderer.DrawVerticalLine(RealiseX(x), ref colour, thickness);
        }

        public void DrawHorizontalVirtualLine(float y, ref Color colour, int thickness)
        {
            if (minY > y || maxY < y)
            {
                return;
            }
            renderer.DrawHorizontalLine(RealiseY(y), ref colour, thickness);
        }

        private void DrawVirtualLabel(float x, float y, int realOffsetX, int realOffsetY, string text, ref Color colour)
        {
            renderer.DrawCenteredText(RealiseX(x) + realOffsetX, RealiseY(y) - realOffsetY, text, ref colour);
        }

        public void DrawAxesLabels()
        {
            int realOriginX = RealiseX(0);
            int realOriginY = RealiseY(0);


            float originX = 0;
            float originY = 0;
            int originOffsetX = -10;
            int originOffsetY = -10;
            ref Color colour = ref App.MainApp.graphingColours.axesColour;
            if (realOriginX < 0)
            {
                originX = minX;
                originOffsetX = 10;
                colour = ref App.MainApp.graphingColours.axesLabelsOutOfRange;
            }
            else if (realOriginX > pixelWidth)
            {
                originX = maxX;
                originOffsetX = -10;
                colour = ref App.MainApp.graphingColours.axesLabelsOutOfRange;
            }
            if (realOriginY < 0)
            {
                originY = maxY;
                originOffsetY = -10;
                colour = ref App.MainApp.graphingColours.axesLabelsOutOfRange;
            }
            else if (realOriginY > pixelHeight)
            {
                originY = minY;
                originOffsetY = 10;
                colour = ref App.MainApp.graphingColours.axesLabelsOutOfRange;
            }
            DrawVirtualLabel(originX, originY, originOffsetX, originOffsetY, "0", ref colour);


            colour = ref App.MainApp.graphingColours.axesColour;
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
                        colour = ref App.MainApp.graphingColours.axesLabelsOutOfRange;
                    }
                    else if (realOriginY > pixelHeight)
                    {
                        yValue = minY;
                        offset = 10;
                        colour = ref App.MainApp.graphingColours.axesLabelsOutOfRange;
                    }
                    DrawVirtualLabel(x, yValue, 0, offset, x.ToString(decimalPlaces), ref colour);
                }
                x += majorGridXStep;
            }

            colour = ref App.MainApp.graphingColours.axesColour;
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
                        colour = ref App.MainApp.graphingColours.axesLabelsOutOfRange;
                    }
                    else if (realOriginX > pixelWidth)
                    {
                        xValue = maxX;
                        offset = -10;
                        colour = ref App.MainApp.graphingColours.axesLabelsOutOfRange;
                    }
                    DrawVirtualLabel(xValue, y, offset, 0, y.ToString(decimalPlaces), ref colour);
                }
                y += majorGridYStep;
            }
        }
    }

    internal class GraphRenderer : Renderer
    {
        public GraphRenderer(ref BitmapRenderer newRenderer)
        {
            renderer = newRenderer;
        }

        public void DrawGraph(BSPNode root)
        {
            DrawCell(ref root);
            for (int index = 0; index < root.children.Count; index++)
            {
                DrawGraph(root.children[index]);
            }
        }

        public void DrawCell(ref BSPNode node)
        {
            int realXMin = RealiseX(node.xMin);
            int realXMax = RealiseX(node.xMax);
            int realYMin = RealiseY(node.yMin);
            int realYMax = RealiseY(node.yMax);
            if (node.containsGraph && node.children.Count == 0)
            {
                renderer.DrawRectangle(realXMin, realYMax, realXMax - realXMin, realYMin - realYMax, ref App.MainApp.graphingColours.axesColour);
            }
        }
    }

    internal class BSPRenderer : Renderer
    {
        public BSPRenderer(ref BitmapRenderer newRenderer)
        {
            renderer = newRenderer;
        }

        public void DrawBSP(BSPNode root)
        {
            if (root.children.Count == 0)
            {
                DrawBSPNode(ref root);
            }
            for (int index = 0; index < root.children.Count; index++)
            {
                DrawBSP(root.children[index]);
            }
        }

        public void DrawBSPNode(ref BSPNode node)
        {
            int realXMin = RealiseX(node.xMin);
            int realXMax = RealiseX(node.xMax);
            int realYMin = RealiseY(node.yMin);
            int realYMax = RealiseY(node.yMax);
            if (node.containsGraph)
            {
                renderer.DrawBorder(realXMin, realYMax, realXMax - realXMin, realYMin - realYMax, ref App.MainApp.graphingColours.BSPLineValid, 1);
            }
            else
            {
                renderer.DrawBorder(realXMin, realYMax, realXMax - realXMin, realYMin - realYMax, ref App.MainApp.graphingColours.BSPLineInvalid, 1);
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

    public class GraphingColours
    {
        public Color background;
        public Color axesColour;
        public Color axesLabelsOutOfRange;
        public Color majorGridColour;
        public Color minorGridColour;
        public Color performanceStats;
        public Color BSPLineInvalid;
        public Color BSPLineValid;

        public void UpdateColours()
        {
            System.Windows.Media.Color importColour;

            importColour = (System.Windows.Media.Color)App.MainApp.FindResource("GraphBackground");
            background = Color.FromArgb(255, importColour.R, importColour.G, importColour.B);

            importColour = (System.Windows.Media.Color)App.MainApp.FindResource("GraphAxes");
            axesColour = Color.FromArgb(255, importColour.R, importColour.G, importColour.B);

            importColour = (System.Windows.Media.Color)App.MainApp.FindResource("GraphAxesLabelsOutOfRange");
            axesLabelsOutOfRange = Color.FromArgb(255, importColour.R, importColour.G, importColour.B);

            importColour = (System.Windows.Media.Color)App.MainApp.FindResource("GraphMajorGrid");
            majorGridColour = Color.FromArgb(255, importColour.R, importColour.G, importColour.B);

            importColour = (System.Windows.Media.Color)App.MainApp.FindResource("GraphMinorGrid");
            minorGridColour = Color.FromArgb(255, importColour.R, importColour.G, importColour.B);

            importColour = (System.Windows.Media.Color)App.MainApp.FindResource("GraphPerformanceStats");
            performanceStats = Color.FromArgb(255, importColour.R, importColour.G, importColour.B);

            importColour = (System.Windows.Media.Color)App.MainApp.FindResource("GraphBSPLineInvalid");
            BSPLineInvalid = Color.FromArgb(255, importColour.R, importColour.G, importColour.B);

            importColour = (System.Windows.Media.Color)App.MainApp.FindResource("GraphBSPLineValid");
            BSPLineValid = Color.FromArgb(255, importColour.R, importColour.G, importColour.B);
        }
    }
}
