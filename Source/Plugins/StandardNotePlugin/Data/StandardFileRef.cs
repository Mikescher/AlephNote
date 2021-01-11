using System;

namespace AlephNote.Plugins.StandardNote
{
    public class StandardFileRef 
    { 
        public readonly Guid UUID;
        public readonly string Type;

        public StandardFileRef(Guid uUID, string type)
        {
            UUID = uUID;
            Type = type;
        }
    }
}
