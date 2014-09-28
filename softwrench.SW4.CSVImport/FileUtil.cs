using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW4_HistoricalImport
{
    public static class FileUtil
    {
        public static string GetFileName(string filepath)
        {
            string result = null;
            if (filepath.Contains('/'))
            {
                string[] split = filepath.Split('/');
                result = split[split.Length - 1];
            }
            else if (filepath.Contains('\\'))
            {
                string[] split = filepath.Split('\\');
                result = split[split.Length - 1];

            }
            else result = filepath;

            return result;
        }
    }
}
