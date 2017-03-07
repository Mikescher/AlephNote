using System;
using System.Windows.Input;

namespace AlephNote.WPF.MVVM
{
	/// <summary>
	/// http://stackoverflow.com/a/22286816/1761622
	/// </summary>
	public class RelayCommand : ICommand
	{
		#region Fields

		readonly Action<object> _execute;
		readonly Predicate<object> _canExecute;

		#endregion

		#region Constructors

		public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = execute;
			_canExecute = canExecute;
		}

		public RelayCommand(Action execute, Predicate<object> canExecute = null)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = x=>execute();
			_canExecute = canExecute;
		}

		#endregion

		#region ICommand Members

		public bool CanExecute(object parameter)
		{
			if (_canExecute == null) return true;
			return _canExecute.Invoke(parameter);
		}
		
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
		
		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		#endregion
	}
}
