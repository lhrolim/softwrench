﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using cts.commons.simpleinjector;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using SpreadsheetLight;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using System.Data;
using System.IO;

namespace softWrench.sW4.Util {
    public class ExcelUtil : ISingletonComponent {
        
        private readonly I18NResolver _i18NResolver;

        private readonly IContextLookuper _contextLookuper;

        private readonly StatusColorResolver _statusColorsService;

        private readonly string defaultDateTimeFormat;

        private Dictionary<string, String> _cellStyleDictionary;

        public ExcelUtil(I18NResolver i18NResolver, IContextLookuper contextLookuper, StatusColorResolver statusColorsService, IConfigurationFacade facade) {
            _i18NResolver = i18NResolver;
            _contextLookuper = contextLookuper;
            _statusColorsService = statusColorsService;
            
            defaultDateTimeFormat = facade != null ? facade.Lookup<string>(ConfigurationConstants.DateTimeFormat) : "dd/MM/yyyy HH:mm";
        }



        public SLDocument ConvertGridToExcel(ApplicationListResult result, InMemoryUser user) {
            var schema = result.Schema;
            var application = schema.ApplicationName;

            var colorStatusDict = _statusColorsService.GetColorsAsDict(application);

            if(colorStatusDict == null) {
                colorStatusDict = _statusColorsService.GetColorsAsDict(application, true);
            }

            var resultItems = result.ResultObject;

            IEnumerable<ApplicationFieldDefinition> applicationFields = schema.Fields;
            using (var ms = new System.IO.MemoryStream())
            using (var xl = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook)) {
                // attributes of elements
                // the xml writer

                xl.AddWorkbookPart();
                var worksheetpart = xl.WorkbookPart.AddNewPart<WorksheetPart>();

                var writer = OpenXmlWriter.Create(worksheetpart);

                var worksheet = new Worksheet();
                writer.WriteStartElement(worksheet);

                // create sheetdata element
                writer.WriteStartElement(new SheetData());

                var stylesPart = xl.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = new Stylesheet();

                // create 2 fonts (one normal, one header)
                createFonts(stylesPart);

                _cellStyleDictionary = new Dictionary<string, string>();

                // create fills
                BuildFills(stylesPart, colorStatusDict);

                // blank border list
                stylesPart.Stylesheet.Borders = new Borders { Count = 1 };
                stylesPart.Stylesheet.Borders.AppendChild(new Border());

                // blank cell format list
                stylesPart.Stylesheet.CellStyleFormats = new CellStyleFormats { Count = 1 };
                stylesPart.Stylesheet.CellStyleFormats.AppendChild(new CellFormat());

                // cell format list
                CreateCellFormats(stylesPart);

                var rowIdx = 1;

                // create header row
                CreateHeaderRow(applicationFields, writer, rowIdx, schema.Name);
                //count up row
                rowIdx++;

                // write data rows

                foreach (var item in resultItems) {
                    rowIdx = WriteRow(user, item, rowIdx, writer, applicationFields, schema, colorStatusDict);
                }


                #region end worksheet

                writer.WriteEndElement();
                writer.Close();
                    
                // write root element
                writer = OpenXmlWriter.Create(xl.WorkbookPart);
                writer.WriteStartElement(new Workbook());
                writer.WriteStartElement(new Sheets());

                writer.WriteElement(new Sheet {
                    Name = "Sheet1",
                    SheetId = 1,
                    Id = xl.WorkbookPart.GetIdOfPart(worksheetpart)
                });
                    
                // end Sheets
                writer.WriteEndElement();
                // end Workbook
                writer.WriteEndElement();
                writer.Close();

                xl.Close();


                var excelFile = new SLDocument(ms);
                SetColumnWidth(excelFile, applicationFields);
                #endregion
                return excelFile;
            }
        }


        public byte[] ConvertDatasetToExcel(DataSet dataset) {
            using (MemoryStream mem = new MemoryStream()) {
                using (var document = SpreadsheetDocument.Create(mem, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook)) {
                    var workbookPart = document.AddWorkbookPart();

                    document.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                    document.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

                    foreach (System.Data.DataTable table in dataset.Tables) {

                        var sheetPart = document.WorkbookPart.AddNewPart<WorksheetPart>();
                        var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                        sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                        DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = document.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                        string relationshipId = document.WorkbookPart.GetIdOfPart(sheetPart);

                        uint sheetId = 1;
                        if (sheets.Elements<Sheet>().Count() > 0) {
                            sheetId =
                                sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                        }

                        DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                        sheets.Append(sheet);

                        DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                        List<String> columns = new List<string>();
                        foreach (System.Data.DataColumn column in table.Columns) {
                            columns.Add(column.ColumnName);

                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                            headerRow.AppendChild(cell);
                        }


                        sheetData.AppendChild(headerRow);

                        foreach (System.Data.DataRow dsrow in table.Rows) {
                            DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                            foreach (String col in columns) {
                                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                                cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString()); //
                                newRow.AppendChild(cell);
                            }

                            sheetData.AppendChild(newRow);
                        }
                    }

                    workbookPart.Workbook.Save();
                    document.Close();
                    return mem.ToArray();
                }
            }
        }

        private int WriteRow(InMemoryUser user, AttributeHolder item, int rowIdx, OpenXmlWriter writer, IEnumerable<ApplicationFieldDefinition> applicationFields,
            ApplicationSchemaDefinition schema, Dictionary<string, string> colorStatusDict) {
            var attributes = item;

            // create new row
            var xmlAttributes = new List<OpenXmlAttribute>
            {
                new OpenXmlAttribute("r", null, rowIdx.ToString(CultureInfo.InvariantCulture))
            };
            writer.WriteStartElement(new Row(), xmlAttributes);

            var nonHiddenFields = applicationFields.Where(ShouldShowField());
            foreach (var applicationField in nonHiddenFields) {
                object dataAux;
                attributes.TryGetValue(applicationField.Attribute, out dataAux);
                if (dataAux == null && applicationField.Attribute.StartsWith("#") &&
                    Char.IsNumber(applicationField.Attribute[1])) {
                    attributes.TryGetValue(applicationField.Attribute.Substring(2), out dataAux);
                }

                var data = dataAux == null ? string.Empty : dataAux.ToString();

                xmlAttributes = new List<OpenXmlAttribute> { new OpenXmlAttribute("t", null, "str") };
                // this is the data type ("t"), with CellValues.String ("str")

                // that's the default style
                var styleId = "1";
                if (applicationField.Attribute.Contains("status")) {
                    var success = getColor(string.IsNullOrEmpty(data.Trim()) ? "NULL" : data.Trim().ToLower(), schema.Name, ref styleId, colorStatusDict);
                    if (!success) {
                        // check if status is something like NEW 1/4 (and make sure it doesn't match e.g. NEW REQUEST).
                        var match = Regex.Match(data.Trim(), "(([A-Z]+ )+)[1-9]+/[1-9]+");
                        if (match.Success) {
                            var status = match.Groups[2].Value.Trim();
                            var compundStatus = getColor(status.ToLower(), schema.Name, ref styleId, colorStatusDict);
                            if (!compundStatus) {
                                styleId = "1";
                            }
                        } else {
                            styleId = "1";
                        }
                    }
                }
                xmlAttributes.Add(new OpenXmlAttribute("s", null, styleId));

                // start a cell
                writer.WriteStartElement(new Cell(), xmlAttributes);
                // write cell content
                DateTime dtTimeAux;
                var formatToUse = defaultDateTimeFormat;
                if (applicationField.RendererParameters.ContainsKey("format")) {
                    formatToUse = applicationField.RendererParameters["format"].ToString();
                }
                var dateParsed = DateTime.TryParse(data, out dtTimeAux);
                var dataToCell = data;

                if (dateParsed) {
                    dataToCell = dtTimeAux.FromMaximoToUser(user).ToString(formatToUse);
                }
                if (dataToCell == "-666") {
                    //this magic number should never be displayed! 
                    //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                    dataToCell = "";
                }
                writer.WriteElement(new CellValue(dataToCell));
                // end cell
                writer.WriteEndElement();
            }

            // next row
            rowIdx++;
            // end row
            writer.WriteEndElement();
            return rowIdx;
        }

        private bool getColor(string status, string schemaName, ref string styleId, Dictionary<string, string> colorStatusDict) {
            if (colorStatusDict.ContainsKey(status)) {
                return _cellStyleDictionary.TryGetValue(status, out styleId);
            }

            return false;
        }

        private static Func<ApplicationFieldDefinition, bool> ShouldShowField() {
            return f => {
                var rendererParameters = f.RendererParameters;
                if (rendererParameters.ContainsKey(FieldRendererConstants.EXPORTTOEXCEL)) {
                    return "true".Equals(rendererParameters[FieldRendererConstants.EXPORTTOEXCEL]);
                }
                return !f.IsHidden;
            };
        }

        private void CreateHeaderRow(IEnumerable<ApplicationFieldDefinition> applicationFields, OpenXmlWriter writer, int rowIdx, string schemaId) {
            var xmlAttributes = new List<OpenXmlAttribute>
            {
                new OpenXmlAttribute("r", null, rowIdx.ToString(CultureInfo.InvariantCulture))
            };
            writer.WriteStartElement(new Row(), xmlAttributes);
            foreach (var applicationField in applicationFields.Where(ShouldShowField())) {
                //Exporting to Excel, even if field is hidden
                xmlAttributes = new List<OpenXmlAttribute>{
                    // add new datatype for cell
                    new OpenXmlAttribute("t", null, "str"),
                    // add header style
                    new OpenXmlAttribute("s", null, "2")
                };

                writer.WriteStartElement(new Cell(), xmlAttributes);
                writer.WriteElement(new CellValue(GetI18NLabel(applicationField, schemaId)));

                // this is for Cell
                writer.WriteEndElement();
            }

            // end Row
            writer.WriteEndElement();
        }

        private string GetI18NLabel(ApplicationFieldDefinition applicationField, string schemaId) {
            var module = _contextLookuper.LookupContext().Module;
            if (module != null) {
                return applicationField.Label;
            }
            var i18NKey = applicationField.ApplicationName + "." + applicationField.Attribute;
            var i18NValue = _i18NResolver.I18NValue(i18NKey, applicationField.Label, new object[] { schemaId });
            return i18NValue;
        }

        private void CreateCellFormats(WorkbookStylesPart stylesPart) {
            stylesPart.Stylesheet.CellFormats = new CellFormats();
            // empty one for index 0, seems to be required
            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat());
            // no color
            createCellFormat(stylesPart, 0, 0, 0, 0, true);

            foreach(var kwp in _cellStyleDictionary) {
                createCellFormat(stylesPart, 0, 0, 0, Convert.ToUInt32(kwp.Value), true);
            }
            
            // header style
            createCellFormat(stylesPart, 0, 1, 0, 0, true);

            stylesPart.Stylesheet.CellFormats.Count = Convert.ToUInt32(_cellStyleDictionary.Count + 2);
        }

        private void createCellFormat(WorkbookStylesPart stylesPart, UInt32 formatId, UInt32 fontId, UInt32 borderId, UInt32 fillId, bool applyFill) {
            stylesPart.Stylesheet.CellFormats.AppendChild(
                new CellFormat {
                    FormatId = formatId,
                    FontId = fontId,
                    BorderId = borderId,
                    FillId = fillId,
                    ApplyFill = applyFill
                }).AppendChild(new Alignment {
                    Horizontal = HorizontalAlignmentValues.Left,
                    WrapText = BooleanValue.FromBoolean(true),
                    Vertical = VerticalAlignmentValues.Top
                });
        }

        private void createFonts(WorkbookStylesPart stylesPart) {
            stylesPart.Stylesheet.Fonts = new Fonts();

            // normal font
            var name = new FontName { Val = "Calibri" };
            var size = new FontSize { Val = 10 };
            stylesPart.Stylesheet.Fonts.AppendChild(new Font { FontName = name, FontSize = size });

            // bold font for header
            var boldFont = new Bold();
            stylesPart.Stylesheet.Fonts.AppendChild(new Font { FontName = (FontName)name.Clone(), FontSize = (FontSize)size.Clone(), Bold = boldFont });

            stylesPart.Stylesheet.Fonts.Count = 2;
        }

        private void BuildFills(WorkbookStylesPart stylesPart, Dictionary<string, string> colors) {
            stylesPart.Stylesheet.Fills = new Fills();
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = createColor("d8d8d8") });
            _cellStyleDictionary.Add("header", "2");

            var i = 3;
            foreach (var kvp in colors)
            {
                var fill = createColor(kvp.Value.StartsWith("#") ? kvp.Value.Substring(1) : kvp.Value);
                stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = fill });

                _cellStyleDictionary.Add(kvp.Key, i.ToString());

                i++;
            }

            stylesPart.Stylesheet.Fills.Count = Convert.ToUInt32(i + 2);
        }

        private PatternFill createColor(string colorhex) {
            var color = new PatternFill {
                PatternType = PatternValues.Solid,
                ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString(colorhex) },
                BackgroundColor = new BackgroundColor { Indexed = 64 }
            };
            return color;
        }


        private void SetColumnWidth(SLDocument excelFile, IEnumerable<ApplicationFieldDefinition> applicationFields) {
            var columnIndex = 1;
            foreach (var applicationField in applicationFields) {
                double excelWidthAux = 0;
                object excelwidthAux;
                if (applicationField.IsHidden) {
                    continue;
                }
                applicationField.RendererParameters.TryGetValue("excelwidth", out excelwidthAux);
                if (excelwidthAux != null) double.TryParse(excelwidthAux.ToString(), out excelWidthAux);
                var excelWidth = excelWidthAux > 0 ? excelWidthAux : (applicationField.Label.Length * 1.5);
                excelFile.SetColumnWidth(columnIndex, excelWidth);
                columnIndex++;
            }
        }


    }
}
