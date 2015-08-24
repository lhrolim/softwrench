using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Web.Models.Hapag {
    public class DashboardDefinitionModel {
        private readonly string _dashboardDataJson;

        public DashboardDefinitionModel(IList<DashboardDefinition> dashboardDefinitionList) {
            if (dashboardDefinitionList != null && dashboardDefinitionList.Count > 0) {
                _dashboardDataJson = JsonConvert.SerializeObject(dashboardDefinitionList, Newtonsoft.Json.Formatting.None,

                    new JsonSerializerSettings() {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
            }
        }

        public string DashboardDataJson {
            get { return _dashboardDataJson; }
        }
    }

    public class DashboardDefinition {
        public String Id { get; private set; }
        public String Title { get; set; }
        public String ApplicationName { get; private set; }
        public String SchemaId { get; private set; }
        public SchemaMode Mode { get; private set; }
        public Int32 PageSize { get; private set; }
        public String SearchParams { get; private set; }
        public String SearchValues { get; private set; }
        public Int32 TotalCount { get; internal set; }
        public String Tooltip { get; set; }
        //Schema key to be used upon click on view all
        public String ViewAllSchema { get; private set; }
        //Schema key to be used upon click on an item of the dashboard
        public String DetailSchema { get; private set; }
        public String IdFieldName { get; private set; }

        public DashboardDefinition(String id, String title, String applicationName, String schemaId,
            SchemaMode mode, Int32 pageSize, Int32 totalCount, String tooltip, string viewAllSchema = "list", string detailSchema = "detail", string idFieldName = "") {
            Id = id;
            Title = title;
            ApplicationName = applicationName;
            SchemaId = schemaId;
            Mode = mode;
            PageSize = pageSize;
            TotalCount = totalCount;
            Tooltip = tooltip;
            ViewAllSchema = viewAllSchema;
            DetailSchema = detailSchema;
            IdFieldName = idFieldName;
        }

        public static DashboardDefinition Get5ElementsInstance
            (String id, String applicationName, String schemaId, String title, 
            String tooltip, string viewAllSchema = "list", 
            string detailSchema = "detail", string idFieldName = "") {
            return new DashboardDefinition(id, title, applicationName, schemaId, SchemaMode.output, 5, 0, tooltip, viewAllSchema,detailSchema,idFieldName);
        }

        public static DashboardDefinition GetDefaultInstance(String id, String applicationName, String schemaId, String title, String tooltip) {
            return new DashboardDefinition(id, title, applicationName, schemaId, SchemaMode.output, 0, 0, tooltip);
        }


    }
}