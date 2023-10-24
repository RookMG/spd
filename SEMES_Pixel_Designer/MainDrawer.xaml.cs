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
        private double[] activeVec = { 0, 0 };
        private int activeNth = -1;
        private bool isLeftMouseHold = false;
        private List<System.Windows.Shapes.Ellipse> vertexHighlights = new List<System.Windows.Shapes.Ellipse>();

        public MainDrawer()
        {
            InitializeComponent();
            temp_poly.MouseLeftButtonDown += Polygon_MouseLeftButtonDown;
        }

        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            // Get the x and y coordinates of the mouse pointer.
            System.Windows.Point position = e.GetPosition(this);
            double pX = position.X;
            double pY = position.Y;

            object[] posi = new object[] { pX, pY };

            Utils.Mediator.NotifyColleagues("MainWindow.ShowMousePosition", posi);
        }

        public void Polygon_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (isLeftMouseHold) return;
            isLeftMouseHold = true;

            if (vertexHighlights.Count == 0)
            {
                int i = 0;
                foreach (var vertex in temp_poly.Points)
                {
                    var vertexHighlight = new System.Windows.Shapes.Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Red,
                    };

                    Canvas.SetLeft(vertexHighlight, vertex.X - 5);
                    Canvas.SetTop(vertexHighlight, vertex.Y - 5);
                    ((Canvas)temp_poly.Parent).Children.Add(vertexHighlight);

                    vertexHighlights.Add(vertexHighlight);
                    int nth = i++;
                    vertexHighlight.MouseLeftButtonDown += (obj, evt) => Vertex_MouseLeftButtonDown(obj, evt, nth);
                }
                Utils.Mediator.NotifyColleagues("MainWindow.ShowEntitiesPosition", temp_poly.Points);
            }

            var mousePosition = e.GetPosition(temp_poly);
            activeVec[0] = -mousePosition.Y;
            activeVec[1] = -mousePosition.X;
            MouseMove += Polygon_MouseMove;
            MouseLeftButtonUp += Polygon_MouseLeftButtonUp;
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

        private void Vertex_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Canvas_MouseLeftButtonUp");
            MouseLeftButtonUp -= Vertex_MouseLeftButtonUp;
            MouseMove -= Vertex_MouseMove;
            isLeftMouseHold = false;
            activeNth = -1;
        }

        private void Vertex_MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("Canvas_MouseMove");
            var canvas = sender as Page;
            if (canvas == null) return;

            var mousePosition = e.GetPosition(canvas);
            //Console.WriteLine($"X: {mousePosition.X}, Y: {mousePosition.Y}");

            temp_poly.Points[activeNth] = new System.Windows.Point(mousePosition.X, mousePosition.Y);
            Canvas.SetLeft(vertexHighlights[activeNth], mousePosition.X - 5);
            Canvas.SetTop(vertexHighlights[activeNth], mousePosition.Y - 5);
        }

        private void Polygon_MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("Polygon_MouseMove");
            var canvas = sender as Page;
            if (canvas == null) return;

            var mousePosition = e.GetPosition(canvas);
            Console.WriteLine($"X: {mousePosition.X}, Y: {mousePosition.Y}");

            Canvas.SetLeft(temp_poly, mousePosition.X + activeVec[1]);
            Canvas.SetTop(temp_poly, mousePosition.Y + activeVec[0]);
        }
    }
}
