(function (angular) {
    'use strict';



    function chicagosrService($q, alertService, searchService, restService) {


        function latestWorklogType(event) {
            const datamap = event.fields;
            var worklogType = '';
            var worklogs = datamap["worklog_"];
            if (worklogs == null || worklogs.length < 1) {
                return '';
            }
            worklogs = worklogs.sort((a, b) => (a.createdate < b.createdate) ? 1 : ((b.createdate < a.createdate) ? -1 : 0));
            worklogType = worklogs[0]['logtype'];
            return worklogType;
        }

        function afterChangeImpact(event) {
            var datamap = event.fields;
            datamap['impact'] = datamap['impacturgency_.impact'];
            var urgency = datamap['urgency'];
            if (urgency == null) {
                datamap['urgency'] = datamap['impacturgency_.urgency'];
            }
            datamap['internalpriority'] = datamap['impacturgency_.internalpriority'];
            if (datamap['impacturgency_.impact'] == null) {
                datamap['impact'] = null;
                datamap['urgency'] = null;
                datamap['internalpriority'] = null;
            }
        };

        function afterChangeUrgency(event) {
            var datamap = event.fields;
            datamap['urgency'] = datamap['impacturgency_.urgency'];
            var impact = datamap['impact'];
            if (impact == null) {
                datamap['impact'] = datamap['impacturgency_.impact'];
            }
            datamap['internalpriority'] = datamap['impacturgency_.internalpriority'];
            if (datamap['impacturgency_.urgency'] == null) {
                datamap['impact'] = null;
                datamap['urgency'] = null;
                datamap['internalpriority'] = null;
            }
        };

        function beforeChangeStatus(event) {
            const newStatus = event.newValue;
            const owner = event.fields["owner"];
            const resolverGroup = event.fields["itdresolvergroup"];
            if (newStatus.equalsIc('INPROG') && owner == null) {
                alertService.alert("Owner field cannot be null when changing the status to INPROG.");
                return false;
            }
            if (newStatus.equalsIc('SLAHOLD') && !latestWorklogType(event).toUpperCase().startsWith('SLA')) {
                alertService.alert("The status of SLA Hold cannot be applied to an SR unless the most recent Work Log Type is SLA.");
                return false;
            }
            if (newStatus.equalsIc('Rejected') && !latestWorklogType(event).toUpperCase().startsWith('REASON REJECTING')) {
                alertService.alert("The status of Rejected cannot be applied to an SR unless the most recent Work Log Type is Reason Rejecting.");
                return false;
            }
            if (newStatus.equalsIc('Resolved') && resolverGroup == null) {
                alertService.alert("Resolver Group must be valued in order to change the status to Resolved.");
                return false;
            }
            return true;
        };

        function afterChangeReportedBy(event) {
            var datamap = event.fields;
            var searchData = {
                personid: datamap['reportedby'],
                isprimary: '1'
            };
            var operation = searchService.getSearchOperationById("EQ");
            var extraparams = {
                searchOperators: {
                    "personid": operation,
                    "isprimary": operation
                }
            }
            datamap['department'] = datamap['reportedbyp_.department'] || datamap['department'];
            return $q.all([
                searchService.searchWithData("email", searchData, "list", extraparams),
                searchService.searchWithData("phone", searchData, "list", extraparams)
            ]).then(function (result) {
                var emailResult = result[0].data.resultObject[0];
                datamap['reportedemail'] = emailResult ? emailResult['emailaddress'] : '';
                var phoneResult = result[1].data.resultObject[0];
                datamap['reportedphone'] = phoneResult ? phoneResult['phonenum'] : '';
            });
        };


        function afterchangeownergroup(event) {
            const dm = event.fields;
            if (window.isNullOrEmpty(dm['ownergroup'])) {
                dm['ownergroup'] = null;
                return $q.when();
            }
            if (dm["owner"] === null) {
                //no need to reevaluate owner
                return $q.when();
            }

            return restService.get("ChicagoSr", "IsOwnerMember", { newOwnerGroup: dm["ownergroup"], owner: dm["owner"] }).then(result => {
                var stillMember = result.data;
                if (!stillMember) {
                    alertService.alert("Please select another owner");
                    dm['owner'] = null;
                }
            });


        };

        const service = {
            afterChangeImpact,
            afterChangeUrgency,
            afterchangeownergroup,
            beforeChangeStatus,
            afterChangeReportedBy
        };

        return service;
    }

    angular
      .module('chicago')
      .clientfactory('srService', ['$q', 'alertService', 'searchService', 'restService', chicagosrService]);

})(angular);
