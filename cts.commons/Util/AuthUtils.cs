using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace cts.commons.Util {
    public class AuthUtils {

        public static byte[] StandardHmacKey = Encoding.ASCII.GetBytes("CTS2017LEJA");

        public static IPrincipal CurrentPrincipal {
            get { return System.Threading.Thread.CurrentPrincipal; }
        }

        public static string CurrentPrincipalLogin {
            get { return CurrentPrincipal.Identity.Name; }
        }

        public static string GetSha1HashData(string data) {
            return GetHashData(data, SHA1.Create());
        }

        public static string GetMd5HashData(string data) {
            return GetHashData(data, MD5.Create());
        }

        public static string GetHashData(string data, HashAlgorithm hashAlgorithm = null) {
            if (hashAlgorithm == null) {
                hashAlgorithm = SHA1.Create();
            }
            var hashData = hashAlgorithm.ComputeHash(Encoding.Default.GetBytes(data));
            var returnValue = new StringBuilder();
            for (int i = 0; i < hashData.Length; i++) {
                returnValue.Append(hashData[i].ToString(CultureInfo.InvariantCulture));
            }
            return returnValue.ToString();
        }

        public static bool ValidateSHA1HashData(string inputData, string storedHashData, HashAlgorithm hashAlgorithm = null) {
            string getHashInputData = GetHashData(inputData, hashAlgorithm);
            return string.Compare(getHashInputData, storedHashData, StringComparison.CurrentCultureIgnoreCase) == 0;
        }

        public static string HmacShaEncode(string input, byte[] key = null) {
            if (key == null) {
                key = StandardHmacKey;
            }
            var myhmacsha1 = new HMACSHA1(key);
            var byteArray = Encoding.ASCII.GetBytes(input);
            var stream = new MemoryStream(byteArray);
            return myhmacsha1.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
        }


    }
}
