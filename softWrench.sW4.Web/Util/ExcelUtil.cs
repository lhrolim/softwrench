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

namespace softWrench.sW4.Web.Util {
    public class ExcelUtil : ISingletonComponent {

        public SLDocument ConvertGridToExcel(string application, ApplicationMetadataSchemaKey key, ApplicationListResult result) {
            IEnumerable<ApplicationFieldDefinition> applicationFields = result.Schema.Fields;
            //var theme = new SLThemeSettings();
            //theme.Accent1Color = System.Drawing.Color.Yellow;
            //theme.Accent2Color = System.Drawing.Color.Blue;
            //var excelFile = new SLDocument(theme);

            //var defaultStyle = excelFile.CreateStyle();
            //defaultStyle.Alignment.Horizontal = HorizontalAlignmentValues.Left;
            //defaultStyle.Alignment.Vertical = VerticalAlignmentValues.Bottom;
            //defaultStyle.Font.FontName = "Calibri";
            //defaultStyle.Font.FontSize = 10;
            //defaultStyle.SetWrapText(true);

            //var headerStyle = excelFile.CreateStyle();
            //headerStyle.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            //headerStyle.Alignment.Vertical = VerticalAlignmentValues.Bottom;
            //headerStyle.Font.FontName = "Calibri";
            //headerStyle.Font.FontSize = 10;
            //headerStyle.Font.Bold = true;

            //var solidColorStyle = excelFile.CreateStyle();
            //solidColorStyle.Fill.SetPatternType(PatternValues.Solid);

            //BuildHeader(excelFile, headerStyle, applicationFields);

            //var rowIndex = 1;
            //var currentPage = 1;

            ////while (currentPage <= result.PageCount) {
            ////    var requestDTO = PaginatedSearchRequestDto.DefaultInstance();
            ////    requestDTO.PageNumber = currentPage;

            ////    dataResponse = new DataController().Get(application,
            ////                                            new DataRequestAdapter() {
            ////                                                Key = key,
            ////                                                SearchDTO = requestDTO
            ////                                            });
            ////    result = dataResponse as ApplicationListResult;

            ////Export To Excel even in case the Field is Hidden in the Grid.
            //string value = string.Empty;

            //foreach (var item in result.ResultObject) {
            //    rowIndex++;
            //    var attributes = item.Attributes;
            //    var columnIndex = 1;

            //    var nonHiddenFields = applicationFields.Where(f => !f.IsHidden || (f.Renderer.ParametersAsDictionary().TryGetValue(FieldRendererConstants.EXPORTTOEXCEL, out value)));
            //    foreach (var applicationField in nonHiddenFields) {
            //        object dataAux;
            //        attributes.TryGetValue(applicationField.Attribute, out dataAux);
            //        var data = dataAux == null ? string.Empty : dataAux.ToString();

            //        var style = defaultStyle;
            //        FillCell(excelFile, rowIndex, columnIndex, data, defaultStyle);

            //        if (applicationField.Attribute == "status" && ApplicationConfiguration.ClientName == "hapag")
            //        {

            //            switch (data.Trim())
            //            {
            //                case "NEW":
            //                    solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Orange);
            //                    break;
            //                case "QUEUED":
            //                    solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Yellow);
            //                    break;
            //                case "INPROG":
            //                    solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Yellow);
            //                    break;
            //                case "CLOSED":
            //                    solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Green);
            //                    break;
            //                case "CANCELLED":
            //                    solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Red);
            //                    break;
            //                case "RESOLVED":
            //                    solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Blue);
            //                    break;
            //                case "SLAHOLD":
            //                    solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Blue);
            //                    break;
            //                case "RESOLVCONF":
            //                    solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Green);
            //                    break;
            //                default:
            //                    solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Transparent);
            //                    break;
            //            }
            //            //FillCell(excelFile, rowIndex, columnIndex, data, solidColorStyle);
            //            style = solidColorStyle;
            //        }
            //        FillCell(excelFile, rowIndex, columnIndex, data, style);
            //        columnIndex++;
            //    }
            //    excelFile.AutoFitRow(rowIndex);
            //}
            
            //SetColumnWidth(excelFile, applicationFields);
            //return excelFile;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                SpreadsheetDocument xl = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
                List<OpenXmlAttribute> oxa;
                OpenXmlWriter oxw;

                xl.AddWorkbookPart();
                WorksheetPart wsp = xl.WorkbookPart.AddNewPart<WorksheetPart>();

                oxw = OpenXmlWriter.Create(wsp);

                oxw.WriteStartElement(new Worksheet());
                oxw.WriteStartElement(new SheetData());

                var stylesPart = xl.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = new Stylesheet();

                // blank font list
                stylesPart.Stylesheet.Fonts = new Fonts();
                stylesPart.Stylesheet.Fonts.Count = 1;
                FontName name = new FontName();
                name.Val = "Calibri";
                FontSize size = new FontSize();
                size.Val = 10;
                stylesPart.Stylesheet.Fonts.AppendChild(new Font {FontName = name, FontSize = size });

                // create fills
                stylesPart.Stylesheet.Fills = new Fills();

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
                stylesPart.Stylesheet.CellFormats = new CellFormats();
                // empty one for index 0, seems to be required
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat());
                // no color
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 0, BorderId = 0, FillId = 0, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Left, WrapText = BooleanValue.FromBoolean(true), Vertical = VerticalAlignmentValues.Top });
                // red
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 0, BorderId = 0, FillId = 2, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Left, WrapText = BooleanValue.FromBoolean(true), Vertical = VerticalAlignmentValues.Top });
                // green
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 0, BorderId = 0, FillId = 3, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Left, WrapText = BooleanValue.FromBoolean(true), Vertical = VerticalAlignmentValues.Top });
                // yellow
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 0, BorderId = 0, FillId = 4, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Left, WrapText = BooleanValue.FromBoolean(true), Vertical = VerticalAlignmentValues.Top });
                // orange
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 0, BorderId = 0, FillId = 5, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Left, WrapText = BooleanValue.FromBoolean(true), Vertical = VerticalAlignmentValues.Top });
                // blue
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 0, BorderId = 0, FillId = 6, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Left, WrapText = BooleanValue.FromBoolean(true), Vertical = VerticalAlignmentValues.Top });
               
                stylesPart.Stylesheet.CellFormats.Count = 7;
               
                
                string value = string.Empty;
                int rowIdx = 1;
                foreach (var item in result.ResultObject)
                {
                   var attributes = item.Attributes;

                   oxa = new List<OpenXmlAttribute>();
                   // this is the row index
                   oxa.Add(new OpenXmlAttribute("r", null, rowIdx.ToString()));

                   oxw.WriteStartElement(new Row(), oxa);

                   int cellIdx = 1;
                   var nonHiddenFields = applicationFields.Where(f => !f.IsHidden || (f.Renderer.ParametersAsDictionary().TryGetValue(FieldRendererConstants.EXPORTTOEXCEL, out value)));
                   foreach (var applicationField in nonHiddenFields)
                   {
                       object dataAux;
                       attributes.TryGetValue(applicationField.Attribute, out dataAux);
                       var data = dataAux == null ? string.Empty : dataAux.ToString();

                       oxa = new List<OpenXmlAttribute>();
                       // this is the data type ("t"), with CellValues.String ("str")
                       oxa.Add(new OpenXmlAttribute("t", null, "str"));
                       
                       switch (data.Trim())
                       {
                           case "NEW":
                               //solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Orange);
                               oxa.Add(new OpenXmlAttribute("s", null, "5"));
                               break;
                           case "QUEUED":
                               //solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Yellow);
                               oxa.Add(new OpenXmlAttribute("s", null, "4"));
                               break;
                           case "INPROG":
                              // solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Yellow);
                               oxa.Add(new OpenXmlAttribute("s", null, "4"));
                               break;
                           case "CLOSED":
                               //solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Green);
                               oxa.Add(new OpenXmlAttribute("s", null, "3"));
                               break;
                           case "CANCELLED":
                               //solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Red);
                               oxa.Add(new OpenXmlAttribute("s", null, "2"));
                               break;
                           case "RESOLVED":
                              // solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Blue);
                               oxa.Add(new OpenXmlAttribute("s", null, "6"));
                               break;
                           case "SLAHOLD":
                              // solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Blue);
                               oxa.Add(new OpenXmlAttribute("s", null, "6"));
                               break;
                           case "RESOLVCONF":
                               //solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Green);
                               oxa.Add(new OpenXmlAttribute("s", null, "3"));
                               break;
                           default:
                              // solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Transparent);
                               oxa.Add(new OpenXmlAttribute("s", null, "1"));
                               break;
                       }
                       

                       oxw.WriteStartElement(new Cell(), oxa);

                       oxw.WriteElement(new CellValue(data.ToString()));

                       // this is for Cell
                       oxw.WriteEndElement();

                       cellIdx++;
                   }

                   rowIdx++;
                   // this is for Row
                   oxw.WriteEndElement();
                }

                
                // this is for Worksheet
                oxw.WriteEndElement();
                oxw.Close();

                oxw = OpenXmlWriter.Create(xl.WorkbookPart);
                oxw.WriteStartElement(new Workbook());
                oxw.WriteStartElement(new Sheets());

                // you can use object initialisers like this only when the properties
                // are actual properties. SDK classes sometimes have property-like properties
                // but are actually classes. For example, the Cell class has the CellValue
                // "property" but is actually a child class internally.
                // If the properties correspond to actual XML attributes, then you're fine.
                oxw.WriteElement(new Sheet()
                {
                    Name = "Sheet1",
                    SheetId = 1,
                    Id = xl.WorkbookPart.GetIdOfPart(wsp)
                });

                // this is for Sheets
                oxw.WriteEndElement();
                // this is for Workbook
                oxw.WriteEndElement();
                oxw.Close();

                xl.Close();


                var excelFile = new SLDocument(ms);
                return excelFile;
            }
        }


        private void buildFills(WorkbookStylesPart stylesPart)
        {
            // create a solid red fill
            var solidRed = new PatternFill() { PatternType = PatternValues.Solid };
            solidRed.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FFFF0000") }; // red fill
            solidRed.BackgroundColor = new BackgroundColor { Indexed = 64 };

            // create green
            var green = new PatternFill() { PatternType = PatternValues.Solid };
            green.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FF006836") }; 
            green.BackgroundColor = new BackgroundColor { Indexed = 64 };

            // create yellow
            var yellow = new PatternFill() { PatternType = PatternValues.Solid };
            yellow.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FFffff00") }; 
            yellow.BackgroundColor = new BackgroundColor { Indexed = 64 };

            // create orange
            var orange = new PatternFill() { PatternType = PatternValues.Solid };
            orange.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FFff7d00") }; 
            orange.BackgroundColor = new BackgroundColor { Indexed = 64 };

            // create blue
            var blue = new PatternFill() { PatternType = PatternValues.Solid };
            blue.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("ff002fff") }; 
            blue.BackgroundColor = new BackgroundColor { Indexed = 64 };


            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel

            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = solidRed });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = green });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = yellow });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = orange });
            stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = blue });
            stylesPart.Stylesheet.Fills.Count = 7;
        }


        private static void BuildHeader(SLDocument excelFile, SLStyle headerStyle, IEnumerable<ApplicationFieldDefinition> applicationFields) {
            var i = 1;
            string value = string.Empty;
            foreach (var applicationField in applicationFields) {
                //Exporting to Excel, even if field is hidden
                if (applicationField.IsHidden && !applicationField.Renderer.ParametersAsDictionary().TryGetValue(FieldRendererConstants.EXPORTTOEXCEL, out value)) {
                    continue;
                }
                excelFile.SetCellValue(1, i, applicationField.Label);
                excelFile.SetCellStyle(1, i, headerStyle);
                i++;
            }
        }

        private static void FillCell(SLDocument excelFile, int rowIndex, int columnIndex, string data, SLStyle slStyle) {
            DateTime dtTimeAux;
            var dataToCell = DateTime.TryParse(data, out dtTimeAux) ?
                dtTimeAux.ToString("dd/MM/yyyy H:mm") : data;
            
            excelFile.SetCellValue(rowIndex, columnIndex, dataToCell);
            excelFile.SetCellStyle(rowIndex, columnIndex, slStyle);
        }

        private static void SetColumnWidth(SLDocument excelFile, IEnumerable<ApplicationFieldDefinition> applicationFields) {
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
