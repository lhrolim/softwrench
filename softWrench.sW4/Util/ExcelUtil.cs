using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SimpleInjector;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.Text.RegularExpressions;
using NHibernate.Linq;
using softwrench.sw4.Shared2.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softWrench.sW4.Util {
    public class ExcelUtil : ISingletonComponent {

        private readonly Dictionary<string, string> _cellStyleDictionary;
        private readonly Dictionary<string, string> _changeCellStyleDictionary;

        private readonly I18NResolver _i18NResolver;

        private readonly IContextLookuper _contextLookuper;


        public ExcelUtil(I18NResolver i18NResolver, IContextLookuper contextLookuper) {
            _i18NResolver = i18NResolver;
            _contextLookuper = contextLookuper;
            // setup style dictionary for back colors
            // 2 = red, 3 = green, 4 = yellow, 5 = orange, 6 = blue, 7 = white
            _cellStyleDictionary = new Dictionary<string, string>{
                {"ACC_CAT", "6"},
                {"APPR", "6"},
                {"ASSESSES", "6"},
                {"AUTH", "6"},
                {"AUTHORIZED", "6"},
                {"CAN", "2"},
                {"CANCELLED", "2"},
                {"CLOSE", "3"},
                {"CLOSED", "3"},
                {"COMP", "3"},
                {"DRAFT", "7"},
                {"FAIL", "2"},
                {"FAILED", "2"},
                {"FAILPIR", "2"},
                {"HISTEDIT", "3"},
                {"HOLDINPRG", "6"},
                {"IMPL", "3"},
                {"IMPLEMENTED", "3"},
                {"INFOPEND", "6"},
                {"INPRG", "4"},
                {"INPROG", "4"},
                {"NEW", "5"},
                {"NOTREQ", "2"},
                {"null", "4"},
                {"PENDING", "4"},
                {"PENDAPPR", "6"},
                {"PLANNED", "6"},
                {"QUEUED", "4"},
                {"REJECTED", "2"},
                {"RCACOMP", "6"},
                {"RESOLVCONF", "3"},
                {"RESOLVED", "6"},
                {"REVIEW", "3"},
                {"SCHED", "6"},
                {"SLAHOLD", "6"},
                {"WAPPR", "5"},
                {"WMATL", "6"},
                {"WSCH", "5"}
            };

            _changeCellStyleDictionary = new Dictionary<string, string>{
                {"ACC_CAT", "6"}
             };
        }


        public byte[] ConvertGridToCsv(InMemoryUser user, ApplicationSchemaDefinition schema, IEnumerable<AttributeHolder> rows, Func<AttributeHolder, ApplicationFieldDefinition,string, string> ColumnValueDelegate = null) {
            var csv = new StringBuilder();
            var enumerableFields = schema.Fields.Where(ShouldShowField());
            var fields = enumerableFields as IList<ApplicationFieldDefinition> ?? enumerableFields.ToList();
            // HEADER: line of comma separated labels
            var header = string.Join(",", fields.Select(field => GetI18NLabel(field, schema.Name)));
            csv.AppendLine(header);
            // ROWS
            rows.ForEach(item => {
                var row = new StringBuilder();
                var values = fields.Select(field => {
                    var data = GetValueAsString(item, field);
                    if (ColumnValueDelegate != null) {
                        data = ColumnValueDelegate(item, field,data);
                    }
                    var displayableData = AsDisplayableData(data, field, user);
                    return AsCsvCompliantData(displayableData);
                });
                row.Append(string.Join(",", values));
                csv.AppendLine(row.ToString());
            });
            // dump to byte array
            return csv.ToString().GetBytes();
        }

        public byte[] ConvertGridToExcel(InMemoryUser user, ApplicationSchemaDefinition schema, IEnumerable<AttributeHolder> rows) {
            IEnumerable<ApplicationFieldDefinition> applicationFields = schema.Fields;

            using (var resultStream = new MemoryStream())
            using (var ms = new MemoryStream())
            using (var xl = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook)) {
                //                var ms = new MemoryStream();

                // attributes of elements
                // the xml writer

                OpenXmlWriter writer;
                int rowIdx;
                var worksheetpart = Setup(xl, out writer, out rowIdx, schema);

                // create header row
                CreateHeaderRow(applicationFields, writer, rowIdx, schema.Name);
                //count up row
                rowIdx++;

                // write data rows
                DoWriteRows(user, schema, rows, rowIdx, writer, applicationFields);


                Finish(writer, xl, worksheetpart);

                return ms.ToArray();
            }
        }

        private static void Finish(OpenXmlWriter writer, SpreadsheetDocument xl, WorksheetPart worksheetpart) {
            // end worksheet
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
        }

        private WorksheetPart Setup(SpreadsheetDocument xl, out OpenXmlWriter writer, out int rowIdx, ApplicationSchemaDefinition schema) {
            xl.AddWorkbookPart();
            var worksheetpart = xl.WorkbookPart.AddNewPart<WorksheetPart>();

            writer = OpenXmlWriter.Create(worksheetpart);

            var worksheet = new Worksheet();
            //worksheetpart.Worksheet = worksheet;
            writer.WriteStartElement(worksheet);

            //changes
            SetColumnWidth(writer, schema);

            // create sheetdata element
            writer.WriteStartElement(new SheetData());

            var stylesPart = xl.WorkbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = new Stylesheet();

            // create 2 fonts (one normal, one header)
            createFonts(stylesPart);


            // create fills
            BuildFills(stylesPart);

            // blank border list
            stylesPart.Stylesheet.Borders = new Borders { Count = 1 };
            stylesPart.Stylesheet.Borders.AppendChild(new Border());

            // blank cell format list
            stylesPart.Stylesheet.CellStyleFormats = new CellStyleFormats { Count = 1 };
            stylesPart.Stylesheet.CellStyleFormats.AppendChild(new CellFormat());

            // cell format list
            CreateCellFormats(stylesPart);

            rowIdx = 1;
            return worksheetpart;
        }

        /// <summary>
        /// Return a string that conforms to CSV rules:
        /// https://en.wikipedia.org/wiki/Comma-separated_values#Basic_rules
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string AsCsvCompliantData(string data) {
            var hasQuotes = data.Contains("\"");
            var hasLineBreak = data.Contains("\n") || data.Contains("\r");
            var hasComma = data.Contains(",");
            if (hasQuotes) {
                return "\"" + data.Replace("\"", "\"\"") + "\"";
            }
            if (hasComma || hasLineBreak) {
                return "\"" + data + "\"";
            }
            return data;
        }

        /// <summary>
        /// Gets the data in the field as a string. Handles edge cases (numbers, #, etc).
        /// </summary>
        /// <param name="holder"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private string GetValueAsString(AttributeHolder holder, IDefaultValueApplicationDisplayable field) {
            var attributes = holder.Attributes;
            object dataAux;
            attributes.TryGetValue(field.Attribute, out dataAux);
            if (dataAux == null && field.Attribute.StartsWith("#") && char.IsNumber(field.Attribute[1])) {
                attributes.TryGetValue(field.Attribute.Substring(2), out dataAux);
            }
            return dataAux == null ? string.Empty : dataAux.ToString();
        }

        /// <summary>
        /// Returns a string that can be diaplayed in a report from a raw string data, 
        /// handling date formatting and magic strings/numbers/codes.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="field"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private string AsDisplayableData(string data, IApplicationDisplayable field, InMemoryUser user) {
            DateTime dtTimeAux;
            var formatToUse = "dd/MM/yyyy HH:mm";
            if (field.RendererParameters.ContainsKey("format")) {
                formatToUse = field.RendererParameters["format"];
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
            return dataToCell;
        }

        private void DoWriteRows(InMemoryUser user, ApplicationSchemaDefinition schema, IEnumerable<AttributeHolder> rows, int rowIdx,
            OpenXmlWriter writer, IEnumerable<ApplicationFieldDefinition> applicationFields) {
            foreach (var item in rows) {
                // create new row
                var xmlAttributes = new List<OpenXmlAttribute>
                {
                    new OpenXmlAttribute("r", null, rowIdx.ToString(CultureInfo.InvariantCulture))
                };
                writer.WriteStartElement(new Row(), xmlAttributes);

                var nonHiddenFields = applicationFields.Where(ShouldShowField());
                foreach (var applicationField in nonHiddenFields) {
                    var data = GetValueAsString(item, applicationField);

                    xmlAttributes = new List<OpenXmlAttribute> { new OpenXmlAttribute("t", null, "str") };
                    // this is the data type ("t"), with CellValues.String ("str")

                    // that's the default style
                    var styleId = "1";
                    if (applicationField.Attribute.Contains("status") && ApplicationConfiguration.ClientName == "hapag") {
                        var success = getColor(data.Trim(), schema.Name, ref styleId);

                        if (!success) {
                            // check if status is something like NEW 1/4 (and make sure it doesn't match e.g. NEW REQUEST).
                            var match = Regex.Match(data.Trim(), "(([A-Z]+ )+)[1-9]+/[1-9]+");
                            if (match.Success) {
                                var status = match.Groups[2].Value.Trim();
                                var compundStatus = getColor(status, schema.Name, ref styleId);
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
                    var dataToCell = AsDisplayableData(data, applicationField, user);
                    writer.WriteElement(new CellValue(dataToCell));
                    // end cell
                    writer.WriteEndElement();
                }

                // next row
                rowIdx++;
                // end row
                writer.WriteEndElement();
            }
        }

        private bool getColor(string status, string schemaName, ref string styleId) {
            var success = false;
            //var styleId2 = "1";
            if (schemaName != null && schemaName.Equals("change")) {
                success = _changeCellStyleDictionary.TryGetValue(status, out styleId);
            }
            if (!success) {
                success = _cellStyleDictionary.TryGetValue(status, out styleId);
            }
            // styleId = styleId2;
            return success;
        }

        private static Func<ApplicationFieldDefinition, bool> ShouldShowField() {
            return f => {
                var rendererParameters = f.Renderer.ParametersAsDictionary();
                if (rendererParameters.ContainsKey(FieldRendererConstants.Exporttoexcel)) {
                    return "true".EqualsIc(rendererParameters[FieldRendererConstants.Exporttoexcel]);
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
                    new OpenXmlAttribute("s", null, "8")
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
            var i18NValue = _i18NResolver.I18NValue(i18NKey, applicationField.Label, schemaId);
            return i18NValue;
        }

        private void CreateCellFormats(WorkbookStylesPart stylesPart) {
            stylesPart.Stylesheet.CellFormats = new CellFormats();
            // empty one for index 0, seems to be required
            stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat());
            // no color
            createCellFormat(stylesPart, 0, 0, 0, 0, true);
            // red
            createCellFormat(stylesPart, 0, 0, 0, 2, true);
            // green
            createCellFormat(stylesPart, 0, 0, 0, 3, true);
            // yellow
            createCellFormat(stylesPart, 0, 0, 0, 4, true);
            // orange
            createCellFormat(stylesPart, 0, 0, 0, 5, true);
            // blue
            createCellFormat(stylesPart, 0, 0, 0, 6, true);
            // white
            createCellFormat(stylesPart, 0, 0, 0, 7, true);

            // header style
            createCellFormat(stylesPart, 0, 1, 0, 0, true);

            stylesPart.Stylesheet.CellFormats.Count = 8;
        }

        private void createCellFormat(WorkbookStylesPart stylesPart, uint formatId, uint fontId, uint borderId, uint fillId, bool applyFill) {
            stylesPart.Stylesheet.CellFormats.AppendChild(
                new CellFormat {
                    FormatId = formatId, FontId = fontId, BorderId = borderId, FillId = fillId, ApplyFill = applyFill
                }).AppendChild(new Alignment {
                    Horizontal = HorizontalAlignmentValues.Left, WrapText = BooleanValue.FromBoolean(true), Vertical = VerticalAlignmentValues.Top
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

        private void BuildFills(WorkbookStylesPart stylesPart) {
            stylesPart.Stylesheet.Fills = new Fills();

            // create a solid red fill
            var solidRed = new PatternFill {
                PatternType = PatternValues.Solid,
                ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FFFF0000") },
                BackgroundColor = new BackgroundColor { Indexed = 64 }
            };

            // create green
            var green = createColor("FF006836");
            // create yellow
            var yellow = createColor("FFffff00");
            // create orange
            var orange = createColor("FFff7d00");
            // create blue
            var blue = createColor("ff002fff");
            // create white
            var white = createColor("ffffffff");

            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel

            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = solidRed });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = green });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = yellow });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = orange });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = blue });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = white });

            stylesPart.Stylesheet.Fills.Count = 7;
        }

        private PatternFill createColor(string colorhex) {
            var color = new PatternFill {
                PatternType = PatternValues.Solid,
                ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString(colorhex) },
                BackgroundColor = new BackgroundColor { Indexed = 64 }
            };
            return color;
        }


        private void SetColumnWidth(OpenXmlWriter writer, ApplicationSchemaDefinition schema) {
            Columns cs = new Columns();
            writer.WriteStartElement(cs);

            var columnIndex = 1;
            IEnumerable<ApplicationFieldDefinition> applicationFields = schema.Fields;
            foreach (var applicationField in applicationFields) {
                double excelWidthAux;
                string excelwidthAux;
                string exporttoexcel;
                applicationField.RendererParameters.TryGetValue("exporttoexcel", out exporttoexcel);
                if ((exporttoexcel != null && !exporttoexcel.Equals("true")) || (applicationField.IsHidden && exporttoexcel == null)) {
                    continue;
                }
                applicationField.RendererParameters.TryGetValue("excelwidth", out excelwidthAux);
                double.TryParse(excelwidthAux, out excelWidthAux);
                var excelWidth = excelWidthAux > 0 ? excelWidthAux : (applicationField.Label.Length * 1.5);

                Column column = new Column();
                column.Width = excelWidth;

                column.Min = Convert.ToUInt32(columnIndex);
                column.Max = Convert.ToUInt32(columnIndex);
                // write column
                writer.WriteStartElement(column);
                writer.WriteEndElement();

                columnIndex++;
            }
            // end columns
            writer.WriteEndElement();
        }


    }
}
