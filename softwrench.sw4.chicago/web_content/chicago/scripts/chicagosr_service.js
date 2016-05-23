(function () {
    'use strict';

    angular
      .module('sw_layout')
      .factory('chicagosrService', ['alertService', chicagosrService]);

    function chicagosrService(alertService) {
        var service = {
            afterChangeImpact: afterChangeImpact,
            afterChangeUrgency: afterChangeUrgency,
            beforeChangeStatus: beforeChangeStatus
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

        return service;
    }
})();
