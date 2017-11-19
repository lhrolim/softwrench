using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using softwrench.sw4.problem.classes;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data {
    public class JsonXmlProblemHandler : IProblemHandler {

        public void OnProblemRegister(Problem problem) {
            problem.ReadOnly = true;
        }

        public void OnProblemSolved() {

        }

        bool IProblemHandler.DelegateToMainApplication() {
            return false;
        }


        public string ProblemHandler() {
            return Name();
        }

        public static string Name() {
            return typeof(JSonXmlProblemData).Name;
        }

        public string ApplicationName() {
            return null;
        }

        public string ClientName() {
            return null;
        }

        public ApplicationMetadataSchemaKey OnLoad(AttributeHolder resultObject, string data) {
            var deserialized = JSonXmlProblemData.Deserialize(data);

            resultObject.SetAttribute("#json", deserialized.Json);
            resultObject.SetAttribute("#xml", deserialized.Xml);
            
            return new ApplicationMetadataSchemaKey("jsonxmldetail");
        }
    }
}
