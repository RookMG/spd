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

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// EntityDetails.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EntityDetails : Page
    {
        public EntityDetails()
        {
            InitializeComponent();

            // TODO : 초기화 함수들 작성
            Utils.Mediator.Register("MainDrawer.DrawCanvas", DrawCanvas);

        }
        public void DrawCanvas(object obj)
        {
            // TODO : 화면에 그리는 부분
            // 
            // MVVM이라면 이곳에서 ViewModel 변경
            // MVVM이 아니라면 이곳에서 화면에 그리는 로직 작성
        }
    }
}
