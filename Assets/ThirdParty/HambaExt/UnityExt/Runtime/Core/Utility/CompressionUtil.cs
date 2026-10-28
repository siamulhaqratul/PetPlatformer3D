using System.IO.Compression;
using System.IO;
using UnityExt.Debugging;

namespace UnityExt
{
    /// <summary>Utility methods for compressing and decompressing text (JSON) using GZip with configurable encoding.</summary>
    public static class CompressionUtil
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>Compresses a JSON (text) string using GZip with default encoding.</summary>
        /// <param name="jsonString">JSON text to compress (must be non-null/non-empty).</param>
        /// <returns>Compressed byte array.</returns>
        public static byte[] CompressText(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                GLog.ThrowArgument("Input JSON string cannot be null or empty.", nameof(jsonString));
            }

            byte[] jsonBytes = EncodingUtil.GetBytes(jsonString);

            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    gzipStream.Write(jsonBytes, 0, jsonBytes.Length);
                }

                return outputStream.ToArray();
            }
        }

        /// <summary>Compresses a JSON (text) string using GZip with the specified encoding.</summary>
        /// <param name="jsonString">JSON text to compress (must be non-null/non-empty).</param>
        /// <param name="encoding">Encoding type to convert the string to bytes before compression.</param>
        /// <returns>Compressed byte array.</returns>
        public static byte[] CompressText(string jsonString, GameTextEncodingType encoding)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                GLog.ThrowArgument("Input JSON string cannot be null or empty.", nameof(jsonString));
            }

            byte[] jsonBytes = EncodingUtil.GetBytes(jsonString, encoding);

            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    gzipStream.Write(jsonBytes, 0, jsonBytes.Length);
                }

                return outputStream.ToArray();
            }
        }

        /// <summary>Decompresses a GZip-compressed byte array into a string using default encoding.</summary>
        /// <param name="compressedData">Compressed GZip data (non-null/non-empty).</param>
        /// <returns>Decompressed string.</returns>
        public static string DecompressByte(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
            {
                GLog.ThrowArgument("Input compressed data cannot be null or empty.", nameof(compressedData));
            }

            using (var inputStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                byte[] decompressedBytes = outputStream.ToArray();
                return EncodingUtil.GetString(decompressedBytes);
            }
        }

        /// <summary>Decompresses a GZip-compressed byte array and returns the raw decompressed bytes.</summary>
        /// <param name="compressedData">Compressed GZip data (non-null/non-empty).</param>
        /// <returns>Raw decompressed bytes.</returns>
        public static byte[] DecompressByteIntoByte(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
            {
                GLog.ThrowArgument("Input compressed data cannot be null or empty.", nameof(compressedData));
            }

            using (var inputStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                byte[] decompressedBytes = outputStream.ToArray();
                return decompressedBytes;
            }
        }
    }
}