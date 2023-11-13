using SEMES_Pixel_Designer.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace SEMES_Pixel_Designer.View
{
    /// <summary>
    /// TcpIpLog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TcpIpLog : Page
    {
        public TcpIpLog()
        {
            InitializeComponent();
            DataContext = TcpIpLogViewModel.Instance;
            Loaded += TcpIpLog_Loaded;
        }

        private void TcpIpLog_Loaded(object sender, RoutedEventArgs e)
        {
            // 아이템이 추가될 때마다 스크롤을 최하단으로 이동하는 이벤트 핸들러 등록
            ((INotifyCollectionChanged)logListView.Items).CollectionChanged += LogListView_CollectionChanged;
        }

        private void LogListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && logListView.Items.Count > 0)
            {
                // 아이템이 추가되었을 때만 스크롤을 최하단으로 이동
                var lastItem = logListView.Items[logListView.Items.Count - 1];

                // ScrollViewer를 찾음
                ScrollViewer scrollViewer = FindScrollViewer(logListView);

                // ScrollViewer가 존재하면 스크롤을 최하단으로 이동
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }

        private ScrollViewer FindScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = FindScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
