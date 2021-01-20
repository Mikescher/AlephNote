using AlephNote.PluginInterface.Util;
using MSHC.Lang.Collections;
using MSHC.Math.Encryption;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

namespace AlephNote.Plugins.StandardNote
{
    [DebuggerDisplay("ItemsKey (( {UUID} ))")]
    public class StandardFileItemsKey : IEquatable<StandardFileItemsKey>
    {
        public readonly Guid UUID;
		public readonly string Version;

		public readonly DateTimeOffset CreationDate;     // raw creation date from SN API
        public readonly DateTimeOffset ModificationDate; // raw modification date from SN API

        public readonly byte[] Key;
        public readonly byte[] AuthKey;
		public readonly bool IsDefault;

		public readonly string RawAppData;

		public StandardFileItemsKey(Guid uuid, string version, DateTimeOffset creationDate, DateTimeOffset modificationDate, byte[] key, byte[] authkey, bool isdefault, string appdata)
        {
            UUID             = uuid;
			Version          = version;
            CreationDate     = creationDate;
            ModificationDate = modificationDate;
			Key              = key;
			AuthKey          = authkey;
			IsDefault        = isdefault;
			RawAppData       = appdata;
		}

        public XElement Serialize()
		{
			var x = new XElement("itemskey",
				new XAttribute("ID", UUID.ToString("P")),
				new XAttribute("Version", Version),
				new XAttribute("AuthKey", EncodingConverter.ByteToHexBitFiddleUppercase(AuthKey ?? new byte[0])),
				new XAttribute("Default", IsDefault),
				new XAttribute("CreationDate", CreationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture)),
				new XAttribute("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture)),
				new XAttribute("AppData", RawAppData),
				EncodingConverter.ByteToHexBitFiddleUppercase(Key));

			return x;
		}

		public static StandardFileItemsKey Deserialize(XElement e)
		{
			var id      = XHelper.GetAttributeGuid(e, "ID");
			var version = XHelper.GetAttributeString(e, "Version");
			var defKey  = XHelper.GetAttributeBool(e, "Default");
			var cdate   = XHelper.GetAttributeDateTimeOffsetOrDefault(e, "CreationDate", DateTimeOffset.MinValue);
			var mdate   = XHelper.GetAttributeDateTimeOffsetOrDefault(e, "ModificationDate", DateTimeOffset.Now);
			var appdata = XHelper.GetAttributeString(e, "AppData");
			var key     = EncodingConverter.StringToByteArrayCaseInsensitive(e.Value);
			var authkey = EncodingConverter.StringToByteArrayCaseInsensitive(XHelper.GetAttributeStringOrDefault(e, "AuthKey", ""));

			if (authkey.Length == 0) authkey = null;

			return new StandardFileItemsKey(id, version, cdate, mdate, key, authkey, defKey, appdata);
		}

		public override int GetHashCode()
		{
			return UUID.GetHashCode() * 1523 + Key.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardFileTag);
		}

		public bool Equals(StandardFileItemsKey other)
		{
			if (other == null) return false;
			return 
				UUID == other.UUID &&
				CreationDate == other.CreationDate &&
				ModificationDate == other.ModificationDate &&
				Key.ListEquals(other.Key, (a, b) => a == b);
		}

		public static bool operator ==(StandardFileItemsKey left, StandardFileItemsKey right) => left?.Equals(right) ?? ReferenceEquals(right, null);

		public static bool operator !=(StandardFileItemsKey left, StandardFileItemsKey right) => !(left?.Equals(right) ?? ReferenceEquals(right, null));

	}
}
