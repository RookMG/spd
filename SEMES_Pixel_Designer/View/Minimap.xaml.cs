using SEMES_Pixel_Designer.Utils;
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
using System.Windows.Shapes;

namespace SEMES_Pixel_Designer.View
{
    /// <summary>
    /// Minimap.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Minimap : Window
    {
        public Minimap()
        {
            InitializeComponent();
            SizeChanged += new SizeChangedEventHandler(Coordinates.MinimapRef.ResizeWindow);
        }
    }

    public class MinimapCanvas : Canvas
    {
        public Polygon CanvasPosition;
        private Point offset;
        public MinimapCanvas()
        {
            Coordinates.MinimapRef = this;
            CanvasPosition = new Polygon
            {
                Fill = Coordinates.highlightBrush,
                Stroke = Coordinates.defaultColorBrush,
            };
            CanvasPosition.Points.Add(new Point(0, 0));
            CanvasPosition.Points.Add(new Point(0, 0));
            CanvasPosition.Points.Add(new Point(0, 0));
            CanvasPosition.Points.Add(new Point(0, 0));
            Children.Add(CanvasPosition);

            MouseRightButtonUp += Move_MouseRightButtonUp;
            MouseLeftButtonDown += Drag_MouseLeftButtonDown;
            MouseWheel += Zoom_MouseWheel;

        }
        public void ResizeWindow(object sender, SizeChangedEventArgs e)
        {
            AdjustRatio();
        }

        public void UpdatePosition()
        {
            double sx = (Coordinates.minX - Coordinates.glassLeft) * ActualWidth / (Coordinates.GetGlassRight() - Coordinates.glassLeft),
                sy = (Coordinates.GetGlassTop() - Coordinates.minY) * ActualHeight / (Coordinates.GetGlassTop() - Coordinates.glassBottom),
                ex = (Coordinates.maxX - Coordinates.glassLeft) * ActualWidth / (Coordinates.GetGlassRight() - Coordinates.glassLeft),
                ey = (Coordinates.GetGlassTop() - Coordinates.maxY) * ActualHeight / (Coordinates.GetGlassTop() - Coordinates.glassBottom);
            CanvasPosition.Points[0] = new Point(sx, sy);
            CanvasPosition.Points[1] = new Point(ex, sy);
            CanvasPosition.Points[2] = new Point(ex, ey);
            CanvasPosition.Points[3] = new Point(sx, ey);
        }

        public void AdjustRatio()
        {
            if((Coordinates.GetGlassTop() - Coordinates.glassBottom) / (Coordinates.GetGlassRight() - Coordinates.glassLeft) > Window.GetWindow(this).ActualHeight / Window.GetWindow(this).ActualWidth)
            {
                Height = Window.GetWindow(this).ActualHeight - 40;
                Width = ActualHeight * (Coordinates.GetGlassRight() - Coordinates.glassLeft) / (Coordinates.GetGlassTop() - Coordinates.glassBottom);
            }
            else
            {
                Width = Window.GetWindow(this).ActualWidth - 40;
                Height = ActualWidth * (Coordinates.GetGlassTop() - Coordinates.glassBottom) / (Coordinates.GetGlassRight() - Coordinates.glassLeft);
            }
            UpdatePosition();
        }

        private void Drag_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            offset = new Point(e.GetPosition(this).X, e.GetPosition(this).Y);
            MouseMove += Drag_MouseMove;
            MouseLeftButtonUp += Drag_MouseLeftButtonUp;
        }

        private void Drag_MouseMove(object sender, MouseEventArgs e)
        {
            double dx = (e.GetPosition(this).X - offset.X) * (Coordinates.GetGlassRight() - Coordinates.glassLeft) / ActualWidth,
                dy = -(e.GetPosition(this).Y - offset.Y) * (Coordinates.GetGlassTop() - Coordinates.glassBottom) / ActualHeight;
            Coordinates.minX += dx;
            Coordinates.minY += dy;
            Coordinates.maxX += dx;
            Coordinates.maxY += dy;
            offset = new Point(e.GetPosition(this).X, e.GetPosition(this).Y);
            Coordinates.CanvasRef.UpdateCanvas();
        }

        private void Drag_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseMove -= Drag_MouseMove;
            MouseLeftButtonUp -= Drag_MouseLeftButtonUp;
        }
        private void Move_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            double x = e.GetPosition(this).X * (Coordinates.GetGlassRight() - Coordinates.glassLeft) / ActualWidth,
                y = (ActualHeight - e.GetPosition(this).Y) * (Coordinates.GetGlassTop() - Coordinates.glassBottom) / ActualHeight,
                w = (Coordinates.maxX - Coordinates.minX)/2,
                h = (Coordinates.maxY - Coordinates.minY)/2;
            Coordinates.maxX = x + w;
            Coordinates.minX = x - w;
            Coordinates.maxY = y + h;
            Coordinates.minY = y - h;
            Coordinates.CanvasRef.UpdateCanvas();
        }

        private void Zoom_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;
            double scaleFactor = e.Delta > 0 ? 1.1 : 0.91;
            Width = ActualWidth*scaleFactor;
            AdjustRatio();
        }
    }
}
