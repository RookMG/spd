using netDxf;
using netDxf.Collections;
using netDxf.Entities;
using SEMES_Pixel_Designer.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;

namespace SEMES_Pixel_Designer.Utils
{

    public static class Coordinates
    {
        public static double minX = 0.0, minY = 0.0, maxX = 1000.0, maxY = 1000.0, ratio = 1.0, gridSpacing = 0.5,
            glassBottom = 0, glassLeft = 0, patternWidth = 1000, patternHeight = 1000, patternRows = 20, patternCols = 20, patternMarginX = 0, patternMarginY = 0;

        public static MainCanvas CanvasRef;
        public static MinimapCanvas MinimapRef;
        public static List<System.Windows.Shapes.Line> gridLines = new List<System.Windows.Shapes.Line>();
        public static System.Windows.Controls.TextBlock gridInfoText = new System.Windows.Controls.TextBlock();
        public static SolidColorBrush gridBrush = new SolidColorBrush(Color.FromArgb(0x99, 0x99, 0x99, 0x99)),
            defaultColorBrush = Brushes.Black,
            backgroundColorBrush = Brushes.White,
            transparentBrush = Brushes.Transparent,
            selectedColorBrush = Brushes.Red;

        public static Path canvasOutlinePath;
        public static StreamGeometry geometry;

        public static Func<UIElement, int> BindCanvasAction;
        public static Action<UIElement> UnbindCanvasAction;
        public static Action<UIElement, int> SetZIndexAction;
        public static Action<UIElement, double> SetLeftAction, SetTopAction;

        public static readonly double 
            //MINIMUM_VISIBLE_SIZE = 5, 
            MIN_GRID_SIZE = 15, 
            MAX_PATTERN_VIEW = 6,
             CANVAS_MARGIN = 200;

        public static void UpdateRange(DrawingEntities entities)
        {
            gridSpacing = 0.5;
            minX = minY = double.MaxValue;
            maxX = maxY = double.MinValue;
            foreach (netDxf.Entities.Line line in entities.Lines)
            {
                minX = Math.Min(minX, line.StartPoint.X);
                maxX = Math.Max(maxX, line.StartPoint.X);
                minY = Math.Min(minY, line.StartPoint.Y);
                maxY = Math.Max(maxY, line.StartPoint.Y);
                minX = Math.Min(minX, line.EndPoint.X);
                maxX = Math.Max(maxX, line.EndPoint.X);
                minY = Math.Min(minY, line.EndPoint.Y);
                maxY = Math.Max(maxY, line.EndPoint.Y);
            }
            foreach (netDxf.Entities.Polyline2D polyline in entities.Polylines2D)
            {
                foreach (netDxf.Entities.Polyline2DVertex point in polyline.Vertexes)
                {
                    minX = Math.Min(minX, point.Position.X);
                    maxX = Math.Max(maxX, point.Position.X);
                    minY = Math.Min(minY, point.Position.Y);
                    maxY = Math.Max(maxY, point.Position.Y);
                }
            }
            if (entities.Lines.Count() + entities.Polylines2D.Count() == 0)
            {
                minX = 0.0;
                minY = 0.0;
                maxX = 1000.0;
                maxY = 1000.0;
            }

            glassLeft = minX;
            glassBottom = minY;
            patternWidth = maxX - minX;
            patternHeight = maxY - minY;
            maxX = minX + Math.Min(MAX_PATTERN_VIEW,patternCols) * patternWidth;
            maxY = minY + Math.Min(MAX_PATTERN_VIEW,patternRows) * patternHeight;
            AdjustRatio();
        }

        public static void AdjustRatio()
        {
            if ((maxY - minY) * CanvasRef.ActualWidth > CanvasRef.ActualHeight * (maxX - minX))
            {
                double dy = (maxX - minX) * CanvasRef.ActualHeight / CanvasRef.ActualWidth - maxY + minY;
                maxY += dy;
            }
            else
            {
                double dx = (maxY - minY) * CanvasRef.ActualWidth / CanvasRef.ActualHeight - maxX + minX;
                maxX += dx;
            }
            ratio = CanvasRef.ActualWidth / (maxX - minX);
            if (MinimapRef == null) return;
            MinimapRef.AdjustRatio();
        }
        public static void DrawGrid()
        {
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new System.Windows.Point(ToScreenX(glassLeft), ToScreenY(glassBottom)), true /* is filled */, true /* is closed */);
                ctx.LineTo(new System.Windows.Point(ToScreenX(glassLeft), ToScreenY(GetGlassTop())), true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(new System.Windows.Point(ToScreenX(GetGlassRight()), ToScreenY(GetGlassTop())), true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(new System.Windows.Point(ToScreenX(GetGlassRight()), ToScreenY(glassBottom)), true /* is stroked */, false /* is smooth join */);
                ctx.BeginFigure(new System.Windows.Point(-1,-1), true /* is filled */, true /* is closed */);
                ctx.LineTo(new System.Windows.Point(-1, CanvasRef.ActualHeight + 1), true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(new System.Windows.Point(CanvasRef.ActualWidth + 1, CanvasRef.ActualHeight + 1), true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(new System.Windows.Point(CanvasRef.ActualWidth + 1, -1), true /* is stroked */, false /* is smooth join */);
            }

            gridSpacing = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(maxY - minY, maxX - minX))));
            if (ToScreenX(minX + gridSpacing * 0.1) < MIN_GRID_SIZE) gridSpacing *= 10;
            double sX = Math.Floor(minX / gridSpacing) * gridSpacing, eX = (1 + Math.Floor(maxX / gridSpacing)) * gridSpacing,
                 sY = Math.Floor(minY / gridSpacing) * gridSpacing, eY = (1 + Math.Floor(maxY / gridSpacing)) * gridSpacing;
            foreach (System.Windows.Shapes.Line line in gridLines) UnbindCanvasAction(line);
            gridLines.Clear();
            for (double mx = sX; mx <= eX; mx += gridSpacing)
            {
                for (int i = 0; i < 10; i++)
                {
                    double x = mx + i * gridSpacing / 10;
                    System.Windows.Shapes.Line line = new System.Windows.Shapes.Line
                    {
                        Stroke = gridBrush,
                        X1 = ToScreenX(x),
                        Y1 = ToScreenY(sY),
                        X2 = ToScreenX(x),
                        Y2 = ToScreenY(eY),
                        StrokeThickness = i == 0 ? 3 : 1
                    };
                    gridLines.Add(line);
                    BindCanvasAction(line);
                    SetZIndexAction(line, -1);
                }
            }
            for (double my = sY; my <= eY; my += gridSpacing)
            {
                for (int i = 0; i < 10; i++)
                {
                    double y = my + i * gridSpacing * 0.1;
                    System.Windows.Shapes.Line line = new System.Windows.Shapes.Line
                    {
                        Stroke = gridBrush,
                        X1 = ToScreenX(sX),
                        Y1 = ToScreenY(y),
                        X2 = ToScreenX(eX),
                        Y2 = ToScreenY(y),
                        StrokeThickness = i == 0 ? 3 : 1
                    };
                    gridLines.Add(line);
                    BindCanvasAction(line);
                    SetZIndexAction(line, -1);
                }
            }

            System.Windows.Shapes.Line infoLine = new System.Windows.Shapes.Line
            {
                Stroke = defaultColorBrush,
                X1 = 10 + ToScreenX(minX),
                Y1 = CanvasRef.ActualHeight - 10,
                X2 = 10 + ToScreenX(minX + gridSpacing * 0.1),
                Y2 = CanvasRef.ActualHeight - 10,
                StrokeThickness = 3
            };

            gridLines.Add(infoLine);
            BindCanvasAction(infoLine);
            SetZIndexAction(infoLine, -1);
            gridInfoText.Text = " : " + gridSpacing * 0.1;
            SetLeftAction(gridInfoText, 15 + ToScreenX(minX + gridSpacing * 0.1));
            SetTopAction(gridInfoText, CanvasRef.ActualHeight - 10 - gridInfoText.FontSize);
        }

        public static double GetGlassRight()
        {
            return glassLeft + patternWidth * patternCols + patternMarginX * (patternCols - 1);
        }

        public static double GetGlassTop()
        {
            return glassBottom + patternHeight * patternRows + patternMarginY * (patternRows - 1);
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

        public static double getPatternOffsetX(int column)
        {
            return column * (patternWidth + patternMarginX);
        }
        public static double getPatternOffsetY(int row)
        {
            return row * (patternHeight + patternMarginY);
        }

        public static string ToolTip(double dxfX, double dxfY)
        {
            return string.Format("x : {0}, y : {1}", dxfX, dxfY);
        }

    }


    public class PointEntity
    {
        public Path path;
        public StreamGeometry geometry;
        private UIElement source = null;
        private bool isDragging = false, deleted = false;
        private System.Windows.Point position, offset;
        private Action<double, double> updateParentAction;

        public static readonly double P_RADIUS = 4, SELECT_RADIUS = 8;

        public PointEntity(double x, double y, Action<double, double> updateParentAction)
        {
            path = new Path();
            geometry = new StreamGeometry();

            path.Fill = Coordinates.selectedColorBrush;
            path.MouseLeftButtonDown += MouseLeftButtonDown;
            path.MouseLeftButtonUp += MouseLeftButtonUp;
            position = new System.Windows.Point(x, y);

            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new System.Windows.Point(-P_RADIUS, 0), true /* is filled */, true /* is closed */);
                ctx.LineTo(new System.Windows.Point(0, -P_RADIUS), true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(new System.Windows.Point(P_RADIUS, 0), true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(new System.Windows.Point(0, P_RADIUS), true /* is stroked */, false /* is smooth join */);
            }
            geometry.FillRule = FillRule.EvenOdd;
            path.Data = geometry;
            Coordinates.SetZIndexAction(path, 4);
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
            Coordinates.BindCanvasAction(path);
        }

        public void UnbindCanvas()
        {
            Coordinates.UnbindCanvasAction(path);
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

        public void Delete()
        {
            path.Visibility = Visibility.Collapsed;
            UnbindCanvas();
            deleted = true;
        }
        public void Restore()
        {
            path.Visibility = Visibility.Visible;
            //BindCanvas();
            deleted = false;
        }
        public void Remove()
        {
            // TODO : 인스턴스 삭제 방법 찾기
        }

        public void MovePosition(double x, double y)
        {
            position = new System.Windows.Point(x, y);
            Coordinates.SetLeftAction(path, x);
            Coordinates.SetTopAction(path, y);
        }

        private void MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Coordinates.CanvasRef.MouseLeftButtonDown -= Coordinates.CanvasRef.Select_MouseLeftButtonDown;
            source = (UIElement)sender;
            Mouse.Capture(source);
            isDragging = true;
            offset = new System.Windows.Point(Coordinates.ToDxfX(position.X), Coordinates.ToDxfY(position.Y));
            path.MouseMove += MouseMove;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;
            UpdatePosition(e.GetPosition(Coordinates.CanvasRef).X, e.GetPosition(Coordinates.CanvasRef).Y);
        }

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            path.MouseMove -= MouseMove;
            Coordinates.CanvasRef.MouseLeftButtonDown += Coordinates.CanvasRef.Select_MouseLeftButtonDown;
            Mouse.Capture(null);
            isDragging = false;

            System.Windows.Point from = new System.Windows.Point(offset.X, offset.Y),
                to = new System.Windows.Point(Coordinates.ToDxfX(e.GetPosition(Coordinates.CanvasRef).X), Coordinates.ToDxfY(e.GetPosition(Coordinates.CanvasRef).Y));

            UpdatePosition(e.GetPosition(Coordinates.CanvasRef).X, e.GetPosition(Coordinates.CanvasRef).Y);
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () =>
                {
                    UpdatePosition(Coordinates.ToScreenX(from.X), Coordinates.ToScreenY(from.Y));
                },
                () =>
                {
                    UpdatePosition(Coordinates.ToScreenX(to.X), Coordinates.ToScreenY(to.Y));
                },
                () =>
                {
                }
            ));

        }
    }

    public enum PolygonEntityType
    {
        DOT = 0,
        LINE = 1,
        POLYLINE = 2,
        UNDEFINED = 3
    }

    public class PolygonEntity : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Path path, selectArea;
        StreamGeometry geometry;
        //public Polygon selectArea;

        public PointCollection dxfCoords = null;
        private PointCollection mouseOffsets = null, dxfOffsets = null;
        private List<PointEntity> points = null;
        public double minX, minY, maxX, maxY;

        // dxf 파일에 직접 접근할 때 사용
        private EntityObject entityObject = null;
        private PolygonEntityType entityType;

        public bool selected = false, visible = false, deleted = false;
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected != value)
                {
                    ToggleSelected(value);
                    OnPropertyChanged("Selected");
                }
            }
        }

        private List<Action<double, double>> setDxfCoordAction;

        public static List<PolygonEntity> selectedEntities = new List<PolygonEntity>();
        public static List<CopyData> clipboard = new List<CopyData>();

        public event PropertyChangedEventHandler PropertyChanged;

        public class CopyData
        { 
            public PointCollection dxfCoords { get; set; }
            public PolygonEntityType type { get; set; }
            public Vector3 offset { get; set; }

            public EntityObject GetEntityObject()
            {
                if (type == PolygonEntityType.LINE)
                {
                    netDxf.Entities.Line line = new netDxf.Entities.Line(new Vector2(dxfCoords[0].X, dxfCoords[0].Y), new Vector2(dxfCoords[1].X, dxfCoords[1].Y));
                    return line;
                }
                else if (type == PolygonEntityType.POLYLINE)
                {
                    Polyline2D polyline = new Polyline2D();
                    foreach (System.Windows.Point p in dxfCoords) polyline.Vertexes.Add(new Polyline2DVertex(p.X, p.Y));
                    return polyline;
                }

                return null;
            }

        }


        #region 생성자
        // 공통으로 사용되는 변수들 초기화
        // 단독으로 사용 X
        public void init()
        {
            // 생성자 아님!
            // 생성시 공통적으로 호출되는 내용들
            dxfCoords = new PointCollection();
            path = new Path();
            selectArea = new Path();
            geometry = new StreamGeometry();
            setDxfCoordAction = new List<Action<double, double>>();


            selectArea.MouseLeftButtonDown += MouseLeftButtonDown;
            path.Stroke = Coordinates.defaultColorBrush;
            selectArea.Stroke = Coordinates.transparentBrush;
            // path.Fill = Coordinates.fillColorBrush;
            path.StrokeThickness = 1;
            selectArea.StrokeThickness = 13;

            Coordinates.BindCanvasAction(path);
            Coordinates.BindCanvasAction(selectArea);
            Coordinates.SetZIndexAction(path, 1);
            Coordinates.SetZIndexAction(selectArea, 2);

            points = new List<PointEntity>();

        }

        // Line 생성자
        public PolygonEntity(netDxf.Entities.Line line)
        {
            init();
            entityObject = line;
            entityType = PolygonEntityType.LINE;

            InitDraw();
        }

        // 2d polyline 생성자
        // 3d polyline이 있다면 파일 오픈 시 2d polyline으로 변경해 사용 중 (Mainwindow.OpenDxf 참고);
        public PolygonEntity(Polyline2D polyline)
        {
            init();
            entityObject = polyline;
            entityType = PolygonEntityType.POLYLINE;

            InitDraw();
        }

        public PolygonEntity(Polygon polygon, PolygonEntityType type)
        {
            if (type == PolygonEntityType.POLYLINE)
            {
                List<Vector2> vertexes = new List<Vector2>();
                foreach (var point in polygon.Points)
                {
                    vertexes.Add(new Vector2(Coordinates.ToDxfX(point.X), Coordinates.ToDxfY(point.Y)));
                }
                Polyline2D polyline = new Polyline2D(vertexes);
                MainWindow.doc.Entities.Add(polyline);
                entityObject = polyline;
            }
            else if (type == PolygonEntityType.LINE)
            {
                netDxf.Entities.Line line = new netDxf.Entities.Line(
                    new Vector2(Coordinates.ToDxfX(polygon.Points[0].X), Coordinates.ToDxfY(polygon.Points[0].Y)),
                    new Vector2(Coordinates.ToDxfX(polygon.Points[1].X), Coordinates.ToDxfY(polygon.Points[1].Y))
                );
                MainWindow.doc.Entities.Add(line);
                entityObject = line;
            }

            init();
            entityType = type;

            InitDraw();
        }

        #endregion

        public PolygonEntityType GetPolygonType()
        {
            return entityType;
        }

        public EntityObject GetEntityObject()
        {
            return entityObject;
        }

        public static void CopySelected()
        {
            Coordinates.CanvasRef.pasteCount = 1;
            clipboard.Clear();
            foreach (PolygonEntity entity in selectedEntities)
            {
                clipboard.Add(new CopyData
                {
                    dxfCoords = entity.dxfCoords.Clone(),
                    type = entity.entityType,
                    offset = new Vector3(Coordinates.minX, Coordinates.minY, 0)
                });
            }
        }


        public void AddPoint(double dxfX, double dxfY)
        {
            // TODO : DXF 파일에서의 점 추가
            var idx = dxfCoords.Count;
            dxfCoords.Add(new System.Windows.Point(dxfX, dxfY));
            double screenX = Coordinates.ToScreenX(dxfX);
            double screenY = Coordinates.ToScreenY(dxfY);
            PointEntity p = new PointEntity(screenX, screenY, (nx, ny) => { UpdatePoint(nx, ny, idx, true); ReDraw(); });
            p.path.ToolTip = Coordinates.ToolTip(dxfX, dxfY);
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

        public void InitDraw()
        {
            if (entityType == PolygonEntityType.POLYLINE)
            {
                foreach (var point in ((Polyline2D)entityObject).Vertexes)
                {
                    setDxfCoordAction.Add((double x, double y) => { point.Position = new netDxf.Vector2(x, y); });
                    AddPoint(point.Position);
                }
            }
            else if (entityType == PolygonEntityType.LINE)
            {
                netDxf.Entities.Line line = (netDxf.Entities.Line)entityObject;
                setDxfCoordAction.Add((double x, double y) => { line.StartPoint = new netDxf.Vector3(x, y, 0); });
                setDxfCoordAction.Add((double x, double y) => { line.EndPoint = new netDxf.Vector3(x, y, 0); });
                AddPoint(line.StartPoint);
                AddPoint(line.EndPoint);
            }
            geometry.FillRule = FillRule.EvenOdd;
            path.Data = geometry;
            selectArea.Data = geometry;
            ReDraw();
        }

        public void ReDraw()
        {
            if (dxfCoords == null || deleted) return;

            minX = maxX = dxfCoords[0].X;
            minY = maxY = dxfCoords[0].Y;
            for (int i = 0; i < dxfCoords.Count; i++) UpdatePoint(i);

            using (StreamGeometryContext ctx = geometry.Open())
            {
                int rStart = 0, cStart = 0;
                while (Coordinates.getPatternOffsetX(cStart + 1) < Coordinates.minX - Coordinates.glassLeft) cStart++;
                while (Coordinates.getPatternOffsetY(rStart + 1) < Coordinates.minY - Coordinates.glassBottom) rStart++;
                for(int r = rStart; Coordinates.getPatternOffsetY(r) <= Coordinates.maxY && r < Coordinates.patternRows; r++)
                {
                    double yStart = Coordinates.getPatternOffsetY(r);
                    for (int c = cStart; Coordinates.getPatternOffsetX(c) <= Coordinates.maxX && c <Coordinates.patternCols; c++)
                    {
                        double xStart = Coordinates.getPatternOffsetX(c);
                        ctx.BeginFigure(new System.Windows.Point(Coordinates.ToScreenX(xStart + dxfCoords[0].X), Coordinates.ToScreenY(yStart + dxfCoords[0].Y)), true /* is filled */, true /* is closed */);
                        for (int i = 1; i < dxfCoords.Count; i++)
                            ctx.LineTo(new System.Windows.Point(Coordinates.ToScreenX(xStart + dxfCoords[i].X), Coordinates.ToScreenY(yStart + dxfCoords[i].Y)), true /* is stroked */, false /* is smooth join */);

                    }
                }

            }
            geometry.FillRule = FillRule.EvenOdd;
            path.Data = geometry;
            selectArea.Data = geometry;
            //bool valid =
            //    // 최소 크기 이상
            //    //(Math.Max(maxX - minX, maxY - minY) > Coordinates.MINIMUM_VISIBLE_SIZE / Coordinates.ratio)
            //// 화면에 포함됨
            // //&& 
            // maxX >= Coordinates.minX && minX <= Coordinates.maxX && maxY >= Coordinates.minY && minY <= Coordinates.maxY;
            //if (valid)
            //{
            //    using (StreamGeometryContext ctx = geometry.Open())
            //    {
            //        ctx.BeginFigure(new System.Windows.Point(Coordinates.ToScreenX(dxfCoords[0].X), Coordinates.ToScreenY(dxfCoords[0].Y)), true /* is filled */, true /* is closed */);
            //        for (int i = 1; i < dxfCoords.Count; i++)
            //            ctx.LineTo(new System.Windows.Point(Coordinates.ToScreenX(dxfCoords[i].X), Coordinates.ToScreenY(dxfCoords[i].Y)), true /* is stroked */, false /* is smooth join */);
            //    }
            //    geometry.FillRule = FillRule.EvenOdd;
            //    path.Data = geometry;
            //}
            //if (valid == visible) return;
            //if (valid)
            //{
            //    visible = true;
            //    path.Visibility = Visibility.Visible;
            //    Coordinates.BindCanvasAction(path);
            //    if (!selected) return;
            //    foreach (PointEntity p in points) p.path.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    visible = false;
            //    path.Visibility = Visibility.Collapsed;
            //    Coordinates.UnbindCanvasAction(path);
            //    if (!selected) return;
            //    foreach (PointEntity p in points) p.path.Visibility = Visibility.Collapsed;
            //}
        }

        public void Delete()
        {
            ToggleSelected(false);
            path.Visibility = Visibility.Collapsed;
            Coordinates.UnbindCanvasAction(path);

            selectArea.Visibility = Visibility.Collapsed;
            Coordinates.UnbindCanvasAction(selectArea);

            Coordinates.CanvasRef.DrawingEntities.Remove(this);

            MainWindow.doc.Entities.Remove(entityObject);
            foreach (PointEntity pointEntity in points) pointEntity.Delete();
            deleted = true;
        }
        public void Restore()
        {
            path.Visibility = Visibility.Visible;
            Coordinates.BindCanvasAction(path);

            selectArea.Visibility = Visibility.Visible;
            Coordinates.BindCanvasAction(selectArea);

            Coordinates.CanvasRef.DrawingEntities.Add(this);

            MainWindow.doc.Entities.Add(entityObject);
            foreach (PointEntity pointEntity in points) pointEntity.Restore();
            deleted = false;
            ReDraw();
        }
        public void Remove()
        {
            foreach (PointEntity pointEntity in points) pointEntity.Remove();
            // TODO : 인스턴스 삭제 방법 찾기
        }

        public void UpdateColor()
        {
            path.Stroke = Coordinates.defaultColorBrush;
            if (!selected) return;
            foreach (PointEntity point in points) point.path.Fill = Coordinates.defaultColorBrush;
        }

        private void UpdatePoint(int idx)
        {
            UpdatePoint(Coordinates.ToScreenX(dxfCoords[idx].X), Coordinates.ToScreenY(dxfCoords[idx].Y), idx, false);
        }

        private void UpdatePoint(List<double[]> positions)
        {
            for (int i = 0; i < positions.Count; i++) UpdatePoint(Coordinates.ToScreenX(positions[i][0]), Coordinates.ToScreenY(positions[i][1]), i, true);
        }

        private void UpdatePoint(PointCollection positions)
        {
            for (int i = 0; i < positions.Count; i++) UpdatePoint(Coordinates.ToScreenX(positions[i].X), Coordinates.ToScreenY(positions[i].Y), i, true);
        }

        private void UpdatePoint(double screenX, double screenY, int idx, bool updateDxf)
        {
            // polygon.Points[idx] = new System.Windows.Point(screenX, screenY);


            //selectArea.Points[idx] = new System.Windows.Point(screenX, screenY);
            points[idx].MovePosition(screenX, screenY);

            double dxfX = Coordinates.ToDxfX(screenX);
            double dxfY = Coordinates.ToDxfY(screenY);
            minX = Math.Min(minX, dxfX);
            minY = Math.Min(minY, dxfY);
            maxX = Math.Max(maxX, dxfX);
            maxY = Math.Max(maxY, dxfY);

            points[idx].path.ToolTip = Coordinates.ToolTip(dxfX, dxfY);

            if (!updateDxf) return;
            setDxfCoordAction[idx](dxfX, dxfY);
            dxfCoords[idx] = new System.Windows.Point(dxfX, dxfY);
        }

        public void ToggleSelected(bool status)
        {
            if (status == selected) return;

            // TODO : 구현
            if (status)
            {
                selectedEntities.Add(this);
                path.Stroke = Coordinates.selectedColorBrush;
                foreach (PointEntity point in points)
                {
                    point.BindCanvas();
                }

                
            }
            else
            {
                selectedEntities.Remove(this);
                path.Stroke = Coordinates.defaultColorBrush;
                foreach (PointEntity point in points) point.UnbindCanvas();
            }

            selected = status;

            OnPropertyChanged("Selected");
            Mediator.NotifyColleagues("EntityDetails.ShowEntityComboBox", null);
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

            Coordinates.CanvasRef.MouseLeftButtonDown -= Coordinates.CanvasRef.Select_MouseLeftButtonDown;

            //source = (UIElement)sender;
            //Mouse.Capture(source);

            foreach (PolygonEntity selectedEntity in selectedEntities)
            {
                selectedEntity.mouseOffsets = new PointCollection();
                for (int i = 0; i < selectedEntity.dxfCoords.Count; i++)
                {
                    selectedEntity.mouseOffsets.Add(new System.Windows.Point(Coordinates.ToScreenX(selectedEntity.dxfCoords[i].X) - e.GetPosition(Coordinates.CanvasRef).X, Coordinates.ToScreenY(selectedEntity.dxfCoords[i].Y) - e.GetPosition(Coordinates.CanvasRef).Y));
                }
                selectedEntity.dxfOffsets = selectedEntity.dxfCoords.Clone();
            }
            Coordinates.CanvasRef.MouseMove += MouseMove;
            Coordinates.CanvasRef.MouseLeftButtonUp += MouseLeftButtonUp;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            foreach (PolygonEntity selectedEntity in selectedEntities)
            {
                if (selectedEntity.mouseOffsets == null) continue;
                for (int i = 0; i < selectedEntity.dxfCoords.Count; i++)
                {
                    selectedEntity.UpdatePoint(selectedEntity.mouseOffsets[i].X + e.GetPosition(Coordinates.CanvasRef).X, selectedEntity.mouseOffsets[i].Y + e.GetPosition(Coordinates.CanvasRef).Y, i, true);
                }
                selectedEntity.ReDraw();
            }
        }

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Coordinates.CanvasRef.MouseMove -= MouseMove;
            Coordinates.CanvasRef.MouseLeftButtonUp -= MouseLeftButtonUp;
            Coordinates.CanvasRef.MouseLeftButtonDown += Coordinates.CanvasRef.Select_MouseLeftButtonDown;
            List<Action> forward = new List<Action>(), backward = new List<Action>();
            foreach (PolygonEntity selectedEntity in selectedEntities)
            {
                PointCollection from = selectedEntity.dxfOffsets.Clone();
                List<double[]> to = new List<double[]>();
                for (int i = 0; i < selectedEntity.dxfCoords.Count; i++)
                {
                    to.Add(new double[] { Coordinates.ToDxfX(selectedEntity.mouseOffsets[i].X + e.GetPosition(Coordinates.CanvasRef).X), Coordinates.ToDxfY(selectedEntity.mouseOffsets[i].Y + e.GetPosition(Coordinates.CanvasRef).Y) });
                }
                forward.Add(() => { selectedEntity.UpdatePoint(to); selectedEntity.ReDraw(); });
                backward.Add(() => { selectedEntity.UpdatePoint(from); selectedEntity.ReDraw(); });
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

            //Mouse.Capture(null);
            mouseOffsets = null;

        }

        public PolygonEntityType GetPolygonType()
        {
            return entityType;
        }

        public EntityObject GetEntityObject()
        {
            return entityObject;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
