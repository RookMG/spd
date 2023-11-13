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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SEMES_Pixel_Designer.ViewModel;

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// ToolBar.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToolBar : Page
    {
        public ToolBar()
        {
            InitializeComponent();
            DataContext = new CommandDataContext();
        }

        private void Clone_Button_Click(object sender, RoutedEventArgs e)
        {
            SEMES_Pixel_Designer.CloneEntity setCloneWindow = new SEMES_Pixel_Designer.CloneEntity();
            setCloneWindow.ShowDialog();
        }
    }
}