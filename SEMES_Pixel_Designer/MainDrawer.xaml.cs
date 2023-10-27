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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// MainDrawer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainDrawer : Page
    {
        private double[] activeVec = { 0, 0 };
        private int activeNth = -1;
        private bool isLeftMouseHold = false;
        private List<System.Windows.Shapes.Ellipse> vertexHighlights = new List<System.Windows.Shapes.Ellipse>();

        public MainDrawer()
        {
            InitializeComponent();
        }

        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            // Get the x and y coordinates of the mouse pointer.
            System.Windows.Point position = e.GetPosition(this);
            double pX = position.X;
            double pY = position.Y;

            object[] posi = new object[] { pX, pY };

            Utils.Mediator.NotifyColleagues("MainWindow.ShowMousePosition", posi);
        }
    }
}
