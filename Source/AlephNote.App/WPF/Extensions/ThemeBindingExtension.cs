using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Reflection;
using System.Windows;
using AlephNote.Common.Util;
using AlephNote.Common.Themes;
using AlephNote.WPF.Converter;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;

namespace AlephNote.WPF.Extensions
{
	[MarkupExtensionReturnType(typeof(object))]
	[ContentProperty("ThemeKey")]
	public class ThemeBindingExtension : MarkupExtension, IThemeListener
	{
		private class ThemeBindingProxy : INotifyPropertyChanged
		{
			public readonly string Key;
			public readonly string Convert;
			public readonly string ResType;

			public object Value => ThemeBindingExtension.GetValue(Key, Convert, ResType);

			public ThemeBindingProxy(string k, string c, string t) { Key = k; Convert = c; ResType = t; }

			public event PropertyChangedEventHandler PropertyChanged;

			public void TriggerChange()
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
			}
		}

		/// <summary>
		/// Key in theme.xml file
		/// </summary>
		public string ThemeKey { get; set; }

		/// <summary>
		/// optionally convert theme resource
		/// Supported:
		///  - "ToColor"
		/// </summary>
		public string Convert { get; set; } = "";

		/// <summary>
		/// optionally convert theme resource
		/// Supported:
		///  - "Value"
		///  - "ImageSource"
		/// </summary>
		public string ResType { get; set; } = "Value";

		/// <summary>
		/// [TRUE]
		/// Return value direct and update via dependencyObject.SetValue
		/// Is faster than proxy variant but does not work in Styles and Resources
		/// 
		/// [FALSE]
		/// Return Binding to ThemeBindingProxy object and update via INotifyPropertyChanged
		/// Is slower than normal but works everywhere
		/// 
		/// </summary>
		public bool Proxy { get; set; } = true;

		private static readonly Dictionary<string, ThemeBindingProxy> _proxies = new Dictionary<string, ThemeBindingProxy>();
		private readonly List<WeakReference> _targetObjects = new List<WeakReference>();

		private object _targetProperty;

		public ThemeBindingExtension()
		{
#if DEBUG
			if (Application.Current.MainWindow == null) return; // designmode
#endif
			ThemeManager.Inst.RegisterSlave(this);
		}

		public ThemeBindingExtension(string themekey)
		{
			ThemeKey = themekey;

#if DEBUG
			if (Application.Current.MainWindow == null) return; // designmode
#endif

			ThemeManager.Inst.RegisterSlave(this);
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			RegisterTarget(serviceProvider);

			// when used in a template the _targetProperty may be null - in this case return this
			if (_targetProperty == null) return this;

			if (Proxy)
			{
				var p = GetProxy(ThemeKey, Convert, ResType);
				var binding = new Binding("Value") { Source = p };
				return binding.ProvideValue(serviceProvider);
			}
			else
			{
				return GetValue(ThemeKey, Convert, ResType);
			}

		}
		
		protected virtual void RegisterTarget(IServiceProvider serviceProvider)
		{
			var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
			var target = provideValueTarget?.TargetObject;
				
			if (target != null && target.GetType().FullName != "System.Windows.SharedDp")
			{
				_targetProperty = provideValueTarget.TargetProperty;
				_targetObjects.Add(new WeakReference(target));
			}
		}

		public void OnThemeChanged()
		{
			if (Proxy)
			{
				var dictKey = $"('{ThemeKey}';'{Convert}';'{ResType}')";
				if (_proxies.TryGetValue(dictKey, out var v)) v.TriggerChange();
			}
			else
			{
				foreach (var reference in _targetObjects)
				{
					if (reference.IsAlive) UpdateTargetInternal(reference.Target);
				}
			}
		}

		protected virtual void UpdateTargetInternal(object target)
		{
			try
			{
				if (_targetProperty is DependencyProperty dprop)
				{
					(target as DependencyObject)?.SetValue(dprop, GetValue(ThemeKey, Convert, ResType));
				}
				else if (_targetProperty is PropertyInfo propinfo)
				{
					propinfo.SetValue(target, GetValue(ThemeKey, Convert, ResType), null);
				}
			}
			catch (Exception e)
			{
				App.Logger.Error("ThemeBinding", $"UpdateTargetInternal failed for '{ThemeKey}':'{Convert}':'{ResType}'", e);
				App.Logger.ShowExceptionDialog("Update theme failed", e, $"UpdateTargetInternal failed for '{ThemeKey}':'{Convert}':'{ResType}'");
			}
		}

		private static ThemeBindingProxy GetProxy(string key, string convert, string rtype)
		{
			var dictKey = $"('{key}';'{convert}';'{rtype}')";

			if (_proxies.TryGetValue(dictKey, out var proxy)) return proxy;

			proxy = new ThemeBindingProxy(key, convert, rtype);
			_proxies[dictKey] = proxy;
			return proxy;
		}

		private static object GetValue(string key, string converter, string rtype)
		{
#if DEBUG
			if (Application.Current.MainWindow == null) return null; // designmode
#endif

			if (rtype == "Value")
			{
				var obj = ThemeManager.Inst.CurrentThemeSet.Get(key);

				if (string.IsNullOrWhiteSpace(converter))
				{
					if (obj is ColorRef cref) return ColorRefToBrush.Convert(cref);
					if (obj is BrushRef bref) return BrushRefToBrush.Convert(bref);
					if (obj is ThicknessRef tref) return tref.ToWThickness();
					if (obj is CornerRadiusRef rref) return rref.ToCornerRad();

					throw new Exception($"Cannot convert {obj?.GetType()} to 'brush'");
				}
				else if (converter.Equals("ToColor", StringComparison.InvariantCultureIgnoreCase))
				{
					if (obj is ColorRef cref)     return cref.ToWCol();
					if (obj is BrushRef bref)     return bref.GradientSteps.First().Item2.ToWCol();

					throw new Exception($"Cannot convert {obj?.GetType()} with '{converter}'");
				}
				else
				{
					throw new Exception($"Unknown converter '{converter}'");
				}
			}
			else if (rtype == "ImageSource")
			{
				return ThemeManager.Inst.CurrentThemeSet.GetBitmapImageResource(key);
			}
			else
			{
				throw new Exception($"Unknown ResType '{rtype}'");
			}


		}

		/// <summary>
		/// Is an associated target still alive ie not garbage collected
		/// </summary>
		public bool IsTargetAlive
		{
			get
			{
				if (Proxy) return true;

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
