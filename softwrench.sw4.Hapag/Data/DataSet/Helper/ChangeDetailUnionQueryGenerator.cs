using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.Hapag.Data.Init;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {
    public class ChangeDetailUnionQueryGenerator : ISingletonComponent {

        private readonly ChangeWhereClauseProvider _changeWhereClauseProvider;
        private readonly IContextLookuper _contextLookuper;

        public ChangeDetailUnionQueryGenerator(ChangeWhereClauseProvider changeWhereClauseProvider, IContextLookuper contextLookuper) {
            _changeWhereClauseProvider = changeWhereClauseProvider;
            _contextLookuper = contextLookuper;
        }

        const string BaseIdWoChangeQuery =

         @"select
      wochange.workorderid AS workorderid
      FROM wochange AS wochange
      INNER JOIN sr AS sr_ ON
      (
         wochange.origrecordid = sr_.ticketid
         AND wochange.origrecordclass = 'SR'
         AND wochange.woclass = 'CHANGE'
      )
      INNER JOIN relatedrecord AS relatedsr_ ON
      (
         wochange.wonum = relatedsr_.recordkey
         AND relatedsr_.relatedrecclass = 'SR'
         AND relatedsr_.class = 'CHANGE'
      )
      INNER JOIN sr AS relatedsr_sr_ ON
      (
         relatedsr_.relatedreckey = relatedsr_sr_.ticketid
      )
      WHERE EXISTS
      (
         SELECT
         1
         FROM pmchgotherapprovers approvals_
         WHERE ( wochange.wonum = approvals_.wonum )
         AND approvals_.approvergroup IN
         (
            {1}
         )
      )
      AND ( wochange.pluspcustomer LIKE 'HLC-%' )
    -- those with joined tickets for approvals group 2
      UNION ALL
      select
      wochange.workorderid AS workorderid
      FROM wochange AS wochange
      INNER JOIN sr AS sr_ ON
      (
         wochange.origrecordid = sr_.ticketid
         AND wochange.origrecordclass = 'SR'
         AND wochange.woclass = 'CHANGE'
      )
      INNER JOIN relatedrecord AS relatedsr_ ON
      (
         wochange.wonum = relatedsr_.recordkey
         AND relatedsr_.relatedrecclass = 'SR'
         AND relatedsr_.class = 'CHANGE'
      )
      INNER JOIN sr AS relatedsr_sr_ ON
      (
         relatedsr_.relatedreckey = relatedsr_sr_.ticketid
      )
      left join ci as ci_ on (wochange.cinum = ci_.cinum)
      WHERE EXISTS
      (
         SELECT
         1
         FROM woactivity AS woactivity_
         WHERE wochange.wonum = woactivity_.parent
         AND woactivity_.ownergroup IN
         (
            {1}
         )
      )
      AND ( wochange.pluspcustomer LIKE 'HLC-%' )
    -- those WITHOUT joined tickets for approvals group 1
      {2}";

        const string NonTicketIdUnionQuery = @"UNION ALL
select wochange.workorderid   AS workorderid 
       FROM   wochange AS wochange
       INNER JOIN pmchgotherapprovers approvals_ 
       ON ( wochange.wonum = approvals_.wonum  AND approvals_.approvergroup IN ({0})) 
       left join ci as ci_ on (wochange.cinum = ci_.cinum)
       WHERE ( wochange.pluspcustomer LIKE 'HLC-%' ) 
       and not exists (select 1 from sr AS sr_ where ( wochange.origrecordid = sr_.ticketid ))
      
    -- those WITHOUT joined tickets for ownergroup 2
    UNION ALL
      select wochange.workorderid   AS workorderid 
       FROM   wochange AS wochange
       INNER JOIN  woactivity AS woactivity_ 
       ON  wochange.wonum = woactivity_.parent 
       AND woactivity_.ownergroup IN ({0}) 
       left join ci as ci_ on (wochange.cinum = ci_.cinum)  
       where ( wochange.pluspcustomer LIKE 'HLC-%' ) 
       and not exists (select 1 from sr AS sr_ where ( wochange.origrecordid = sr_.ticketid ))";


        const string SRIDUnionQuery = @"SELECT
      srforchange.ticketid AS ticketid
      FROM sr AS srforchange
      LEFT JOIN asset AS asset_ ON
      (
         srforchange.assetnum = asset_.assetnum
         AND srforchange.siteid = asset_.siteid
      )
      WHERE
      (
         (
            srforchange.classificationid IS NULL OR srforchange.classificationid NOT LIKE '8151%'
         )
      )
      AND
      (
         NOT EXISTS
         (
            SELECT
            1
            FROM wochange wo4sr_
            WHERE wo4sr_.origrecordid = srforchange.ticketid
            AND wo4sr_.origrecordclass = 'SR'
            AND wo4sr_.woclass = 'CHANGE'
         )
         AND NOT EXISTS
         (
            SELECT
            1
            FROM relatedrecord relatedcr_
            WHERE relatedcr_.relatedreckey = srforchange.ticketid
            AND relatedcr_.relatedrecclass = 'SR'
            AND relatedcr_.class = 'CHANGE'
         )
         AND srforchange.templateid IN
         (
            'HLCDECHG','HLCDECHSSO','HLCDECHTUI'
         )
      )
      AND ( srforchange.pluspcustomer LIKE 'HLC-%' )";

        public string GenerateQuery() {

            var context = _contextLookuper.LookupContext();
            var isTomOrItom = context.IsInModule(FunctionalRole.Itom) || context.IsInModule(FunctionalRole.Tom);
            if (isTomOrItom) {
                //HAP-805, tom or itom roles can see everything in the grid
                return null;
            }

            var externalTemplateIds = _changeWhereClauseProvider.GetTemplateIds();
            var user = SecurityFacade.CurrentUser();

            //force cache

            var personGroupsForQuery = user.GetPersonGroupsForQuery();

            var extraUnions = NonTicketIdUnionQuery.Fmt(personGroupsForQuery);

            var completeUnionIdQuery = BaseIdWoChangeQuery.Fmt(externalTemplateIds, personGroupsForQuery, extraUnions);

            return "wochange.workorderid in ({0}) ".Fmt(completeUnionIdQuery);
        }

        public string GenerateSrQuery() {

            var context = _contextLookuper.LookupContext();
            var isTomOrItom = context.IsInModule(FunctionalRole.Itom) || context.IsInModule(FunctionalRole.Tom);
            if (isTomOrItom) {
                //HAP-805, tom or itom roles can see everything in the grid
                return null;
            }

            return "SR.ticketid in ({0})".Fmt(SRIDUnionQuery);


        }

    }



}
