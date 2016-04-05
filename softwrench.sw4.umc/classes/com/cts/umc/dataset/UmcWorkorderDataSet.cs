using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Util;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;

namespace softWrench.sW4.umc.classes.com.cts.umc.dataset {
    public class UmcWorkorderDataSet : BaseWorkorderDataSet {

        public UmcWorkorderDataSet(ISWDBHibernateDAO swdbDao) : base(swdbDao) {
            
        }

        protected override IEnumerable<IAssociationOption> GetClassStructureType(OptionFieldProviderParameters parameters, string ticketclass, string searchString = null) {

            // TODO: Change the design to use a tree view component
            var query = BuildQuery(parameters, ticketclass, searchString);

            var result = MaxDAO.FindByNativeQuery(query, null);

            return result.Select(record => {
                    var label = string.Format("{0}{1}{2}{3}{4}",
                        record["CLASS_5"] == null ? "" : record["CLASS_5"] + "/",
                        record["CLASS_4"] == null ? "" : record["CLASS_4"] + "/",
                        record["CLASS_3"] == null ? "" : record["CLASS_3"] + "/",
                        record["CLASS_2"] == null ? "" : record["CLASS_2"] + "/",
                        record["CLASS_1"] == null ? "" : record["CLASS_1"] + " (" + record["DESCRIPTION_1"] + ")");
                    return new AssociationOption(record["ID"], label);
                });
        }

        protected override string BuildQuery(OptionFieldProviderParameters parameters, string ticketclass, string searchString = null) {
            
            var classStructureQuery = string.Format(@"SELECT  c.classstructureid AS ID, 
                                                               p3.classificationid AS CLASS_5, 
                                                               p2.classificationid AS CLASS_4,  
                                                               p1.classificationid AS CLASS_3, 
                                                               p.classificationid  AS CLASS_2, 
                                                               c.classificationid  AS CLASS_1,
                                                               c.description AS DESCRIPTION_1
                                                       FROM classstructure {3} c
                                                       left join classstructure {3} p on p.classstructureid = c.parent
                                                       left join classstructure {3} p1 on p1.classstructureid = p.parent
                                                       left join classstructure {3} p2 on p2.classstructureid = p1.parent
                                                       left join classstructure {3} p3 on p3.classificationid = p2.parent
                                                       WHERE 
                                                               c.haschildren = 0 and
                                                               (c.orgid is null or (c.orgid is not null and c.orgid  =  '{0}' )) and
                                                               (c.siteid is null or (c.siteid is not null and c.siteid  =  '{1}' )) and
                                                               c.classstructureid in (select classusewith.classstructureid 
                                                                                        from classusewith  
                                                                                        where classusewith.classstructureid=c.classstructureid
                                                                                        and objectname= '{2}')",
                                    parameters.OriginalEntity.GetAttribute("orgid"),
                                    parameters.OriginalEntity.GetAttribute("siteid"),
                                    ReferenceEquals(parameters.OriginalEntity.GetAttribute("class"), "") ? "" : ticketclass,
                                    ApplicationConfiguration.IsOracle(DBType.Maximo) ? "" : "as"
                                    );
            if (searchString != null) {
                classStructureQuery += string.Format(@" and ( UPPER(COALESCE(p3.classificationid,'')) like '%{0}%' or UPPER(COALESCE(p3.description,'')) like '%{0}%' or
                                                              UPPER(COALESCE(p2.classificationid,'')) like '%{0}%' or UPPER(COALESCE(p2.description,'')) like '%{0}%' or
                                                              UPPER(COALESCE(p1.classificationid,'')) like '%{0}%' or UPPER(COALESCE(p1.description,'')) like '%{0}%' or
                                                              UPPER(COALESCE(p.classificationid,''))  like '%{0}%' or UPPER(COALESCE(p.description,''))  like '%{0}%' or
                                                              UPPER(COALESCE(c.classificationid,''))  like '%{0}%' or UPPER(COALESCE(c.description,''))  like '%{0}%' )", 
                                                            searchString.ToUpper());
            }
            return classStructureQuery;
        }

        public override string ApplicationName() {
            return "workorder";
        }

        public override string ClientFilter() {
            return "umc";
        }

    }
}
