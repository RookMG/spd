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
        static public SPDCanvas CanvasRef;
        private void ChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            if (CanvasRef != null)
            {
                CanvasRef.ChangeCanvasColor();
            }

        }
    }

    public class MainCanvas : Canvas
    {
        public MainCanvas()
        {
            // 초기설정
            //MainDrawer.CanvasRef = this;
            PolygonEntity.BindCanvasAction = Children.Add;
            PointEntity.BindCanvasAction = Children.Add;
            PointEntity.SetX = SetLeft;
            PointEntity.SetY = SetTop;
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainCanvas), new FrameworkPropertyMetadata(typeof(MainCanvas)));
            ClipToBounds = true;
            Background = Brushes.Beige;


            Utils.Mediator.Register("MainDrawer.DrawCanvas", DrawCanvas);





            PolygonEntity ptest = new PolygonEntity();
            ptest.AddPoint(200, 200);
            ptest.AddPoint(300, 200);

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

        public void DrawCanvas(object obj)
        {
            Children.Clear();

            List<PolygonEntity> Lines = new List<PolygonEntity>();
            foreach (var line in MainWindow.doc.Entities.Lines)
            {
                var lineEntity = new PolygonEntity();
                //MessageBox.Show(line.StartPoint.ToString());
                //lineEntity.AddPoint(line.StartPoint.X, line.StartPoint.Y);
                //lineEntity.AddPoint(line.EndPoint.X, line.EndPoint.Y);

                lineEntity.AddPoint(100, 100);
                lineEntity.AddPoint(300, 150);
                Lines.Add(lineEntity);
            }

        }

    }

    public class SPDCanvas : Canvas
    {
        #region 변수 모음
        private readonly MatrixTransform _transform = new MatrixTransform();
        private Point _initialMousePosition;
        private bool _dragging;
        private UIElement _selectedElement;
        private Vector _draggingDelta;
        private List<PolygonEntity> _polygonEntitys = new List<PolygonEntity>();
        private List<Line> _gridLines = new List<Line>();
        private Color _lineColor = Color.FromArgb(0xFF, 0x66, 0x66, 0x66);
        public float Zoomfactor { get; set; } = 1.1f;
        #endregion



        public SPDCanvas()
        {
            MainDrawer.CanvasRef = this;

            MouseDown += Canvas_MouseDown;
            MouseUp += Canvas_MouseUp;
            MouseMove += Canvas_MouseMove;
            MouseWheel += Canvas_MouseWheel;

            for (int x = -4000; x <= 4000; x += 100)
            {
                Line verticalLine = new Line
                {
                    Stroke = new SolidColorBrush(_lineColor),
                    X1 = x,
                    Y1 = -4000,
                    X2 = x,
                    Y2 = 4000
                };

                if (x % 1000 == 0)
                {
                    verticalLine.StrokeThickness = 6;
                }
                else
                {
                    verticalLine.StrokeThickness = 2;
                }

                Children.Add(verticalLine);
                _gridLines.Add(verticalLine);
            }

            for (int y = -4000; y <= 4000; y += 100)
            {
                Line horizontalLine = new Line
                {
                    Stroke = new SolidColorBrush(_lineColor),
                    X1 = -4000,
                    Y1 = y,
                    X2 = 4000,
                    Y2 = y
                };

                if (y % 1000 == 0)
                {
                    horizontalLine.StrokeThickness = 6;
                }
                else
                {
                    horizontalLine.StrokeThickness = 2;
                }

                Children.Add(horizontalLine);
                _gridLines.Add(horizontalLine);
            }

            PolygonEntity.BindCanvasAction = Children.Add;
            PointEntity.BindCanvasAction = Children.Add;
            PointEntity.SetX = SetLeft;
            PointEntity.SetY = SetTop;
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SPDCanvas), new FrameworkPropertyMetadata(typeof(SPDCanvas)));
            ClipToBounds = true;
            Background = Brushes.White;

            Utils.Mediator.Register("MainDrawer.DrawCanvas", DrawCanvas);

            PolygonEntity ptest = new PolygonEntity();
            ptest.AddPoint(200, 200);
            ptest.AddPoint(300, 200);
            _polygonEntitys.Add(ptest);

            PolygonEntity ptest2 = new PolygonEntity();
            ptest2.AddPoint(100, 100);
            ptest2.AddPoint(300, 150);
            ptest2.AddPoint(200, 100);
            ptest2.AddPoint(200, 50);
            _polygonEntitys.Add(ptest2);

            PolygonEntity ptest3 = new PolygonEntity();
            ptest3.AddPoint(10, 100);
            ptest3.AddPoint(30, 150);
            ptest3.AddPoint(70, 100);
            ptest3.AddPoint(20, 50);
            _polygonEntitys.Add(ptest3);

        }



        public void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _initialMousePosition = _transform.Inverse.Transform(e.GetPosition(this));
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                if (this.Children.Contains((UIElement)e.Source))
                {
                    _selectedElement = (UIElement)e.Source;
                    Point mousePosition = Mouse.GetPosition(this);
                    double x = Canvas.GetLeft(_selectedElement);
                    double y = Canvas.GetTop(_selectedElement);
                    Point elementPosition = new Point(x, y);
                    _draggingDelta = elementPosition - mousePosition;
                }
                _dragging = true;
            }
        }

        public void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragging = false;
            _selectedElement = null;
        }

        public void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point mousePosition = _transform.Inverse.Transform(e.GetPosition(this));
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);
                var translate = new TranslateTransform(delta.X, delta.Y);
                _transform.Matrix = translate.Value * _transform.Matrix;

                foreach (UIElement child in this.Children)
                {
                    child.RenderTransform = _transform;
                }
            }

            if (_dragging && e.LeftButton == MouseButtonState.Pressed)
            {
                double x = Mouse.GetPosition(this).X;
                double y = Mouse.GetPosition(this).Y;

                if (_selectedElement != null)
                {
                    Canvas.SetLeft(_selectedElement, x + _draggingDelta.X);
                    Canvas.SetTop(_selectedElement, y + _draggingDelta.Y);
                }
            }
        }

        public void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            float scaleFactor = Zoomfactor;
            if (e.Delta < 0)
            {
                scaleFactor = 1f / scaleFactor;
            }

            Point mousePostion = e.GetPosition(this);

            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            _transform.Matrix = scaleMatrix;

            foreach (UIElement child in this.Children)
            {
                double x = Canvas.GetLeft(child);
                double y = Canvas.GetTop(child);

                double sx = x * scaleFactor;
                double sy = y * scaleFactor;

                Canvas.SetLeft(child, sx);
                Canvas.SetTop(child, sy);

                child.RenderTransform = _transform;
            }
        }

        public void DrawCanvas(object obj)
        {
            Children.Clear();

            List<PolygonEntity> Lines = new List<PolygonEntity>();
            foreach (var line in MainWindow.doc.Entities.Lines)
            {
                var lineEntity = new PolygonEntity();
                //MessageBox.Show(line.StartPoint.ToString());
                //lineEntity.AddPoint(line.StartPoint.X, line.StartPoint.Y);
                //lineEntity.AddPoint(line.EndPoint.X, line.EndPoint.Y);
             
                lineEntity.AddPoint(100, 100);
                lineEntity.AddPoint(300, 150);
                Lines.Add(lineEntity);
            }

        }

        public void ChangeCanvasColor()
        {
            try
            {
                if (Background == Brushes.Black)
                {
                    Background = Brushes.White;
                    foreach (PolygonEntity child in _polygonEntitys)
                    {
                        child.polygon.Stroke = Brushes.Black;
                    }
                }
                else
                {
                    Background = Brushes.Black;
                    foreach (PolygonEntity child in _polygonEntitys)
                    {
                        child.polygon.Stroke = Brushes.White;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("예외 발생: " + ex.Message);
            }
        }


    }


}