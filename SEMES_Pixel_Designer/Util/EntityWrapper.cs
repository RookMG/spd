﻿using netDxf;
using netDxf.Collections;
using netDxf.Entities;
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
        public static double minX = 0.0, minY = 0.0, maxX = 1000.0, maxY = 1000.0, ratio = 1.0, gridSpacing = 0.5;
        public static MainCanvas CanvasRef;
        public static List<System.Windows.Shapes.Line> gridLines = new List<System.Windows.Shapes.Line>();
        public static SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(0x66, 0x66, 0x66, 0x66));

        public static Func<UIElement, int> BindCanvasAction;
        public static Action<UIElement> UnbindCanvasAction;
        public static Action<UIElement, int> SetZIndexAction;

        public static void UpdateRange(DrawingEntities entities)
        {
            gridSpacing = 0.5;
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
            AdjustRatio();
        }

        public static void AdjustRatio()
        {
            if ((maxY - minY) / (maxX - minX) > CanvasRef.ActualHeight / CanvasRef.ActualWidth)
            {
                double dx = (maxY - minY) * CanvasRef.ActualWidth / (CanvasRef.ActualHeight) - maxX + minX;
                maxX += dx / 2;
                minX -= dx / 2;
            }
            else
            {
                double dy = (maxX - minX) * CanvasRef.ActualHeight / (CanvasRef.ActualWidth) - maxY + minY;
                maxY += dy / 2;
                minY -= dy / 2;
            }
            ratio = CanvasRef.ActualWidth / (maxX - minX);
        }
        public static void DrawGrid()
        {
            gridSpacing = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(maxY - minY, maxX - minX))));
            double sX = Math.Floor(minX / gridSpacing) * gridSpacing, eX = (1 + Math.Floor(maxX / gridSpacing)) * gridSpacing,
                 sY = Math.Floor(minY / gridSpacing) * gridSpacing, eY = (1 + Math.Floor(maxY / gridSpacing)) * gridSpacing;
            foreach (System.Windows.Shapes.Line line in gridLines) UnbindCanvasAction(line);
            gridLines.Clear();
            for (double mx = sX; mx <= eX; mx += gridSpacing)
            {
                for (int i=0;i<10;i++)
                {
                    double x = mx + i * gridSpacing / 10;
                    System.Windows.Shapes.Line line = new System.Windows.Shapes.Line
                    {
                        Stroke = brush,
                        X1 = ToScreenX(x),
                        Y1 = ToScreenY(sY),
                        X2 = ToScreenX(x),
                        Y2 = ToScreenY(eY),
                        StrokeThickness = i==0?3:1
                    };
                    gridLines.Add(line);
                    BindCanvasAction(line);
                }
            }
            for (double my = sY; my <= eY; my += gridSpacing)
            {
                for (int i = 0; i < 10; i++)
                {
                    double y = my + i * gridSpacing / 10;
                    System.Windows.Shapes.Line line = new System.Windows.Shapes.Line
                    {
                        Stroke = brush,
                        X1 = ToScreenX(sX),
                        Y1 = ToScreenY(y),
                        X2 = ToScreenX(eX),
                        Y2 = ToScreenY(y),
                        StrokeThickness = i==0?3:1
                    };
                    gridLines.Add(line);
                    BindCanvasAction(line);
                }
            }
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

        public static string ToolTip(double dxfX, double dxfY)
        {
            return string.Format("x : {0}, y : {1}",dxfX,dxfY);
        }

    }


    public class PointEntity
    {
        public System.Windows.Shapes.Ellipse point, selectArea;

        private UIElement source = null;
        private bool isDragging = false;
        private System.Windows.Point position, offset;
        private Action<double, double> updateParentAction;

        public static Action<UIElement, double> SetX, SetY;
        public static readonly double P_RADIUS = 4, SELECT_RADIUS = 8;

        public PointEntity(double x, double y, Action<double, double> updateParentAction)
        {
            point = new System.Windows.Shapes.Ellipse
            {
                Width = P_RADIUS * 2,
                Height = P_RADIUS * 2,
                Fill = Brushes.Black,
            };
            selectArea = new System.Windows.Shapes.Ellipse
            {
                Width = SELECT_RADIUS * 2,
                Height = SELECT_RADIUS * 2,
                Fill = Brushes.Transparent,
                // Stroke = Brushes.Black,
            };
            selectArea.MouseLeftButtonDown += MouseLeftButtonDown;
            selectArea.MouseLeftButtonUp += MouseLeftButtonUp;
            position = new System.Windows.Point(x, y);

            Coordinates.SetZIndexAction(point, 3);
            Coordinates.SetZIndexAction(selectArea, 4);
            // BindCanvasAction(point);
            // BindCanvasAction(selectArea);
            UpdatePosition();
            this.updateParentAction = updateParentAction;
        }

        public PointEntity(double x, double y) : this(x, y, null)
        {
        }

        public void BindCanvas()
        {
            Coordinates.BindCanvasAction(point);
            Coordinates.BindCanvasAction(selectArea);
        }

        public void UnbindCanvas()
        {
            Coordinates.UnbindCanvasAction(point);
            Coordinates.UnbindCanvasAction(selectArea);
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
        public Polygon polygon, selectArea;
        private UIElement source = null;

        private PointCollection offsets = null;
        private List<PointEntity> points = null;

        // dxf 파일에 직접 접근할 때 사용
        private EntityObject entityObject = null;

        private bool selected = false, visible = false;

        private List<double[]> dxfCoords = new List<double[]>();
        private List<Action<double, double>> setDxfCoordAction = new List<Action<double, double>>();

        public static List<PolygonEntity> selectedEntities = new List<PolygonEntity>();


        #region 생성자
        // 공통으로 사용되는 변수들 초기화
        // 단독으로 사용 X
        public void init()
        {
            // 생성자 아님!
            // 생성시 공통적으로 호출되는 내용들

            polygon = new Polygon();
            selectArea = new Polygon();


            selectArea.MouseLeftButtonDown += MouseLeftButtonDown;
            polygon.Fill = Brushes.Transparent;
            polygon.Stroke = Brushes.Black;
            polygon.StrokeThickness = 1;

            selectArea.Stroke = Brushes.Transparent;
            selectArea.StrokeThickness = 10;
            points = new List<PointEntity>();
            Coordinates.SetZIndexAction(polygon, 1);
            Coordinates.SetZIndexAction(selectArea, 2);

            //Coordinates.BindCanvasAction(polygon);
            //Coordinates.BindCanvasAction(selectArea);
        }

        // Line 생성자
        public PolygonEntity(netDxf.Entities.Line line)
        {
            init();
            entityObject = line;
            setDxfCoordAction.Add((double x, double y) => { line.StartPoint = new netDxf.Vector3(x, y, 0); });
            setDxfCoordAction.Add((double x, double y) => { line.EndPoint = new netDxf.Vector3(x, y, 0); });
            dxfCoords.Add(new double[] { line.StartPoint.X, line.StartPoint.Y });
            dxfCoords.Add(new double[] { line.EndPoint.X, line.EndPoint.Y });
            AddPoint(line.StartPoint);
            AddPoint(line.EndPoint);
            ReDraw();
        }

        // 2d polyline 생성자
        // 3d polyline이 있다면 파일 오픈 시 2d polyline으로 변경해 사용 중 (Mainwindow.OpenDxf 참고);
        public PolygonEntity(netDxf.Entities.Polyline2D polyline)
        {
            init();
            entityObject = polyline;
            setDxfCoordAction = new List<Action<double, double>>();
            foreach (var point in polyline.Vertexes)
            {
                setDxfCoordAction.Add((double x, double y) => { point.Position = new netDxf.Vector2(x, y); });
                dxfCoords.Add(new double[] { point.Position.X, point.Position.Y });
                AddPoint(point.Position);
            }
            ReDraw();
        }

        public PolygonEntity(Polygon polygon)
        {
            List<Vector2> vertexes = new List<Vector2>();
            foreach (var point in polygon.Points)
            {
                vertexes.Add(new Vector2(Coordinates.ToDxfX(point.X), Coordinates.ToDxfY(point.Y)));
            }
            Polyline2D polyline = new Polyline2D(vertexes);
            MainWindow.doc.Entities.Add(polyline);


            init();
            entityObject = polyline;
            setDxfCoordAction = new List<Action<double, double>>();
            foreach (var point in polyline.Vertexes)
            {
                setDxfCoordAction.Add((double x, double y) => { point.Position = new netDxf.Vector2(x, y); });
                dxfCoords.Add(new double[] { point.Position.X, point.Position.Y });
                AddPoint(point.Position);
            }
            ReDraw();
        }
        #endregion


        public void AddPoint(double dxfX, double dxfY)
        {
            // TODO : DXF 파일에서의 점 추가
            var idx = polygon.Points.Count;
            double screenX = Coordinates.ToScreenX(dxfX);
            double screenY = Coordinates.ToScreenY(dxfY);
            polygon.Points.Add(new System.Windows.Point(screenX, screenY));
            selectArea.Points.Add(new System.Windows.Point(screenX, screenY));
            PointEntity p = new PointEntity(screenX, screenY, (nx, ny) => { UpdatePoint(nx, ny, idx, true); });
            p.selectArea.ToolTip = Coordinates.ToolTip(dxfX, dxfY);
            points.Add(p);
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
            bool inCanvas = false;
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                UpdatePoint(i);
                if (Coordinates.minX < dxfCoords[i][0] && Coordinates.maxX > dxfCoords[i][0]
                    && Coordinates.minY < dxfCoords[i][1] && Coordinates.maxY > dxfCoords[i][1]) inCanvas = true;
            }
            if (inCanvas == visible) return;
            if (inCanvas)
            {
                visible = true;

                Coordinates.BindCanvasAction(polygon);
                Coordinates.BindCanvasAction(selectArea);
            }
            else
            {
                visible = false;

                Coordinates.UnbindCanvasAction(polygon);
                Coordinates.UnbindCanvasAction(selectArea);
            }
        }

        public void Delete()
        {
            ToggleSelected(false);
            polygon.Visibility = Visibility.Collapsed;
            Coordinates.UnbindCanvasAction(polygon);
            Coordinates.UnbindCanvasAction(selectArea);
            MainWindow.doc.Entities.Remove(entityObject);
        }
        public void Restore()
        {
            polygon.Visibility = Visibility.Visible;
            Coordinates.BindCanvasAction(polygon);
            Coordinates.BindCanvasAction(selectArea);
            MainWindow.doc.Entities.Add(entityObject);
        }
        public void Remove()
        {
            // TODO : 인스턴스 삭제 방법 찾기
            polygon.Visibility = Visibility.Collapsed;
            Coordinates.UnbindCanvasAction(polygon);
            Coordinates.UnbindCanvasAction(selectArea);
        }

        private void UpdatePoint(int idx)
        {
            UpdatePoint(Coordinates.ToScreenX(dxfCoords[idx][0]), Coordinates.ToScreenY(dxfCoords[idx][1]), idx, false);
        }

        private void UpdatePoint(List<double[]> positions)
        {
            for(int i=0;i<positions.Count;i++) UpdatePoint(Coordinates.ToScreenX(positions[i][0]), Coordinates.ToScreenY(positions[i][1]), i, true);
        }

        private void UpdatePoint(double screenX, double screenY, int idx, bool updateDxf)
        {
            polygon.Points[idx] = new System.Windows.Point(screenX, screenY);
            selectArea.Points[idx] = new System.Windows.Point(screenX, screenY);
            points[idx].MovePosition(screenX, screenY);

            double dxfX = Coordinates.ToDxfX(screenX);
            double dxfY = Coordinates.ToDxfY(screenY);

            points[idx].selectArea.ToolTip = Coordinates.ToolTip(dxfX, dxfY);

            if (!updateDxf) return;
            setDxfCoordAction[idx](dxfX, dxfY);
            dxfCoords[idx][0] = dxfX;
            dxfCoords[idx][1] = dxfY;
        }

        private void ToggleSelected(bool status)
        {
            if (status == selected) return;

            // TODO : 구현
            if (status)
            {
                selectedEntities.Add(this);
                polygon.Stroke = Brushes.Red;
                foreach (PointEntity point in points) point.BindCanvas();
            }
            else
            {
                selectedEntities.Remove(this);
                polygon.Stroke = Brushes.Black;
                foreach (PointEntity point in points) point.UnbindCanvas();
            }
            selected = status;
        }

        static public void ClearSelected()
        {
            while (selectedEntities.Count > 0) selectedEntities[0].ToggleSelected(false);
        }

        private void MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ToggleSelected(!selected);
                return;
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                ToggleSelected(true);
                return;
            }
            else if (!selected)
            {
                ClearSelected();
                ToggleSelected(true);
                return;
            }


            source = (UIElement)sender;
            Mouse.Capture(source);

            foreach(PolygonEntity selectedEntity in selectedEntities)
            {
                selectedEntity.offsets = new PointCollection();
                for (int i = 0; i < selectedEntity.polygon.Points.Count; i++)
                {
                    selectedEntity.offsets.Add(new System.Windows.Point(selectedEntity.polygon.Points[i].X - e.GetPosition(Coordinates.CanvasRef).X, selectedEntity.polygon.Points[i].Y - e.GetPosition(Coordinates.CanvasRef).Y));
                }
            }
            Coordinates.CanvasRef.MouseMove += MouseMove;
            Coordinates.CanvasRef.MouseLeftButtonUp += MouseLeftButtonUp;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            foreach (PolygonEntity selectedEntity in selectedEntities)
            {
                if (selectedEntity.offsets == null) continue;
                for (int i = 0; i < selectedEntity.polygon.Points.Count; i++)
                {
                    selectedEntity.UpdatePoint(selectedEntity.offsets[i].X + e.GetPosition(Coordinates.CanvasRef).X, selectedEntity.offsets[i].Y + e.GetPosition(Coordinates.CanvasRef).Y, i, false);
                }
            }
        }

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Coordinates.CanvasRef.MouseMove -= MouseMove;
            Coordinates.CanvasRef.MouseLeftButtonUp -= MouseLeftButtonUp;
            List<Action> forward = new List<Action>(), backward = new List<Action>();
            foreach (PolygonEntity selectedEntity in selectedEntities)
            {
                List<double[]> from = new List<double[]>(), to = new List<double[]>();
                for (int i = 0; i < selectedEntity.polygon.Points.Count; i++)
                {
                    from.Add(selectedEntity.dxfCoords[i].Clone() as double[]);
                    to.Add(new double[] { Coordinates.ToDxfX(selectedEntity.offsets[i].X + e.GetPosition(Coordinates.CanvasRef).X), Coordinates.ToDxfY(selectedEntity.offsets[i].Y + e.GetPosition(Coordinates.CanvasRef).Y) });
                }
                forward.Add(() => { selectedEntity.UpdatePoint(to); });
                backward.Add(() => { selectedEntity.UpdatePoint(from); });
            }
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    foreach (Action action in backward) action();
                },
                () => {
                    foreach (Action action in forward) action();
                },
                () => {
                }
            ));

            Mouse.Capture(null);
            offsets = null;

        }

    }

}
