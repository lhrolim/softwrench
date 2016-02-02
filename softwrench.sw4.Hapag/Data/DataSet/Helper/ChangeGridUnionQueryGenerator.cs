using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {
    internal class ChangeGridUnionQueryGenerator {

        const string BaseQuery = @"


        -- those with joined tickets for approvals group 1
        select wochange.workorderid   AS workorderid, 
               sr_.ticketid           AS sr_ticketid, 
               relatedsr_.recordkey   AS relatedsr_recordkey, 
               relatedsr_sr_.ticketid AS relatedsr_sr_ticketid, 
               wochange.wonum         AS wonum, 
               CASE 
                 WHEN ( relatedsr_.relatedreckey IS NOT NULL 
                        AND relatedsr_sr_.description IS NOT NULL ) THEN 
                 relatedsr_sr_.description 
                 WHEN ( sr_.ticketid IS NOT NULL 
                        AND sr_.description IS NOT NULL ) THEN sr_.description 
                 ELSE wochange.description 
               END                    AS hlagchangesummary, 
               CASE 
                 WHEN sr_.ticketid IS NOT NULL THEN sr_.ticketid 
                 ELSE relatedsr_.relatedreckey 
               END                    AS hlagchangeticketid, 
               wochange.cinum         AS cinum, 
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
               INNER JOIN sr AS sr_ 
                      ON ( wochange.origrecordid = sr_.ticketid 
                           AND wochange.origrecordclass = 'SR' 
                           AND wochange.woclass = 'CHANGE' ) 
               INNER JOIN relatedrecord AS relatedsr_ 
                      ON ( wochange.wonum = relatedsr_.recordkey 
                           AND relatedsr_.relatedrecclass = 'SR' 
                           AND relatedsr_.class = 'CHANGE' ) 
               INNER JOIN sr AS relatedsr_sr_ 
                      ON ( relatedsr_.relatedreckey = relatedsr_sr_.ticketid ) 
        WHERE   EXISTS (SELECT 1 
                         FROM   pmchgotherapprovers approvals_ 
                         WHERE  ( wochange.wonum = approvals_.wonum ) 
                                AND approvals_.approvergroup IN ( 
                                    'C-HLC-WW-X-ISM-SSO', 'C-HLC-WW-RO-EXT', 
                                    'C-HLC-WW-X-TUI', 'C-HLC-WW-EFU-TUI', 
                                    'C-HLC-WW-EFU-SSO' 
                                      )) 
                                                          
               AND ( wochange.pluspcustomer LIKE 'HLC-%' ) 

        -- those with joined tickets for approvals group 2
        UNION
        select wochange.workorderid   AS workorderid,
               sr_.ticketid           AS sr_ticketid,
               relatedsr_.recordkey   AS relatedsr_recordkey,
               relatedsr_sr_.ticketid AS relatedsr_sr_ticketid,
               wochange.wonum         AS wonum,
               CASE
                 WHEN ( relatedsr_.relatedreckey IS NOT NULL
                        AND relatedsr_sr_.description IS NOT NULL ) THEN
                 relatedsr_sr_.description
                 WHEN ( sr_.ticketid IS NOT NULL
                        AND sr_.description IS NOT NULL ) THEN sr_.description
                 ELSE wochange.description
               END                    AS hlagchangesummary,
               CASE
                 WHEN sr_.ticketid IS NOT NULL THEN sr_.ticketid
                 ELSE relatedsr_.relatedreckey
               END                    AS hlagchangeticketid,
               wochange.cinum         AS cinum,
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
               INNER JOIN sr AS sr_
                      ON ( wochange.origrecordid = sr_.ticketid
                           AND wochange.origrecordclass = 'SR'
                           AND wochange.woclass = 'CHANGE' )
               INNER JOIN relatedrecord AS relatedsr_
                      ON ( wochange.wonum = relatedsr_.recordkey
                           AND relatedsr_.relatedrecclass = 'SR'
                           AND relatedsr_.class = 'CHANGE' )
               INNER JOIN sr AS relatedsr_sr_
                      ON ( relatedsr_.relatedreckey = relatedsr_sr_.ticketid )
               WHERE EXISTS (SELECT 1
                             FROM   woactivity AS woactivity_
                             WHERE  wochange.wonum = woactivity_.parent
                                    AND woactivity_.ownergroup IN (
                                        'C-HLC-WW-X-ISM-SSO', 'C-HLC-WW-RO-EXT',
                                        'C-HLC-WW-X-TUI', 'C-HLC-WW-EFU-TUI',
                                        'C-HLC-WW-EFU-SSO'
                                                                  )) 
               AND ( wochange.pluspcustomer LIKE 'HLC-%' )



        UNION

        -- those WITHOUT joined tickets for approvals group 1
        select wochange.workorderid   AS workorderid, 
               NULL AS sr_ticketid, 
               NULL AS relatedsr_recordkey, 
               NULL AS relatedsr_sr_ticketid, 
               wochange.wonum         AS wonum, 
               wochange.description    AS hlagchangesummary, 
               NULL AS hlagchangeticketid, 
               wochange.cinum         AS cinum, 
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
               ON ( wochange.wonum = approvals_.wonum  AND approvals_.approvergroup IN ('C-HLC-WW-X-ISM-SSO', 'C-HLC-WW-RO-EXT', 'C-HLC-WW-X-TUI', 'C-HLC-WW-EFU-TUI','C-HLC-WW-EFU-SSO')) 
               AND ( wochange.pluspcustomer LIKE 'HLC-%' ) 
               and not exists (select 1 from sr AS sr_ where ( wochange.origrecordid = sr_.ticketid ))
        UNION 

        -- those WITHOUT joined tickets for approvals group 2
        select wochange.workorderid   AS workorderid, 
               NULL AS sr_ticketid, 
               NULL AS relatedsr_recordkey, 
               NULL AS relatedsr_sr_ticketid, 
               wochange.wonum         AS wonum, 
               wochange.description    AS hlagchangesummary, 
               NULL AS hlagchangeticketid, 
               wochange.cinum         AS cinum, 
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
               AND woactivity_.ownergroup IN ( 
              'C-HLC-WW-X-ISM-SSO', 'C-HLC-WW-RO-EXT', 
              'C-HLC-WW-X-TUI', 'C-HLC-WW-EFU-TUI', 
              'C-HLC-WW-EFU-SSO' ) 
               AND ( wochange.pluspcustomer LIKE 'HLC-%' ) 
               and not exists (select 1 from sr AS sr_ where ( wochange.origrecordid = sr_.ticketid ))

        UNION ALL 

        -- the rest
        SELECT NULL, 
               NULL, 
               NULL, 
               NULL, 
               '-666'                  AS zeroedattr, 
               srforchange.description AS hlagchangesummary, 
               srforchange.ticketid    AS ticketid, 
               asset_.description      AS asset_description, 
               NULL, 
               NULL, 
               CASE 
                 WHEN srforchange.pluspcustomer IS NOT NULL 
                      AND srforchange.pluspcustomer = 'HLC-00' THEN 'HLAG' 
                 WHEN srforchange.pluspcustomer IS NOT NULL 
                      AND Length(srforchange.pluspcustomer) >= 3 THEN Substr( 
                 srforchange.pluspcustomer, Length( 
                 srforchange.pluspcustomer) - 2, 3) 
                 ELSE '' 
               END                     AS hlagpluspcustomer, 
               CASE 
                 WHEN Locate('@', srforchange.reportedby) > 0 THEN Substr( 
                 srforchange.reportedby, 1, 
                 Locate('@', srforchange.reportedby) - 1) 
                 ELSE srforchange.reportedby 
               END                     AS hlagreportedby, 
               srforchange.status      AS status 
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
                     AND srforchange.templateid IN ( 'HLCDECHSSO' ) ) 
               AND ( srforchange.pluspcustomer LIKE 'HLC-%' ) 
        ORDER  BY wonum DESC ;

        ";


        internal static string GenerateQuery(EntityMetadata metadata, SearchRequestDto searchDTO) {
            return BaseQuery;
        }
    }
}
