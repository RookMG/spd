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
            
            Utils.Mediator.Register("StatusBar.ShowMousePosition", PrintMousePosition);
            Utils.Mediator.Register("StatusBar.PrintEntityPosition", PrintEntityPosition);
        }
        public void PrintMousePosition(object obj)
        {
            //TODO : 구현
            double[] size = new double[2];
            var sizeList = obj as IEnumerable;
            int count = 0;
            foreach (var item in sizeList)
            {
                size[count] = Convert.ToDouble(item);
                count++;
            }
            posi_x.Text = "x : " + size[0].ToString();
            posi_y.Text = "y : " + size[1].ToString();
        }

        public void PrintEntityPosition(object obj)
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
        }
    }
}
