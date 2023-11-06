using System;
using System.Collections;
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
    /// StatusBar.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StatusBar : Page
    {
        public StatusBar()
        {
            InitializeComponent();
            PrintFilepath(null);
            Utils.Mediator.Register("StatusBar.ShowMousePosition", PrintMousePosition);
            Utils.Mediator.Register("StatusBar.PrintFilepath", PrintFilepath);
            // Utils.Mediator.Register("StatusBar.PrintEntityPosition", PrintEntityPosition);
        }
        public void PrintMousePosition(object obj)
        {
            Point p = (Point)obj;
            positionText.Text = string.Format("Mouse Position : ( {0:F4}, {1:F4} )", p.X, p.Y);
        }

        public void PrintFilepath(object obj)
        {
            string path = (string)obj;
            filePathText.Text = "File Directory : "+(path==null?"새 파일":path);
        }

            /*public void PrintEntityPosition(object obj)
            {
                // 각 점의 X와 Y 좌표를 저장할 리스트
                var tmp = (PointCollection)obj;
                foreach (var vertex in tmp)
                {
                    var add_textblock = new TextBlock();
                    add_textblock.Width = 100;
                    add_textblock.Text = "x : " + (vertex.X).ToString() + "  y : " + (vertex.Y).ToString();
                    ((StackPanel)posi_x.Parent).Children.Add(add_textblock);
                }
            }*/
    }
}
