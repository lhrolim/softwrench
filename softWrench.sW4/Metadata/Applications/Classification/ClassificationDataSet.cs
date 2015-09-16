using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Metadata.Applications.Classification {
    public class ClassificationDataSet : ISingletonComponent
    {
        public enum ClassStructureType { 
            Asset = 0, 
            Incident,
            Sr,
            Workorder,
            Solution
        };

        public static IEnumerable<IAssociationOption> GetClassStructureType(ClassStructureType type, OptionFieldProviderParameters parameters) {
            var maxDAO = SimpleInjectorGenericFactory.Instance.GetObject<MaximoHibernateDAO>(typeof(MaximoHibernateDAO));
            var user = SecurityFacade.CurrentUser();

            var query = string.Format(@"SELECT c.classstructureid AS ID,
                                               p3.classificationid AS CLASS_5,
                                               p2.classificationid AS CLASS_4,
                                               p1.classificationid AS CLASS_3,
                                               p.classificationid AS CLASS_2,
                                               c.classificationid AS CLASS_1
                                        from classstructure as c
                                        left join classstructure as p on p.classstructureid = c.parent
                                        left join classstructure as p1 on p1.classstructureid = p.parent
                                        left join classstructure as p2 on p2.classstructureid = p1.parent
                                        left join classstructure as p3 on p3.classificationid = p2.parent
                                        where
                                        c.haschildren = 0
                                        and (c.orgid is null or (c.orgid is not null and c.orgid = '{0}' ))
                                        and (c.siteid is null or (c.siteid is not null and c.siteid = '{1}' ))
                                        and c.classstructureid in (select classusewith.classstructureid
                                        from classusewith
                                        where classusewith.classstructureid=c.classstructureid
                                        and objectname= '{2}')",
                                        parameters.OriginalEntity.GetAttribute("orgid") ?? user.OrgId,
                                        parameters.OriginalEntity.GetAttribute("siteid") ?? user.SiteId,
                                        type.ToString().ToUpper());

            var result = maxDAO.FindByNativeQuery(query, null);
            var list = new List<AssociationOption>();

            if (result.Any()) {
                foreach (var record in result)
                {
                    list.Add(new AssociationOption(record["ID"],
                        String.Format("{0}{1}{2}{3}{4}",
                            record["CLASS_5"] == null ? "" : record["CLASS_5"] + "/",
                            record["CLASS_4"] == null ? "" : record["CLASS_4"] + "/",
                            record["CLASS_3"] == null ? "" : record["CLASS_3"] + "/",
                            record["CLASS_2"] == null ? "" : record["CLASS_2"] + "/",
                            record["CLASS_1"] ?? ""
                            )));
                }
            }

            return list;
        }
    }
}
