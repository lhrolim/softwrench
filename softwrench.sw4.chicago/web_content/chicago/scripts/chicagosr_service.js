(function () {
    'use strict';

    angular
      .module('sw_layout')
      .factory('chicagosrService', ['$q', 'alertService', 'searchService', chicagosrService]);

    function chicagosrService($q, alertService, searchService) {
        var service = {
            afterChangeImpact: afterChangeImpact,
            afterChangeUrgency: afterChangeUrgency,
            beforeChangeStatus: beforeChangeStatus,
            afterChangeReportedBy: afterChangeReportedBy
        };

        function latestWorklogType(datamap) {
            var worklogType = '';
            var worklogs = datamap.fields["worklog_"];
            worklogs.sort((a, b) => (a.createdate > b.createdate) ? 1 : ((b.createdate > a.createdate) ? -1 : 0));
            worklogType = worklogs[0]['logtype'];
            return worklogType;
        }

        function afterChangeImpact(datamap) {
            datamap.fields['impact'] = datamap.fields['impacturgency_.impact'];
            var urgency = datamap.fields['urgency'];
            if (urgency == null) {
                datamap.fields['urgency'] = datamap.fields['impacturgency_.urgency'];
            }
            datamap.fields['internalpriority'] = datamap.fields['impacturgency_.internalpriority'];
            if (datamap.fields['impacturgency_.impact'] == null) {
                datamap.fields['impact'] = null;
                datamap.fields['urgency'] = null;
                datamap.fields['internalpriority'] = null;
            }
        };

        function afterChangeUrgency(datamap) {
            datamap.fields['urgency'] = datamap.fields['impacturgency_.urgency'];
            var impact = datamap.fields['impact'];
            if (impact == null) {
                datamap.fields['impact'] = datamap.fields['impacturgency_.impact'];
            }
            datamap.fields['internalpriority'] = datamap.fields['impacturgency_.internalpriority'];
            if (datamap.fields['impacturgency_.urgency'] == null) {
                datamap.fields['impact'] = null;
                datamap.fields['urgency'] = null;
                datamap.fields['internalpriority'] = null;
            }
        };

        function beforeChangeStatus(datamap) {
            var newStatus = datamap.newValue;
            var owner = datamap.fields["owner"];
            var resolverGroup = datamap.fields["itdresolvergroup"];
            if (newStatus.equalsIc('INPROG') && owner == null) {
                alertService.alert("Owner field cannot be null when changing the status to INPROG.");
                return false;
            }
            if (newStatus.equalsIc('SLAHOLD') && !latestWorklogType(datamap).startsWith('SLA')) {
                alertService.alert("The status of SLA Hold cannot be applied to an SR unless the most recent Work Log Type is SLA.");
                return false;
            }
            if (newStatus.equalsIc('Rejected') && !latestWorklogType(datamap).startsWith('Reason Rejecting')) {
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
            datamap['department'] = datamap['reportedbyp_.department'];
            return $q.all([
                searchService.searchWithData("email", searchData, "list", extraparams),
                searchService.searchWithData("phone", searchData, "list", extraparams)
            ]).then(function(result) {
                var emailResult = result[0].data.resultObject[0];
                datamap['reportedemail'] = emailResult ? emailResult.fields['emailaddress'] : '';
                var phoneResult = result[1].data.resultObject[0];
                datamap['reportedphone'] = phoneResult ? phoneResult.fields['phonenum'] : '';
            });
        };

        return service;
    }
})();
