﻿using System;
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
using System.Windows.Shapes;
using SEMES_Pixel_Designer.Utils;

namespace SEMES_Pixel_Designer.View
{
    /// <summary>
    /// SetCell.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SetCell : Window
    {
        public SetCell()
        {
            InitializeComponent();
        }
        private void CellModify(object sender, RoutedEventArgs e)
        {
            Mediator.NotifyColleagues("MainDrawer.SetCell_Clicked", null);
        }
    }
}
