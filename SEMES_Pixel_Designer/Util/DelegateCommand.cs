using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SEMES_Pixel_Designer.Utils
{
    public class DelegateCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new NullReferenceException("execute can not null");

            _execute = execute;
            _canExecute = canExecute;
        }

        public DelegateCommand(Action<object> execute) : this(execute, null)
        {

        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }
    }



    class CommandDataContext
    {
        public DelegateCommand DelCommand { get; set; } = new DelegateCommand((param)=> {
            Utils.Mediator.NotifyColleagues((string)param, null); 
        });
    }
}
