using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Metadata.Applications.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagAssetListReportDataSet : BaseApplicationDataSet {

        public static void FieldsHandler(IEnumerable<AttributeHolder> resultObject) {
            foreach (var attributeHolder in resultObject) {
                LocationDescriptionHandler(attributeHolder);
            }
        }

        private static void LocationDescriptionHandler(AttributeHolder attributeHolder) {
            FillCustomLocationDescriptionFields(attributeHolder, "#floor", "FL:");
            FillCustomLocationDescriptionFields(attributeHolder, "#room", "RO:");
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
                                attributeHolder.Attributes[field] = attribute[0];
                            }
                        }
                    }
                }
            } catch {
                DataSetUtil.FillBlank(attributeHolder, field);
            }
        }



    }
}
