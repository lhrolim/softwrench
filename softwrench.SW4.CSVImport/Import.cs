using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Microsoft.VisualBasic.FileIO;

namespace SW4_Import
{
    public class Import
    {
        private string _name;
        private string _table;
        private Dictionary<string, Dictionary<string, string>> _data;

        public string Name { get { return _name; } set { _name = value; } }
        public string Table { get { return _table; } set { _table = value; } }
        public Dictionary<string, Dictionary<string, string>> Data { get { return _data; } set { _data = value; } }

        public Import(string filepath)
        {
           using (TextFieldParser fieldParser = new TextFieldParser(filepath))
            {
                Data = new Dictionary<string, Dictionary<string, string>>();

                fieldParser.TextFieldType = FieldType.Delimited;
                fieldParser.Delimiters = new string[] { "," };
                fieldParser.HasFieldsEnclosedInQuotes = true;
                string[] fields;

                for (int i = 1; i < ConfigReader.FirstDataRow; i++)
                {
                    fields = fieldParser.ReadFields();
                }

                while (!fieldParser.EndOfData)
                {
                    fields = fieldParser.ReadFields();

                    var fieldValues = new Dictionary<string, string>();
                    foreach (string fieldName in ConfigReader.FieldMappings.Keys)
                    {
                        var fieldIndex = ConfigReader.FieldMappings[fieldName];
                        if (fields[fieldIndex].Length > 0)
                            if(!ConfigReader.ConvertToDateTime.Contains(fieldName))
                                fieldValues.Add(fieldName, fields[fieldIndex]);
                            else
                                fieldValues.Add(fieldName, Convert.ToDateTime(fields[fieldIndex]).ToString("yyyy-MM-dd'T'hh:mm:ss"));
                    }
                    foreach (string fieldName in ConfigReader.StaticFieldList.Keys)
                    {
                        fieldValues.Add(fieldName, ConfigReader.StaticFieldList[fieldName]);
                    }
                    Data.Add(fields[ConfigReader.UniqueIDIndex], fieldValues);

                }

            }
        }
    }
}
