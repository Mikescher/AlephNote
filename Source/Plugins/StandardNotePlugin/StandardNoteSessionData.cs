using AlephNote.PluginInterface.Util;
using System;
using System.Xml.Linq;

namespace AlephNote.Plugins.StandardNote
{
    public class StandardNoteSessionData
    {
        public string Version;

        public string Token;
        public string RefreshToken;

        public DateTimeOffset AccessExpiration;
        public DateTimeOffset RefreshExpiration;

        public string Identifier;
        public string PasswordNonce;
        public DateTimeOffset ParamsCreated;

        public string AccountEmail;
        public Guid AccountUUID;

        public byte[] RootKey_MasterKey;
        public byte[] RootKey_ServerPassword;
        public byte[] RootKey_MasterAuthKey;

        public static XElement Serialize(string key, StandardNoteSessionData data)
        {
            if (data == null) return new XElement(key, new XAttribute("null", true));

            var r = new XElement(key);

            r.Add(new XElement("Token",             data.Token));
            r.Add(new XElement("RefreshToken",      data.RefreshToken));

            r.Add(new XElement("AccessExpiration",  XHelper.ToString(data.AccessExpiration)));
            r.Add(new XElement("RefreshExpiration", XHelper.ToString(data.RefreshExpiration)));

            r.Add(new XElement("Identifier",        data.Identifier));
            r.Add(new XElement("PasswordNonce",     data.PasswordNonce));
            r.Add(new XElement("ParamsCreated",     data.ParamsCreated));

            r.Add(new XElement("AccountEmail",      data.AccountEmail));
            r.Add(new XElement("AccountUUID",       data.AccountUUID));

            r.Add(new XElement("RootKey", 
                new XElement("ServerPassword", data.RootKey_ServerPassword),
                new XElement("MasterKey", data.RootKey_MasterKey)));

            r.Add(new XElement("Version", data.Version));

            return r;
        }

        public static StandardNoteSessionData Deserialize(XElement elem)
        {
            if (elem == null) return null;
            if (elem.Attribute("null")?.Value?.ToLower() == "true") return null;

            var sessiondata = new StandardNoteSessionData();

            sessiondata.Token                  = XHelper.GetChildValueString(elem, "Token");
            sessiondata.RefreshToken           = XHelper.GetChildValueString(elem, "RefreshToken");

            sessiondata.AccessExpiration       = XHelper.GetChildValueDateTimeOffset(elem, "AccessExpiration");
            sessiondata.RefreshExpiration      = XHelper.GetChildValueDateTimeOffset(elem, "RefreshExpiration");

            sessiondata.Identifier             = XHelper.GetChildValueString(elem, "Identifier");
            sessiondata.PasswordNonce          = XHelper.GetChildValueString(elem, "PasswordNonce");
            sessiondata.ParamsCreated          = XHelper.GetChildValueDateTimeOffset(elem, "ParamsCreated");

            sessiondata.AccountEmail           = XHelper.GetChildValueString(elem, "AccountEmail");
            sessiondata.AccountUUID            = XHelper.GetChildValueGUID(elem, "AccountUUID");

            sessiondata.RootKey_MasterKey      = XHelper.GetChildValueString(XHelper.GetChildOrThrow(elem, "RootKey"), "MasterKey");
            sessiondata.RootKey_ServerPassword = XHelper.GetChildValueString(XHelper.GetChildOrThrow(elem, "RootKey"), "ServerPassword");

            sessiondata.Version                = XHelper.GetChildValueString(elem, "Version");

            return sessiondata;
        }
    }
}
