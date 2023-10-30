using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SEMES_Pixel_Designer.Utils
{
    class EntityWrapper
    {
    }

    static class Coordinates
    {
        static public double minX = 0.0, minY = 0.0, maxX = 1000.0, maxY = 1000.0;
        static public MainCanvas CanvasRef;


        static public void updateRange(List<double> x, List<double> y)
        {
            if (x.Count <= 0) return;
            minX = x.Min();
            minY = y.Min();
            maxX = x.Max();
            maxY = y.Max();
        }

        static public double toScreenX(double dxfX)
        {
            return (dxfX - minX) * CanvasRef.ActualWidth / (maxX - minX);
        }

        static public double toScreenY(double dxfY)
        {
            return CanvasRef.ActualHeight * (1 - (dxfY - minY) / (maxY - minY));
        }

        static public double toDxfX(double screenX)
        {
            return screenX * (maxX - minX) / CanvasRef.ActualWidth + minX;
        }

        static public double toDxfY(double screenY)
        {
            return (CanvasRef.ActualWidth - screenY) * (maxY - minY) / CanvasRef.ActualWidth + minY;
        }

    }


    public class PointEntity
    {
        public Ellipse point, selectArea;

        private UIElement source = null;
        private bool isDragging = false;
        private Point position, offset;
        private Action<double, double> updateParentAction;

        public static Func<UIElement, int> BindCanvasAction;
        public static Action<UIElement, double> SetX, SetY;
        public static double P_RADIUS = 5, SELECT_RADIUS = 10;

        public PointEntity(double x, double y, Action<double, double> updateParentAction)
        {
            point = new Ellipse
            {
                Width = P_RADIUS * 2,
                Height = P_RADIUS * 2,
                Fill = Brushes.Black,
            };
            selectArea = new Ellipse
            {
                Width = SELECT_RADIUS * 2,
                Height = SELECT_RADIUS * 2,
                Fill = Brushes.Transparent,
                // Stroke = Brushes.Black,
            };
            selectArea.MouseLeftButtonDown += MouseLeftButtonDown;
            selectArea.MouseLeftButtonUp += MouseLeftButtonUp;
            position = new Point(x, y);

            BindCanvasAction(point);
            BindCanvasAction(selectArea);
            UpdatePosition();
            this.updateParentAction = updateParentAction;
        }

        public PointEntity(double x, double y) : this(x, y, null)
        {
        }

        private void UpdatePosition()
        {
            UpdatePosition(position.X, position.Y);
        }

        public void UpdatePosition(double x, double y)
        {
            SetX(point, x - P_RADIUS);
            SetY(point, y - P_RADIUS);
            SetX(selectArea, x - SELECT_RADIUS);
            SetY(selectArea, y - SELECT_RADIUS);

            if (updateParentAction != null) updateParentAction(x, y);
        }

        private void MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            source = (UIElement)sender;
            Mouse.Capture(source);
            isDragging = true;
            offset = position;
            selectArea.MouseMove += MouseMove;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;
            UpdatePosition(e.GetPosition(Coordinates.CanvasRef).X, e.GetPosition(Coordinates.CanvasRef).Y);
        }

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            isDragging = false;
            selectArea.MouseMove -= MouseMove;
        }
    }

    public class PolygonEntity
    {
        public Polygon polygon;

        private UIElement source = null;
        private PointCollection offsets = null;
        private List<PointEntity> points = null;

        public static Func<UIElement, int> BindCanvasAction;
        public PolygonEntity()
        {
            polygon = new Polygon();
            polygon.MouseLeftButtonDown += MouseLeftButtonDown;
            polygon.MouseLeftButtonUp += MouseLeftButtonUp;
            polygon.Fill = Brushes.Transparent;
            polygon.Stroke = Brushes.Black;
            polygon.StrokeThickness = 1;

            points = new List<PointEntity>();

            BindCanvasAction(polygon);
        }


        public void AddPoint(double dxfX, double dxfY)
        {
            var idx = polygon.Points.Count;
            double x = Coordinates.toScreenX(dxfX);
            double y = Coordinates.toScreenY(dxfY);
            polygon.Points.Add(new System.Windows.Point(x, y));
            points.Add(new PointEntity(x, y, (nx, ny) => { polygon.Points[idx] = new Point(nx, ny); }));
        }

        private void updatePoint(double x, double y, int idx)
        {
            polygon.Points[idx] = new Point(x, y);
            points[idx].UpdatePosition(x, y);
        }

        private void MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            source = (UIElement)sender;
            Mouse.Capture(source);
            offsets = new PointCollection();
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                offsets.Add(new Point(polygon.Points[i].X - e.GetPosition(Coordinates.CanvasRef).X, polygon.Points[i].Y - e.GetPosition(Coordinates.CanvasRef).Y));
            }
            polygon.MouseMove += MouseMove;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (offsets == null) return;

            for (int i = 0; i < polygon.Points.Count; i++)
            {
                updatePoint(offsets[i].X + e.GetPosition(Coordinates.CanvasRef).X, offsets[i].Y + e.GetPosition(Coordinates.CanvasRef).Y, i);
            }
        }

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            offsets = null;
            polygon.MouseMove -= MouseMove;
        }

    }

}
