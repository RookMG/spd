using netDxf;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SEMES_Pixel_Designer.View
{
    /// <summary>
    /// ExportFile.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ExportFile : Window
    {
        static ExportOption option;
        public ExportFile()
        {
            InitializeComponent();
            option = new ExportOption();
        }

        public void Export(object sender, RoutedEventArgs e)
        {
            DxfDocument exportDoc = new DxfDocument();
            foreach (PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
            {
                netDxf.Entities.EntityObject entityObject = entity.GetEntityObject();
                if (option.Red && entityObject.Color != AciColor.Red) continue;
                if (option.Green && entityObject.Color != AciColor.Green) continue;
                if (option.Blue && entityObject.Color != AciColor.Blue) continue;
                if (option.Selected&&!entity.selected) continue;
                
                for(int r = 0; r< entity.cell.patternRows; r++)
                {
                    for(int c = 0; c < entity.cell.patternCols; c++)
                    {
                        netDxf.Entities.EntityObject clone = (netDxf.Entities.EntityObject)entityObject.Clone();
                        clone.TransformBy(Matrix3.Identity,new Vector3(entity.cell.getPatternOffsetX(c), entity.cell.getPatternOffsetY(r), 0));
                        exportDoc.Entities.Add(clone);
                    }
                }
            }
            exportDoc.Layers["0"].Description = "Exported";
            SaveFileDialog dlgSaveAsFile = new SaveFileDialog();
            dlgSaveAsFile.Title = "파일 저장";
            if (TcpIp.iniData.TryGetValue("default_path", out string value))
            {
                dlgSaveAsFile.InitialDirectory = value;
            }
            dlgSaveAsFile.Filter = "dxf file (*.dxf) | *.dxf";

            // 파일 번호 관리
            dlgSaveAsFile.FileName = DateTime.Now.ToString("yyMMdd_HHmmss")+"_Export";


            if (dlgSaveAsFile.ShowDialog().ToString() == "OK")
            {
                // System.Windows.MessageBox.Show(dlgSaveAsFile.FileName);
                exportDoc.Save(dlgSaveAsFile.FileName);
                Close();
            }
        }

    }


    public class ExportOption
    {
        private int optionNum;
        public ExportOption()
        {
            optionNum = 0;
        }
        public bool All
        {
            get => optionNum == 0;
            set => optionNum = 0;
        }
        public bool Red
        {
            get => optionNum == 1;
            set => optionNum = 1;
        }
        public bool Blue
        {
            get => optionNum == 2;
            set => optionNum = 2;
        }
        public bool Green
        {
            get => optionNum == 3;
            set => optionNum = 3;
        }
        public bool Selected
        {
            get => optionNum == 4;
            set => optionNum = 4;
        }

    }
}
