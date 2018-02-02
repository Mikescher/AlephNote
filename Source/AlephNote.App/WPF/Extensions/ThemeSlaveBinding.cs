using AlephNote.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace AlephNote.WPF.Extensions
{
	public sealed class ThemeSlaveBinding : MarkupExtension, IThemeListener
	{
		private Binding _sourceBinding;
		public Binding SourceBinding { get { return _sourceBinding; } set { _sourceBinding = value; } }

		private object _targetProperty;
		private readonly List<WeakReference> _targetObjects = new List<WeakReference>();

		public ThemeSlaveBinding()
		{
			ThemeManager.Inst.RegisterSlave(this);
		}

		public ThemeSlaveBinding(Binding b)
		{
			_sourceBinding = b;
			ThemeManager.Inst.RegisterSlave(this);
		}

		public bool IsTargetAlive => true;// (_targetObjects.Count == 0) || _targetObjects.Any(reference => reference.IsAlive);

		public void OnThemeChanged()
		{
			foreach (var reference in _targetObjects)
			{
				if (reference.IsAlive) UpdateTargetInternal(reference.Target);
			}
		}

		private void UpdateTargetInternal(object target)
		{
			try
			{
				if (_targetProperty is DependencyProperty dp)
				{
					var dependencyObject = target as FrameworkElement;
					if (target is FrameworkElement fe)
					{
						fe.GetBindingExpression(dp).UpdateTarget();
					}
				}
			}
			catch (Exception e)
			{
				App.Logger.Error("ThemeSlaveBinding", $"UpdateTargetInternal failed", e);
				App.Logger.ShowExceptionDialog("Update theme failed", e, $"UpdateTargetInternal failed");
			}
		}

		private void RegisterTarget(IServiceProvider serviceProvider)
		{
			var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
			if (provideValueTarget != null)
			{
				var target = provideValueTarget.TargetObject;

				if (target != null && target.GetType().FullName != "System.Windows.SharedDp")
				{
					_targetProperty = provideValueTarget.TargetProperty;
					_targetObjects.Add(new WeakReference(target));
				}
			}
		}

		public override object ProvideValue(IServiceProvider sp)
		{
			RegisterTarget(sp);

			if (_sourceBinding == null) throw new ArgumentNullException(nameof(SourceBinding));

			if (!(sp is IProvideValueTarget pvt)) return null; // prevents XAML Designer crashes

			if (!(pvt.TargetObject is DependencyObject dobj)) return pvt.TargetObject; // required for template re-binding

			var dp = (DependencyProperty)pvt.TargetProperty;

			var be = _sourceBinding.ProvideValue(sp);

			return be;
		}
	}
}
