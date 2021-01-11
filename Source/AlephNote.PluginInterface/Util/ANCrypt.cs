using Konscious.Security.Cryptography;
using Sodium;

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

        public static byte[] XChaCha20Decrypt(byte[] ciphertext, byte[] nonce, byte[] key, byte[] assocData)
        {
            return SecretAeadXChaCha20Poly1305.Decrypt(ciphertext, nonce, key, assocData);
        }

        public static byte[] XChaCha20Encrypt(byte[] plaintext, byte[] nonce, byte[] key, byte[] assocData)
        {
            return SecretAeadXChaCha20Poly1305.Encrypt(plaintext, nonce, key, assocData);
        }
    }
}
