using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using cts.commons.web.Controller;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Metadata.Applications.UI;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using CompressionUtil = cts.commons.Util.CompressionUtil;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.configuration.Reports {
    public class ChicagoExcelController : FileDownloadController {
        private const string ApplicationName = "servicerequest";

        private readonly IMaximoHibernateDAO _dao;
        private readonly ExcelUtil _excelUtil;

        public ChicagoExcelController(ExcelUtil excelUtil, IMaximoHibernateDAO dao) {
            _excelUtil = excelUtil;
            _dao = dao;
        }
        /// <summary>
        /// This report should output the total number of tickets openend by each department on the current year, on a tickettype grouping
        /// https://controltechnologysolutions.atlassian.net/browse/SWWEB-2543
        /// </summary>
        /// <returns></returns>
        public FileContentResult TicketTypeByDepartment() {
            var departmentQueryResult = _dao.FindByNativeQuery("Select distinct (tickettype) as tickettype from sr where tickettype is not null order by tickettype");
            var departmentDisplayables = new List<IApplicationDisplayable>{
                new ApplicationFieldDefinition(ApplicationName, "department", "City Department")
            };
            departmentDisplayables.AddRange(departmentQueryResult.Select(s => s["tickettype"]).ToHashSet().Select(s => new ApplicationFieldDefinition(ApplicationName, s, s)));
            departmentDisplayables.Add(new ApplicationFieldDefinition(ApplicationName, "total", "Total"));

            var queryResult = _dao.FindByNativeQuery(ChicagoReportProvider.GetTicketTypeByDepartment());

            var datamaps = BuildDatamapsFromQuery(queryResult, "department", "tickettype");

            var schema = new ApplicationSchemaDefinition {
                ApplicationName = ApplicationName,
                Displayables = departmentDisplayables
            };

            var listResult = ApplicationListResult.FixedListResult(datamaps, schema);
            return DoExport("DepartmentByType.xlsx", listResult, SecurityFacade.CurrentUser());
        }

        /// <summary>
        /// This report should output the total number of tickets openend by each department on the current year, on a daily basis grouping
        /// https://controltechnologysolutions.atlassian.net/browse/SWWEB-2547
        /// </summary>
        /// <returns></returns>
        public FileContentResult GetDepartmentCount() {


            var departmentQueryResult = _dao.FindByNativeQuery("Select distinct (department) as department from sr where department is not null order by department");

            var departmentDisplayables = new List<IApplicationDisplayable>{
                new ApplicationFieldDefinition(ApplicationName, "creationdate", "Date")
                {
                    Renderer =  new FieldRenderer("datetime","format=MM/dd/yyyy",null,null)
                }
            };

            departmentDisplayables.AddRange(departmentQueryResult.Select(s => s["department"]).ToHashSet().Select(s => new ApplicationFieldDefinition(ApplicationName, s, s)));
            departmentDisplayables.Add(new ApplicationFieldDefinition(ApplicationName, "total", "Total"));
            var queryResult = _dao.FindByNativeQuery(ChicagoReportProvider.GetTicketByDepartmentQuery());


            //Building excel spreadsheet, refer to https://controltechnologysolutions.atlassian.net/browse/SWWEB-2547
            var datamaps = BuildDatamapsFromQuery(queryResult, "creationdate", "department");

            var schema = new ApplicationSchemaDefinition {
                ApplicationName = ApplicationName,
                Displayables = departmentDisplayables
            };

            var listResult = ApplicationListResult.FixedListResult(datamaps, schema);
            return DoExport("departmentcount.xlsx", listResult, SecurityFacade.CurrentUser());

        }
        /// <summary>
        /// This method converts the query results which is grouped using the rowattribute and columnatrribute 
        /// into an excel format where lines get summed based on the rowattribute value (creationdate, or departmenttype).
        /// </summary>
        /// <param name="queryResult"></param>
        /// <param name="rowAttribute"></param>
        /// <param name="columnAttribute"></param>
        /// <returns></returns>
        private static IEnumerable<DataMap> BuildDatamapsFromQuery(IEnumerable<Dictionary<string, string>> queryResult, string rowAttribute, string columnAttribute) {

            var datamaps = new List<DataMap>();
            var rowTotal = 0;
            string currentRow = null;
            //an iterator for the current row being parsed from the query
            var currentDatamapIterator = new Dictionary<string, string>();
            //this will hold a row for the sum of all the data
            var lineGrandTotal = new Dictionary<string, object>();
            lineGrandTotal[rowAttribute] = "Grand Total";

            foreach (var row in queryResult) {
                var rowAttributeValue = row[rowAttribute];
                var columnAttributeValue = row[columnAttribute];
                var countByDeptDate = row["countnumber"];

                //either the first time we're dealing with that column, or summing up to the already existing totals
                if (!lineGrandTotal.ContainsKey(columnAttributeValue)) {
                    lineGrandTotal[columnAttributeValue] = int.Parse(countByDeptDate);
                } else {
                    var storedValue = (int)lineGrandTotal[columnAttributeValue];
                    lineGrandTotal[columnAttributeValue] = storedValue + int.Parse(countByDeptDate);
                }

                if (rowAttributeValue != currentRow) {
                    if (currentRow != null) {
                        //a new date has been reached --> let's finish the currentdatamap
                        currentDatamapIterator = CloseLine(currentDatamapIterator, datamaps, ref rowTotal);
                    }
                    currentRow = rowAttributeValue;

                    //first time, date was null
                    if (!currentDatamapIterator.ContainsKey(rowAttribute)) {
                        currentDatamapIterator[rowAttribute] = currentRow;
                    }
                    currentDatamapIterator[columnAttributeValue] = countByDeptDate;

                    rowTotal += int.Parse(countByDeptDate);

                } else {
                    //same date, but for a different department
                    currentDatamapIterator[columnAttributeValue] = countByDeptDate;
                    rowTotal += int.Parse(countByDeptDate);
                }
            }

            //add last date found
            CloseLine(currentDatamapIterator, datamaps, ref rowTotal);
            lineGrandTotal["total"] = lineGrandTotal.Values.OfType<int>().Sum(s=> (int)s);

            datamaps.Add(DataMap.GetInstanceFromDictionary(ApplicationName, lineGrandTotal));

            return datamaps;
        }

        private static Dictionary<string, string> CloseLine(Dictionary<string, string> currentDatamap, List<DataMap> datamaps, ref int dateTotal) {
            currentDatamap["total"] = dateTotal.ToString();
            datamaps.Add(DataMap.GetInstanceFromStringDictionary(ApplicationName, currentDatamap));
            currentDatamap = new Dictionary<string, string>();
            dateTotal = 0;
            return currentDatamap;
        }


        protected FileContentResult DoExport(string fileName, ApplicationListResult dataResponse, InMemoryUser loggedInUser) {

            var excelFile = _excelUtil.ConvertGridToExcel(dataResponse, loggedInUser);
            using (var stream = new MemoryStream()) {
                excelFile.SaveAs(stream);
                stream.Close();
                var result = new FileContentResult(CompressionUtil.Compress(stream.ToArray()),
                    System.Net.Mime.MediaTypeNames.Application.Octet) {
                    FileDownloadName = (string)StringUtil.FirstLetterToUpper(fileName)
                };
                Response.AddHeader("Content-encoding", "gzip");
                return result;
            }
        }

    }
}
