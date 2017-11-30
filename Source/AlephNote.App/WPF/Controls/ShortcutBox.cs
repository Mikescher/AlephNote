using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Controls
{
	public class ShortcutBox : TextBox
	{
		private static readonly Key[] SPECIAL_KEYS =
		{
			Key.LeftShift,
			Key.RightShift,
			Key.LeftAlt,
			Key.RightAlt,
			Key.LeftCtrl,
			Key.RightCtrl,
			Key.None,
			Key.LWin,
			Key.RWin,
		};

		public static readonly DependencyProperty ShortcutKeyProperty =
			DependencyProperty.Register(
				"ShortcutKey",
				typeof(AlephKey),
				typeof(ShortcutBox),
				new FrameworkPropertyMetadata(AlephKey.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, a) => ((ShortcutBox)o).OnChanged(a)));

		public AlephKey ShortcutKey
		{
			get { return (AlephKey)GetValue(ShortcutKeyProperty); }
			set { SetValue(ShortcutKeyProperty, value); }
		}

		public static readonly DependencyProperty ShortcutModifiersProperty =
			DependencyProperty.Register(
				"ShortcutModifiers",
				typeof(AlephModifierKeys),
				typeof(ShortcutBox),
				new FrameworkPropertyMetadata(AlephModifierKeys.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, a) => ((ShortcutBox)o).OnChanged(a)));

		public AlephModifierKeys ShortcutModifiers
		{
			get { return (AlephModifierKeys)GetValue(ShortcutModifiersProperty); }
			set { SetValue(ShortcutModifiersProperty, value); }
		}

		public ShortcutBox()
		{
			PreviewKeyDown += OnPreviewKeyDown;
		}

		private void OnPreviewKeyDown(object me, KeyEventArgs e)
		{
			if (SPECIAL_KEYS.Contains(e.Key))
			{
				e.Handled = true;
				return;
			}

			ShortcutKey = (AlephKey)e.Key;

			ShortcutModifiers = (AlephModifierKeys)e.KeyboardDevice.Modifiers;

			e.Handled = true;
		}

		private void OnChanged(DependencyPropertyChangedEventArgs args)
		{
			Text = GetGestureString();
		}

		private string GetGestureString()
		{
			if (ShortcutKey == AlephKey.None) return "None";
			if (ShortcutModifiers == AlephModifierKeys.None) return ShortcutKey.ToString();

			List<string> elements = new List<string>(4);
			if (ShortcutModifiers.HasFlag(AlephModifierKeys.Control)) elements.Add("Ctrl");
			if (ShortcutModifiers.HasFlag(AlephModifierKeys.Alt)) elements.Add("Alt");
			if (ShortcutModifiers.HasFlag(AlephModifierKeys.Shift)) elements.Add("Shift");
			if (ShortcutModifiers.HasFlag(AlephModifierKeys.Windows)) elements.Add("Win");
			elements.Add(ShortcutKey.ToString());

			return string.Join(" + ", elements);
		}
	}
}
