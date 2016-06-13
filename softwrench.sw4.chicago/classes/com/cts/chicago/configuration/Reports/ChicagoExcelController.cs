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
            var datamaps = BuildDatamapsFromQuery(queryResult);

            var schema = new ApplicationSchemaDefinition {
                ApplicationName = ApplicationName,
                Displayables = departmentDisplayables
            };

            var listResult = ApplicationListResult.FixedListResult(datamaps, schema);
            return DoExport("departmentcount.xlsx", listResult, SecurityFacade.CurrentUser());

        }

        private static IEnumerable<DataMap> BuildDatamapsFromQuery(List<Dictionary<string, string>> queryResult) {

            var datamaps = new List<DataMap>();
            var dateTotal = 0;
            string currentDate = null;
            var currentDatamap = new Dictionary<string, string>();

            foreach (var row in queryResult) {
                var rowDate = row["creationdate"];
                var rowDepartment = row["department"];
                var countByDeptDate = row["countnumber"];



                if (rowDate != currentDate) {
                    if (currentDate != null) {
                        //a new date has been reached --> let's finish the currentdatamap
                        currentDatamap = CloseLine(currentDatamap, datamaps, ref dateTotal);
                    }
                    currentDate = rowDate;

                    //first time, date was null
                    if (!currentDatamap.ContainsKey("creationdate")) {
                        //first column of the excel will be a date
                        currentDatamap["creationdate"] = currentDate;
                    }
                    currentDatamap[rowDepartment] = countByDeptDate;

                    dateTotal += int.Parse(countByDeptDate);

                } else {
                    //same date, but for a different department
                    currentDatamap[rowDepartment] = countByDeptDate;
                    dateTotal += int.Parse(countByDeptDate);
                }
            }

            //add last date found
            CloseLine(currentDatamap, datamaps, ref dateTotal);
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
