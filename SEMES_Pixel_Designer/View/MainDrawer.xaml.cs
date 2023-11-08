using netDxf;
using netDxf.Entities;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SEMES_Pixel_Designer.Utils.PolygonEntity;
using Ellipse = System.Windows.Shapes.Ellipse;

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// MainDrawer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainDrawer : Page
    {
        public MainDrawer()
        {
            InitializeComponent();
            mainCanvas.ClipToBounds = true;

            // 성능 체크 해봐야함. 화면이 제대로 업데이트 되지 않는 문제가 있음
            mainCanvas.CacheMode = new BitmapCache
            {
                EnableClearType = false,
                RenderAtScale = 1,
                SnapsToDevicePixels = false,
            };
        }
            
    }

    public class MainCanvas : Canvas
    {
        public bool blackBackground = false;

        public List<PolygonEntity> DrawingEntities = new List<PolygonEntity>();
        public double[] offset = null;
        public Polygon drawingPolygon = null;
        public Ellipse drawingEllipse = null;
        public readonly double PASTE_OFFSET = 5, MIN_SELECT_LENGTH = 10;
        public int pasteCount = 0;
        public MainCanvas()
        {
            // 초기설정
            Coordinates.CanvasRef = this;
            SizeChanged += new SizeChangedEventHandler(ResizeWindow);

            Coordinates.BindCanvasAction = Children.Add;
            Coordinates.UnbindCanvasAction = Children.Remove;
            Coordinates.SetZIndexAction = SetZIndex;
            Coordinates.SetLeftAction = SetLeft;
            Coordinates.SetTopAction = SetTop;
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainCanvas), new FrameworkPropertyMetadata(typeof(MainCanvas)));
            ClipToBounds = true;
            Background = Coordinates.backgroundColorBrush;

            Children.Add(Coordinates.gridInfoText);
            SetZIndex(Coordinates.gridInfoText,-1);

            Utils.Mediator.Register("MainDrawer.DrawCanvas", DrawCanvas);
            Utils.Mediator.Register("MainDrawer.FitScreen", FitScreen);
            Utils.Mediator.Register("MainDrawer.DrawLine", DrawLine);
            Utils.Mediator.Register("MainDrawer.DrawRectangle", DrawRectangle);
            Utils.Mediator.Register("MainDrawer.DrawPolygon", DrawPolygon);
            Utils.Mediator.Register("MainDrawer.ColorBackground", ColorBackground);
            Utils.Mediator.Register("MainDrawer.Zoom", Zoom);
            Utils.Mediator.Register("MainDrawer.Paste", Paste);
            Utils.Mediator.Register("MainDrawer.CloneEntities", CloneEntities);
            Utils.Mediator.Register("MainDrawer.DeleteEntities", (obj)=> { 
                DeleteEntities(selectedEntities); 
            });

            MouseMove += Info_MouseMove;

            MouseLeftButtonDown += Select_MouseLeftButtonDown;
            MouseWheel += Zoom_MouseWheel;
            MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;

        }

        public void UpdateCanvas()
        {
            pasteCount = 0;
            Coordinates.DrawGrid();
            foreach (PolygonEntity entity in DrawingEntities) entity.ReDraw();
            foreach (System.Windows.Shapes.Line gridLine in Coordinates.gridLines)
            {
                gridLine.MouseLeftButtonDown += Select_MouseLeftButtonDown;
                gridLine.MouseWheel += Zoom_MouseWheel;
                gridLine.MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;
            }
        }

        public void ResizeWindow(object sender, SizeChangedEventArgs e)
        {
            Coordinates.maxX = Coordinates.minX + e.NewSize.Width / Coordinates.ratio;
            Coordinates.minY = Coordinates.maxY - e.NewSize.Height / Coordinates.ratio;
            UpdateCanvas();
        }

        public void FitScreen(object obj)
        {
            Coordinates.UpdateRange(MainWindow.doc.Entities);
            UpdateCanvas();
        }

        public void DrawCanvas(object obj)
        {
            Children.Clear();
            Children.Add(Coordinates.gridInfoText);
            SetZIndex(Coordinates.gridInfoText, -1);
            UpdateLayout();

            Coordinates.UpdateRange(MainWindow.doc.Entities);
            Coordinates.DrawGrid();

            DrawingEntities.Clear();

            foreach (var line in MainWindow.doc.Entities.Lines)
            {
                DrawingEntities.Add(new PolygonEntity(line));
            }

            foreach (var polyline in MainWindow.doc.Entities.Polylines2D)
            {
                DrawingEntities.Add(new PolygonEntity(polyline));
            }

        }

        public void ColorBackground(object obj)
        {
            blackBackground = !blackBackground;
            Coordinates.backgroundColorBrush = blackBackground ? Brushes.Black : Brushes.White;
            Coordinates.defaultColorBrush = blackBackground ? Brushes.White : Brushes.Black;
            Background = Coordinates.backgroundColorBrush;
            foreach (PolygonEntity entity in DrawingEntities) entity.UpdateColor();
        }

        public void Paste(object obj)
        {
            double offset = pasteCount * PASTE_OFFSET / Coordinates.ratio;
            List<PolygonEntity> pasted = new List<PolygonEntity>();
            foreach (CopyData data in clipboard)
            {
                EntityObject entity = data.GetEntityObject();
                entity.TransformBy(Matrix3.Identity, new Vector3(Coordinates.minX + offset, Coordinates.minY - offset, 0) - data.offset);
                MainWindow.doc.Entities.Add(entity);
                if (data.type == PolygonEntityType.LINE)
                {
                    pasted.Add(new PolygonEntity(entity as netDxf.Entities.Line));
                }
                else if (data.type == PolygonEntityType.POLYLINE)
                {
                    pasted.Add(new PolygonEntity(entity as Polyline2D));
                }

            }
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    foreach (PolygonEntity entity in pasted)
                    {
                        DrawingEntities.Add(entity);
                    }
                },
                () => {
                    foreach (PolygonEntity entity in pasted) { 
                        DrawingEntities.Remove(entity);
                        entity.Delete();
                    }
                },
                () =>
                {
                    foreach (PolygonEntity entity in pasted)
                    {
                        DrawingEntities.Add(entity);
                        entity.Restore();
                    }
                },
                () =>
                {
                    foreach (PolygonEntity entity in pasted) entity.Remove();
                }
            ));
            pasteCount++;
        }

        public void CloneEntities(object obj)
        {
            //Window.GetWindow(this).Close();
            var param = (Tuple<int, int, double, double>)obj;
            int R = param.Item1;
            int C = param.Item2;
            double intervalX = param.Item4;
            double intervalY = -param.Item3;

            //int R = 30, C = 30;
            //double intervalX = 100, intervalY = -100;

            List<CopyData> clipboardBackup = new List<CopyData>();
            foreach (CopyData copyData in clipboard) clipboardBackup.Add(copyData);
            CopySelected();

            List<PolygonEntity> cloned = new List<PolygonEntity>();
            foreach (CopyData data in clipboard)
            {
                for(int r = 0; r < R; r++) { 
                    for(int c = 0; c < C; c++)
                    {
                        if (r == 0 && c == 0) continue;

                        EntityObject entity = data.GetEntityObject(); ;
                        entity.TransformBy(Matrix3.Identity, new Vector3(r*intervalX, c*intervalY, 0));
                        MainWindow.doc.Entities.Add(entity);
                        if (data.type == PolygonEntityType.LINE)
                        {
                            cloned.Add(new PolygonEntity(entity as netDxf.Entities.Line));
                        }
                        else if (data.type == PolygonEntityType.POLYLINE)
                        {
                            cloned.Add(new PolygonEntity(entity as Polyline2D));
                        }
                    }
                }
            }
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    foreach (PolygonEntity entity in cloned)
                    {
                        DrawingEntities.Add(entity);
                    }
                },
                () => {
                    foreach (PolygonEntity entity in cloned)
                    {
                        DrawingEntities.Remove(entity);
                        entity.Delete();
                    }
                },
                () =>
                {
                    foreach (PolygonEntity entity in cloned)
                    {
                        DrawingEntities.Add(entity);
                        entity.Restore();
                    }
                },
                () =>
                {
                    foreach (PolygonEntity entity in cloned) entity.Remove();
                }
            ));
            clipboard = clipboardBackup;
        }

        public void Zoom(object scaleFactor)
        {
            Zoom((double)scaleFactor, new System.Windows.Point(ActualWidth / 2, ActualHeight / 2));
        }

        public void Zoom(double scaleFactor, System.Windows.Point center)
        {
            double xFactor = (Coordinates.maxX - Coordinates.minX) * scaleFactor,
                yFactor = (Coordinates.maxY - Coordinates.minY) * scaleFactor;
            Coordinates.maxX += xFactor * (ActualWidth - center.X) / ActualWidth;
            Coordinates.minX -= xFactor * center.X / ActualWidth;
            Coordinates.maxY += yFactor * center.Y / ActualHeight;
            Coordinates.minY -= yFactor * (ActualHeight - center.Y) / ActualHeight;
            Coordinates.AdjustRatio();
            UpdateCanvas();
        }

        private void DeleteEntities(IEnumerable<PolygonEntity> entities)
        {
            List<PolygonEntity> target = new List<PolygonEntity>(entities);
            if (target.Count == 0) return;
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    foreach (PolygonEntity entity in target) entity.Restore();
                },
                () => {
                    foreach (PolygonEntity entity in target) entity.Delete();
                },
                () => {
                    foreach (PolygonEntity entity in target) entity.Remove();
                }
            ));
        }

        private void DrawLine(object obj)
        {
            ClearSelected();
            drawingPolygon = new Polygon
            {
                Fill = Coordinates.fillColorBrush,
                Stroke = Coordinates.defaultColorBrush,
            };
            drawingPolygon.StrokeDashArray.Add(5);
            drawingPolygon.StrokeDashArray.Add(5);

            drawingEllipse = new Ellipse
            {
                Fill = Coordinates.fillColorBrush,
                Stroke = Coordinates.defaultColorBrush,
                Width = 5,
                Height = 5,
            };

            drawingPolygon.Points.Add(new System.Windows.Point());
            SetLeft(drawingEllipse, -10);
            SetTop(drawingEllipse, -10);

            Children.Add(drawingPolygon);
            Children.Add(drawingEllipse);



            MouseLeftButtonDown -= Select_MouseLeftButtonDown;
            MouseRightButtonDown -= MoveCanvas_MouseRightButtonDown;

            MouseMove += DrawLine_MouseMove;
            MouseLeftButtonUp += DrawLine_MouseLeftButtonUp;
            MouseRightButtonUp += DrawLine_MouseRightButtonUp;

        }
        private void DrawRectangle(object obj)
        {
            ClearSelected();
            drawingPolygon = new Polygon
            {
                Fill = Coordinates.fillColorBrush,
                Stroke = Coordinates.defaultColorBrush,
            };
            drawingPolygon.StrokeDashArray.Add(5);
            drawingPolygon.StrokeDashArray.Add(5);

            drawingEllipse = new Ellipse
            {
                Fill = Coordinates.fillColorBrush,
                Stroke = Coordinates.defaultColorBrush,
                Width = 5,
                Height = 5,
            };

            drawingPolygon.Points.Add(new System.Windows.Point());
            SetLeft(drawingEllipse, -10);
            SetTop(drawingEllipse, -10);

            Children.Add(drawingPolygon);
            Children.Add(drawingEllipse);



            MouseLeftButtonDown -= Select_MouseLeftButtonDown;
            MouseRightButtonDown -= MoveCanvas_MouseRightButtonDown;

            MouseMove += DrawRectangle_MouseMove;
            MouseLeftButtonUp += DrawRectangle_MouseLeftButtonUp;
            MouseRightButtonUp += DrawRectangle_MouseRightButtonUp;

        }

        private void DrawPolygon(object obj)
        {
            ClearSelected();
            drawingPolygon = new Polygon
            {
                Fill = Coordinates.fillColorBrush,
                Stroke = Coordinates.defaultColorBrush,
            };
            drawingPolygon.StrokeDashArray.Add(5);
            drawingPolygon.StrokeDashArray.Add(5);

            drawingEllipse = new Ellipse
            {
                Fill = Coordinates.fillColorBrush,
                Stroke = Coordinates.defaultColorBrush,
                Width = 5,
                Height = 5,
            };

            drawingPolygon.Points.Add(new System.Windows.Point());
            SetLeft(drawingEllipse, -10);
            SetTop(drawingEllipse, -10);

            Children.Add(drawingPolygon);
            Children.Add(drawingEllipse);



            MouseLeftButtonDown -= Select_MouseLeftButtonDown;
            MouseRightButtonDown -= MoveCanvas_MouseRightButtonDown;

            MouseMove += DrawPolygon_MouseMove;
            MouseLeftButtonUp += DrawPolygon_MouseLeftButtonUp;
            MouseRightButtonUp += DrawPolygon_MouseRightButtonUp;

        }




        #region 마우스 이벤트

        private void Info_MouseMove(object sender, MouseEventArgs e)
        {
            Mediator.NotifyColleagues("StatusBar.ShowMousePosition", new System.Windows.Point(Coordinates.ToDxfX(e.GetPosition(this).X), Coordinates.ToDxfY(e.GetPosition(this).Y)));
        }

        private void Zoom_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleFactor = 0.1;
            Zoom(e.Delta < 0 ? scaleFactor * 1.1 : -scaleFactor, e.GetPosition(this));
        }


        public void Select_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if(Children.Contains(drawingPolygon)) Children.Remove(drawingPolygon);
            drawingPolygon = new Polygon
            {
                Fill = Coordinates.fillColorBrush,
                Stroke = Coordinates.defaultColorBrush,
                StrokeThickness = 0.5
            };
            drawingPolygon.StrokeDashArray.Add(5);
            drawingPolygon.StrokeDashArray.Add(5);

            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));


            Children.Add(drawingPolygon);

            MouseMove += Select_MouseMove;
            MouseLeftButtonUp += Select_MouseLeftButtonUp;
        }
        private void Select_MouseMove(object sender, MouseEventArgs e)
        {
            drawingPolygon.Points[1] = new System.Windows.Point(drawingPolygon.Points[0].X, e.GetPosition(this).Y);
            drawingPolygon.Points[2] = new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
            drawingPolygon.Points[3] = new System.Windows.Point(e.GetPosition(this).X, drawingPolygon.Points[0].Y);
        }
        private void Select_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            double minSelX = Math.Min(drawingPolygon.Points[2].X, drawingPolygon.Points[0].X),
                   maxSelX = Math.Max(drawingPolygon.Points[2].X, drawingPolygon.Points[0].X),
                   minSelY = Math.Min(drawingPolygon.Points[2].Y, drawingPolygon.Points[0].Y),
                   maxSelY = Math.Max(drawingPolygon.Points[2].Y, drawingPolygon.Points[0].Y);
            if (maxSelX + maxSelY - minSelX - minSelY > MIN_SELECT_LENGTH && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl) && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                ClearSelected();
            }
            // TODO: 영역 안의 폴리곤 선택

            List<double> x = new List<double>(), y = new List<double>();
            foreach (PolygonEntity entity in DrawingEntities)
            {
                
                x.Clear();
                y.Clear();
                foreach(var point in entity.dxfCoords)
                {
                    x.Add(Coordinates.ToScreenX(point.X));
                    y.Add(Coordinates.ToScreenY(point.Y));
                }
                double minX = x.Min(), minY = y.Min(), maxX = x.Max(), maxY = y.Max();
                if (maxX >= minSelX && minX <= maxSelX && maxY >= minSelY && minY <= maxSelY) entity.ToggleSelected((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) ?!entity.selected: true);
            }

            Children.Remove(drawingPolygon);

            MouseMove -= Select_MouseMove;
            MouseLeftButtonUp -= Select_MouseLeftButtonUp;
        }
        

        private void MoveCanvas_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            offset = new double[] { e.GetPosition(this).X, e.GetPosition(this).Y, Coordinates.minX , Coordinates.minY };
            MouseMove += MoveCanvas_MouseMove;
            MouseRightButtonUp += MoveCanvas_MouseRightButtonUp;
        }

        private void MoveCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (offset == null) return;
            double dx = (Coordinates.maxX - Coordinates.minX) * (offset[0] - e.GetPosition(this).X) / ActualWidth, 
                dy = (Coordinates.maxY - Coordinates.minY) * (e.GetPosition(this).Y - offset[1]) / ActualHeight;
            Coordinates.maxX = dx + Coordinates.maxX - Coordinates.minX + offset[2];
            Coordinates.minX = dx + offset[2];

            Coordinates.maxY = dy + Coordinates.maxY - Coordinates.minY + offset[3];
            Coordinates.minY = dy + offset[3];

            UpdateCanvas();
        }

        private void MoveCanvas_MouseRightButtonUp(object sender, MouseEventArgs e)
        {
            MouseMove -= MoveCanvas_MouseMove;
            offset = null;
        }





        private void DrawLine_MouseMove(object sender, MouseEventArgs e)
        {
            SetLeft(drawingEllipse, e.GetPosition(this).X - 2.5);
            SetTop(drawingEllipse, e.GetPosition(this).Y - 2.5);
            drawingPolygon.Points[drawingPolygon.Points.Count - 1] = new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
        }
        private void DrawLine_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));

            if (drawingPolygon.Points.Count != 3) return;


            
            MouseMove -= DrawLine_MouseMove;
            MouseLeftButtonUp -= DrawLine_MouseLeftButtonUp;
            MouseRightButtonUp -= DrawLine_MouseRightButtonUp;


            Children.Remove(drawingPolygon);
            Children.Remove(drawingEllipse);

            PolygonEntity polygonEntity = new PolygonEntity(drawingPolygon, PolygonEntityType.LINE);
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    DrawingEntities.Add(polygonEntity);
                },
                () => {
                    DrawingEntities.Remove(polygonEntity);
                    polygonEntity.Delete();
                },
                () =>
                {
                    DrawingEntities.Add(polygonEntity);
                    polygonEntity.Restore();
                },
                () =>
                {
                    polygonEntity.Remove();
                }
            ));

            UpdateLayout();
            MouseLeftButtonDown += Select_MouseLeftButtonDown;
            MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;
        }

        private void DrawLine_MouseRightButtonUp(object sender, MouseEventArgs e)
        {

            MouseMove -= DrawLine_MouseMove;
            MouseLeftButtonUp -= DrawLine_MouseLeftButtonUp;
            MouseRightButtonUp -= DrawLine_MouseRightButtonUp;


            Children.Remove(drawingPolygon);
            Children.Remove(drawingEllipse);
            drawingPolygon.Points.Clear();

            UpdateLayout();
            MouseLeftButtonDown += Select_MouseLeftButtonDown;
            MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;
        }



        private void DrawRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            SetLeft(drawingEllipse, e.GetPosition(this).X - 2.5);
            SetTop(drawingEllipse, e.GetPosition(this).Y - 2.5);

            if(drawingPolygon.Points.Count == 1)
            {
                drawingPolygon.Points[0] = new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
            }
            else
            {
                drawingPolygon.Points[1] = new System.Windows.Point(drawingPolygon.Points[0].X, e.GetPosition(this).Y);
                drawingPolygon.Points[2] = new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
                drawingPolygon.Points[3] = new System.Windows.Point(e.GetPosition(this).X, drawingPolygon.Points[0].Y);
            }
        }
        private void DrawRectangle_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if(drawingPolygon.Points.Count == 1)
            {
                drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
                drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
                drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
            }
            else if(drawingPolygon.Points.Count == 4)
            {
                MouseMove -= DrawRectangle_MouseMove;
                MouseLeftButtonUp -= DrawRectangle_MouseLeftButtonUp;
                MouseRightButtonUp -= DrawRectangle_MouseRightButtonUp;



                PolygonEntity polygonEntity = new PolygonEntity(drawingPolygon, PolygonEntityType.POLYLINE);
                Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
                (
                    () => {
                        DrawingEntities.Add(polygonEntity);
                    },
                    () => {
                        DrawingEntities.Remove(polygonEntity);
                        polygonEntity.Delete();
                    },
                    () =>
                    {
                        DrawingEntities.Add(polygonEntity);
                        polygonEntity.Restore();
                    },
                    () =>
                    {
                        polygonEntity.Remove();
                    }
                ));

                Children.Remove(drawingPolygon);
                Children.Remove(drawingEllipse);
                UpdateLayout();
                MouseLeftButtonDown += Select_MouseLeftButtonDown;
                MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;
            }
        }

        private void DrawRectangle_MouseRightButtonUp(object sender, MouseEventArgs e)
        {

            MouseMove -= DrawRectangle_MouseMove;
            MouseLeftButtonUp -= DrawRectangle_MouseLeftButtonUp;
            MouseRightButtonUp -= DrawRectangle_MouseRightButtonUp;


            Children.Remove(drawingPolygon);
            Children.Remove(drawingEllipse);
            drawingPolygon.Points.Clear();


            UpdateLayout();
            MouseLeftButtonDown += Select_MouseLeftButtonDown;
            MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;
        }


        private void DrawPolygon_MouseMove(object sender, MouseEventArgs e)
        {
            SetLeft(drawingEllipse, e.GetPosition(this).X - 2.5);
            SetTop(drawingEllipse, e.GetPosition(this).Y - 2.5);
            drawingPolygon.Points[drawingPolygon.Points.Count - 1] = new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
        }
        private void DrawPolygon_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
        }

        private void DrawPolygon_MouseRightButtonUp(object sender, MouseEventArgs e)
        {

            MouseMove -= DrawPolygon_MouseMove;
            MouseLeftButtonUp -= DrawPolygon_MouseLeftButtonUp;
            MouseRightButtonUp -= DrawPolygon_MouseRightButtonUp;


            Children.Remove(drawingPolygon);
            Children.Remove(drawingEllipse);
            drawingPolygon.Points.RemoveAt(drawingPolygon.Points.Count - 1);
            if (drawingPolygon.Points.Count <= 1) return;


            PolygonEntity polygonEntity = new PolygonEntity(drawingPolygon, PolygonEntityType.POLYLINE);
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    DrawingEntities.Add(polygonEntity);
                },
                () => {
                    DrawingEntities.Remove(polygonEntity);
                    polygonEntity.Delete();
                },
                () =>
                {
                    DrawingEntities.Add(polygonEntity);
                    polygonEntity.Restore();
                },
                () =>
                {
                    polygonEntity.Remove();
                }
            ));

            UpdateLayout();
            MouseLeftButtonDown += Select_MouseLeftButtonDown;
            MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;
        }


        #endregion
    }


}