﻿using SEMES_Pixel_Designer.Utils;
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

        }

    }

    public class MainCanvas : Canvas
    {


        public List<PolygonEntity> Lines = new List<PolygonEntity>();
        public List<PolygonEntity> Polylines = new List<PolygonEntity>();
        public double[] offset = null;

        public MainCanvas()
        {
            // 초기설정
            Coordinates.CanvasRef = this;
            SizeChanged += new SizeChangedEventHandler((object sender, SizeChangedEventArgs e) => UpdateCanvas());

            PolygonEntity.BindCanvasAction = Children.Add;
            PointEntity.BindCanvasAction = Children.Add;
            PointEntity.SetX = SetLeft;
            PointEntity.SetY = SetTop;
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainCanvas), new FrameworkPropertyMetadata(typeof(MainCanvas)));
            ClipToBounds = true;
            Background = Brushes.White;


            Utils.Mediator.Register("MainDrawer.DrawCanvas", DrawCanvas);
            Utils.Mediator.Register("MainDrawer.FitScreen", FitScreen );

            MouseWheel += _MouseWheel;
            MouseRightButtonDown += _MouseRightButtonDown;
            MouseRightButtonUp += _MouseRightButtonUp;

        }

        public void UpdateCanvas()
        {
            foreach (PolygonEntity line in Lines) line.ReDraw();
            foreach (PolygonEntity polyline in Polylines) polyline.ReDraw();

        }

        public void FitScreen(object obj)
        {
            Coordinates.UpdateRange(MainWindow.doc.Entities);
            UpdateCanvas();
        }

        public void DrawCanvas(object obj)
        {
            Children.Clear();

            Coordinates.UpdateRange(MainWindow.doc.Entities);


            Lines.Clear();
            Polylines.Clear();

            foreach (var line in MainWindow.doc.Entities.Lines)
            {
                Lines.Add(new PolygonEntity(line));
            }

            foreach (var polyline in MainWindow.doc.Entities.Polylines2D)
            {
                Polylines.Add(new PolygonEntity(polyline));
            }

        }

        private void _MouseWheel(object sender, MouseWheelEventArgs e)
        {
            float scaleFactor = 0.1f;
            Point mousePostion = e.GetPosition(this);
            double xFactor = (Coordinates.maxX - Coordinates.minX) * (e.Delta < 0 ? scaleFactor : -scaleFactor),
                yFactor = (Coordinates.maxY - Coordinates.minY) * (e.Delta < 0 ? scaleFactor : -scaleFactor);
            Coordinates.maxX += xFactor * (ActualWidth - mousePostion.X) / ActualWidth;
            Coordinates.minX -= xFactor * mousePostion.X / ActualWidth;
            Coordinates.maxY += yFactor * mousePostion.Y / ActualHeight;
            Coordinates.minY -= yFactor * (ActualHeight - mousePostion.Y) / ActualHeight;
            UpdateCanvas();
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

        private void _MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseMove -= _MouseMove;
            offset = null;
        }
    }
}