using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Http;
using DocumentFormat.OpenXml.Spreadsheet;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Controllers;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using SpreadsheetLight;

namespace softWrench.sW4.Web.Util {
    public class ExcelUtil : ISingletonComponent {

        public SLDocument ConvertGridToExcel(string application, ApplicationMetadataSchemaKey key, ApplicationListResult result) {
            IEnumerable<ApplicationFieldDefinition> applicationFields = result.Schema.Fields;

            var theme = new SLThemeSettings();
            theme.Accent1Color = System.Drawing.Color.Yellow;
            theme.Accent2Color = System.Drawing.Color.Blue;
            var excelFile = new SLDocument(theme);

            var defaultStyle = excelFile.CreateStyle();
            defaultStyle.Alignment.Horizontal = HorizontalAlignmentValues.Left;
            defaultStyle.Alignment.Vertical = VerticalAlignmentValues.Bottom;
            defaultStyle.Font.FontName = "Calibri";
            defaultStyle.Font.FontSize = 10;
            defaultStyle.SetWrapText(true);

            var headerStyle = excelFile.CreateStyle();
            headerStyle.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            headerStyle.Alignment.Vertical = VerticalAlignmentValues.Bottom;
            headerStyle.Font.FontName = "Calibri";
            headerStyle.Font.FontSize = 10;
            headerStyle.Font.Bold = true;

            var solidColorStyle = excelFile.CreateStyle();
            solidColorStyle.Fill.SetPatternType(PatternValues.Solid);

            BuildHeader(excelFile, headerStyle, applicationFields);

            var rowIndex = 2;
            var currentPage = 1;

            //while (currentPage <= result.PageCount) {
            //    var requestDTO = PaginatedSearchRequestDto.DefaultInstance();
            //    requestDTO.PageNumber = currentPage;

            //    dataResponse = new DataController().Get(application,
            //                                            new DataRequestAdapter() {
            //                                                Key = key,
            //                                                SearchDTO = requestDTO
            //                                            });
            //    result = dataResponse as ApplicationListResult;

            //Export To Excel even in case the Field is Hidden in the Grid.
            string value = string.Empty;

            foreach (var item in result.ResultObject) {
                rowIndex++;
                var attributes = item.Attributes;
                var columnIndex = 1;

                var nonHiddenFields = applicationFields.Where(f => !f.IsHidden || (f.Renderer.ParametersAsDictionary().TryGetValueAsString(FieldRendererConstants.EXPORTTOEXCEL, out value)));
                foreach (var applicationField in nonHiddenFields) {
                    object dataAux;
                    attributes.TryGetValue(applicationField.Attribute, out dataAux);
                    var data = dataAux == null ? string.Empty : dataAux.ToString();

                    FillCell(excelFile, rowIndex, columnIndex, data, defaultStyle);

                    if (applicationField.Attribute == "status" && ApplicationConfiguration.ClientName == "hapag") {

                        switch (data.Trim()) {
                            case "NEW":
                                solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Orange);
                                break;
                            case "QUEUED":
                                solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Yellow);
                                break;
                            case "INPROG":
                                solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Yellow);
                                break;
                            case "CLOSED":
                                solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Green);
                                break;
                            case "CANCELLED":
                                solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Red);
                                break;
                            case "RESOLVED":
                                solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Blue);
                                break;
                            case "SLAHOLD":
                                solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Blue);
                                break;
                            case "RESOLVCONF":
                                solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Green);
                                break;
                            default:
                                solidColorStyle.Fill.SetPatternForegroundColor(System.Drawing.Color.Transparent);
                                break;
                        }
                        FillCell(excelFile, rowIndex, columnIndex, data, solidColorStyle);
                    }
                    columnIndex++;
                }
                excelFile.AutoFitRow(rowIndex);
            }
            SetColumnWidth(excelFile, applicationFields);
            return excelFile;
        }


        private static void BuildHeader(SLDocument excelFile, SLStyle headerStyle, IEnumerable<ApplicationFieldDefinition> applicationFields) {
            var i = 1;
            string value = string.Empty;
            foreach (var applicationField in applicationFields) {
                //Exporting to Excel, even if field is hidden
                if (applicationField.IsHidden && !applicationField.Renderer.ParametersAsDictionary().TryGetValueAsString(FieldRendererConstants.EXPORTTOEXCEL, out value)) {
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
                applicationField.RendererParameters.TryGetValueAsString("excelwidth", out excelwidthAux);
                double.TryParse(excelwidthAux, out excelWidthAux);
                var excelWidth = excelWidthAux > 0 ? excelWidthAux : (applicationField.Label.Length * 1.5);
                excelFile.SetColumnWidth(columnIndex, excelWidth);
                columnIndex++;
            }
        }


    }
}
