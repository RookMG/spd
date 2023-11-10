using SEMES_Pixel_Designer.Utils;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// CloneEntities.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CloneEntity : Window
    {
        public CloneEntity()
        {
            InitializeComponent();

            Emp e = new Emp()
            {
                Rows = 0,
                Cols = 0,
                Rowi = 0,
                Coli = 0
            };

            this.DataContext = e;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Emp emp = this.DataContext as Emp;

            object tuple = Tuple.Create(emp.Rows, emp.Cols, emp.Rowi, emp.Coli);
            Coordinates.CanvasRef.CloneEntities(tuple);
            //Window.GetWindow(this).Close();
            //DialogResult = true;
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex _regex = new Regex("[^0-9]+");
            e.Handled = _regex.IsMatch(e.Text);
        }
    }

    public class Emp
    {
        public int Rows
        {
            get; set;
        }
        public int Cols
        {
            get; set;
        }
        public double Rowi
        {
            get; set;
        }
        public double Coli
        {
            get; set;
        }
    }

    //public class Converter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return values.Clone();
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
