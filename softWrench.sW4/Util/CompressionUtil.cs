using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace softWrench.sW4.Util {
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

        public static string CompressRtf(string rtf) {

            string ret = String.Empty;
            
            // The WPF components (RichTextBox and TextRange) require a STA thread to run
            Thread t = new Thread(
                () => {
                    try {
                        RichTextBox rtb = new RichTextBox();
                        var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);

                        using (var rtfMemoryStream = new MemoryStream()) {
                            using (var rtfStreamWriter = new StreamWriter(rtfMemoryStream)) {
                                rtfStreamWriter.Write(rtf);
                                rtfStreamWriter.Flush();
                                rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                                textRange.Load(rtfMemoryStream, DataFormats.Rtf);
                            }
                        }

                        using (var rtfMemoryStream = new MemoryStream()) {
                            textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                            textRange.Save(rtfMemoryStream, System.Windows.DataFormats.Rtf);
                            rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                            using (var rtfStreamReader = new StreamReader(rtfMemoryStream)) {
                                ret = rtfStreamReader.ReadToEnd();
                            }
                        }
                    }
                    catch (Exception e) {
                        ret = e.Message;
                    }
                });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            return ret;
        }
    }
}
