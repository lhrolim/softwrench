using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Http;
using DocumentFormat.OpenXml.Spreadsheet;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Controllers;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using SpreadsheetLight;
using log4net;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace softWrench.sW4.Web.Util {
    public class ExcelUtil : ISingletonComponent {

        private Dictionary<String, String> cellStyleDictionary;

        public ExcelUtil() {
            // setup style dictionary for back colors
            cellStyleDictionary = new Dictionary<string, string>();
            cellStyleDictionary.Add("NEW", "5");
            cellStyleDictionary.Add("QUEUED", "4");
            cellStyleDictionary.Add("INPROG", "4");
            cellStyleDictionary.Add("CLOSED", "3");
            cellStyleDictionary.Add("CANCELLED", "2");
            cellStyleDictionary.Add("RESOLVED", "6");
            cellStyleDictionary.Add("SLAHOLD", "6");
            cellStyleDictionary.Add("RESOLVCONF", "3");
        }

        public SLDocument ConvertGridToExcel(string application, ApplicationMetadataSchemaKey key, ApplicationListResult result) {
            IEnumerable<ApplicationFieldDefinition> applicationFields = result.Schema.Fields;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            using (SpreadsheetDocument xl = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook)) {
                // attributes of elements
                List<OpenXmlAttribute> xmlAttributes;
                // the xml writer
                OpenXmlWriter writer;

                xl.AddWorkbookPart();
                WorksheetPart worksheetpart = xl.WorkbookPart.AddNewPart<WorksheetPart>();

                writer = OpenXmlWriter.Create(worksheetpart);

                Worksheet worksheet = new Worksheet();
                writer.WriteStartElement(worksheet);

                // create sheetdata element
                writer.WriteStartElement(new SheetData());

                var stylesPart = xl.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = new Stylesheet();

                // create 2 fonts (one normal, one header)
                createFonts(stylesPart);


                // create fills
                buildFills(stylesPart);

                // blank border list
                stylesPart.Stylesheet.Borders = new Borders();
                stylesPart.Stylesheet.Borders.Count = 1;
                stylesPart.Stylesheet.Borders.AppendChild(new Border());

                // blank cell format list
                stylesPart.Stylesheet.CellStyleFormats = new CellStyleFormats();
                stylesPart.Stylesheet.CellStyleFormats.Count = 1;
                stylesPart.Stylesheet.CellStyleFormats.AppendChild(new CellFormat());

                // cell format list
                createCellFormats(stylesPart);

                int rowIdx = 1;
                string value = string.Empty;

                // create header row
                createHeaderRow(applicationFields, writer, rowIdx, ref value);
                //count up row
                rowIdx++;

                // write data rows
                value = string.Empty;
                foreach (var item in result.ResultObject) {
                    var attributes = item.Attributes;

                    // create new row
                    xmlAttributes = new List<OpenXmlAttribute>();
                    xmlAttributes.Add(new OpenXmlAttribute("r", null, rowIdx.ToString()));
                    writer.WriteStartElement(new Row(), xmlAttributes);

                    var nonHiddenFields = applicationFields.Where(ShouldShowField());
                    foreach (var applicationField in nonHiddenFields) {
                        object dataAux;
                        attributes.TryGetValue(applicationField.Attribute, out dataAux);
                        var data = dataAux == null ? string.Empty : dataAux.ToString();

                        xmlAttributes = new List<OpenXmlAttribute>();
                        // this is the data type ("t"), with CellValues.String ("str")
                        xmlAttributes.Add(new OpenXmlAttribute("t", null, "str"));

                        // that's the default style
                        var styleId = "1";
                        if (applicationField.Attribute.Contains("status") && ApplicationConfiguration.ClientName == "hapag") {
                            bool success = cellStyleDictionary.TryGetValue(data.Trim(), out styleId);
                            if (!success) {
                                // check if status is something like NEW 1/4 (and make sure it doesn't match e.g. NEW REQUEST).
                                Match match = Regex.Match(data.Trim(), "(([A-Z]+ )+)[1-9]+/[1-9]+");
                                if (match.Success) {
                                    String status = match.Groups[2].Value.Trim();
                                    bool compundStatus = cellStyleDictionary.TryGetValue(status, out styleId);
                                    if (!compundStatus)
                                        styleId = "1";
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
                        var dataToCell = DateTime.TryParse(data, out dtTimeAux) ?
                            dtTimeAux.ToString("dd/MM/yyyy H:mm") : data;
                        writer.WriteElement(new CellValue(dataToCell.ToString()));
                        // end cell
                        writer.WriteEndElement();
                    }

                    // next row
                    rowIdx++;
                    // end row
                    writer.WriteEndElement();
                }


                // end worksheet
                writer.WriteEndElement();
                writer.Close();


                // write root element
                writer = OpenXmlWriter.Create(xl.WorkbookPart);
                writer.WriteStartElement(new Workbook());
                writer.WriteStartElement(new Sheets());

                writer.WriteElement(new Sheet() {
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
                return excelFile;
            }
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

        private void createHeaderRow(IEnumerable<ApplicationFieldDefinition> applicationFields, OpenXmlWriter writer, int rowIdx, ref string value) {
            List<OpenXmlAttribute> xmlAttributes;
            xmlAttributes = new List<OpenXmlAttribute>();
            xmlAttributes.Add(new OpenXmlAttribute("r", null, rowIdx.ToString()));
            writer.WriteStartElement(new Row(), xmlAttributes);
            foreach (var applicationField in applicationFields) {
                //Exporting to Excel, even if field is hidden
                if (applicationField.IsHidden && !applicationField.Renderer.ParametersAsDictionary().TryGetValue(FieldRendererConstants.Exporttoexcel, out value)) {
                    continue;
                }

                xmlAttributes = new List<OpenXmlAttribute>();
                // add new datatype for cell
                xmlAttributes.Add(new OpenXmlAttribute("t", null, "str"));
                // add header style
                xmlAttributes.Add(new OpenXmlAttribute("s", null, "7"));

                writer.WriteStartElement(new Cell(), xmlAttributes);
                writer.WriteElement(new CellValue(applicationField.Label));

                // this is for Cell
                writer.WriteEndElement();
            }

            // end Row
            writer.WriteEndElement();
        }

        private void createCellFormats(WorkbookStylesPart stylesPart) {
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

            // header style
            createCellFormat(stylesPart, 0, 1, 0, 0, true);

            stylesPart.Stylesheet.CellFormats.Count = 8;
        }

        private void createCellFormat(WorkbookStylesPart stylesPart, UInt32 formatId, UInt32 fontId, UInt32 borderId, UInt32 fillId, bool applyFill) {
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
            FontName name = new FontName();
            name.Val = "Calibri";
            FontSize size = new FontSize();
            size.Val = 10;
            stylesPart.Stylesheet.Fonts.AppendChild(new Font { FontName = name, FontSize = size });

            // bold font for header
            Bold boldFont = new Bold();
            stylesPart.Stylesheet.Fonts.AppendChild(new Font { FontName = (FontName)name.Clone(), FontSize = (FontSize)size.Clone(), Bold = boldFont });

            stylesPart.Stylesheet.Fonts.Count = 2;
        }

        private void buildFills(WorkbookStylesPart stylesPart) {
            stylesPart.Stylesheet.Fills = new Fills();

            // create a solid red fill
            var solidRed = new PatternFill() { PatternType = PatternValues.Solid };
            solidRed.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FFFF0000") }; // red fill
            solidRed.BackgroundColor = new BackgroundColor { Indexed = 64 };

            // create green
            var green = createColor("FF006836");
            // create yellow
            var yellow = createColor("FFffff00");
            // create orange
            var orange = createColor("FFff7d00");
            // create blue
            var blue = createColor("ff002fff");

            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel

            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = solidRed });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = green });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = yellow });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = orange });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = blue });

            stylesPart.Stylesheet.Fills.Count = 7;
        }

        private PatternFill createColor(string colorhex) {
            var color = new PatternFill() { PatternType = PatternValues.Solid };
            color.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString(colorhex) };
            color.BackgroundColor = new BackgroundColor { Indexed = 64 };
            return color;
        }


        private void SetColumnWidth(SLDocument excelFile, IEnumerable<ApplicationFieldDefinition> applicationFields) {
            var columnIndex = 1;
            foreach (var applicationField in applicationFields) {
                double excelWidthAux;
                string excelwidthAux;
                if (applicationField.IsHidden) {
                    continue;
                }
                applicationField.RendererParameters.TryGetValue("excelwidth", out excelwidthAux);
                double.TryParse(excelwidthAux, out excelWidthAux);
                var excelWidth = excelWidthAux > 0 ? excelWidthAux : (applicationField.Label.Length * 1.5);
                excelFile.SetColumnWidth(columnIndex, excelWidth);
                columnIndex++;
            }
        }


    }
}
