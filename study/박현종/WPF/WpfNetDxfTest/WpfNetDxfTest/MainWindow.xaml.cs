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
			List<Vector2> vectorList = new List<Vector2>()
			{
				new Vector2(9, 9),
				new Vector2(9, 12),
				new Vector2(12, 12),
				new Vector2(12, 9)
			};
			var entityPoint1 = new netDxf.Entities.Point(new Vector2(3, 3));
			var entityPoint2 = new netDxf.Entities.Point(new Vector2(6, 6));
			var entityLine = new netDxf.Entities.Line(new Vector2(5, 5), new Vector2(10, 5));
			var entityPolyline = new netDxf.Entities.Polyline2D(vectorList, true);
			// add your entities here
			doc.Entities.Add(entityPoint1);
			doc.Entities.Add(entityPoint2);
			doc.Entities.Add(entityLine);
			doc.Entities.Add(entityPolyline);
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
