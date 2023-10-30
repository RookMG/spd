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
        public MainCanvas()
        {
            // 초기설정
            Coordinates.CanvasRef = this;
            SizeChanged += new SizeChangedEventHandler((object sender, SizeChangedEventArgs e) => { 
                DrawCanvas(null);
            });

            PolygonEntity.BindCanvasAction = Children.Add;
            PointEntity.BindCanvasAction = Children.Add;
            PointEntity.SetX = SetLeft;
            PointEntity.SetY = SetTop;
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainCanvas), new FrameworkPropertyMetadata(typeof(MainCanvas)));
            ClipToBounds = true;
            Background = Brushes.Beige;


            Utils.Mediator.Register("MainDrawer.DrawCanvas", DrawCanvas);


        }

        public void DrawCanvas(object obj)
        {
            Children.Clear();
            List<double> x = new List<double>(), y = new List<double>();
            List<PolygonEntity> Lines = new List<PolygonEntity>();
            foreach (var line in MainWindow.doc.Entities.Lines)
            {
                x.Add(line.StartPoint.X);
                y.Add(line.StartPoint.Y);
                x.Add(line.EndPoint.X);
                y.Add(line.EndPoint.Y);
            }
            Coordinates.updateRange(x, y);


            foreach (var line in MainWindow.doc.Entities.Lines)
            {
                var lineEntity = new PolygonEntity();
                lineEntity.AddPoint(line.StartPoint.X, line.StartPoint.Y);
                lineEntity.AddPoint(line.EndPoint.X, line.EndPoint.Y);

                Lines.Add(lineEntity);
            }
            //MessageBox.Show(Coordinates.minX + " , " + Coordinates.minY + " , " + Coordinates.maxX + " , " + Coordinates.maxY);
            //MessageBox.Show(Coordinates.CanvasRef.ActualHeight+" , "+Coordinates.CanvasRef.ActualWidth);
        }

    }
}