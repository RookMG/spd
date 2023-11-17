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
    /// MakeCell.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MakeCell : Window
    {
        public MakeCell()
        {
            InitializeComponent();
        }

        private void cng_bool(object sender, RoutedEventArgs e)
        {
            Mediator.NotifyColleagues("MainDrawer.MakeNewcell_click", null);
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex _regex = new Regex("/^[0-9]+(.[0-9]+)?$/");
            e.Handled = _regex.IsMatch(e.Text);
        }
    }
}
