﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using cts.commons.portable.Util;

namespace softWrench.sW4.Util {
    public class FileUtils {

        //        public static String NormalizeDirectorySlashes(string dir) {
        //            if (!dir.StartsWith("\\")) {
        //                dir = "\\" + dir;
        //            }
        //            if (!dir.EndsWith("/\\")) {
        //                dir = dir + "/\\";
        //            }
        //            return dir;
        //        }
        /// <summary>
        /// returns name of a file, given the full directory name, truncating its name to the maxChars value
        /// </summary>
        /// <param name="attachmentPath"></param>
        /// <param name="maxChars"></param>
        /// <returns></returns>
        public static string GetNameFromPath(string attachmentPath, int maxChars = 1000) {
            if (attachmentPath == null) {
                return null;
            }

            // extract file name from full file path
            var lastIndexOf = attachmentPath.LastIndexOf("/", System.StringComparison.Ordinal);
            var filename = lastIndexOf != -1 ? attachmentPath.Substring(lastIndexOf + 1) : attachmentPath;


            return Truncate(filename, maxChars);

        }

        public static string Truncate(string filename, int maxChars) {
            if (filename == null) {
                return null;
            }

            int lastIndexOf;
            if (filename.Length > maxChars) {
                // extract file name, truncking the name without extension
                lastIndexOf = filename.LastIndexOf(".", System.StringComparison.Ordinal);
                filename =
                    lastIndexOf != -1
                        ? filename.Substring(0, maxChars - (filename.Length - lastIndexOf)) +
                          filename.Substring(lastIndexOf)
                        : filename.Substring(0, maxChars);
            }
            return filename;
        }

        public static void DoWithLines(string filePath, Action<string> action) {
            string line;
            var file = new System.IO.StreamReader(filePath);
            while ((line = file.ReadLine()) != null) {
                action(line);
            }
            file.Close();

        }

        public static byte[] ToByteArrayFromHtmlString(string content) {
            var base64String = GetB64PartOnly(content);
            return System.Convert.FromBase64String(base64String);
        }

        public static string GenerateCheckSumHash(byte[] byteArray) {
            using (var sha1 = new SHA1CryptoServiceProvider()) {
                var hash = Convert.ToBase64String(sha1.ComputeHash(byteArray));
                return hash;
            }
        }


        public static string GetB64PartOnly(string content) {
            var indexOf = content.IndexOf(',');
            if (indexOf == -1) {
                return content;
            }
            var base64String = content.Substring(indexOf + 1);
            return base64String;
        }

        public static string GetFormattedAttachmentString(string fileB64, string contentType) {
            var validator = "data:{0};base64,".Fmt(contentType);
            if (fileB64.StartsWith(validator)) {
                return fileB64;
            }
            var formattedAttachmentString = validator + fileB64;
            return formattedAttachmentString;
        }

        public static byte[] GenerateZip(List<Tuple<byte[], string>> files) {
            if (files == null) {
                return null;
            }

            using (var compressedFileStream = new MemoryStream()) {
                //Create an archive and store the stream in memory.
                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, false)) {
                    foreach (var caseAttachmentModel in files) {
                        //Create a zip entry for each attachment
                        var zipEntry = zipArchive.CreateEntry(caseAttachmentModel.Item2);

                        //Get the stream of the attachment
                        using (var originalFileStream = new MemoryStream(caseAttachmentModel.Item1)) {
                            using (var zipEntryStream = zipEntry.Open()) {
                                //Copy the attachment stream to the zip entry stream
                                originalFileStream.CopyTo(zipEntryStream);
                            }
                        }
                    }

                }
                return compressedFileStream.ToArray();
            }
        }
    }
}
