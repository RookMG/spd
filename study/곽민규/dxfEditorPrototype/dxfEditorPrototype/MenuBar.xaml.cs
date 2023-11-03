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
    /// MenuBar.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MenuBar : Page
    {
        public MenuBar()
        {
            InitializeComponent();
        }


        private void NewDocument(object sender, RoutedEventArgs e)
        {

            Utils.Mediator.NotifyColleagues("MainWindow.NewDxf", null);
        }

        private void OpenDocument(object sender, RoutedEventArgs e)
        {
            Utils.Mediator.NotifyColleagues("MainWindow.OpenDxf", null);
        }
    }

}
