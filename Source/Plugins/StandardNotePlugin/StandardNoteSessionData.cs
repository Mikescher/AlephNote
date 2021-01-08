using AlephNote.PluginInterface.Util;
using MSHC.Serialization;
using System;
using System.Xml.Linq;

namespace AlephNote.Plugins.StandardNote
{
    public class StandardNoteSessionData
    {
        public string Version;

        public string Token;
        public string RefreshToken;

        public DateTimeOffset? AccessExpiration;
        public DateTimeOffset? RefreshExpiration;

        public string Identifier;
        public string PasswordNonce;
        public DateTimeOffset? ParamsCreated;

        public string AccountEmail;
        public Guid? AccountUUID;

        public byte[] RootKey_MasterKey;
        public byte[] RootKey_ServerPassword;
        public byte[] RootKey_MasterAuthKey;

        public static XElement Serialize(string key, StandardNoteSessionData data)
        {
            if (data == null) return new XElement(key, new XAttribute("null", true));

            var r = new XElement(key);

            r.Add(XHelper2.TypeString.ToXElem("Token",             data.Token));
            r.Add(XHelper2.TypeString.ToXElem("RefreshToken",      data.RefreshToken));

            r.Add(XHelper2.TypeNullableDateTimeOffset.ToXElem("AccessExpiration",  data.AccessExpiration));
            r.Add(XHelper2.TypeNullableDateTimeOffset.ToXElem("RefreshExpiration", data.RefreshExpiration));

            r.Add(XHelper2.TypeString.ToXElem("Identifier",        data.Identifier));
            r.Add(XHelper2.TypeString.ToXElem("PasswordNonce",     data.PasswordNonce));
            r.Add(XHelper2.TypeNullableDateTimeOffset.ToXElem("ParamsCreated",     data.ParamsCreated));

            r.Add(XHelper2.TypeString.ToXElem("AccountEmail",      data.AccountEmail));
            r.Add(XHelper2.TypeNullableGuid.ToXElem("AccountUUID",       data.AccountUUID));

            r.Add(new XElement("RootKey",
                XHelper2.TypeByteArrayHex.ToXElem("ServerPassword", data.RootKey_ServerPassword),
                XHelper2.TypeByteArrayHex.ToXElem("MasterAuthKey",  data.RootKey_MasterAuthKey),
                XHelper2.TypeByteArrayHex.ToXElem("MasterKey",      data.RootKey_MasterKey)));

            r.Add(XHelper2.TypeString.ToXElem("Version", data.Version));

            return r;
        }

        public static StandardNoteSessionData Deserialize(XElement elem)
        {
            if (elem == null) return null;
            if (elem.Attribute("null")?.Value?.ToLower() == "true") return null;

            var sessiondata = new StandardNoteSessionData();

            sessiondata.Token                  = XHelper2.TypeString.FromOptionalChildXElem(elem, "Token", null);
            sessiondata.RefreshToken           = XHelper2.TypeString.FromOptionalChildXElem(elem, "RefreshToken", null);

            sessiondata.AccessExpiration       = XHelper2.TypeNullableDateTimeOffset.FromOptionalChildXElem(elem, "AccessExpiration", null);
            sessiondata.RefreshExpiration      = XHelper2.TypeNullableDateTimeOffset.FromOptionalChildXElem(elem, "RefreshExpiration", null);

            sessiondata.Identifier             = XHelper2.TypeString.FromOptionalChildXElem(elem, "Identifier", null);
            sessiondata.PasswordNonce          = XHelper2.TypeString.FromOptionalChildXElem(elem, "PasswordNonce", null);
            sessiondata.ParamsCreated          = XHelper2.TypeNullableDateTimeOffset.FromOptionalChildXElem(elem, "ParamsCreated", null);

            sessiondata.AccountEmail           = XHelper2.TypeString.FromOptionalChildXElem(elem, "AccountEmail", null);
            sessiondata.AccountUUID            = XHelper2.TypeNullableGuid.FromOptionalChildXElem(elem, "AccountUUID", null);

            sessiondata.RootKey_MasterKey      = XHelper2.TypeByteArrayHex.FromOptionalChildXElem(XHelper.GetChildOrThrow(elem, "RootKey"), "MasterKey", null);
            sessiondata.RootKey_MasterAuthKey  = XHelper2.TypeByteArrayHex.FromOptionalChildXElem(XHelper.GetChildOrThrow(elem, "RootKey"), "MasterAuthKey", null);
            sessiondata.RootKey_ServerPassword = XHelper2.TypeByteArrayHex.FromOptionalChildXElem(XHelper.GetChildOrThrow(elem, "RootKey"), "ServerPassword", null);

            sessiondata.Version                = XHelper2.TypeString.FromOptionalChildXElem(elem, "Version", null);

            return sessiondata;
        }
    }
}
