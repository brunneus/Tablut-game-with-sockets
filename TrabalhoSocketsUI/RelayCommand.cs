using System;
using System.Diagnostics;
using System.Windows.Input;

namespace TrabalhoSocketsUI
{
    public class RelayCommand : ICommand
    {
        #region Members
        readonly Func<bool> _canExecute;
        readonly Action _execute;

        public event EventHandler CanExecuteChanged;
        #endregion

        #region Constructors
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecutee)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            return _canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            _execute.Invoke();
        }

        #endregion
    }

    public class RelayCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Action<T> _actionToExecute;
        private Func<T, bool> _canExecuteFunc;

        public RelayCommand(Action<T> action)
        {
            _actionToExecute = action;
        }

        public RelayCommand(Action<T>  action, Func<T, bool> canExecute)
        {
            _actionToExecute = action;
            _canExecuteFunc = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecuteFunc == null)
                return true;

            return _canExecuteFunc.Invoke((T)parameter);
        }

        public void Execute(object parameter)
        {
            _actionToExecute.Invoke((T)parameter);
        }
    }


}
