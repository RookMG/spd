using MVVMtutorial.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MVVMtutorial.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private MainModel myModel = null;
        private Student student = null;
        private StudentList items;

        public StudentList Items
        {
            get
            {
                return this.items;
            }
        }

        public MainViewModel()
        {
            myModel = new MainModel();
            student = new Student();
            this.items = new StudentList();
        }

        public string Dollar
        {
            get
            {
                if(string.IsNullOrEmpty(myModel.dollar))
                {
                    Won = "0";
                }
                else
                {
                    int num = -1;
                    if(int.TryParse(myModel.dollar, out num))
                    {
                        int result = num * 1160;
                        Won = result.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Please insert number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Dollar = "";
                        Won = "0";
                    }
                }
                return myModel.dollar;
            }
            set
            {
                if (myModel.dollar != value)
                {
                    myModel.dollar = value;
                    OnPropertyChanged("Dollar");
                }
            }
        }

        public string Won
        {
            get
            {
                return myModel.won;
            }
            set
            {
                myModel.won = value;
                OnPropertyChanged("Won");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
