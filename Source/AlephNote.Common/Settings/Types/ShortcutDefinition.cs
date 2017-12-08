using AlephNote.Common.AlephXMLSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using AlephNote.Common.MVVM;

namespace AlephNote.Common.Settings.Types
{

	public class ShortcutDefinition : ObservableObject, IAlephCustomFlatSerializableField
	{
		public static readonly ShortcutDefinition DEFAULT = new ShortcutDefinition(AlephShortcutScope.None, AlephModifierKeys.None, AlephKey.None);

		private AlephShortcutScope _scope;
		public AlephShortcutScope Scope { get { return _scope; } set { _scope = value; OnPropertyChanged(); } }

		private AlephModifierKeys modifiers;
		public AlephModifierKeys Modifiers { get { return modifiers; } set { modifiers = value; OnPropertyChanged(); } }

		private AlephKey _key;
		public AlephKey Key { get { return _key; } set { _key = value; OnPropertyChanged(); } }

		public ShortcutDefinition(AlephShortcutScope s, AlephModifierKeys m, AlephKey k)
		{
			Scope = s;
			Modifiers = m;
			Key = k;
		}

		public object DeserializeNew(string source)
		{
			if (string.IsNullOrWhiteSpace(source)) return new ShortcutDefinition(AlephShortcutScope.None, AlephModifierKeys.None, AlephKey.None);

			var kk = AlephKey.None;
			var mm = AlephModifierKeys.None;
			var ss = AlephShortcutScope.Window;

			source = source.Trim();
			foreach (var v in Enum.GetValues(typeof(AlephShortcutScope)).Cast<AlephShortcutScope>())
			{
				var estr = $"[{v.ToString().ToLower()}]";
				if (source.ToLower().EndsWith(estr))
				{
					ss = v;
					source = source.Substring(0, source.Length - estr.Length).Trim();
					break;
				}
			}

			foreach (var elem in source.Split('+'))
			{
				if (Enum.TryParse<AlephModifierKeys>(elem.Trim(), true, out var mk))
				{
					mm = mm | mk;
					continue;
				}

				if (Enum.TryParse<AlephKey>(elem.Trim(), true, out var pk))
				{
					kk = pk;
					continue;
				}

				throw new Exception($"Unknown KeyCode: '{elem}'");
			}

			if (kk == AlephKey.None) throw new Exception($"Keycode 'None' is no supported");

			return new ShortcutDefinition(ss, mm, kk);
		}

		public string GetGestureStr()
		{
			if (Key == AlephKey.None) return "";
			if (Modifiers == AlephModifierKeys.None) return Key.ToString();

			List<string> elements = new List<string>(4);
			if (Modifiers.HasFlag(AlephModifierKeys.Control)) elements.Add("Ctrl");
			if (Modifiers.HasFlag(AlephModifierKeys.Alt)) elements.Add("Alt");
			if (Modifiers.HasFlag(AlephModifierKeys.Shift)) elements.Add("Shift");
			if (Modifiers.HasFlag(AlephModifierKeys.Windows)) elements.Add("Win");
			elements.Add(Key.ToString());

			return string.Join("+", elements);
		}

		public object GetTypeStr()
		{
			return "ShortcutDefinition";
		}

		public string Serialize()
		{
			if (Key == AlephKey.None) return "";
			if (Modifiers == AlephModifierKeys.None)
			{
				if (Scope == AlephShortcutScope.Window)
					return Key.ToString();
				else
					return Key.ToString() + " [" + Scope.ToString() + "]";
			}

			List<string> elements = new List<string>(4);

			if (Modifiers.HasFlag(AlephModifierKeys.Control)) elements.Add(AlephModifierKeys.Control.ToString());
			if (Modifiers.HasFlag(AlephModifierKeys.Alt)) elements.Add(AlephModifierKeys.Alt.ToString());
			if (Modifiers.HasFlag(AlephModifierKeys.Shift)) elements.Add(AlephModifierKeys.Shift.ToString());
			if (Modifiers.HasFlag(AlephModifierKeys.Windows)) elements.Add(AlephModifierKeys.Windows.ToString());
			elements.Add(Key.ToString());

			if (Scope == AlephShortcutScope.Window)
				return string.Join(" + ", elements);
			else
				return string.Join(" + ", elements) + " [" + Scope.ToString() + "]";
		}
	}
}
