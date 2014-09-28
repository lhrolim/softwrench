using SW4_HistoricalImport;
using SW4_HistoricalImport.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace SW4_Import {
    class Program {
        static void Main(string[] args) {
            var importConfigPath = "ImportConfig.xml";//ConfigurationManager.AppSettings["ImportConfigPath"];
            var importSchemaPath = "ImportConfigSchema.xsd";//ConfigurationManager.AppSettings["ImportSchemaPath"];
            var successfulValidation = SchemaValidator.Validate(importConfigPath, importSchemaPath);
            if (successfulValidation == true) {
                var importCount = ConfigReader.GetImportConfigCount(importConfigPath);
                for (int i = 0; i < importCount; i++) {
                    ConfigReader.LoadImportConfig(importConfigPath, i);
                    List<string> files = new List<string>();
                    files = Directory.GetFiles(@ConfigReader.InputFolder, ConfigReader.StaticInputFileName + "*").ToList();

                    foreach (string filePath in files) {
                        Import import = new Import(filePath);
                        var queries = QueryBuilder.BuildQueryForImport(import);
                        IDataConnector dataConnector = null;
                        var providerName = ConfigurationManager.ConnectionStrings["Database"].ProviderName.ToLower();
                        if (providerName == "mysql.data.mysqlclient") {
                            dataConnector = new MysqlDataConnector();
                        } else if (providerName == "system.data.sqlclient" || providerName == "system.data.sql") {
                            dataConnector = new SqlDataConnector();
                        }
                        var importSuccess = dataConnector.SubmitQueriesToDatabase(queries);
                        var shouldMove = !"false".Equals(ConfigurationManager.AppSettings["move"]);
                        if (importSuccess && shouldMove)
                            File.Move(@ConfigReader.InputFolder + FileUtil.GetFileName(filePath),
                                      @ConfigReader.OutputFolder + FileUtil.GetFileName(filePath));
                    }
                }
            } else {
                throw new Exception("Import configuration file does not match the import configuration schema");
            }
        }
    }

}
