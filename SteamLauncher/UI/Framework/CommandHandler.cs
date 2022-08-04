using System;
using System.Windows.Input;

namespace SteamLauncher.UI.Framework
{
    public class CommandHandler : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Creates instance of the command handler
        /// </summary>
        /// <param name="execute">Action to be executed by the command</param>
        /// <param name="canExecute">A boolean property defining whether the command can be executed</param>
        public CommandHandler(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Wires CanExecuteChanged event 
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Forces checking if execute is allowed
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }

    public class CommandHandler<TParameter> : ICommand
    {
        private readonly Action<TParameter> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// Creates instance of the command handler generic
        /// </summary>
        /// <param name="execute">Action to be executed by the command</param>
        /// <param name="canExecute">A boolean property defining if the command can be executed</param>
        public CommandHandler(Action<TParameter> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Wires CanExecuteChanged event 
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Forces checking if execute is allowed
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
            //return _canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            _execute((TParameter)parameter);
        }
    }
}
