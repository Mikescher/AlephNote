using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AlephNote.WPF.BindingProxies
{
	public abstract class AbstractLegacyBinding<TType> : FrameworkElement, INotifyPropertyChanged
	{
		#region Properties

		public static readonly DependencyProperty ElementProperty = DependencyProperty.Register("Element", typeof(object), typeof(AbstractLegacyBinding<TType>), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (o, a) => ((AbstractLegacyBinding<TType>)o).ElementChanged()));
		public object Element
		{
			get { return GetValue(ElementProperty); }
			set { SetValue(ElementProperty, value); }
		}

		private string _propertyPath = null;
		public string PropertyPath
		{
			get { return _propertyPath; }
			set { _propertyPath = value; OnPropertyChanged(); Init(); }
		}

		private string _changedEventPath = null;
		public string ChangedEventPath
		{
			get { return _changedEventPath; }
			set { _changedEventPath = value; OnPropertyChanged(); Init(); }
		}

		public static readonly DependencyProperty TargetBindingProperty = DependencyProperty.Register("TargetBinding", typeof(TType), typeof(AbstractLegacyBinding<TType>), new FrameworkPropertyMetadata(default(TType), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, a) => ((AbstractLegacyBinding<TType>)o).TargetBindingChanged(a)));
		public TType TargetBinding
		{
			get { return (TType)GetValue(TargetBindingProperty); }
			set { SetValue(TargetBindingProperty, value); }
		}

		#endregion

		private Delegate subscribedDelegate;
		private EventInfo subscribedEventInfo;
		private object subscribedEventElement;
		private IndirectProperty<TType> _indirectProperty = null;

		private bool _suppressChangedEvent = false;

		protected AbstractLegacyBinding()
		{
			Visibility = Visibility.Collapsed;
			Init();
		}

		private void Init()
		{
			if (Element == null || PropertyPath == null || ChangedEventPath == null)
			{
				_indirectProperty = null;

				TargetBinding = default(TType);
				return;
			}

			try
			{
				if (subscribedDelegate != null)
				{
					try
					{
						EventProxy.Unsubscribe(subscribedEventElement, subscribedEventInfo, subscribedDelegate);
					}
					finally
					{
						subscribedDelegate = null;
						subscribedEventInfo = null;
						subscribedEventElement = null;
					}
				}

				_indirectProperty = IndirectProperty<TType>.Create(Element, PropertyPath);
				var evt = ReflectionPathResolver.GetEvent(Element, ChangedEventPath);
				var dlgt = EventProxy.Subscribe(evt.Item1, evt.Item2, OnProxyChangedEvent);

				subscribedDelegate     = dlgt;
				subscribedEventInfo    = evt.Item2;
				subscribedEventElement = evt.Item1;

				TargetBinding = _indirectProperty.Get();
			}
			catch (Exception)
			{
				_indirectProperty = null;
			}
		}

		private void OnProxyChangedEvent()
		{
			try
			{
				_suppressChangedEvent = true;
				if (_indirectProperty != null) TargetBinding = _indirectProperty.Get();
			}
			finally
			{
				_suppressChangedEvent = false;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ElementChanged()
		{
			Init();
		}

		private void TargetBindingChanged(DependencyPropertyChangedEventArgs args)
		{
			if (_suppressChangedEvent) return;

			var v = (TType) args.NewValue;

			if (_indirectProperty != null)
			{
				if (!EqualityComparer<TType>.Default.Equals(v, _indirectProperty.Get()))
				{
					_indirectProperty.Set((TType)args.NewValue);
				}
			}
		}
	}
}
