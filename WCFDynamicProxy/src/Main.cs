using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WcfSamples.DynamicProxy
{
    class Main2
    {

        partial class Test
        {
            [XmlText]
            public Boolean a;
        }


        static void Main(string[] args)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Test));
            var test = new Test();
            test.a = true;
            TextWriter writer = new StreamWriter("c:\test.maximo");

            ser.Serialize(writer, test);
            writer.Close();
        }    
    }
}
