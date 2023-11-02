﻿using netDxf;
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
            //mainCanvas.CacheMode = new BitmapCache
            //{
            //    EnableClearType=false,
            //    RenderAtScale=1,
            //    SnapsToDevicePixels=false,
            //};
        }

    }

    public class MainCanvas : Canvas
    {


        public List<PolygonEntity> DrawingEntities = new List<PolygonEntity>();
        public double[] offset = null;
        public Polygon drawingPolygon = null;
        public Ellipse drawingEllipse = null;
        public readonly double PASTE_OFFSET = 5;
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
            Background = Brushes.White;

            Children.Add(Coordinates.gridInfoText);
            SetZIndex(Coordinates.gridInfoText,-1);

            Utils.Mediator.Register("MainDrawer.DrawCanvas", DrawCanvas);
            Utils.Mediator.Register("MainDrawer.FitScreen", FitScreen);
            Utils.Mediator.Register("MainDrawer.DrawPolygon", DrawPolygon);
            Utils.Mediator.Register("MainDrawer.Zoom", Zoom);
            Utils.Mediator.Register("MainDrawer.Paste", Paste);
            Utils.Mediator.Register("MainDrawer.CloneEntities", CloneEntities);
            Utils.Mediator.Register("MainDrawer.DeleteEntities", (obj)=> { 
                DeleteEntities(PolygonEntity.selectedEntities); 
            });
            MouseWheel += _MouseWheel;
            MouseRightButtonDown += _MouseRightButtonDown;
            MouseRightButtonUp += _MouseRightButtonUp;

            


        }

        public void UpdateCanvas()
        {
            pasteCount = 0;
            Coordinates.DrawGrid();
            foreach (PolygonEntity line in DrawingEntities) line.ReDraw();
            foreach (System.Windows.Shapes.Line gridLine in Coordinates.gridLines)
            {
                gridLine.MouseWheel += _MouseWheel;
                gridLine.MouseRightButtonDown += _MouseRightButtonDown;
                gridLine.MouseRightButtonUp += _MouseRightButtonUp;
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

        public void Paste(object obj)
        {
            double offset = pasteCount * PASTE_OFFSET / Coordinates.ratio;
            List<PolygonEntity> pasted = new List<PolygonEntity>();
            foreach (CopyData data in clipboard)
            {
                EntityObject entity = data.entity.Clone() as EntityObject;
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
            int R = 30, C = 30;
            double intervalX = 100, intervalY = -100;

            List<CopyData> clipboardBackup = new List<CopyData>();
            foreach (CopyData copyData in clipboard) clipboardBackup.Add(copyData);
            PolygonEntity.CopySelected();

            List<PolygonEntity> cloned = new List<PolygonEntity>();
            foreach (CopyData data in clipboard)
            {
                for(int r = 0; r < R; r++) { 
                    for(int c = 0; c < C; c++)
                    {
                        if (r == 0 && c == 0) continue;

                        EntityObject entity = data.entity.Clone() as EntityObject;
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

        private void _MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleFactor = 0.1;
            Zoom(e.Delta < 0 ? scaleFactor * 1.1 : -scaleFactor, e.GetPosition(this));
        }


        private void _MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            offset = new double[] { e.GetPosition(this).X, e.GetPosition(this).Y, Coordinates.minX , Coordinates.minY };
            MouseMove += _MouseMove;
        }

        private void _MouseMove(object sender, MouseEventArgs e)
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

        private void _MouseRightButtonUp(object sender, MouseEventArgs e)
        {
            MouseMove -= _MouseMove;
            offset = null;
        }

        private void DrawPolygon(object obj)
        {
            PolygonEntity.ClearSelected();
            drawingPolygon = new Polygon
            {
                Fill = Brushes.Transparent,
                Stroke = Brushes.Black,
                StrokeDashOffset = 1,
            };

            drawingEllipse = new Ellipse
            {
                Fill = Brushes.Transparent,
                Stroke = Brushes.Black,
                Width = 5,
                Height = 5,
            };

            Children.Add(drawingPolygon);
            Children.Add(drawingEllipse);
            drawingPolygon.Points.Add(new System.Windows.Point(-10, -10));

            MouseMove += DrawPolygon_MouseMove;
            MouseLeftButtonUp += DrawPolygon_MouseLeftButtonUp;
            MouseRightButtonUp += DrawPolygon_MouseRightButtonUp;
            MouseRightButtonDown -= _MouseRightButtonDown;
            MouseRightButtonUp -= _MouseRightButtonUp;
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
            MouseRightButtonDown += _MouseRightButtonDown;
            MouseRightButtonUp += _MouseRightButtonUp;

            Children.Remove(drawingPolygon);
            Children.Remove(drawingEllipse);
            drawingPolygon.Points.RemoveAt(drawingPolygon.Points.Count-1);
            if (drawingPolygon.Points.Count == 0) return;
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
    }
}