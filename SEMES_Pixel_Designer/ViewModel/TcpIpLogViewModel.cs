using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SEMES_Pixel_Designer.ViewModel
{
    public class TcpIpLogViewModel : INotifyPropertyChanged
    {
        private static TcpIpLogViewModel _instance;

        public static TcpIpLogViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TcpIpLogViewModel();
                }
                return _instance;
            }
        }


        public TcpIpLogViewModel()
        {
            _logMessageList = new ObservableCollection<string>();
        }

        private ObservableCollection<string> _logMessageList;
        public ObservableCollection<string> LogMessageList
        {
            get { return _logMessageList; }
            set
            {
                _logMessageList = value;
                OnPropertyChanged(nameof(LogMessageList));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
