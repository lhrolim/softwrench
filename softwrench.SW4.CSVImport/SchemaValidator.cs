using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace SW4_Import
{
    public static class SchemaValidator
    {
        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            throw new Exception();
        }

        public static bool Validate(string xmlpath, string schemapath)
        {
            try
            {
                XmlDocument xmld = new XmlDocument();
                xmld.Load(@xmlpath);
                xmld.Schemas.Add(null, @schemapath);
                xmld.Validate(ValidationCallBack);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + " ||| " + ex.InnerException);
                return false;
            }
        }
    }
}
