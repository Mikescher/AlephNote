using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.PluginInterface.Util
{
	public sealed class DirectoryPath : IEquatable<DirectoryPath>
	{
		private static readonly DirectoryPath ROOT = new DirectoryPath(Enumerable.Empty<string>());

		private readonly List<string> _path;

		private DirectoryPath(IEnumerable<string> path)
		{
			_path = path.ToList();
		}

		public static DirectoryPath Root() => ROOT;
		public static DirectoryPath Create(IEnumerable<string> path) => new DirectoryPath(path);

		public IEnumerable<string> Enumerate() => _path;

		public XElement[] Serialize()
		{
			var result = new List<XElement>();
			foreach (var elem in _path) result.Add(new XElement("PathComponent", new XAttribute("Name", elem)));
			return result.ToArray();
		}

		public static DirectoryPath Deserialize(IEnumerable<XElement> childs)
		{
			return new DirectoryPath(childs.Select(c => c.Attribute("Name")).Where(a => a != null).Select(a => a.Value));
		}

		public override string ToString()
		{
			return $"[{string.Join("/", _path)}]";
		}

		public bool IsRoot()
		{
			return _path.Count == 0;
		}

		public DirectoryPath SubDir(string comp)
		{
			return Create(Enumerate().Concat(new[] {comp}));
		}

		public bool EqualsIgnoreCase(DirectoryPath other)
		{
			if (other == null) return false;

			if (_path.Count != other._path.Count) return false;

			for (int i = 0; i < _path.Count; i++)
			{
				if (_path[i].ToLower() != other._path[i].ToLower()) return false;
			}

			return true;
		}

		public bool Equals(DirectoryPath other)
		{
			return EqualsIgnoreCase(other);
		}

		public override bool Equals(object obj)
		{
			if (obj is DirectoryPath d) return Equals(d); else return false;
		}

		public override int GetHashCode()
		{
			return string.Join("\r\n", _path).GetHashCode();
		}
	}
}
