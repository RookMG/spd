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
using netDxf.Entities;
using Microsoft.Win32;

namespace HelloWPFApp
{
    /// <summary>
    /// ToolBar.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToolBar : Page
    {
        public ToolBar()
        {
            InitializeComponent();
        }

        private void NewDocumentButton(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("새 문서");

            //string file = "abcd.dxf";
            //DxfDocument doc = new DxfDocument();
            //var entity = new netDxf.Entities.Line(new Vector2(5, 5), new Vector2(10, 5));
            //doc.Entities.Add(entity);
            //doc.Save(file);

        }

        private void LoadDocumentButton(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "dxf 파일 (*.dxf)|*.dxf";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
            }
        }

        private void SaveDocumentButton(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("새 문서");

            //string file = "abcd.dxf";
            //DxfDocument doc = new DxfDocument();
            //var entity = new netDxf.Entities.Line(new Vector2(5, 5), new Vector2(10, 5));
            //doc.Entities.Add(entity);
            //doc.Save(file);

        }
    }
}
