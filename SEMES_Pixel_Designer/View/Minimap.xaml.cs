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
        }
    }

    public class MinimapCanvas : Canvas
    {
        public Polygon CanvasPosition;
        private Point offset;
        public MinimapCanvas()
        {
            Coordinates.MinimapRef = this;
            SizeChanged += new SizeChangedEventHandler(ResizeWindow);
            CanvasPosition = new Polygon
            {
                Fill = Coordinates.transparentBrush,
                Stroke = Coordinates.defaultColorBrush,
            };
            CanvasPosition.Points.Add(new Point(0, 0));
            CanvasPosition.Points.Add(new Point(0, 0));
            CanvasPosition.Points.Add(new Point(0, 0));
            CanvasPosition.Points.Add(new Point(0, 0));
            Children.Add(CanvasPosition);

            CanvasPosition.MouseLeftButtonDown += Move_MouseLeftButtonDown;
            MouseWheel += Zoom_MouseWheel;

        }
        public void ResizeWindow(object sender, SizeChangedEventArgs e)
        {
            AdjustRatio();
        }

        public void UpdatePosition()
        {
            // TODO : 구현
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
            // TODO : 구현
            Height = ActualWidth * (Coordinates.GetGlassTop() - Coordinates.glassBottom) / (Coordinates.GetGlassRight() - Coordinates.glassLeft);
            UpdatePosition();
        }

        private void Move_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            offset = new Point(e.GetPosition(this).X, e.GetPosition(this).Y);
            MouseMove += Move_MouseMove;
            MouseLeftButtonUp += Move_MouseLeftButtonUp;
        }

        private void Move_MouseMove(object sender, MouseEventArgs e)
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

        private void Move_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseMove -= Move_MouseMove;
            MouseLeftButtonUp -= Move_MouseLeftButtonUp;
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
