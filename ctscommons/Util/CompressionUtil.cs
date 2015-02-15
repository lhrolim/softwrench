using System.IO;
using System.IO.Compression;

namespace ctscommons.Util {
    public class CompressionUtil {

        public static byte[] Compress(byte[] plainData) {
            byte[] compressesData = null;
            using (var outputStream = new MemoryStream()) {
                using (var zip = new GZipStream(outputStream, CompressionMode.Compress)) {
                    zip.Write(plainData, 0, plainData.Length);
                }
                //Dont get the MemoryStream data before the GZipStream is closed 
                //since it doesn’t yet contain complete compressed data.
                //GZipStream writes additional data including footer information when its been disposed
                compressesData = outputStream.ToArray();
            }

            return compressesData;
        }

        public static byte[] Decompress(byte[] zippedData) {
            byte[] decompressedData = null;
            using (var outputStream = new MemoryStream()) {
                using (var inputStream = new MemoryStream(zippedData)) {
                    using (var zip = new GZipStream(inputStream, CompressionMode.Decompress)) {
                        zip.CopyTo(outputStream);
                    }
                }
                decompressedData = outputStream.ToArray();
            }

            return decompressedData;
        }

      
    }
}
