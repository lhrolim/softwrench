using System;
using System.Collections.Generic;
using System.Data;

namespace softWrench.sW4.Web.Models.Report {
    public class ReportModel {

        public String ReportDefinition { get; set; }
        public String ReportDataSourceName { get; set; }
        public DataTable ReportData { get; set; }        
        public IDictionary<string,string> ReportParameters {get; set;}

        public ReportModel() {
        }

        public ReportModel(String reportDefinition, String reportDataSourceName, DataTable reportData, IDictionary<string,string> paramters) {
            ReportDataSourceName = reportDataSourceName;
            ReportDefinition = reportDefinition;
            ReportData = reportData;
            ReportParameters = paramters;
        }
    }
}