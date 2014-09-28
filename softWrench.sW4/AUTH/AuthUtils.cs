using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace softWrench.sW4.AUTH {
    public class AuthUtils {
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


    }
}
