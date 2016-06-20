(function () {
    'use strict';

    angular
      .module('sw_layout')
      .factory('toshibasrService', ['$q', 'searchService', chicagosrService]);

    function chicagosrService($q, searchService) {
        var service = {
            afterChangeAffectedPerson: afterChangeAffectedPerson
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

        return service;
    }
})();
