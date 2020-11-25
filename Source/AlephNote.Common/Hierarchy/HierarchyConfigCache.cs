using AlephNote.Common.Util;
using AlephNote.PluginInterface.Util;
using MSHC.Serialization;
using MSHC.Util.Threads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AlephNote.Common.Hierarchy
{
    public class HierarchyConfigCache
    {
        private readonly string _filepath;

        private readonly object _masterLock = new object();

		private string _xml;
		private readonly Dictionary<Guid, HierarchyConfigData> _data = new Dictionary<Guid, HierarchyConfigData>();

        private readonly DelayedCombiningInvoker invSave;

        private HierarchyConfigCache(string fp, string xml)
        {
            _filepath = fp;
			_xml      = xml;

			invSave = DelayedCombiningInvoker.Create(SaveDirect, 15 * 1000, 2 * 60 * 1000);
        }

		public static HierarchyConfigCache LoadFromFile(string path)
		{

			try
			{
				if (!File.Exists(path))
				{
					LoggerSingleton.Inst.Trace("HierarchyConfigCache", $"Load '{path}' (file not found)");
					return new HierarchyConfigCache(path, string.Empty);
				}

				var xml = File.ReadAllText(path);

				LoggerSingleton.Inst.Trace("HierarchyConfigCache", $"Load '{path}'", xml);

				var hcc = new HierarchyConfigCache(path, xml);
				
				var xdoc = XDocument.Parse(xml);
				if (xdoc.Root.Name != "cache") throw new Exception("Missing <cache> element");

				var data = xdoc.Root.Element("data");
				if (data == null) throw new Exception("Missing <data> element");

                foreach (var acc in data.Elements("account"))
				{
					var d = HierarchyConfigData.Deserialize(acc);
					hcc._data[d.Item2] = d.Item1;
				}

				return hcc;
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.Error("HierarchyConfigCache", $"Could not load HierarchyConfig from file '{path}'", e);
				var hce = new HierarchyConfigCache(path, string.Empty);
				hce.ForceSaveNow();
				return hce;
			}
		}

		public void ForceSaveNow()
		{
			LoggerSingleton.Inst.Trace("HierarchyConfigCache", $"Force Save");

			invSave.CancelPendingRequests();
			SaveDirect();
		}

		public void SaveIfDirty()
		{
			if (invSave.HasPendingRequests()) ForceSaveNow();
		}

		private void SaveDirect()
		{
			LoggerSingleton.Inst.Trace("HierarchyConfigCache", $"Execute Save");

			lock (_masterLock)
			{
				try
				{
					var root = new XElement("cache");

					var data = new XElement("data");
                    foreach (var dat in _data)
                    {
						data.Add(dat.Value.Serialize(dat.Key));
					}
					root.Add(data);

					var xmlcontent = XHelper.ConvertToStringFormatted(new XDocument(root));

					if (xmlcontent == _xml)
					{
						LoggerSingleton.Inst.Trace("HierarchyConfigCache", $"Not Saved (no xml diff)");
						return;
					}

					File.WriteAllText(_filepath, xmlcontent, Encoding.UTF8);
					_xml = xmlcontent;

					LoggerSingleton.Inst.Trace("HierarchyConfigCache", $"Saved", xmlcontent);
				}
				catch (Exception e)
				{
					LoggerSingleton.Inst.Error("HierarchyConfigCache", $"Could not save HierarchyConfig to file '{_filepath}'", e);
				}
			}
		}

        public void UpdateAndRequestSave(Guid account, HierarchicalWrapper_Folder folders, DirectoryPath selectedFolderPath, string selectedNote)
        {
			LoggerSingleton.Inst.Trace("HierarchyConfigCache", $"Request Save (full)");

            var dat = new HierarchyConfigData
            {
                SelectedFolder = selectedFolderPath,
                SelectedNote   = selectedNote,
                Entry          = folders.ToHCEntry()
            };

            lock (_masterLock)
			{
				_data[account] = dat;
			}
			invSave.Request();
		}

		public void UpdateAndRequestSave(Guid account, DirectoryPath selectedFolderPath, string selectedNote)
		{
			LoggerSingleton.Inst.Trace("HierarchyConfigCache", $"Request Save (selection)");

			lock (_masterLock)
			{
				if (!_data.ContainsKey(account)) new HierarchyConfigData();

				_data[account].SelectedNote   = selectedNote;
				_data[account].SelectedFolder = selectedFolderPath;

			}
			invSave.Request();
		}

		public HierarchyConfigData Get(Guid id)
		{
			lock (_masterLock)
			{
				if (_data.ContainsKey(id)) return _data[id];
				return _data[id] = new HierarchyConfigData();
			}
		}
    }
}
