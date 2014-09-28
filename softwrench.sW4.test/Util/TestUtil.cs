using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sW4.test.Util {
    class TestUtil {
        internal static IEnumerable<string> ClientNames() {
            var baseDir= AppDomain.CurrentDomain.BaseDirectory + "\\Client\\";
            var clients = Directory.GetDirectories(baseDir);
            IList<string> clientNames = new List<string>();
            foreach (var client in clients) {
                clientNames.Add(client.Replace(baseDir, ""));
            }
            return new List<string>(clientNames).Where(c => !c.Contains("@internal"));
        }
    }
}
