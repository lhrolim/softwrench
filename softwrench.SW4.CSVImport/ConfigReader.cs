using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SW4_Import
{
    public static class ConfigReader
    {
        private static string _importName = "";
        private static string _importTable = "";
        private static string _inputFolder = "";
        private static string _outputFolder = "";
        private static string _staticInputFileName = "";
        private static string _inputFileExt = "";
        private static int _firstDataRow = -1;
        private static int _uniqueIDIndex = -1;
        private static Dictionary<string, int> _fieldMappings = new Dictionary<string, int>();
        private static Dictionary<string, string> _staticFieldList = new Dictionary<string, string>();
        private static List<string> _convertToDateTime = new List<string>();

        public static string ImportName { get { return _importName; } set { _importName = value; } }
        public static string ImportTable { get { return _importTable; } set { _importTable = value; } }
        public static string InputFolder { get { return _inputFolder; } set { _inputFolder = value; } }
        public static string OutputFolder { get { return _outputFolder; } set { _outputFolder = value; } }
        public static string StaticInputFileName { get { return _staticInputFileName; } set { _staticInputFileName = value; } }
        public static string InputFileExt { get { return _inputFileExt; } set { _inputFileExt = value; } }
        public static int FirstDataRow { get { return _firstDataRow; } set { _firstDataRow = value; } }
        public static int UniqueIDIndex { get { return _uniqueIDIndex; } set { _uniqueIDIndex = value; } }
        public static Dictionary<string, int> FieldMappings { get { return _fieldMappings; } set { _fieldMappings = value; } }
        public static Dictionary<string, string> StaticFieldList { get { return _staticFieldList; } set { _staticFieldList = value; } }
        public static List<String> ConvertToDateTime { get { return _convertToDateTime; } set { _convertToDateTime = value; } }

        public static int GetImportConfigCount(string importConfigPath)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(importConfigPath);  


            XmlNodeList xnList = xml.SelectNodes("//Import");
            return xnList.Count;
        }

        public static void LoadImportConfig(string importConfigPath, int nodeNum)
        {
            ImportName = "";
            ImportTable = "";
            InputFolder = "";
            OutputFolder = "";
            StaticInputFileName = "";
            InputFileExt = "";
            FirstDataRow = -1;
            UniqueIDIndex = -1;
            FieldMappings = new Dictionary<string, int>();
            StaticFieldList = new Dictionary<string, string>();
            ConvertToDateTime = new List<string>();

            XmlDocument xml = new XmlDocument();
            xml.Load(importConfigPath); 

            XmlNodeList xnList = xml.SelectNodes("//Import");
            XmlNode SettingsList = xnList[nodeNum];

            XmlNodeList InputNames = SettingsList.SelectNodes("//Name");
            ImportName = InputNames[nodeNum].InnerText;

            XmlNodeList InputTypes = SettingsList.SelectNodes("//Table");
            ImportTable = InputTypes[nodeNum].InnerText;

            XmlNodeList IONodesList = SettingsList.SelectNodes("//IO");
            XmlNodeList MappingNodesList = SettingsList.SelectNodes("//FieldMappings");
            XmlNodeList StaticFieldNodeList = SettingsList.SelectNodes("//StaticFields");
            
            XmlNode IONode = IONodesList[nodeNum];
            XmlNode MappingNode = MappingNodesList[nodeNum];
            XmlNode StaticFieldNode = StaticFieldNodeList[nodeNum];
            
            foreach (XmlNode io in IONode.ChildNodes)
            {
                switch (io.Name)
                {
                    case "InputFolder":
                        InputFolder = io.InnerText;
                        break;
                    case "OutputFolder":
                        OutputFolder = io.InnerText;
                        break;
                    case "StaticInputFileName":
                        StaticInputFileName = io.InnerText;
                        break;
                    case "InputFileExt":
                        InputFileExt = io.InnerText;
                        break;
                    case "FirstDataRow":
                        FirstDataRow = Convert.ToInt32(io.InnerText);
                        break;
                }
            }


            foreach (XmlNode map in MappingNode.ChildNodes)
            {
                FieldMappings.Add(map.Attributes["name"].Value, Convert.ToInt32(map.InnerText)-1);
                var uID = map.Attributes["uniqueID"];
                if(uID != null)
                {
                    var hasUniqueID = false;
                    Boolean.TryParse(map.Attributes["uniqueID"].Value, out hasUniqueID);
                    if (hasUniqueID)
                        UniqueIDIndex = Convert.ToInt32(map.InnerText) - 1;
                }
                var type = map.Attributes["type"];
                if (type != null)
                {
                    if (type.Value.ToLower() == "datetime")
                        ConvertToDateTime.Add(map.Attributes["name"].Value);
                }
            }

            var importDate = DateTime.Now.ToString("yyyy-MM-dd'T'hh:mm:ss");
            foreach (XmlNode map in StaticFieldNode.ChildNodes)
            {
                var property = map.Attributes["property"];
                if (property == null)
                {
                    StaticFieldList.Add(map.Attributes["name"].Value, map.InnerText);
                }
                else if (property.Value.ToLower() == "datetime.now")
                    StaticFieldList.Add(map.Attributes["name"].Value, importDate);

            }
        }
    }
}
