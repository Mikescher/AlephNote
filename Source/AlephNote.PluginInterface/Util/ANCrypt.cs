using Konscious.Security.Cryptography;

namespace AlephNote.PluginInterface.Util
{
    public static class ANCrypt
    {
        public static byte[] Argon2(byte[] password, byte[] salt, int iterations, int memory, int outputLength)
        {
            var argon2 = new Argon2id(password)
            {
                DegreeOfParallelism = 1,
                MemorySize = memory,
                Iterations = iterations,
                Salt = salt,
            };
            
            return argon2.GetBytes(outputLength);
        }
    }
}
