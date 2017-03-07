using System;
using System.Windows.Input;

namespace AlephNote.WPF.MVVM
{
	/// <summary>
	/// http://stackoverflow.com/a/22286816/1761622
	/// </summary>
	public class RelayCommand<T> : ICommand
	{
		#region Fields

		readonly Action<T> _execute;
		readonly Predicate<T> _canExecute;

		#endregion

		#region Constructors

		public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = execute;
			_canExecute = canExecute;
		}

		#endregion

		#region ICommand Members
		
		public bool CanExecute(object parameter)
		{
			if (_canExecute == null) return true;
			return _canExecute.Invoke((T)parameter);
		}
		
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
		
		public void Execute(object parameter)
		{
			_execute((T)parameter);
		}

		#endregion
	}
}
