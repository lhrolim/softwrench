using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softwrench.sw4.problem.classes.api;

namespace softWrench.sW4.Data {

    public class JSonXmlProblemData : IProblemData {

        public string Xml { get; set; }
        public string Json { get; set; }

        public JSonXmlProblemData(string xmlCurrentData, JObject jsonOriginalData) {
            Xml = xmlCurrentData;
            if (jsonOriginalData != null) {
                Json = jsonOriginalData.ToString(Formatting.None);
            }

        }

        public string Serialize() {

            dynamic root = new JObject();

            dynamic jsonContent = new JObject();
            jsonContent.content = Json;
            jsonContent.type = "json";

            dynamic xmlContent = new JObject();
            xmlContent.content = Xml;
            xmlContent.type = "xml";


            root.xml = xmlContent;
            root.json = jsonContent;

            return root.ToString();


        }

        public static JSonXmlProblemData Deserialize(string rawData) {
            var ob = JObject.Parse(rawData);
            dynamic obj = ob;
            var xml = obj.xml;
            var xmlcontent = xml.content.Value;

            var json = obj.json;
            var jsonContent = json.content.Value;

            return new JSonXmlProblemData(xmlcontent, jsonContent);
        }
    }
}
