using AlephNote.WPF.Util;
using System.Windows;

namespace AlephNote.WPF.Windows
{
	public partial class FirstStartupWindow : IChangeListener
	{
		private readonly FirstStartupViewmodel _vm;
		private readonly MainWindow _owner;

		public FirstStartupWindow(MainWindow owner)
		{
			InitializeComponent();

			Owner = _owner = owner;
			DataContext = _vm = new FirstStartupViewmodel(this);
		}

		private void OnStartSync(object sender, RoutedEventArgs e)
		{
			_vm.StartSync();
		}

		private void OnOK(object sender, RoutedEventArgs e)
		{
			var sett = _owner.VM.Settings.Clone();

			sett.Accounts.Add(_vm.Account);
			sett.ActiveAccount = _vm.Account;

			_owner.VM.Repository.ApplyNewAccountData(_vm.Account, _vm.ValidationResultData, _vm.ValidationResultNotes);

			_owner.VM.ChangeSettings(sett);

			Close();
		}

		private void OnCancel(object sender, RoutedEventArgs e)
		{
			Close();
		}

		public void OnChanged(string source, int id, object value)
		{
			_vm.OnAccountChanged();
		}
	}
}
