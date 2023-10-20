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

using netDxf;
using netDxf.Entities;

namespace WpfDrawTest
{
    public class MainCanvas : Canvas
    {
        private int activeNth = -1;
        private double[] activeVec = { 0, 0 };
        private bool isLeftMouseHold = false;
        private Polygon polygon = new Polygon();
        private List<System.Windows.Shapes.Ellipse> vertexHighlights = new List<System.Windows.Shapes.Ellipse>();

        public MainCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainCanvas), new FrameworkPropertyMetadata(typeof(MainCanvas)));

            var p1 = new System.Windows.Point(50, 50);
            var p2 = new System.Windows.Point(100, 50);
            var p3 = new System.Windows.Point(100, 100);
            polygon.Points.Add(p1);
            polygon.Points.Add(p2);
            polygon.Points.Add(p3);
            polygon.Fill = Brushes.LightSeaGreen;
            polygon.MouseLeftButtonDown += Polygon_MouseLeftButtonDown;

            Background = Brushes.Transparent;
            Children.Add(polygon);
        }

        private void Polygon_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (isLeftMouseHold) return;
            isLeftMouseHold = true;
            Console.WriteLine("Polygon_MouseLeftButtonDown");
            if (vertexHighlights.Count == 0)
            {
                int i = 0;
                foreach (var vertex in polygon.Points)
                {
                    var vertexHighlight = new System.Windows.Shapes.Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Red,
                    };
                    SetLeft(vertexHighlight, vertex.X - 5);
                    SetTop(vertexHighlight, vertex.Y - 5);
                    Children.Add(vertexHighlight);
                    vertexHighlights.Add(vertexHighlight);

                    int nth = i++;
                    vertexHighlight.MouseLeftButtonDown += (obj, evt) => Vertex_MouseLeftButtonDown(obj, evt, nth);
                }
            }
            /*         else
                     {
                         foreach (var vertexHighlight in vertexHighlights)
                         {
                             canvas.Children.Remove(vertexHighlight);
                         }
                         vertexHighlights.Clear();
                     }*/

            var mousePosition = e.GetPosition(polygon);
            activeVec[0] = -mousePosition.Y;
            activeVec[1] = -mousePosition.X;
            MouseMove += Polygon_MouseMove;
            MouseLeftButtonUp += Polygon_MouseLeftButtonUp;



        }
        private void Polygon_MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("Polygon_MouseMove");
            var canvas = sender as MainCanvas;
            if (canvas == null) return;

            var mousePosition = e.GetPosition(canvas);
            Console.WriteLine($"X: {mousePosition.X}, Y: {mousePosition.Y}");

            SetLeft(polygon, mousePosition.X + activeVec[1]);
            SetTop(polygon, mousePosition.Y + activeVec[0]);
        }

        private void Polygon_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Polygon_MouseLeftButtonUp");
            MouseMove -= Polygon_MouseMove;
            MouseLeftButtonUp -= Polygon_MouseLeftButtonUp;
            isLeftMouseHold = false;
        }


        private void Vertex_MouseLeftButtonDown(object sender, MouseEventArgs e, int nth)
        {
            if (isLeftMouseHold) return;
            isLeftMouseHold = true;
            Console.WriteLine($"{nth}-th Vertex_MouseLeftButtonDown");
            var vertex = sender as System.Windows.Shapes.Ellipse;
            if (vertex == null) return;

            activeNth = nth;
            MouseMove += Vertex_MouseMove;
            MouseLeftButtonUp += Vertex_MouseLeftButtonUp;
        }

        private void Vertex_MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("Canvas_MouseMove");
            var canvas = sender as MainCanvas;
            if (canvas == null) return;

            var mousePosition = e.GetPosition(canvas);
            //Console.WriteLine($"X: {mousePosition.X}, Y: {mousePosition.Y}");

            polygon.Points[activeNth] = new System.Windows.Point(mousePosition.X, mousePosition.Y);
            SetLeft(vertexHighlights[activeNth], mousePosition.X - 5);
            SetTop(vertexHighlights[activeNth], mousePosition.Y - 5);
        }

        private void Vertex_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Canvas_MouseLeftButtonUp");
            MouseLeftButtonUp -= Vertex_MouseLeftButtonUp;
            MouseMove -= Vertex_MouseMove;
            isLeftMouseHold = false;
            activeNth = -1;
        }
    }
}
