using System;
using System.Diagnostics;
using System.Windows.Input;

namespace TrabalhoSocketsUI
{
    public class RelayCommand : ICommand
    {
        #region Members
        readonly Func<Object, Boolean> _canExecute;
        readonly Action<Object> _execute;

        public event EventHandler CanExecuteChanged;
        #endregion

        #region Constructors
        public RelayCommand(Action<Object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<Object> execute, Func<Object, Boolean> canExecutee)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            return _canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }

        #endregion
    }

}
