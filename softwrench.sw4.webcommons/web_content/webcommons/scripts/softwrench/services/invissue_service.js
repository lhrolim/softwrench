
(function () {
    'use strict';

    angular.module('sw_layout').factory('invissueService', ['searchService', "$log", invissueService]);

    function invissueService(searchService) {

        var service = {
            afterChangeBin: afterChangeBin
        };

        return service;

        function afterChangeBin(parameters) {
            var fields = parameters['fields'];

            if (fields['binnum'] == null) {
                fields['binbalances_.binnum'] = null;
                fields['binbalances_.lotnum'] = null;
                fields['binbalances_.curbal'] = null;
                return;
            }
            if (fields['binbalances_'] && fields['binnum'] != null) {
                //Check if null rather than nullOrEmpty since the binnum for an association option can be an empty string
                fields['lotnum'] = fields['binbalances_.lotnum'];
                fields['#curbal'] = fields['binbalances_.curbal'];
                fields['curbal'] = fields['binbalances_.curbal'];
                fields['quantity'] = 1;
                //to enable composition details on batch mode
                if (fields['#datamaptype'] == "compositionitem") {
                    fields['matusetransid'] = -1 * (fields["#datamapidx"] - 1);
                }

                
                return;
            };
            // If the binbalances_ record is not filled but the binnum is
            // (binnum filled after itemnum change) then use the available 
            // fields to find an applicable lotnum and curbal. If the binnum
            // has been cleared, clear the lot and curbal
            if (fields['binnum'] != null && fields['binnum'] != " ") {
                var searchData = {
                    orgid: fields['orgid'],
                    siteid: fields['siteid'],
                    itemnum: fields['itemnum'],
                    location: fields['storeloc'],
                    binnum: fields['binnum']
                };
                searchService.searchWithData("invbalances", searchData, "invbalancesList").success(function (data) {
                    var resultObject = data.resultObject;
                    var resultFields = resultObject[0].fields;
                    var lotnum = resultFields['lotnum'];
                    var curbal = resultFields['curbal'];
                    fields['lotnum'] = lotnum;
                    fields['#curbal'] = curbal == null ? 0 : curbal;
                    fields['curbal'] = curbal == null ? 0 : curbal;
                    fields['binbalances_.lotnum'] = lotnum;
                    fields['binbalances_.curbal'] = curbal == null ? 0 : curbal;
                    fields['quantity'] = 1;
                    //to enable composition details on batch mode
                    if (fields['#datamaptype'] == "compositionitem") {
                        //let´s put a negative id so that it gets ignored on maximo side
                        fields['matusetransid'] = -1 *(fields["#datamapidx"]-1);
                    }
                });
            }
            else {
                fields['lotnum'] = null;
                fields['#curbal'] = null;
            }
        };

    }
})();

