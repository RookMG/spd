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

    }
}
