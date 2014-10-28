using System.IO;

namespace softWrench.sW4.Util {
    public class StreamUtils {
        public static Stream CopyStream(Stream inputStream) {
            const int readSize = 256;
            var buffer = new byte[readSize];
            var ms = new MemoryStream();

            var count = inputStream.Read(buffer, 0, readSize);
            while (count > 0) {
                ms.Write(buffer, 0, count);
                count = inputStream.Read(buffer, 0, readSize);
            }
            ms.Seek(0, SeekOrigin.Begin);
            inputStream.Seek(0, SeekOrigin.Begin);
            return ms;
        }


    }
}
