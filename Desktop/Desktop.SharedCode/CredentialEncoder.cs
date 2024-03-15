using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Datahub.Core.DataTransfers
{
    public class CredentialEncoder
    {
        public static string EncodeCredentials(UploadCredentials credentials)
        {
            if (credentials is null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }
            var jsonString = JsonSerializer.Serialize(credentials);
            var utf8bytes = UTF8Encoding.UTF8.GetBytes(jsonString);
            // Create a memory stream to hold the compressed data
            using MemoryStream compressedStream = new();
            // Create a DeflateStream with CompressionMode.Compress
            using (DeflateStream deflateStream = new(compressedStream, CompressionMode.Compress))
            {
                // Write the input data to the DeflateStream
                deflateStream.Write(utf8bytes, 0, utf8bytes.Length);
            }

            // Get the compressed data as a byte array
            byte[] compressedBytes = compressedStream.ToArray();
            return Convert.ToBase64String(compressedBytes);
        }

        public static bool IsValid(string base64)
        {
            return DecodeCredentials(base64) != null;
        }

        public static byte[] Decompress(byte[] compressedBytes)
        {
            using (MemoryStream inputStream = new MemoryStream(compressedBytes))
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(outputStream);
                    }

                    return outputStream.ToArray();
                }
            }
        }

        public static UploadCredentials DecodeCredentials(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                throw new ArgumentException($"'{nameof(base64)}' cannot be null or empty.", nameof(base64));
            }
            try
            {
                var trimmed = base64.Trim();
                var utf8bytes = Convert.FromBase64String(trimmed);
                var utf8string = Encoding.UTF8.GetString(Decompress(utf8bytes));
                return JsonSerializer.Deserialize<UploadCredentials>(utf8string);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
