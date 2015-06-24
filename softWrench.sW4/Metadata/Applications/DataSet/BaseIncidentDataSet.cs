﻿using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.Shared2.Data.Association;
using cts.commons.simpleinjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.DataSet{
    class BaseIncidentDataSet : MaximoApplicationDataSet{

        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters){
            return AssetFilterBySiteFunction(parameters);
        }

        public SearchRequestDto AssetFilterBySiteFunction(AssociationPreFilterFunctionParameters parameters){
            var filter = parameters.BASEDto;
            var location = (string)parameters.OriginalEntity.GetAttribute("location");
            if (location == null){
                return filter;
            }
            filter.AppendSearchEntry("asset.location", location.ToUpper());
            return filter;
        }

        public IEnumerable<IAssociationOption> GetIncidentClassStructureType(OptionFieldProviderParameters parameters) {

            // TODO: Change the design to use a tree view component
            var query = string.Format(@"SELECT  c.classstructureid AS ID, 
                                                p3.classificationid AS CLASS_5, 
                                                p2.classificationid AS CLASS_4,  
                                                p1.classificationid AS CLASS_3, 
                                                p.classificationid  AS CLASS_2, 
                                                c.classificationid  AS CLASS_1
                                        from classstructure as c
                                        left join classstructure as p on p.classstructureid = c.parent
                                        left join classstructure as p1 on p1.classstructureid = p.parent
                                        left join classstructure as p2 on p2.classstructureid = p1.parent
                                        left join classstructure as p3 on p3.classificationid = p2.parent
                                        where 
                                        c.haschildren = 0 
                                        and (c.orgid is null or (c.orgid is not null and c.orgid  =  '{0}' )) 
                                        and (c.siteid is null or (c.siteid is not null and c.siteid  =  '{1}' )) 
                                        and c.classstructureid in (select classusewith.classstructureid 
                                                                    from classusewith  
                                                                    where classusewith.classstructureid=c.classstructureid
                                                                    and objectname= '{2}')",
                                        parameters.OriginalEntity.Attributes["orgid"],
                                        parameters.OriginalEntity.Attributes["siteid"],
                                        parameters.OriginalEntity.Attributes["class"] == "" ? parameters.OriginalEntity.Attributes["class"] : "INCIDENT");

            if (ApplicationConfiguration.IsOracle(DBType.Maximo))
            {
                query = string.Format(@"SELECT  c.classstructureid AS ID, 
                                                p3.classificationid AS CLASS_5, 
                                                p2.classificationid AS CLASS_4,  
                                                p1.classificationid AS CLASS_3, 
                                                p.classificationid  AS CLASS_2, 
                                                c.classificationid  AS CLASS_1
                                        from classstructure  c
                                        left join classstructure  p on p.classstructureid = c.parent
                                        left join classstructure  p1 on p1.classstructureid = p.parent
                                        left join classstructure  p2 on p2.classstructureid = p1.parent
                                        left join classstructure  p3 on p3.classificationid = p2.parent
                                        where 
                                        c.haschildren = 0 
                                        and (c.orgid is null or (c.orgid is not null and c.orgid  =  '{0}' )) 
                                        and (c.siteid is null or (c.siteid is not null and c.siteid  =  '{1}' )) 
                                        and c.classstructureid in (select classusewith.classstructureid 
                                                                    from classusewith  
                                                                    where classusewith.classstructureid=c.classstructureid
                                                                    and objectname= '{2}')",
                                        parameters.OriginalEntity.Attributes["orgid"],
                                        parameters.OriginalEntity.Attributes["siteid"],
                                        parameters.OriginalEntity.Attributes["class"] == "" ? parameters.OriginalEntity.Attributes["class"] : "INCIDENT");
            }

            var result = MaxDAO.FindByNativeQuery(query, null);
            var list = new List<AssociationOption>();

            foreach (var record in result){
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

        public override string ApplicationName(){
            return "incident";
        }

        public override string ClientFilter(){
            return "otb,manchester";
        }
    }
}
