﻿using SEMES_Pixel_Designer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            Closing += ExitHandler;
            Mediator.Register("Minimap.Close",(obj)=> { Close(); });
        }
        public void ExitHandler(object sender, CancelEventArgs e)
        {
            Coordinates.MinimapRef.Children.Clear();
            Coordinates.MinimapRef = null;
        }
    }

    public class MinimapCanvas : Canvas
    {
        public Line CanvasXPosition, CanvasYPosition;
        public Path CellPath;
        public StreamGeometry geometry;
        private Point offset;
        public MinimapCanvas()
        {
            Coordinates.MinimapRef = this;
            CanvasXPosition = new Line
            {
                X1 = 0,
                Y1 = -1,
                X2 = 0,
                Y2 = 0,
                Stroke = Coordinates.selectedColorBrush,
                StrokeThickness = 2
            };
            CanvasYPosition = new Line
            {
                X1 = -1,
                Y1 = 0,
                X2 = 0,
                Y2 = 0,
                Stroke = Coordinates.selectedColorBrush,
                StrokeThickness = 2
            };
            CanvasXPosition.StrokeDashArray = CanvasYPosition.StrokeDashArray = new DoubleCollection(new double[] { 5, 2 });
            CellPath = new Path
            {
                Fill = Brushes.LightGreen,
            };
            CellPath.Data = geometry = new StreamGeometry();
            geometry.FillRule = FillRule.Nonzero;
            Children.Add(CellPath);
            Children.Add(CanvasXPosition);
            Children.Add(CanvasYPosition);
            MouseRightButtonUp += Move_MouseRightButtonUp;
            MouseLeftButtonDown += Drag_MouseLeftButtonDown;
            MouseWheel += Zoom_MouseWheel;

            Loaded += (obj, e) => { AdjustRatio(); };
            

        }
        public void ResizeWindow(object sender, SizeChangedEventArgs e)
        {
            AdjustRatio();
        }

        public void UpdatePosition()
        {
            UpdatePosition(ActualWidth, ActualHeight);
        }

        public void UpdatePosition(double width, double height)
        {
            Children.Clear();
            double x = 0.5 * (Coordinates.minX + Coordinates.maxX - 2 * Coordinates.glassLeft) * width / (Coordinates.glassRight - Coordinates.glassLeft),
                y = 0.5 * (2 * Coordinates.glassTop - Coordinates.minY - Coordinates.maxY) * height / (Coordinates.glassTop - Coordinates.glassBottom);
            CanvasXPosition.X1 = CanvasXPosition.X2 = x;
            CanvasYPosition.Y1 = CanvasYPosition.Y2 = y;
            CanvasYPosition.X2 = width + 1;
            CanvasXPosition.Y2 = height + 1;
            double xRatio = width / (Coordinates.glassRight - Coordinates.glassLeft), yRatio = height / (Coordinates.glassTop - Coordinates.glassBottom);

            Children.Add(CellPath);
            using (StreamGeometryContext ctx = geometry.Open())
            {
                foreach (var cell in Coordinates.CanvasRef.cells)
                {
                    Children.Add(cell.textBlock);
                    cell.textBlock.RenderTransform = new MatrixTransform(1, 0, 0, 1, (cell.patternLeft - Coordinates.glassLeft) * xRatio, (Coordinates.glassTop - cell.GetPatternTop()) * yRatio);

                    ctx.BeginFigure(new Point((cell.patternLeft - Coordinates.glassLeft) * xRatio, (Coordinates.glassTop - cell.patternBottom) * yRatio), true /* is filled */, true /* is closed */);
                    ctx.LineTo(new Point((cell.patternLeft - Coordinates.glassLeft) * xRatio, (Coordinates.glassTop - cell.GetPatternTop()) * yRatio), true /* is stroked */, false /* is smooth join */);
                    ctx.LineTo(new Point((cell.GetPatternRight() - Coordinates.glassLeft) * xRatio, (Coordinates.glassTop - cell.GetPatternTop()) * yRatio), true /* is stroked */, false /* is smooth join */);
                    ctx.LineTo(new Point((cell.GetPatternRight() - Coordinates.glassLeft) * xRatio, (Coordinates.glassTop - cell.patternBottom) * yRatio), true /* is stroked */, false /* is smooth join */);
                }
            }
            Children.Add(CanvasXPosition);
            Children.Add(CanvasYPosition);
        }

        public void AdjustRatio()
        {
            if((Coordinates.glassTop - Coordinates.glassBottom) / (Coordinates.glassRight - Coordinates.glassLeft) > Window.GetWindow(this).ActualHeight / Window.GetWindow(this).ActualWidth)
            {
                UpdatePosition(Width = (Window.GetWindow(this).ActualHeight - 40) * (Coordinates.glassRight - Coordinates.glassLeft) / (Coordinates.glassTop - Coordinates.glassBottom), Height = Window.GetWindow(this).ActualHeight - 40);
            }
            else
            {
                UpdatePosition(Width = Window.GetWindow(this).ActualWidth - 40, Height = (Window.GetWindow(this).ActualWidth - 40) * (Coordinates.glassTop - Coordinates.glassBottom) / (Coordinates.glassRight - Coordinates.glassLeft));
            }
        }

        private void Drag_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            offset = new Point(e.GetPosition(this).X, e.GetPosition(this).Y);
            MouseMove += Drag_MouseMove;
            MouseLeftButtonUp += Drag_MouseLeftButtonUp;
        }

        private void Drag_MouseMove(object sender, MouseEventArgs e)
        {
            double dx = (e.GetPosition(this).X - offset.X) * (Coordinates.glassRight - Coordinates.glassLeft) / ActualWidth,
                dy = -(e.GetPosition(this).Y - offset.Y) * (Coordinates.glassTop - Coordinates.glassBottom) / ActualHeight;
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
            double x = e.GetPosition(this).X * (Coordinates.glassRight - Coordinates.glassLeft) / ActualWidth,
                y = (ActualHeight - e.GetPosition(this).Y) * (Coordinates.glassTop - Coordinates.glassBottom) / ActualHeight,
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
            if ((Window.GetWindow(this).ActualWidth < 100 || Window.GetWindow(this).ActualHeight < 100) && scaleFactor < 1) return;
            Window.GetWindow(this).Width = Window.GetWindow(this).ActualWidth * scaleFactor;
            Window.GetWindow(this).Height = Window.GetWindow(this).ActualHeight * scaleFactor;
            AdjustRatio();
        }
    }
}