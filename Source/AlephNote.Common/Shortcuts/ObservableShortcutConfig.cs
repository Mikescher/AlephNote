﻿using System.Collections.Generic;
using AlephNote.Common.Settings.Types;
using MSHC.WPF.MVVM;

namespace AlephNote.Common.Shortcuts
{
	public class ObservableShortcutConfig : ObservableObject
	{
		private readonly string _identifier;
		public string Identifier { get { return _identifier; } }

		private readonly string _description;
		public string Description { get { return _description; } }

		private AlephKey _key;
		public AlephKey Key { get { return _key; } set { _key = value; OnPropertyChanged(); OnPropertyChanged("Gesture"); } }

		private AlephModifierKeys _modifiers;
		public AlephModifierKeys Modifiers { get { return _modifiers; } set { _modifiers = value; OnPropertyChanged(); OnPropertyChanged("Gesture"); } }

		private AlephShortcutScope _scope;
		public AlephShortcutScope Scope { get { return _scope; } set { _scope = value; OnPropertyChanged(); } }

		private bool _isConflict;
		public bool IsConflict { get { return _isConflict; } set { if (value == _isConflict) return; _isConflict = value; OnPropertyChanged(); }  }

		public string Gesture => GetGestureString();

		public ObservableShortcutConfig(string id, string d, AlephKey k, AlephModifierKeys m, AlephShortcutScope s)
		{
			_identifier = id;
			_description = d;
			_key = k;
			_modifiers = m;
			_scope = s;
			_isConflict = false;
		}

		private string GetGestureString()
		{
			if (Key == AlephKey.None) return "None";
			if (Modifiers == AlephModifierKeys.None) return Key.ToString();

			List<string> elements = new List<string>(4);
			if (Modifiers.HasFlag(AlephModifierKeys.Control)) elements.Add("Ctrl");
			if (Modifiers.HasFlag(AlephModifierKeys.Alt)) elements.Add("Alt");
			if (Modifiers.HasFlag(AlephModifierKeys.Shift)) elements.Add("Shift");
			if (Modifiers.HasFlag(AlephModifierKeys.Windows)) elements.Add("Win");
			elements.Add(Key.ToString());

			return string.Join("+", elements);
		}
	}
}
