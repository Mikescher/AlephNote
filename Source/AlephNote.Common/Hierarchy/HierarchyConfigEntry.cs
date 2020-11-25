using AlephNote.Common.Util;
using AlephNote.PluginInterface.AppContext;
using AlephNote.PluginInterface.Util;
using MSHC.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Common.Hierarchy
{
    public class HierarchyConfigEntry
    {
        public string Name = string.Empty;
        public bool Expanded = true;
        public DirectoryPath Path = DirectoryPath.Root();
        public List<HierarchyConfigEntry> Children = new List<HierarchyConfigEntry>();

        public HierarchyConfigEntry(string name, bool expanded, DirectoryPath path, List<HierarchyConfigEntry> childs)
        {
            Name      = name;
            Expanded  = expanded;
            Path      = path;
            Children  = childs;
        }

        public XElement Serialize()
        {
            var xelem = new XElement("dir",
                new XAttribute("name", Name),
                new XAttribute("path", Path.StrSerialize()),
                new XAttribute("expanded", Expanded));

            foreach (var child in Children) xelem.Add(child.Serialize());

            return xelem;
        }

        public static HierarchyConfigEntry Deserialize(XElement xelem)
        {
            if (xelem.Name != "dir") throw new Exception("missing <dir> element");

            return new HierarchyConfigEntry
            (
                xelem.Attribute("name").Value,
                XElementExtensions.ParseBool(xelem.Attribute("expanded").Value),
                DirectoryPath.StrDeserialize(xelem.Attribute("path").Value),
                xelem.Elements().Select(Deserialize).ToList()
            );
        }

        public void ApplyTo(IReadonlyAlephSettings settings, HierarchicalWrapper_Folder dst)
        {
            if (!dst.GetInternalPath().Equals(Path))
            {
                LoggerSingleton.Inst.Warn("HierarchyConfigCache", $"Path mismatch in ApplyTo: '{Path}' <> '{dst.GetInternalPath()}'");
                return;
            }

            if (settings.RememberHierarchyExpandedState) dst.IsExpanded = Expanded; else dst.IsExpanded = true;

            var counter = 1;
            foreach (var subthis in Children)
            {
                var subdst = dst.SubFolder.FirstOrDefault(p => p.Header == subthis.Name);
                if (subdst == null) continue;

                if (!settings.SortHierarchyFoldersByName) subdst.CustomOrder = counter * 100;
                subthis.ApplyTo(settings, subdst);

                counter++;
            }

            // (new) folders not in hierarchy cache
            foreach (var subdst in dst.SubFolder.Where(subdst => !Children.Any(subthis => subdst.Header == subthis.Name)).OrderBy(p => p.Header))
            {
                if (!settings.SortHierarchyFoldersByName) subdst.CustomOrder = counter * 100;
                subdst.IsExpanded = true;
                counter++;
            }
        }
    }
}
