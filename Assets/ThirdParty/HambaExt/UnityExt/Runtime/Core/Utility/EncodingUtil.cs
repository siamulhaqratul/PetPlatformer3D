using System.Text;
using UnityExt.Debugging;

namespace UnityExt
{
    /// <summary>Utility methods for converting between strings and byte arrays using configurable project encodings.</summary>
    public sealed class EncodingUtil
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>Gets bytes for the provided text using the encoding defined in <c>UConfig.Encoding</c>.</summary>
        /// <param name="text">Input text (can be empty, should not be null if encoding expects data).</param>
        /// <returns>Byte array representing the encoded text.</returns>
        public static byte[] GetBytes(string text)
        {
            return GetBytes(text, UConfig.Encoding);
        }
        /// <summary>Gets bytes for the provided text using a specified encoding type.</summary>
        /// <param name="text">Input text to encode.</param>
        /// <param name="encoding">Encoding type to apply.</param>
        /// <returns>Encoded byte array, or null if encoding type is unknown.</returns>
        public static byte[] GetBytes(string text, GameTextEncodingType encoding)
        {
            var enc = ResolveEncoding(encoding);
            return enc.GetBytes(text);
        }
        /// <summary>Decodes a byte array into a string using the encoding defined in <c>UConfig.Encoding</c>.</summary>
        /// <param name="inputBytes">Encoded input bytes.</param>
        /// <returns>Decoded string, or null if encoding type is unknown.</returns>
        public static string GetString(byte[] inputBytes)
        {
            var enc = ResolveEncoding(UConfig.Encoding);
            return enc.GetString(inputBytes);
        }

        // ========================= PRIVATE METHODS =======================

        private static Encoding ResolveEncoding(GameTextEncodingType encoding)
        {
            switch (encoding)
            {
                case GameTextEncodingType.ASCII: return Encoding.ASCII;
                case GameTextEncodingType.BIG_ENDIAN: return Encoding.BigEndianUnicode;
                case GameTextEncodingType.UNICODE: return Encoding.Unicode;
                case GameTextEncodingType.UTF_32: return Encoding.UTF32;
                case GameTextEncodingType.UTF_7: return Encoding.UTF7;
                case GameTextEncodingType.UTF_8: return Encoding.UTF8;
                default:
                    GLog.PrintError("unknown text encoding");
                    return Encoding.UTF8;
            }
        }
    }
}