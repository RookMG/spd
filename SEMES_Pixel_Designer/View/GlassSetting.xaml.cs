using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SEMES_Pixel_Designer.Utils;

namespace SEMES_Pixel_Designer.View
{
    /// <summary>
    /// GlassSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GlassSetting : Window
    {
        public GlassSetting()
        {
            InitializeComponent();
            DataContext = new ComboBoxGlassSizeList();
        }

        class ComboBoxGlassSizeList
        {
            public List<string> GlassSizeList { get; set; } = new List<string>()
            {
                "270x360",
                "370x470",
                "550x650",
                "730x920",
                "1300x1500",
                "1500x1850",
                "1870x2200",
                "2200x2500",
                "2940x3370",
                "사용자 지정"
            };
        }

        private void Set_Glass(object sender, RoutedEventArgs e)
        {
            Mediator.NotifyColleagues("MainDrawer.SetGlass_click", null);
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex _regex = new Regex("/^[0-9]+(.[0-9]+)?$/");
            e.Handled = _regex.IsMatch(e.Text);
        }
    }
}
