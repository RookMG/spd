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

namespace dxfEditorPrototype
{
    /// <summary>
    /// EntityDetails.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EntityDetails : Page
    {
        public EntityDetails()
        {
            InitializeComponent();
            Utils.Mediator.Register("MainWindow.ShowEntityTypes", ShowEntityTypes);
        }

        public void ShowEntityTypes(object obj)
        {
            foreach (var line in MainWindow.doc.Entities.Lines)
            {
                //var linePlot = WpfPlot1.Plot.AddLine(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);


            }

            foreach (var circle in MainWindow.doc.Entities.Circles)
            {
                //WpfPlot1.Plot.AddCircle(x: circle.Center.X, y: circle.Center.Y, radius: circle.Radius);
            }

            foreach (var ellipse in MainWindow.doc.Entities.Ellipses)
            {
                //var ellipsePlot = WpfPlot1.Plot.AddEllipse(x: ellipse.Center.X, y: ellipse.Center.Y, xRadius: ellipse.MajorAxis / 2, yRadius: ellipse.MinorAxis / 2);

            }
        }
    }
}
