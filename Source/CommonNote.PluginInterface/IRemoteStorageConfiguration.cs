
using System.Xml.Linq;

namespace CommonNote.PluginInterface
{
	public interface IRemoteStorageConfiguration
	{
		XElement Serialize();
		void Deserialize(XElement input);
	}
}
