using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.PluginInterface.Util
{
	public sealed class DirectoryPath
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
	}
}
