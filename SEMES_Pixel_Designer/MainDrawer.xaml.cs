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
        private Point lastPosition;
        private int activeNth = -1;
        private double[] activeVec = { 0, 0 };
        private bool isLeftMouseHold = false;
        private Polygon polygon = new Polygon();
        private List<Ellipse> vertexHighlights = new List<Ellipse>();

        public MainCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainCanvas), new FrameworkPropertyMetadata(typeof(MainCanvas)));
            this.ClipToBounds = true;

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
            var mousePositionCanvas = e.GetPosition(this);
            Console.WriteLine($"mousePositionCanvas: {mousePositionCanvas}");
            lastPosition = mousePositionCanvas;

            if (isLeftMouseHold) return;
            isLeftMouseHold = true;
            Console.WriteLine("Polygon_MouseLeftButtonDown");
            if (vertexHighlights.Count == 0)
            {
                int i = 0;
                foreach (var vertex in polygon.Points)
                {
                    var vertexHighlight = new Ellipse
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
            Console.WriteLine($"mousePosition: {mousePosition}");
            activeVec[0] = -mousePosition.Y;
            activeVec[1] = -mousePosition.X;
            MouseMove += Polygon_MouseMove;
            MouseLeftButtonUp += Polygon_MouseLeftButtonUp;



        }
        private void Polygon_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
            Console.WriteLine($"X: {mousePosition.X}, Y: {mousePosition.Y}");

            // 화면상의 위치 변경 (points의 좌푯값들을 직접 바꾸면 느릴 것 같아서 일단은 이렇게 했습니다.)
            SetLeft(polygon, mousePosition.X + activeVec[1]);
            SetTop(polygon, mousePosition.Y + activeVec[0]);
            Console.WriteLine($"화면상의 polygon위치: {mousePosition.X + activeVec[1]}, {mousePosition.Y + activeVec[0]}");
        }

        private void Polygon_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Polygon_MouseLeftButtonUp");
            var currentPosition = e.GetPosition(this);
            Console.WriteLine($"currentPosition: {currentPosition}");
            Console.WriteLine($"lastPosition: {lastPosition}");
            Console.WriteLine($"point[0]: {polygon.Points[0]}");
            var offsetX = currentPosition.X - lastPosition.X;
            var offsetY = currentPosition.Y - lastPosition.Y;
            Console.WriteLine($"offsetX: {offsetX}");
            Console.WriteLine($"offsetY: {offsetY}");

            // 폴리곤 실제 좌표값 변경 & 표시점 화면 상의 위치 변경
            SetLeft(polygon, 0);
            SetTop(polygon, 0);
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                var currentPointX = polygon.Points[i].X + offsetX;
                var currentPointY = polygon.Points[i].Y + offsetY;
                polygon.Points[i] = new Point(currentPointX, currentPointY);

                SetLeft(vertexHighlights[i], currentPointX - 5);
                SetTop(vertexHighlights[i], currentPointY - 5);

            }

            Console.WriteLine($"polygon X: {polygon.Points[0].X}, polygon Y: {polygon.Points[0].Y}");

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
