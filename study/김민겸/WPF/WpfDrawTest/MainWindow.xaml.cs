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
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var pg = new Polygon();
            var p1 = new System.Windows.Point(50, 50);
            var p2 = new System.Windows.Point(100, 50);
            var p3 = new System.Windows.Point(100, 100);
            pg.Points.Add(p1);
            pg.Points.Add(p2);
            pg.Points.Add(p3);
            pg.Fill = Brushes.LightSeaGreen;
            pg.MouseLeftButtonDown += (sender, e) => Polygon_MouseLeftButtonDown(sender, e, pg.Points[2]);
            
            canvas.Children.Add(pg);


        }

        private List<System.Windows.Shapes.Ellipse> vertexHighlights = new List<System.Windows.Shapes.Ellipse>();
        private void Polygon_MouseLeftButtonDown(object sender, MouseEventArgs e, System.Windows.Point p)
        {
            var polygon = sender as Polygon;
            if (polygon == null) return;

            if(vertexHighlights.Count == 0)
            {
                foreach (var vertex in polygon.Points)
                {
                    var vertexHighlight = new System.Windows.Shapes.Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Red,
                    };
                    Canvas.SetLeft(vertexHighlight, vertex.X - 5);
                    Canvas.SetTop(vertexHighlight, vertex.Y - 5);
                    canvas.Children.Add(vertexHighlight);
                    vertexHighlights.Add(vertexHighlight);
                }
            }
            else
            {
                var mousePosition = e.GetPosition(this);

                double min_dist = Double.MaxValue;
                int min_idx = -1;
                for(int i = 0; i < polygon.Points.Count; ++i)
                {
                    var point = polygon.Points[i];
                    double dx = mousePosition.X - point.X;
                    double dy = mousePosition.Y - point.Y;
                    double dist = dx * dx + dy * dy;
                    if(dist < min_dist)
                    {
                        min_dist = dist;
                        min_idx = i;
                    }
                }

                Console.WriteLine($"index: {min_idx}, dist:{min_dist}");
                if (min_dist < 5*5)
                {

                }
                else
                {
                    foreach (var vertexHighlight in vertexHighlights)
                    {
                        canvas.Children.Remove(vertexHighlight);
                    }
                    vertexHighlights.Clear();
                }

            }
        }
    }
}
