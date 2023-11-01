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
                Fill = Brushes.CadetBlue,
            };
            selectArea = new Ellipse
            {
                Width = SELECT_RADIUS * 2,
                Height = SELECT_RADIUS * 2,
                Fill = Brushes.Transparent,
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
            UpdatePosition(e.GetPosition(MainDrawer.CanvasRef).X, e.GetPosition(MainDrawer.CanvasRef).Y);
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

        public void ChangePolygonColor()
        {
            if(polygon.Stroke == Brushes.Black)
            {
                polygon.Stroke = Brushes.White;
            }
            else
            {
                polygon.Stroke = Brushes.Black;
            }
        }


        public void AddPoint(double x, double y)
        {
            var idx = polygon.Points.Count;
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
                offsets.Add(new Point(polygon.Points[i].X - e.GetPosition(MainDrawer.CanvasRef).X, polygon.Points[i].Y - e.GetPosition(MainDrawer.CanvasRef).Y));
            }
            polygon.MouseMove += MouseMove;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (offsets == null) return;

            for (int i = 0; i < polygon.Points.Count; i++)
            {
                updatePoint(offsets[i].X + e.GetPosition(MainDrawer.CanvasRef).X, offsets[i].Y + e.GetPosition(MainDrawer.CanvasRef).Y, i);
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
