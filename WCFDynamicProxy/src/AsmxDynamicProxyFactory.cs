using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Xml.Schema;
using Microsoft.CSharp;
using System.Security;

namespace WcfSamples.DynamicProxy {

    public class AsmxDynamicProxyFactory : IDynamicProxyFactory {
        private Type _proxyType;

        public AsmxDynamicProxyFactory(String url) : this(url, null, null) {
        }

        public AsmxDynamicProxyFactory(String url, String user, String password) {
            WebClient client = new WebClient();

            if (!String.IsNullOrEmpty(user) && !String.IsNullOrEmpty(password)) {
                client.Credentials = new NetworkCredential(user, password);
            }

            Stream stream = client.OpenRead(url);
            ServiceDescription description = ServiceDescription.Read(stream);
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter();

            importer.AddServiceDescription(description, null, null);
            BindingCollection bindingCollection = description.Bindings;
            foreach (Binding binding in bindingCollection) {

            }


            // Add any imported files
            foreach (System.Xml.Schema.XmlSchema wsdlSchema in description.Types.Schemas) {
                DoImportSchemas(url, wsdlSchema, client, importer);
            }

            importer.Style = ServiceDescriptionImportStyle.Client;

            importer.CodeGenerationOptions = System.Xml.Serialization.CodeGenerationOptions.GenerateProperties;

            CodeNamespace nmspace = new CodeNamespace();
            CodeCompileUnit unit1 = new CodeCompileUnit();

            unit1.Namespaces.Add(nmspace);

            // This is generating the error:
            ServiceDescriptionImportWarnings warning = importer.Import(nmspace, unit1);
            if (warning == 0) {
                CodeDomProvider compiler = new CSharpCodeProvider();
                string[] references = new[] { "System.Web.Services.dll", "System.Xml.dll" };
                CompilerParameters parameters = new CompilerParameters(references);
                CompilerResults results = compiler.CompileAssemblyFromDom(parameters, unit1);

                Type[] assemblyTypes = results.CompiledAssembly.GetExportedTypes();
                _proxyType = assemblyTypes[0];

            }
        }

        private static void DoImportSchemas(string url, XmlSchema wsdlSchema, WebClient client,
            ServiceDescriptionImporter importer) {
            Stream stream;
            foreach (System.Xml.Schema.XmlSchemaObject externalSchema in wsdlSchema.Includes) {
                if (externalSchema is System.Xml.Schema.XmlSchemaImport || externalSchema is XmlSchemaInclude) {
                    Uri baseUri = new Uri(url);
                    Uri schemaUri = new Uri(baseUri, ((System.Xml.Schema.XmlSchemaExternal)externalSchema).SchemaLocation);
                    stream = client.OpenRead(schemaUri);
                    System.Xml.Schema.XmlSchema schema = System.Xml.Schema.XmlSchema.Read(stream, null);
                    DoImportSchemas(url, schema, client, importer);
                    importer.Schemas.Add(schema);
                } 
            }
        }

        public DynamicObject CreateProxy(String methodName) {
            object webServiceProxy = Activator.CreateInstance(_proxyType);
            return new DynamicAsmxProxy(webServiceProxy);
        }

        public DynamicObject CreateMainProxy() {
            object webServiceProxy = Activator.CreateInstance(_proxyType);
            return new DynamicAsmxProxy(webServiceProxy);
        }
    }
}
