(function () {
    'use strict';

    angular
      .module('sw_layout')
      .factory('toshibasrService', ['$q', 'searchService', chicagosrService]);

    function chicagosrService($q, searchService) {
        var service = {
            afterChangeAffectedPerson: afterChangeAffectedPerson,
            afterChangeImpact: afterChangeImpact,
            afterChangeUrgency: afterChangeUrgency,
            afterChangeReportedby: afterChangeReportedby
        };

        function afterChangeAffectedPerson(event) {
            var datamap = event.fields;
            var searchData = {
                personid: datamap['affectedperson'],
                isprimary: '1'
            };
            var operation = searchService.getSearchOperationById("EQ");
            var extraparams = {
                searchOperators: {
                    "personid": operation,
                    "isprimary": operation
                }
            }
            return $q.all([
                searchService.searchWithData("email", searchData, "list", extraparams),
                searchService.searchWithData("phone", searchData, "list", extraparams)
            ]).then(function (result) {
                var emailResult = result[0].data.resultObject[0];
                datamap['affectedemail'] = emailResult ? emailResult.fields['emailaddress'] : '';
                var phoneResult = result[1].data.resultObject[0];
                datamap['affectedphone'] = phoneResult ? phoneResult.fields['phonenum'] : '';
            });
        };

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

        function afterChangeReportedby(event) {
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
            ]).then(function (result) {
                var emailResult = result[0].data.resultObject[0];
                datamap['reportedemail'] = emailResult ? emailResult.fields['emailaddress'] : '';
                var phoneResult = result[1].data.resultObject[0];
                datamap['reportedphone'] = phoneResult ? phoneResult.fields['phonenum'] : '';
            });
        };

        return service;
    }
})();
