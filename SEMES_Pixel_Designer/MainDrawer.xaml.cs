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
        private double[] activeVec = { 0, 0 };
        private int activeNth = -1;
        private bool isLeftMouseHold = false;
        private List<System.Windows.Shapes.Ellipse> vertexHighlights = new List<System.Windows.Shapes.Ellipse>();

        public MainDrawer()
        {
            InitializeComponent();
        }


        static public MainCanvas CanvasRef;
    }

    public class MainCanvas : Canvas
    {
        public MainCanvas()
        {
            // 초기설정
            MainDrawer.CanvasRef = this;
            PolygonEntity.BindCanvasAction = Children.Add;
            PointEntity.BindCanvasAction = Children.Add;
            PointEntity.SetX = SetLeft;
            PointEntity.SetY = SetTop;
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainCanvas), new FrameworkPropertyMetadata(typeof(MainCanvas)));
            ClipToBounds = true;
            Background = Brushes.White;


            PolygonEntity ptest = new PolygonEntity();
            ptest.AddPoint(200, 200);
            ptest.AddPoint(300, 200);
            ptest.AddPoint(200, 300);

            PolygonEntity ptest2 = new PolygonEntity();
            ptest2.AddPoint(100, 100);
            ptest2.AddPoint(300, 150);
            ptest2.AddPoint(200, 100);
            ptest2.AddPoint(200, 50);

            PolygonEntity ptest3 = new PolygonEntity();
            ptest3.AddPoint(10, 100);
            ptest3.AddPoint(30, 150);
            ptest3.AddPoint(70, 100);
            ptest3.AddPoint(20, 50);

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

    }
}
