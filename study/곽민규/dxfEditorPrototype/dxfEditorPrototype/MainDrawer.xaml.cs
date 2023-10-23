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
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using GTE = netDxf.GTE;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using netDxf.Units;
using Attribute = netDxf.Entities.Attribute;
using FontStyle = netDxf.Tables.FontStyle;
using Image = netDxf.Entities.Image;
using Point = netDxf.Entities.Point;
using Trace = netDxf.Entities.Trace;
using ScottPlot;

namespace dxfEditorPrototype
{
    /// <summary>
    /// MainDrawer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainDrawer : Page
    {
        public MainDrawer()
        {
            InitializeComponent();
            WpfPlot1.Plot.Frameless();
            WpfPlot1.Plot.AxisScaleLock(true);

            WpfPlot1.Plot.Style(ScottPlot.Style.Black);
            Utils.Mediator.Register("MainDrawer.Draw", Draw);

        }
        public void Draw(object obj)
        {
            foreach (var line in MainWindow.doc.Entities.Lines)
            {
                var linePlot = WpfPlot1.Plot.AddLine(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
                linePlot.IsHighlighted = true;
                
            }

            foreach (var circle in MainWindow.doc.Entities.Circles)
            {
                WpfPlot1.Plot.AddCircle(x: circle.Center.X, y: circle.Center.Y, radius: circle.Radius);
            }

            foreach (var ellipse in MainWindow.doc.Entities.Ellipses)
            {
                var ellipsePlot = WpfPlot1.Plot.AddEllipse(x: ellipse.Center.X, y: ellipse.Center.Y, xRadius: ellipse.MajorAxis / 2, yRadius: ellipse.MinorAxis / 2);
                ellipsePlot.Rotation = (float)ellipse.Rotation;
            }

            WpfPlot1.Plot.AxisAuto();
            WpfPlot1.Refresh();
        }
    }
}
