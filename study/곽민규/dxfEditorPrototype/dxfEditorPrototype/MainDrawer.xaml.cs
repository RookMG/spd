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

            //WpfPlot1.Plot.Style(ScottPlot.Style.Black);
            Utils.Mediator.Register("MainDrawer.DrawCanvas", DrawCanvas);

        }
        public void DrawCanvas(object obj)
        {
            foreach (var line in MainWindow.doc.Entities.Lines)
            {
                var linePlot = new ScottPlot.Plottable.ScatterPlotDraggable(new double[] { line.StartPoint.X, line.EndPoint.X }, new double[] { line.StartPoint.Y, line.EndPoint.Y });
                // var linePlot = WpfPlot1.Plot.AddLine(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
                
                linePlot.IsHighlighted = true;
                linePlot.DragEnabled = true;

                WpfPlot1.Plot.Add(linePlot);

            }

            foreach (var pline in MainWindow.doc.Entities.Polylines2D)
            {
                double[] x = new double[pline.Vertexes.Count], y = new double[pline.Vertexes.Count];
                for(var i = 0; i < pline.Vertexes.Count; i++)
                {
                    x[i] = pline.Vertexes.ElementAt(i).Position.X;
                    y[i] = pline.Vertexes.ElementAt(i).Position.Y;
                }
                //var plinePlot = new ScottPlot.Plottable.ScatterPlotDraggable(x, y);
                //plinePlot.IsHighlighted = true;
                //plinePlot.DragEnabled = true;

                var plinePlot = new ScottPlot.Plottable.ScatterPlotListDraggable();
                plinePlot.AddRange(x, y);

                WpfPlot1.Plot.Add(plinePlot);

            }

            //foreach (var circle in MainWindow.doc.Entities.Circles)
            //{
            //    WpfPlot1.Plot.AddCircle(x: circle.Center.X, y: circle.Center.Y, radius: circle.Radius);
            //}

            //foreach (var ellipse in MainWindow.doc.Entities.Ellipses)
            //{
            //    var ellipsePlot = WpfPlot1.Plot.AddEllipse(x: ellipse.Center.X, y: ellipse.Center.Y, xRadius: ellipse.MajorAxis / 2, yRadius: ellipse.MinorAxis / 2);
            //    ellipsePlot.Rotation = (float)ellipse.Rotation;
            //}

            //double[] x = ScottPlot.DataGen.Consecutive(50);
            //double[] y = ScottPlot.DataGen.Cos(50);

            //var scatter = new ScottPlot.Plottable.ScatterPlotDraggable(x, y)
            //{
            //    DragEnabled = true,
            //};
            //WpfPlot1.Plot.Add(scatter);


            WpfPlot1.Plot.AxisAuto();
            WpfPlot1.Refresh();
        }
    }


}
