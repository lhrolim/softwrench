using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sW4.Shared2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagAssetListReportDataSet : MaximoApplicationDataSet {

        public static void FieldsHandler(IEnumerable<AttributeHolder> resultObject) {
            foreach (var attributeHolder in resultObject) {
                LocationDescriptionHandler(attributeHolder);
                FillDepartmentField(attributeHolder);
                FillFieldsHandler(attributeHolder);
                DescriptionHandler(attributeHolder);
                FillNoIdField(attributeHolder);
            }
        }

        private static void LocationDescriptionHandler(AttributeHolder attributeHolder) {
            FillCustomLocationDescriptionFields(attributeHolder, "#floor", "FL:");
            FillCustomLocationDescriptionFields(attributeHolder, "#room", "RO:");
        }

        private static void FillDepartmentField(AttributeHolder attributeHolder) {
            const string field = "#department";
            try {
                DataSetUtil.FillBlank(attributeHolder, field);
                var glaccount = attributeHolder.GetAttribute("glaccount");
                if (DataSetUtil.IsValid(glaccount, typeof(String))) {
                    var department = string.Empty;
                    var attribute = Regex.Split(glaccount.ToString(), "-");
                    if (attribute.Count() > 1) {
                        department = attribute[1];
                    }
                    var accountname = attributeHolder.GetAttribute("assetglaccount_.accountname");
                    department += " " + accountname;
                    attributeHolder[field] = department;
                }
            } catch {
                DataSetUtil.FillBlank(attributeHolder, field);
            }
        }

        private static void FillCustomLocationDescriptionFields(AttributeHolder attributeHolder, string field, string filter) {
            try {
                DataSetUtil.FillBlank(attributeHolder, field);
                var location = attributeHolder.GetAttribute("location_.description");
                if (DataSetUtil.IsValid(location, typeof(String))) {
                    var l = location.ToString();
                    if (l.Contains(filter)) {
                        var attributeAux = Regex.Split(l, filter);
                        if (attributeAux.Count() > 1) {
                            var attribute = Regex.Split(attributeAux[1], "/");
                            if (attribute.Count() > 1) {
                                attributeHolder[field] = attribute[0];
                            }
                        }
                    }
                }
            } catch {
                DataSetUtil.FillBlank(attributeHolder, field);
            }
        }

        private static void FillFieldsHandler(AttributeHolder attributeHolder) {
            DataSetUtil.FillField(attributeHolder, "#country", "location_.pluspservaddr_.country");
            DataSetUtil.FillField(attributeHolder, "#manufacturer", "item_.invvendor_.manufacturer");
            DataSetUtil.FillField(attributeHolder, "#billable", "companies_.warrantyview_.description");
            DataSetUtil.FillField(attributeHolder, "#category", "classstructure_.category_.description");
            DataSetUtil.FillField(attributeHolder, "#subcategory", "classstructure_.description");
        }

      

        private static void DescriptionHandler(AttributeHolder attributeHolder) {
            FillDescriptionFields(attributeHolder, "#computername", false);
            FillDescriptionFields(attributeHolder, "#product", true);
        }

        private static void FillDescriptionFields(AttributeHolder attributeHolder, string field, bool isAfter) {
            try {
                DataSetUtil.FillBlank(attributeHolder, field);
                var description = attributeHolder.GetAttribute("description");
                if (DataSetUtil.IsValid(description, typeof(String))) {
                    var attribute = Regex.Split(description.ToString(), "//");
                    if (attribute.Count() > 1) {
                        attributeHolder[field] = attribute[isAfter ? 1 : 0];
                    }
                }
            } catch {
                DataSetUtil.FillBlank(attributeHolder, field);
            }
        }

        private static void FillNoIdField(AttributeHolder attributeHolder) {
            const string field = "#noid";
            try {
                DataSetUtil.FillBlank(attributeHolder, field);
                var parent = attributeHolder.GetAttribute("parent");
                var assetnum = attributeHolder.GetAttribute("assetnum");
                attributeHolder[field] = parent + " " + assetnum;
            } catch {
                DataSetUtil.FillBlank(attributeHolder, field);
            }
        }
    }
}
