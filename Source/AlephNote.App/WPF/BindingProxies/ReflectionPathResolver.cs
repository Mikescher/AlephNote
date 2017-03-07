using System;
using System.Linq;
using System.Reflection;

namespace AlephNote.WPF.BindingProxies
{
	public static class ReflectionPathResolver
	{
		public static Tuple<object, EventInfo> GetEvent(object o, string path)
		{
			var nodes = path.Split('.');

			if (nodes.Length == 0) throw new ArgumentException("path");

			foreach (var node in nodes.Take(nodes.Length - 1))
			{
				o = GetPropertyOrField(o, node);
			}

			return Tuple.Create(o, o.GetType().GetEvent(nodes.Last()));
		}

		public static Tuple<object, PropertyInfo> GetProperty(object o, string path)
		{
			var nodes = path.Split('.');

			if (nodes.Length == 0) throw new ArgumentException("path");

			foreach (var node in nodes.Take(nodes.Length - 1))
			{
				o = GetPropertyOrField(o, node);
			}

			return Tuple.Create(o, o.GetType().GetProperty(nodes.Last()));
		}

		public static Tuple<object, FieldInfo> GetField(object o, string path)
		{
			var nodes = path.Split('.');

			if (nodes.Length == 0) throw new ArgumentException("path");

			foreach (var node in nodes.Take(nodes.Length - 1))
			{
				o = GetPropertyOrField(o, node);
			}

			return Tuple.Create(o, o.GetType().GetField(nodes.Last()));
		}

		public static object GetPropertyOrField(object o, string name)
		{
			var pi = o.GetType().GetProperty(name);
			if (pi != null) return pi.GetValue(o, null);

			var fi = o.GetType().GetField(name);
			if (fi != null) return fi.GetValue(o);

			throw new ArgumentException("path");
		}
	}
}
