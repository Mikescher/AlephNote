using AlephNote.PluginInterface.AppContext;
using AlephNote.PluginInterface.Util;
using MSHC.Serialization;
using System;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Common.Hierachy
{
    public class HierachyConfigData
    {
        public DirectoryPath SelectedFolder = null;
        public string SelectedNote          = null;
        public HierachyConfigEntry Entry    = null;

        public HierachyConfigData()
        {

        }

        public XElement Serialize(Guid key)
        {
            var acc = new XElement("account", new XAttribute("id", key.ToString("B")));
            {
                var dirs = new XElement("directories", new XAttribute("null", Entry == null));
                if (Entry != null) dirs.Add(Entry.Serialize());
                acc.Add(dirs);
            }
            {
                var seldir = new XElement("selectednote", new XAttribute("null", SelectedNote == null), new XText(SelectedNote ?? ""));
                acc.Add(seldir);
            }
            {
                var selpath = new XElement("selectedpath", new XAttribute("null", SelectedNote == null));
                if (SelectedFolder != null)
                {
                    selpath.SetAttributeValue("name", SelectedFolder.StrSerialize());
                    foreach (var e in SelectedFolder.Serialize()) selpath.Add(e);
                }
                acc.Add(selpath);
            }

            return acc;
        }

        public static Tuple<HierachyConfigData, Guid> Deserialize(XElement xelem)
        {
            var hcd = new HierachyConfigData();

            var guid = Guid.Parse(xelem.Attribute("id").Value);

            {
                var dir = xelem.Element("directories");
                if (dir == null) throw new Exception("Missing <directories> element");

                if (XElementExtensions.ParseBool(dir.Attribute("null").Value))
                {
                    hcd = null;
                }
                else
                {
                    var dat = HierachyConfigEntry.Deserialize(dir.Elements().Single());
                    hcd.Entry = dat;
                }
            }
            {
                var dir = xelem.Element("selectednote");
                if (dir == null) throw new Exception("Missing <selectednote> element");

                if (XElementExtensions.ParseBool(dir.Attribute("null").Value))
                {
                    hcd.SelectedNote = null;
                }
                else
                {
                    hcd.SelectedNote = dir.Value;
                }
            }
            {
                var dir = xelem.Element("selectedpath");
                if (dir == null) throw new Exception("Missing <selectedpath> element");

                if (XElementExtensions.ParseBool(dir.Attribute("null").Value))
                {
                    hcd.SelectedFolder = null;
                }
                else
                {
                    hcd.SelectedFolder = DirectoryPath.Deserialize(dir.Elements());
                }
            }

            return Tuple.Create(hcd, guid);
        }

        public void ApplyTo(IReadonlyAlephSettings settings, HierachicalWrapper_Folder dst)
        {
            Entry?.ApplyTo(settings, dst);
        }
    }
}
