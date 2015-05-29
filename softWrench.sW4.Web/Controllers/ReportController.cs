﻿using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.Report;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using System;
using System.Reflection;
using softWrench.sW4.Data.Pagination;
using System.Text;
using softWrench.sW4.Data.Search;
using System.Text.RegularExpressions;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Web.Security;

namespace softWrench.sW4.Web.Controllers {
    [ApplicationAuthorize]
    public class ReportController : Controller {

        private readonly DataController _dataController;
        private const string MISSING = " ";
        public ReportController(DataController dataController) {
            _dataController = dataController;            
        }

        //
        // GET: /Report/
        public ActionResult Index(string application, [FromUri] ReportRequest request) {
            
            // this is necessary for opening the report in a new window
            if (request.ReportName == null ) {
                return View("Index");
            }

            if (request.SearchDTO == null) {
                request.SearchDTO = new PaginatedSearchRequestDto();
            }

            request.SearchDTO.ShouldPaginate = false;
            request.Key.Mode = SchemaMode.input;
            request.Key.Platform = ClientPlatform.Web;
            
            var dataResponse = _dataController.Get(application, request);
            var data = new DataTable();

            if (dataResponse is ApplicationListResult) {
                var listData = ((ApplicationListResult)dataResponse).ResultObject;
                var schema = ((ApplicationListResult)dataResponse).Schema;
                data = ReportUtil.ConvertMapListToTable(
                    listData.Select(f => f.Attributes).ToList(),
                    schema
                    );
            } else if (dataResponse is ApplicationDetailResult) {
                var detailData = ((ApplicationDetailResult)dataResponse).ResultObject;
                var schema = ((ApplicationDetailResult)dataResponse).Schema;
                data = ReportUtil.ConvertMapListToTable(
                    new List<IDictionary<string, object>>() { detailData.Attributes },
                    schema
                    );
            }

            var rdlc = ReportUtil.GetDescriptorPath(request.ReportName);
            var dsName = ReportUtil.GetDataSetName(request.ReportName);

            var parameters = new Dictionary<string, string>();

            if (request.SearchDTO != null && !String.IsNullOrEmpty(request.SearchDTO.SearchParams) && 
                !String.IsNullOrEmpty(request.SearchDTO.SearchValues)) {
                parameters = (Dictionary<string, string>)
                    PopulateReportParamters(request.SearchDTO.SearchParams, request.SearchDTO.SearchValues);
            }
            //Adding SCIM to parameters for IncidentPerLocationSCIMReport
            string scimMatch = "SCIM";
            Regex regex = new Regex(scimMatch, RegexOptions.IgnoreCase);
            if (regex.IsMatch(request.ReportName) && parameters.ContainsKey("commodities_description_header")) {                
                string[] arrTemp = parameters["commodities_description_header"].Split('-');
                parameters.Add("system_header",arrTemp[0] + '-' + arrTemp[1]);
                parameters.Add("component_header",(arrTemp.Length > 2) ? arrTemp[2] : MISSING);
                parameters.Add("item_header",(arrTemp.Length > 3) ? arrTemp[3] : MISSING);
                parameters.Add("module_header", (arrTemp.Length > 4) ? ModuleFieldValue(arrTemp) : MISSING);
            } else if (request.ReportName.Equals("ITCReport") && parameters.ContainsKey("persongroup_description_header")) {
                var strbld = new StringBuilder();
                foreach (var location in parameters["persongroup_description_header"].Split(',')) {
                    var siteAndCostCenter = location.Split('-');
                    strbld.Append("HLAG-");
                    strbld.Append(siteAndCostCenter[0]);
                    strbld.Append(" - ");
                    strbld.Append(siteAndCostCenter[1]);
                    strbld.Append(", ");
                }
                if (strbld.Length > 2) strbld.Remove(strbld.Length - 2, 2);
                parameters["persongroup_description_header"] = strbld.ToString();

                var user = SecurityFacade.CurrentUser();
                parameters["requestor"] = user.FirstName + " " + user.LastName;
            }            

            return View("Index", new ReportModel(rdlc, dsName, data, parameters));            
        }
                
        /// <summary>
        /// Populate the parameters with the filters selected
        /// </summary>
        /// <param name="searchParams">Selected Filters</param>
        /// <param name="searchValues">Selected Filter Values</param>
        /// <returns>Key Value pair of Filter and its Corresponding value</returns>
        private IDictionary<string,string> PopulateReportParamters(string searchParams, string searchValues)
        {
            IDictionary<string, string> reportParameters = new Dictionary<string, string>();
            string[] paramArray = searchParams.Split(new string[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
            string[] valueArray = searchValues.Split(new string[] { ",,," }, StringSplitOptions.RemoveEmptyEntries);
            for(int i=0; i<paramArray.Length; i++)
            {                
                if (paramArray[i].ToLower().Contains("date"))
                {
                    reportParameters.Add("timerange_header", cleanString(valueArray[i]));
                }
                else {
                    reportParameters.Add(paramArray[i].ToLower().Replace(".","") + "_header" , cleanString(valueArray[i]));
                }                
            }
            return reportParameters.Count > 0 ? reportParameters : null;
        }

        /// <summary>
        /// Clean the Values of any unwanted characters
        /// </summary>
        /// <param name="str">String to be cleaned</param>
        /// <returns>String with no characters like(%,<,>,=)</returns>
        private string cleanString(string str)
        {
            StringBuilder sb = new StringBuilder(str);
            sb.Replace("___", "-");
            sb.Replace("<=", "");
            sb.Replace("=", "");
            sb.Replace(">=", "");
            sb.Replace("%", "");
            sb.Replace("<", "");
            sb.Replace(">", "");
            return sb.ToString();
        }

        /// <summary>
        /// Return the Module field Value
        /// </summary>
        /// <param name="arrTemp">The description field, with System/Component/Module/Item</param>
        /// <returns>Module Filed Value and any trailing values.</returns>
        private static string ModuleFieldValue(string[] arrTemp)
        {
            string strTemp = string.Empty;
            for (int i = 4; i < arrTemp.Length; i++)
            {
                strTemp += arrTemp[i] + '-';
            }
            return strTemp.Substring(0, strTemp.Length - 1);
        }        
    }
}
