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

namespace HelloWPFApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        private IDWMouseOperation test;

        public interface IDWMouseOperation
        {
            /// <summary>

            /// </summary>
            void DWMouseDown(object sender, MouseButtonEventArgs e);
            /// <summary>

            /// </summary>
            void DWMouseMove(object sender, MouseEventArgs e);
            /// <summary>

            /// </summary>
            void DWMouseUp(object sender, MouseButtonEventArgs e);
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewDocument(object sender, RoutedEventArgs e)
        {

            MessageBox.Show("점", "test");
        }

        private void Button_Click_Dot(object sender, RoutedEventArgs e)
        {
            
            MessageBox.Show("점", "test");
        }

        private void Button_Click_Line(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("선", "test");
        }

        private void Button_Click_Rectangle(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("사각", "test");
        }

        private void Button_Click_Polygon(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("폴리", "test");
        }

      
    }
}
