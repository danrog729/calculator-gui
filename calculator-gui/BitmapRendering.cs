using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace calculator_gui
{
    public class BitmapRenderer
    {
        public WriteableBitmap bitmap;
        private Bitmap backingBitmap;
        private Graphics graphics;
        private int width;
        private int height;

        public BitmapRenderer(int newWidth, int newHeight)
        {
            width = newWidth;
            height = newHeight;

            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
            backingBitmap = new Bitmap(width, height, bitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmap.BackBuffer);
            graphics = Graphics.FromImage(backingBitmap);
        }

        public void Fill(System.Drawing.Color colour)
        {
            bitmap.Lock();
            graphics.Clear(colour);
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Unlock();
        }

        public void DrawVerticalLine(int x, ref System.Drawing.Color colour, int thickness)
        {
            if (x < 0 || x >= bitmap.PixelWidth)
            {
                return;
            }
            if (x + thickness > bitmap.PixelWidth)
            {
                thickness = bitmap.PixelWidth - x;
            }
            bitmap.Lock();
            graphics.DrawLine(new System.Drawing.Pen(colour, thickness), new System.Drawing.Point(x, 0), new System.Drawing.Point(x, bitmap.PixelHeight));
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(x, 0, thickness, bitmap.PixelHeight));
            bitmap.Unlock();
        }

        public void DrawHorizontalLine(int y, ref System.Drawing.Color colour, int thickness)
        {
            if (y < 0 || y >= bitmap.PixelHeight)
            {
                return;
            }
            if (y + thickness > bitmap.PixelHeight)
            {
                thickness = bitmap.PixelHeight - y;
            }
            bitmap.Lock();
            graphics.DrawLine(new System.Drawing.Pen(colour, thickness), new System.Drawing.Point(0, y), new System.Drawing.Point(bitmap.PixelWidth, y));
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, y, bitmap.PixelWidth, thickness));
            bitmap.Unlock();
        }

        public void DrawLine(int startX, int startY, int endX, int endY, ref System.Drawing.Color colour, int thickness)
        {
            int minX = startX;
            int minY = startY;
            int maxX = endX;
            int maxY = endY;
            if (endX < startX)
            {
                minX = endX;
                maxX = startX;
            }
            if (endY < startY)
            {
                minY = endY;
                maxY = startY;
            }
            if (minX < 0)
            {
                minX = 0;
            }
            if (minY < 0)
            {
                minY = 0;
            }
            if (maxX >= bitmap.PixelWidth)
            {
                maxX = bitmap.PixelWidth - 1;
            }
            if (maxY >= bitmap.PixelHeight)
            {
                maxY = bitmap.PixelHeight - 1;
            }
            bitmap.Lock();
            graphics.DrawLine(new System.Drawing.Pen(colour, thickness), startX, startY, endX, endY);
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(minX, minY, maxX - minX, maxY - minY));
            bitmap.Unlock();
        }

        public void DrawText(int x, int y, string text, ref System.Drawing.Color colour)
        {
            if (x < 0 || x >= bitmap.PixelWidth || y < 0 || y >= bitmap.PixelHeight)
            {
                return;
            }
            bitmap.Lock();
            SizeF size = graphics.MeasureString(text, new Font("Segoe UI", 10));
            graphics.DrawString(text, new Font("Segoe UI", 10), new SolidBrush(colour), new System.Drawing.Point(x, y));
            if (x + size.Width > bitmap.PixelWidth)
            {
                size.Width = bitmap.PixelWidth - x;
            }
            if (y + size.Height > bitmap.PixelHeight)
            {
                size.Height = bitmap.PixelHeight - y;
            }
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(x, y, (int)size.Width, (int)size.Height));
            bitmap.Unlock();
        }

        public void DrawCenteredText(int x, int y, string text, ref System.Drawing.Color colour)
        {
            SizeF size = graphics.MeasureString(text, new Font("Segoe UI", 10));
            DrawText((int)(x - size.Width / 2), (int)(y - size.Height / 2), text, ref colour);
        }

        public void DrawBorder(int x, int y, int width, int height, ref System.Drawing.Color border, int thickness)
        {
            if (width < 0 || height < 0 || x < 0 || x >= bitmap.PixelWidth || y < 0 || y >= bitmap.PixelHeight)
            {
                return;
            }
            if (x + width >= bitmap.PixelWidth)
            {
                width = bitmap.PixelWidth - x;
            }
            if (y + height >= bitmap.PixelHeight)
            {
                height = bitmap.PixelHeight - y;
            }
            bitmap.Lock();
            graphics.DrawRectangle(new System.Drawing.Pen(border, thickness), x, y, width, height);
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(x, y, width, height));
            bitmap.Unlock();
        }

        public void DrawRectangle(int x, int y, int width, int height, ref System.Drawing.Color fill)
        {
            if (width < 0 || height < 0 || x < 0 || x >= bitmap.PixelWidth || y < 0 || y >= bitmap.PixelHeight)
            {
                return;
            }
            if (x + width >= bitmap.PixelWidth)
            {
                width = bitmap.PixelWidth - x;
            }
            if (y + height >= bitmap.PixelHeight)
            {
                height = bitmap.PixelHeight - y;
            }
            bitmap.Lock();
            graphics.FillRectangle(new SolidBrush(fill), x, y, width, height);
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(x, y, width, height));
            bitmap.Unlock();
        }
    }
}
