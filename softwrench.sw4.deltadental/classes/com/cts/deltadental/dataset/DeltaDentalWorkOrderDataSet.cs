using System;
using System.Collections.Generic;
using cts.commons.persistence;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Util;

namespace softwrench.sw4.deltadental.classes.com.cts.deltadental.dataset {
    class DeltaDentalWorkOrderDataSet : BaseWorkorderDataSet {

        public IEnumerable<IAssociationOption> GetWOClassStructureType(OptionFieldProviderParameters parameters)
        {
            return GetClassStructureType(parameters, "WORKORDER");
        }

        protected IEnumerable<IAssociationOption> GetClassStructureType(OptionFieldProviderParameters parameters, string woclass)
        {
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            // Create a where caluse to handle Delta's custom columns used to identify which classifications are valid in which sites
            var customColumnsWhere = siteid != null ? $"and LOWER(c.{siteid}) = '1'" : "";

            // TODO: Change the design to use a tree view component
            var query = string.Format(@"SELECT  c.classstructureid AS ID, 
                                                p3.classificationid AS CLASS_5, 
                                                p2.classificationid AS CLASS_4,  
                                                p1.classificationid AS CLASS_3, 
                                                p.classificationid  AS CLASS_2, 
                                                c.classificationid  AS CLASS_1
                                        from classstructure {3} c
                                        left join classstructure {3} p on p.classstructureid = c.parent
                                        left join classstructure {3} p1 on p1.classstructureid = p.parent
                                        left join classstructure {3} p2 on p2.classstructureid = p1.parent
                                        left join classstructure {3} p3 on p3.classificationid = p2.parent
                                        where 
                                        c.haschildren = 0 
                                        and (c.orgid is null or (c.orgid is not null and c.orgid  =  '{0}' )) 
                                        and (c.siteid is null or (c.siteid is not null and c.siteid  =  '{1}' )) 
                                        and c.classstructureid in (select classusewith.classstructureid 
                                                                    from classusewith  
                                                                    where classusewith.classstructureid=c.classstructureid
                                                                    and objectname= '{2}')
                                        " + customColumnsWhere,
                                        parameters.OriginalEntity.GetAttribute("orgid"),
                                        siteid,
                                        ReferenceEquals(parameters.OriginalEntity.GetAttribute("class"), "") ? "" : woclass,
                                        ApplicationConfiguration.IsOracle(DBType.Maximo) ? "" : "as"
                                        );


            var result = MaxDAO.FindByNativeQuery(query, null);
            var list = new List<AssociationOption>();

            foreach (var record in result)
            {
                list.Add(new AssociationOption(record["ID"],
                         String.Format("{0}{1}{2}{3}{4}",
                                        record["CLASS_5"] == null ? "" : record["CLASS_5"] + "/",
                                        record["CLASS_4"] == null ? "" : record["CLASS_4"] + "/",
                                        record["CLASS_3"] == null ? "" : record["CLASS_3"] + "/",
                                        record["CLASS_2"] == null ? "" : record["CLASS_2"] + "/",
                                        record["CLASS_1"] == null ? "" : record["CLASS_1"]
                                        )));
            }

            return list;
        }

        public override string ApplicationName() {
            return "workorder";
        }

        public override string ClientFilter() {
            return "deltadental";
        }


    }
}
