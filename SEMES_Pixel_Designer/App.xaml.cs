using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            // Pause to show the splash screen for 3 seconds
            System.Threading.Thread.Sleep(1500);
        }
    }
}
