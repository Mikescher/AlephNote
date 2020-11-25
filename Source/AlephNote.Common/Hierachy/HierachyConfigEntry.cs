using AlephNote.Common.Util;
using AlephNote.PluginInterface.AppContext;
using AlephNote.PluginInterface.Util;
using MSHC.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Common.Hierachy
{
    public class HierachyConfigEntry
    {
        public string Name = string.Empty;
        public bool Expanded = true;
        public DirectoryPath Path = DirectoryPath.Root();
        public List<HierachyConfigEntry> Children = new List<HierachyConfigEntry>();

        public HierachyConfigEntry(string name, bool expanded, DirectoryPath path, List<HierachyConfigEntry> childs)
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

        public static HierachyConfigEntry Deserialize(XElement xelem)
        {
            if (xelem.Name != "dir") throw new Exception("missing <dir> element");

            return new HierachyConfigEntry
            (
                xelem.Attribute("name").Value,
                XElementExtensions.ParseBool(xelem.Attribute("expanded").Value),
                DirectoryPath.StrDeserialize(xelem.Attribute("path").Value),
                xelem.Elements().Select(Deserialize).ToList()
            );
        }

        public void ApplyTo(IReadonlyAlephSettings settings, HierachicalWrapper_Folder dst)
        {
            if (!dst.GetInternalPath().Equals(Path))
            {
                LoggerSingleton.Inst.Warn("HierachyConfigCache", $"Path mismatch in ApplyTo: '{Path}' <> '{dst.GetInternalPath()}'");
                return;
            }

            if (settings.RememberHierachyExpandedState) dst.IsExpanded = Expanded;

            var counter = 1;
            foreach (var subthis in Children)
            {
                var subdst = dst.SubFolder.FirstOrDefault(p => p.Header == subthis.Name);
                if (subdst == null) continue;

                if (!settings.SortHierachyFoldersByName) subdst.CustomOrder = counter * 100;
                subthis.ApplyTo(settings, subdst);

                counter++;
            }

            // (new) folders not in hierachy cache
            foreach (var subdst in dst.SubFolder.Where(subdst => !Children.Any(subthis => subdst.Header == subthis.Name)).OrderBy(p => p.Header))
            {
                if (!settings.SortHierachyFoldersByName) subdst.CustomOrder = counter * 100;
                subdst.IsExpanded = true;
                counter++;
            }
        }
    }
}
