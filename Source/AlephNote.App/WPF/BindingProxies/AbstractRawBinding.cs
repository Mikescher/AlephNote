using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AlephNote.WPF.BindingProxies
{
	public abstract class AbstractRawBinding<TType> : FrameworkElement, INotifyPropertyChanged
	{
		public static readonly DependencyProperty ElementProperty = DependencyProperty.Register("Element", typeof(FrameworkElement), typeof(AbstractRawBinding<TType>), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (o, a) => ((AbstractRawBinding<TType>)o).ElementChanged(a)));
		public FrameworkElement Element
		{
			get { return (FrameworkElement)GetValue(ElementProperty); }
			set { SetValue(ElementProperty, value); }
		}

		private string _propertyPath = null;
		public string PropertyPath
		{
			get { return _propertyPath; }
			set { _propertyPath = value; OnPropertyChanged(); Init(); }
		}

		private IndirectProperty<TType> _targetBinding = null;
		public static readonly DependencyProperty TargetBindingProperty = DependencyProperty.Register("TargetBinding", typeof(IndirectProperty<TType>), typeof(AbstractRawBinding<TType>), new FrameworkPropertyMetadata(null, (o, a) => ((AbstractRawBinding<TType>)o).TargetBindingChanged(a)));
		public IndirectProperty<TType> TargetBinding
		{
			get { return (IndirectProperty<TType>)GetValue(TargetBindingProperty); }
			set { SetValue(TargetBindingProperty, _targetBinding); }
		}

		protected AbstractRawBinding()
		{
			Visibility = Visibility.Collapsed;
			Init();
		}

		private void Init()
		{
			if (Element == null)
			{
				_targetBinding = null;
				TargetBinding = _targetBinding;
				return;
			}

			_targetBinding = IndirectProperty<TType>.Create(Element, _propertyPath);
			TargetBinding = _targetBinding;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null)PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ElementChanged(DependencyPropertyChangedEventArgs args)
		{
			Init();
		}

		private void TargetBindingChanged(DependencyPropertyChangedEventArgs args)
		{
			if (args.NewValue != _targetBinding)
			{
				TargetBinding = _targetBinding;
			}
		}
	}
}
