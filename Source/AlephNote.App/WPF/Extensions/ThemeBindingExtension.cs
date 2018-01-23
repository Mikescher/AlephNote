using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Reflection;
using System.Windows;
using AlephNote.Common.Util;
using AlephNote.Common.Themes;
using AlephNote.WPF.Converter;

namespace AlephNote.WPF.Extensions
{
	[MarkupExtensionReturnType(typeof(object))]
	[ContentProperty("ThemeKey")]
	public class ThemeBindingExtension : MarkupExtension, IThemeListener
	{
		public string ThemeKey { get; set; }

		public string Type { get; set; }

		private readonly List<WeakReference> _targetObjects = new List<WeakReference>();

		private object _targetProperty;

		public ThemeBindingExtension()
		{
			ThemeManager.Inst?.RegisterSlave(this);
		}

		public ThemeBindingExtension(string themekey)
		{
			ThemeKey = themekey;
			ThemeManager.Inst?.RegisterSlave(this);
		}

		/// <summary>
		/// Returns the object that corresponds to the specified resource key.
		/// </summary>
		/// <param name="serviceProvider">An object that can provide services for the markup extension.</param>
		/// <returns>The object that corresponds to the specified resource key.</returns>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			RegisterTarget(serviceProvider);
			object result = this;

			// when used in a template the _targetProperty may be null - in this case
			// return this
			//
			if (_targetProperty != null)
			{
				result = GetValue(ThemeKey, Type);
			}
			return result;
		}
		
		protected virtual void RegisterTarget(IServiceProvider serviceProvider)
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

		public void UpdateTargetValue()
		{
			foreach (var reference in _targetObjects)
			{
				if (reference.IsAlive)
				{
					UpdateTargetInternal(reference.Target);
				}
			}
		}

		protected virtual void UpdateTargetInternal(object target)
		{
			if (_targetProperty is DependencyProperty)
			{
				var dependencyObject = target as DependencyObject;
				if (dependencyObject != null)
				{
					dependencyObject.SetValue(_targetProperty as DependencyProperty, GetValue(ThemeKey, Type));
				}
			}
			else if (_targetProperty is PropertyInfo)
			{
				(_targetProperty as PropertyInfo).SetValue(target, GetValue(ThemeKey, Type), null);
			}
		}

		private static object GetValue(string key, string ttype)
		{
#if DEBUG
			if (Application.Current.MainWindow == null) return null; // designmode
#endif

			if (ttype.ToLower() == "brush")
			{
				if (!ThemeManager.Inst.CurrentTheme.AllProperties.TryGetValue(key, out var obj)) throw new Exception("ThemeKey not found: " + key);

				if (obj is ColorRef cref) return ColorRefToBrush.Convert(cref);
				if (obj is BrushRef bref) return BrushRefToBrush.Convert(bref);

				throw new Exception($"Cannot convert {obj?.GetType()} to 'brush'");
			}
			throw new Exception($"Unknown targettype: {ttype}");
		}

		/// <summary>
		/// Is an associated target still alive ie not garbage collected
		/// </summary>
		public bool IsTargetAlive
		{
			get
			{
				// for normal elements the _targetObjects.Count will always be 1
				// for templates the Count may be zero if this method is called
				// in the middle of window elaboration after the template has been
				// instantiated but before the elements that use it have been.  In
				// this case return true so that we don't unhook the extension
				// prematurely
				//
				if (_targetObjects.Count == 0)
					return true;

				// otherwise just check whether the referenced target(s) are alive
				return _targetObjects.Any(reference => reference.IsAlive);
			}
		}
	}
}
