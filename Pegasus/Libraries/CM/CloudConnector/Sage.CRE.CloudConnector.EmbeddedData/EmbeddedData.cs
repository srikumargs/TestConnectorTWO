using System;
using System.IO;
using System.Reflection;
using Sage.CRE.LinkedSource;

namespace Sage.CRE.CloudConnector
{
    /// <summary>
    /// 
    /// </summary>
    public static class EmbeddedData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamToReadFrom"></param>
        /// <param name="readHandler"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Object Read(Stream streamToReadFrom, Func<MemoryStream, Action<String>, Object> dataReadCallback, Action<String> traceWriteLine)
        {
            Object result = null;

            if (Convert.ToBase64String(Assembly.GetCallingAssembly().GetName().GetPublicKeyToken()) == Convert.ToBase64String(GetPublicKeyTokenBytes()))
            {
                using (var decryptedMemoryStream = Cryptography.DecryptStream(streamToReadFrom, CryptoAlgorithm.DES, GetEncryptionKeyBytes(), GetEncryptionIVBytes()))
                {
                    if (dataReadCallback != null)
                    {
                        result = dataReadCallback(decryptedMemoryStream, traceWriteLine);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="streamToWriteTo"></param>
        public static void Write(Stream memoryStream, Stream streamToWriteTo)
        {
            if (Convert.ToBase64String(Assembly.GetCallingAssembly().GetName().GetPublicKeyToken()) == Convert.ToBase64String(GetPublicKeyTokenBytes()))
            {
                using (var encryptedMemoryStream = Cryptography.EncryptStream(memoryStream, CryptoAlgorithm.DES, GetEncryptionKeyBytes(), GetEncryptionIVBytes()))
                {
                    encryptedMemoryStream.WriteTo(streamToWriteTo);
                }
            }
        }

        private static Byte[] GetPublicKeyTokenBytes()
        {
            if (_publicKeyToken == null)
            {
                _publicKeyToken = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken();
            }

            return _publicKeyToken;
        }

        private static Byte[] GetEncryptionKeyBytes()
        {
            if (_keyBytes == null)
            {
                if (_ivBytes == null)
                {
                    // if this is the first time we have ever requested the bytes, them mutate
                    MutateBytes();
                }

                Byte[] publicKeyToken = GetPublicKeyTokenBytes();
                Byte[] bytes = GetBytes();
                _keyBytes = new byte[] { bytes[publicKeyToken[0]], bytes[publicKeyToken[2]], bytes[publicKeyToken[4]], bytes[publicKeyToken[6]], bytes[publicKeyToken[1]], bytes[publicKeyToken[3]], bytes[publicKeyToken[5]], bytes[publicKeyToken[7]] };
            }

            return _keyBytes;
        }

        private static Byte[] GetEncryptionIVBytes()
        {
            if (_ivBytes == null)
            {
                if (_keyBytes == null)
                {
                    // if this is the first time we have ever requested the bytes, them mutate
                    MutateBytes();
                }

                Byte[] publicKeyToken = GetPublicKeyTokenBytes();
                Byte[] bytes = GetBytes();
                _ivBytes = new byte[] { bytes[publicKeyToken[7]], bytes[publicKeyToken[5]], bytes[publicKeyToken[3]], bytes[publicKeyToken[1]], bytes[publicKeyToken[6]], bytes[publicKeyToken[4]], bytes[publicKeyToken[2]], bytes[publicKeyToken[0]] };
            }

            return _ivBytes;
        }

        private static Byte[] GetBytes()
        {
            if (_bytes == null)
            {
                _bytes = new Byte[]
                {
                    0x22, 0xCD, 0xA1, 0xC4, 0xDE, 0xEF, 0x47, 0x82,
                    0x6F, 0x06, 0x17, 0x48, 0x06, 0x2F, 0x4D, 0x0D,
                    0x65, 0xB2, 0x30, 0xA9, 0xDE, 0xBD, 0x45, 0xC3,
                    0x06, 0x0F, 0xA6, 0xBA, 0x4A, 0x06, 0x4D, 0xF6,
                    0xD6, 0x2B, 0x39, 0x1C, 0x8D, 0x83, 0x46, 0x6D,
                    0xC7, 0x8B, 0x63, 0x07, 0x0D, 0x4C, 0x46, 0xE8,
                    0x11, 0xD5, 0x01, 0x70, 0x06, 0x81, 0x47, 0x8B,
                    0xC6, 0x88, 0x78, 0xE0, 0x58, 0xC2, 0x44, 0xD3,
                    0x9C, 0x82, 0xB8, 0xD6, 0xE0, 0x2C, 0x43, 0xBD,
                    0x6D, 0x5D, 0x2C, 0x65, 0x49, 0x3A, 0x45, 0x9E,
                    0x3D, 0x45, 0xEF, 0x86, 0xAC, 0x09, 0x48, 0xE8,
                    0x05, 0xC6, 0x48, 0x04, 0x6C, 0xFB, 0x4B, 0xCD,
                    0x21, 0xDD, 0xDD, 0x21, 0x0B, 0xF4, 0x4E, 0x69,
                    0xBE, 0x78, 0xDC, 0x70, 0x66, 0x27, 0x4E, 0x03,
                    0x49, 0x65, 0xDB, 0x71, 0xA1, 0x40, 0x4B, 0xDC,
                    0xDA, 0xD8, 0x05, 0x7E, 0x20, 0xC0, 0x4A, 0xF1,
                    0xAF, 0xDC, 0xCC, 0xDC, 0x08, 0xF4, 0xB3, 0xCD,
                    0x82, 0xB2, 0xB2, 0xDE, 0x0A, 0xA7, 0x1C, 0xDF,
                    0x88, 0xA0, 0x09, 0xA2, 0x50, 0x1D, 0xD2, 0x01,
                    0x82, 0x9B, 0x40, 0xAC, 0x3A, 0x09, 0x8F, 0x05,
                    0xAA, 0x01, 0x50, 0x04, 0x89, 0x4F, 0xD3, 0xED,
                    0xB6, 0xD4, 0x41, 0x11, 0xD7, 0xD0, 0xB1, 0x44,
                    0x9B, 0xC2, 0x42, 0x24, 0x13, 0x99, 0x17, 0x7C,
                    0xBC, 0x16, 0xC0, 0xA6, 0x5C, 0x3A, 0x3A, 0x18,
                    0x83, 0xE7, 0x89, 0x3E, 0xA2, 0xE8, 0x50, 0xA4,
                    0x9B, 0xEB, 0x00, 0x93, 0x22, 0x53, 0xE5, 0x5A,
                    0x8B, 0x61, 0x4B, 0xB1, 0xFF, 0x82, 0x22, 0xAA,
                    0xBD, 0x9F, 0xF1, 0x04, 0xE5, 0x7B, 0xDC, 0x78,
                    0xB6, 0xDC, 0xE3, 0x21, 0x84, 0x06, 0xCD, 0xD1,
                    0x81, 0xFE, 0x1B, 0xFC, 0xC1, 0xB6, 0xC5, 0x08,
                    0xA1, 0x5C, 0x24, 0x75, 0x27, 0x98, 0x52, 0x3C,
                    0xBE, 0xF0, 0x4B, 0x29, 0x34, 0x97, 0x61, 0x0A
                };
            }

            return _bytes;
        }

        private static void MutateBytes()
        {
            Byte[] bytes = GetBytes();

            // mix it up a little;  this way, even if someone does reflect on the assembly
            // they will have to do a little IL math to figure out the real value of the key
            //
            // there is no significance to this algorithm ... it can be changed, but if it is
            // then the config files must be reencrypted with the new key
            for (int i = 1; i < bytes.Length; i++)
            {
                bytes[i] = (byte)((bytes[i] * bytes[i - 1] * bytes[i - 1]) % 0x7F);
            }
            bytes[0] = (byte)((bytes[0] * bytes[7] * bytes[7]) % 0x7F);
        }

        private static Byte[] _bytes;
        private static Byte[] _keyBytes;
        private static Byte[] _ivBytes;
        private static Byte[] _publicKeyToken;
    }
}
