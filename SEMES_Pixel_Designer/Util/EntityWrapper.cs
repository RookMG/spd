using netDxf.Collections;
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

    public static class Coordinates
    {
        public static double minX = 0.0, minY = 0.0, maxX = 1000.0, maxY = 1000.0;
        public static MainCanvas CanvasRef;


        public static void UpdateRange(DrawingEntities entities)
        {
            List<double> x = new List<double>(), y = new List<double>();
            foreach (netDxf.Entities.Line line in entities.Lines)
            {
                x.Add(line.StartPoint.X);
                y.Add(line.StartPoint.Y);
                x.Add(line.EndPoint.X);
                y.Add(line.EndPoint.Y);
            }
            foreach (netDxf.Entities.Polyline2D polyline in entities.Polylines2D)
            {
                foreach (netDxf.Entities.Polyline2DVertex point in polyline.Vertexes)
                {
                    x.Add(point.Position.X);
                    y.Add(point.Position.Y);
                }
            }

            if (x.Count <= 0) return;
            minX = x.Min();
            minY = y.Min();
            maxX = x.Max();
            maxY = y.Max();
        }

        public static double ToScreenX(double dxfX)
        {
            return (dxfX - minX) * CanvasRef.ActualWidth / (maxX - minX);
        }

        public static double ToScreenY(double dxfY)
        {
            return CanvasRef.ActualHeight * (1 - (dxfY - minY) / (maxY - minY));
        }

        public static double ToDxfX(double screenX)
        {
            return screenX * (maxX - minX) / CanvasRef.ActualWidth + minX;
        }

        public static double ToDxfY(double screenY)
        {
            return (CanvasRef.ActualHeight - screenY) * (maxY - minY) / CanvasRef.ActualHeight + minY;
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
            MovePosition(x, y);

            if (updateParentAction != null) updateParentAction(x, y);
        }

        public void MovePosition(double x, double y)
        {
            SetX(point, x - P_RADIUS);
            SetY(point, y - P_RADIUS);
            SetX(selectArea, x - SELECT_RADIUS);
            SetY(selectArea, y - SELECT_RADIUS);
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
        private List<double[]> dxfCoords = new List<double[]>();
        private List<Action<double, double>> setDxfCoordAction = new List<Action<double, double>>();

        public static Func<UIElement, int> BindCanvasAction;


        #region 생성자
        // 공통으로 사용되는 변수들 초기화
        // 단독으로 사용 X
        public void init()
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

        // Line 생성자
        public PolygonEntity(netDxf.Entities.Line line)
        {
            init();
            setDxfCoordAction.Add((double x, double y) => { line.StartPoint = new netDxf.Vector3(x, y, 0); });
            setDxfCoordAction.Add((double x, double y) => { line.EndPoint = new netDxf.Vector3(x, y, 0); });
            dxfCoords.Add(new double[] { line.StartPoint.X, line.StartPoint.Y });
            dxfCoords.Add(new double[] { line.EndPoint.X, line.EndPoint.Y });
            AddPoint(line.StartPoint);
            AddPoint(line.EndPoint);
        }

        // 2d polyline 생성자
        // 3d polyline이 있다면 파일 오픈 시 2d polyline으로 변경해 사용 중 (Mainwindow.OpenDxf 참고);
        public PolygonEntity(netDxf.Entities.Polyline2D polyline)
        {
            init();
            setDxfCoordAction = new List<Action<double, double>>();
            foreach (var point in polyline.Vertexes)
            {
                setDxfCoordAction.Add((double x, double y) => { point.Position = new netDxf.Vector2(x, y); });
                dxfCoords.Add(new double[] { point.Position.X, point.Position.Y });
                AddPoint(point.Position);
            }
        }
        #endregion


        public void AddPoint(double dxfX, double dxfY)
        {
            // TODO : DXF 파일에서의 점 추가
            var idx = polygon.Points.Count;
            double screenX = Coordinates.ToScreenX(dxfX);
            double screenY = Coordinates.ToScreenY(dxfY);
            polygon.Points.Add(new System.Windows.Point(screenX, screenY));
            points.Add(new PointEntity(screenX, screenY, (nx, ny) => { UpdatePoint(nx, ny, idx, true); }));
        }
        public void AddPoint(netDxf.Vector2 point)
        {
            AddPoint(point.X, point.Y);
        }
        public void AddPoint(netDxf.Vector3 point)
        {
            AddPoint(point.X, point.Y);
        }

        public void ReDraw()
        {
            if (dxfCoords == null) return;
            for (int i = 0; i < polygon.Points.Count; i++) UpdatePoint(i);
        }

        private void UpdatePoint(int idx)
        {
            UpdatePoint(Coordinates.ToScreenX(dxfCoords[idx][0]), Coordinates.ToScreenY(dxfCoords[idx][1]), idx, false);
        }

        private void UpdatePoint(double screenX, double screenY, int idx, bool updateDxf)
        {
            polygon.Points[idx] = new Point(screenX, screenY);
            points[idx].MovePosition(screenX, screenY);

            double dxfX = Coordinates.ToDxfX(screenX);
            double dxfY = Coordinates.ToDxfY(screenY);

            if (!updateDxf) return;
            setDxfCoordAction[idx](dxfX, dxfY);
            dxfCoords[idx][0] = dxfX;
            dxfCoords[idx][1] = dxfY;
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
                UpdatePoint(offsets[i].X + e.GetPosition(Coordinates.CanvasRef).X, offsets[i].Y + e.GetPosition(Coordinates.CanvasRef).Y, i, false);
            }
        }

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            polygon.MouseMove -= MouseMove;
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                UpdatePoint(offsets[i].X + e.GetPosition(Coordinates.CanvasRef).X, offsets[i].Y + e.GetPosition(Coordinates.CanvasRef).Y, i, true);
            }
            Mouse.Capture(null);
            offsets = null;
        }

    }

}
