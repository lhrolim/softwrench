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

        function afterChangeUrgency(datamap) {
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

        function afterChangeReportedby(event) {
            var datamap = event;
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
