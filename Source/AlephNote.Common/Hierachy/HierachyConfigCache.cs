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

namespace AlephNote.Common.Hierachy
{
    public class HierachyConfigCache
    {
        private readonly string _filepath;

        private readonly object _masterLock = new object();

		private string _xml;
		private readonly Dictionary<Guid, HierachyConfigData> _data = new Dictionary<Guid, HierachyConfigData>();

        private readonly DelayedCombiningInvoker invSave;

        private HierachyConfigCache(string fp, string xml)
        {
            _filepath = fp;
			_xml      = xml;

			invSave = DelayedCombiningInvoker.Create(SaveDirect, 15 * 1000, 2 * 60 * 1000);
        }

		public static HierachyConfigCache LoadFromFile(string path)
		{

			try
			{
				if (!File.Exists(path))
				{
					LoggerSingleton.Inst.Trace("HierachyConfigCache", $"Load '{path}' (file not found)");
					return new HierachyConfigCache(path, string.Empty);
				}

				var xml = File.ReadAllText(path);

				LoggerSingleton.Inst.Trace("HierachyConfigCache", $"Load '{path}'", xml);

				var hcc = new HierachyConfigCache(path, xml);
				
				var xdoc = XDocument.Parse(xml);
				if (xdoc.Root.Name != "cache") throw new Exception("Missing <cache> element");

				var data = xdoc.Root.Element("data");
				if (data == null) throw new Exception("Missing <data> element");

                foreach (var acc in data.Elements("account"))
				{
					var d = HierachyConfigData.Deserialize(acc);
					hcc._data[d.Item2] = d.Item1;
				}

				return hcc;
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.Error("HierachyConfigCache", $"Could not load HierachyConfig from file '{path}'", e);
				var hce = new HierachyConfigCache(path, string.Empty);
				hce.ForceSaveNow();
				return hce;
			}
		}

		public void ForceSaveNow()
		{
			LoggerSingleton.Inst.Trace("HierachyConfigCache", $"Force Save");

			invSave.CancelPendingRequests();
			SaveDirect();
		}

		public void SaveIfDirty()
		{
			if (invSave.HasPendingRequests()) ForceSaveNow();
		}

		private void SaveDirect()
		{
			LoggerSingleton.Inst.Trace("HierachyConfigCache", $"Execute Save");

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
						LoggerSingleton.Inst.Trace("HierachyConfigCache", $"Not Saved (no xml diff)");
						return;
					}

					File.WriteAllText(_filepath, xmlcontent, Encoding.UTF8);
					_xml = xmlcontent;

					LoggerSingleton.Inst.Trace("HierachyConfigCache", $"Saved", xmlcontent);
				}
				catch (Exception e)
				{
					LoggerSingleton.Inst.Error("HierachyConfigCache", $"Could not save HierachyConfig to file '{_filepath}'", e);
				}
			}
		}

        public void UpdateAndRequestSave(Guid account, HierachicalWrapper_Folder folders, DirectoryPath selectedFolderPath, string selectedNote)
        {
			LoggerSingleton.Inst.Trace("HierachyConfigCache", $"Request Save (full)");

            var dat = new HierachyConfigData
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
			LoggerSingleton.Inst.Trace("HierachyConfigCache", $"Request Save (selection)");

			lock (_masterLock)
			{
				if (!_data.ContainsKey(account)) new HierachyConfigData();

				_data[account].SelectedNote   = selectedNote;
				_data[account].SelectedFolder = selectedFolderPath;

			}
			invSave.Request();
		}

		public HierachyConfigData Get(Guid id)
		{
			lock (_masterLock)
			{
				if (_data.ContainsKey(id)) return _data[id];
				return _data[id] = new HierachyConfigData();
			}
		}
    }
}
