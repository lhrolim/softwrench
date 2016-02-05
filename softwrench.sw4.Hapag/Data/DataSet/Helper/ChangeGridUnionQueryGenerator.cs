using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {
    public class ChangeGridUnionQueryGenerator : ISingletonComponent {

        private ChangeWhereClauseProvider _changeWhereClauseProvider;

        public ChangeGridUnionQueryGenerator(ChangeWhereClauseProvider changeWhereClauseProvider) {
            _changeWhereClauseProvider = changeWhereClauseProvider;
        }

        #region nonticketunions

        const string NonTicketUnions =
            @"UNION ALL
select wochange.workorderid   AS workorderid, 
       NULL AS sr_ticketid, 
       NULL AS relatedsr_recordkey, 
       NULL AS relatedsr_sr_ticketid, 
       wochange.wonum         AS wonum, 
       wochange.description    AS hlagchangesummary, 
       NULL AS hlagchangeticketid, 
       ci_.ciname AS ci_ciname,
       wochange.wopriority    AS wopriority, 
       wochange.schedstart    AS schedstart, 
       CASE 
         WHEN wochange.pluspcustomer IS NOT NULL 
              AND wochange.pluspcustomer = 'HLC-00' THEN 'HLAG' 
         WHEN wochange.pluspcustomer IS NOT NULL 
              AND Length(wochange.pluspcustomer) >= 3 THEN 
         Substr(wochange.pluspcustomer, Length(wochange.pluspcustomer) - 2, 3) 
         ELSE '' 
       END                    AS hlagpluspcustomer, 
       CASE 
         WHEN Locate('@', wochange.reportedby) > 0 THEN 
         Substr(wochange.reportedby, 1, Locate('@', wochange.reportedby) - 1) 
         ELSE wochange.reportedby 
       END                    AS hlagreportedby, 
       wochange.status        AS status 
FROM   wochange AS wochange
       INNER JOIN pmchgotherapprovers approvals_ 
       ON ( wochange.wonum = approvals_.wonum  AND approvals_.approvergroup IN ({1})) 
       left join ci as ci_ on (wochange.cinum = ci_.cinum)
       WHERE ( wochange.pluspcustomer LIKE 'HLC-%' ) 
       and not exists (select 1 from sr AS sr_ where ( wochange.origrecordid = sr_.ticketid ))
      {0}
    -- those WITHOUT joined tickets for ownergroup 2
    UNION ALL
      select wochange.workorderid   AS workorderid, 
       NULL AS sr_ticketid, 
       NULL AS relatedsr_recordkey, 
       NULL AS relatedsr_sr_ticketid, 
       wochange.wonum         AS wonum, 
       wochange.description    AS hlagchangesummary, 
       NULL AS hlagchangeticketid, 
       ci_.ciname         AS ci_ciname, 
       wochange.wopriority    AS wopriority, 
       wochange.schedstart    AS schedstart, 
       CASE 
         WHEN wochange.pluspcustomer IS NOT NULL 
              AND wochange.pluspcustomer = 'HLC-00' THEN 'HLAG' 
         WHEN wochange.pluspcustomer IS NOT NULL 
              AND Length(wochange.pluspcustomer) >= 3 THEN 
         Substr(wochange.pluspcustomer, Length(wochange.pluspcustomer) - 2, 3) 
         ELSE '' 
       END                    AS hlagpluspcustomer, 
       CASE 
         WHEN Locate('@', wochange.reportedby) > 0 THEN 
         Substr(wochange.reportedby, 1, Locate('@', wochange.reportedby) - 1) 
         ELSE wochange.reportedby 
       END                    AS hlagreportedby, 
       wochange.status        AS status 
       FROM   wochange AS wochange
       INNER JOIN  woactivity AS woactivity_ 
       ON  wochange.wonum = woactivity_.parent 
       AND woactivity_.ownergroup IN ({1}) 
       left join ci as ci_ on (wochange.cinum = ci_.cinum)  
       where ( wochange.pluspcustomer LIKE 'HLC-%' ) 
       and not exists (select 1 from sr AS sr_ where ( wochange.origrecordid = sr_.ticketid ))
       {0}";

        #endregion

        #region nonticketcountunions

        const string NonTicketCountUnions =
        @"UNION ALL
select count(*) as cnt
FROM   wochange AS wochange
       INNER JOIN pmchgotherapprovers approvals_ 
       ON ( wochange.wonum = approvals_.wonum  AND approvals_.approvergroup IN ({1})) 
       left join ci as ci_ on (wochange.cinum = ci_.cinum) 
       where ( wochange.pluspcustomer LIKE 'HLC-%' ) 
       and not exists (select 1 from sr AS sr_ where ( wochange.origrecordid = sr_.ticketid ))
       {0}
-- those WITHOUT joined tickets for approvals group 2
       UNION ALL
select count(*) as cnt
FROM   wochange AS wochange
       INNER JOIN  woactivity AS woactivity_ 
       ON  (wochange.wonum = woactivity_.parent
       AND woactivity_.ownergroup IN ( {1} ) )
       left join ci as ci_ on (wochange.cinum = ci_.cinum)  
       WHERE ( wochange.pluspcustomer LIKE 'HLC-%' ) 
       and not exists (select 1 from sr AS sr_ where ( wochange.origrecordid = sr_.ticketid ))
       {0}";
        #endregion


        const string BaseQuery =
      @"select
      wochange.workorderid AS workorderid,
      sr_.ticketid AS sr_ticketid,
      relatedsr_.recordkey AS relatedsr_recordkey,
      relatedsr_sr_.ticketid AS relatedsr_sr_ticketid,
      wochange.wonum AS wonum,
      CASE WHEN ( relatedsr_.relatedreckey IS NOT NULL AND relatedsr_sr_.description IS NOT NULL ) THEN relatedsr_sr_.description WHEN ( sr_.ticketid IS NOT NULL AND sr_.description IS NOT NULL ) THEN sr_.description ELSE wochange.description END AS hlagchangesummary,
      CASE WHEN sr_.ticketid IS NOT NULL THEN sr_.ticketid ELSE relatedsr_.relatedreckey END AS hlagchangeticketid,
      ci_.ciname AS ci_ciname,
      wochange.wopriority AS wopriority,
      wochange.schedstart AS schedstart,
      CASE WHEN wochange.pluspcustomer IS NOT NULL
      AND wochange.pluspcustomer = 'HLC-00' THEN 'HLAG' WHEN wochange.pluspcustomer IS NOT NULL
      AND Length(wochange.pluspcustomer) >= 3 THEN Substr(wochange.pluspcustomer, Length(wochange.pluspcustomer) - 2, 3) ELSE '' END AS hlagpluspcustomer,
      CASE WHEN Locate('@', wochange.reportedby) > 0 THEN Substr(wochange.reportedby, 1, Locate('@', wochange.reportedby) - 1) ELSE wochange.reportedby END AS hlagreportedby,
      wochange.status AS status
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
         FROM pmchgotherapprovers approvals_
         WHERE ( wochange.wonum = approvals_.wonum )
         AND approvals_.approvergroup IN
         (
            {1}
         )
      )
      AND ( wochange.pluspcustomer LIKE 'HLC-%' )
      {2}
    -- those with joined tickets for approvals group 2
      UNION ALL
      select
      wochange.workorderid AS workorderid,
      sr_.ticketid AS sr_ticketid,
      relatedsr_.recordkey AS relatedsr_recordkey,
      relatedsr_sr_.ticketid AS relatedsr_sr_ticketid,
      wochange.wonum AS wonum,
      CASE WHEN ( relatedsr_.relatedreckey IS NOT NULL AND relatedsr_sr_.description IS NOT NULL ) THEN relatedsr_sr_.description WHEN ( sr_.ticketid IS NOT NULL AND sr_.description IS NOT NULL ) THEN sr_.description ELSE wochange.description END AS hlagchangesummary,
      CASE WHEN sr_.ticketid IS NOT NULL THEN sr_.ticketid ELSE relatedsr_.relatedreckey END AS hlagchangeticketid,
      ci_.ciname AS ci_ciname,
      wochange.wopriority AS wopriority,
      wochange.schedstart AS schedstart,
      CASE WHEN wochange.pluspcustomer IS NOT NULL
      AND wochange.pluspcustomer = 'HLC-00' THEN 'HLAG' WHEN wochange.pluspcustomer IS NOT NULL
      AND Length(wochange.pluspcustomer) >= 3 THEN Substr(wochange.pluspcustomer, Length(wochange.pluspcustomer) - 2, 3) ELSE '' END AS hlagpluspcustomer,
      CASE WHEN Locate('@', wochange.reportedby) > 0 THEN Substr(wochange.reportedby, 1, Locate('@', wochange.reportedby) - 1) ELSE wochange.reportedby END AS hlagreportedby,
      wochange.status AS status
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
      {2}
    -- those WITHOUT joined tickets for approvals group 1
      {4}
    --the rest
      UNION
      ALL
      SELECT
      NULL,
      NULL,
      NULL,
      NULL,
      '-666' AS zeroedattr,
      srforchange.description AS hlagchangesummary,
      srforchange.ticketid AS ticketid,
      asset_.description AS asset_description,
      NULL,
      NULL,
      CASE WHEN srforchange.pluspcustomer IS NOT NULL
      AND srforchange.pluspcustomer = 'HLC-00' THEN 'HLAG' WHEN srforchange.pluspcustomer IS NOT NULL
      AND Length(srforchange.pluspcustomer) >= 3 THEN Substr( srforchange.pluspcustomer, Length( srforchange.pluspcustomer) - 2, 3) ELSE '' END AS hlagpluspcustomer,
      CASE WHEN Locate('@', srforchange.reportedby) > 0 THEN Substr( srforchange.reportedby, 1, Locate('@', srforchange.reportedby) - 1) ELSE srforchange.reportedby END AS hlagreportedby,
      srforchange.status AS status
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
      AND ( srforchange.pluspcustomer LIKE 'HLC-%' )
      {3}
      ";


        const string BaseCountQuery =
      @"select sum(cnt) from (select count(*) as cnt
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
      select count(*) as cnt
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
     {2}
    -- those WITHOUT joined tickets for approvals group 1
{4}
UNION ALL 
select count(*) as cnt
FROM   sr AS srforchange 
       LEFT JOIN asset AS asset_ 
              ON ( srforchange.assetnum = asset_.assetnum 
                   AND srforchange.siteid = asset_.siteid ) 
WHERE  (( srforchange.classificationid IS NULL 
           OR srforchange.classificationid NOT LIKE '8151%' )) 
       AND ( NOT EXISTS (SELECT 1 
                         FROM   wochange wo4sr_ 
                         WHERE  wo4sr_.origrecordid = srforchange.ticketid 
                                AND wo4sr_.origrecordclass = 'SR' 
                                AND wo4sr_.woclass = 'CHANGE') 
             AND NOT EXISTS (SELECT 1 
                             FROM   relatedrecord relatedcr_ 
                             WHERE  relatedcr_.relatedreckey = 
                                    srforchange.ticketid 
                                    AND relatedcr_.relatedrecclass = 'SR' 
                                    AND relatedcr_.class = 'CHANGE') 
             AND srforchange.templateid IN ( {0} ) ) 
       AND ( srforchange.pluspcustomer LIKE 'HLC-%' )
       {3}
)";


        public string GenerateQuery(EntityMetadata metadata, SearchRequestDto searchDTO) {
            var externalTemplateIds = _changeWhereClauseProvider.GetTemplateIds();
            var user = SecurityFacade.CurrentUser();

            //force cache
            var parameters = searchDTO.GetParameters();

            var personGroupsForQuery = user.GetPersonGroupsForQuery();
            var filterQueryMain = GetFilterQuery("wochange", searchDTO);

            var extraUnions = NonTicketUnions.Fmt(getMainQueryExcludingTicketId(searchDTO), personGroupsForQuery);

            if (parameters != null && parameters.ContainsKey("hlagchangeticketid") && parameters.Count == 1) {
                extraUnions = "";
            }

            var filterQuerySecondary = GetFilterQuery("srforchange", searchDTO.unionDTO);
            return BaseQuery.Fmt(externalTemplateIds, personGroupsForQuery, filterQueryMain, filterQuerySecondary, extraUnions);
        }



        private static string GetFilterQuery(string entityName, SearchRequestDto searchDTO) {
            var baseFilterQuery = SearchUtils.GetWhereReplacingParameters(searchDTO, entityName);
            if (baseFilterQuery == null) {
                return "";
            }
            return " and ( " + baseFilterQuery + ") ";
        }

        public string GenerateQueryCount(EntityMetadata metadata, SearchRequestDto searchDTO) {
            var externalTemplateIds = _changeWhereClauseProvider.GetTemplateIds();
            var user = SecurityFacade.CurrentUser();
            var personGroupsForQuery = user.GetPersonGroupsForQuery();
            var filterQueryMain = GetFilterQuery("wochange", searchDTO);

            var parameters = searchDTO.GetParameters();

            var extraUnions = NonTicketCountUnions.Fmt(getMainQueryExcludingTicketId(searchDTO), personGroupsForQuery);

            if (parameters != null && parameters.ContainsKey("hlagchangeticketid") && parameters.Count == 1) {
                extraUnions = "";
            }

            var filterQuerySecondary = GetFilterQuery("srforchange", searchDTO.unionDTO);
            return BaseCountQuery.Fmt(externalTemplateIds, personGroupsForQuery, filterQueryMain, filterQuerySecondary, extraUnions);
        }

        private object getMainQueryExcludingTicketId(SearchRequestDto searchDTO) {
            var cloned = searchDTO.DeepClone();
            var parameters = cloned.GetParameters();
            if (parameters != null && parameters.ContainsKey("hlagchangeticketid")) {
                parameters.Remove("hlagchangeticketid");
                cloned.SearchParams = cloned.SearchParams.Replace("||,hlagchangeticketid", "");
                cloned.SearchParams = cloned.SearchParams.Replace("hlagchangeticketid", "");
            }
            return GetFilterQuery("wochange", cloned);
        }

    }
}
