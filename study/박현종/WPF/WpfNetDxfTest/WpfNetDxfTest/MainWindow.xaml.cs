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
using netDxf.Header;

namespace WpfNetDxfTest
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
			netDxfSaveTest();
        }

		public static void netDxfSaveTest()
		{
			// your DXF file name
			string file = "sample.dxf";

			// create a new document, by default it will create an AutoCad2000 DXF version
			DxfDocument doc = new DxfDocument();
			// an entity
			var entity = new netDxf.Entities.Line(new Vector2(5, 5), new Vector2(10, 5));
			// add your entities here
			doc.Entities.Add(entity);
			// save to file
			doc.Save(file);

			
			// this check is optional but recommended before loading a DXF file
			DxfVersion dxfVersion = DxfDocument.CheckDxfFileVersion(file);
			// netDxf is only compatible with AutoCad2000 and higher DXF versions
			if (dxfVersion < DxfVersion.AutoCad2000) return;
			// load file
			DxfDocument loaded = DxfDocument.Load(file);
			
		}

	}
}
