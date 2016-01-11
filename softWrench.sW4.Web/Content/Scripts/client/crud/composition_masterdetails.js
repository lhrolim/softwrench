app.directive('compositionMasterDetails', function (contextService, formatService, schemaService, iconService, $log) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/composition_masterdetails.html'),
        scope: {
            compositionschemadefinition: '=',
            compositiondata: '=',
            parentdata: '=',
            parentschema: '=',
            relationship: '@',
        },

        controller: function ($scope) {
            var log = $log.getInstance('sw4.composition.master/detail');

            $scope.mapMaster = function (compositiondata, schema) {

                //loop thru the records
                compositiondata.forEach(function (entry) {
                    var master = {};
                    master.icons = [];

                    //loop thru the schema fields
                    schema.displayables.forEach(function (field) {

                        //TODO: for testing only, load mapping from metadata
                        switch (field.attribute) {
                            case 'createdate':
                                master.date = $scope.getFormattedValue(entry.createdate, field, entry);
                                break;
                            case 'message':
                                //remove html tags
                                var string = $('<p>' + entry.message + '</p>').text();

                                //reduce string length
                                if (string.length > 200) {
                                    string = string.substring(0, 200) + '...';
                                }

                                master.message = $scope.getFormattedValue(string, field, entry);
                                break;
                            case 'sendto':
                                master.author = $scope.getFormattedValue(entry.sendto, field, entry);
                                break;
                            case 'subject':
                                master.title = $scope.getFormattedValue(entry.subject, field, entry);
                                break;
                        }

                        //process the icon fields
                        switch (field.rendererType) {
                            case 'icon':
                                var icon = iconService.loadIcon(entry[field.attribute], field);

                                if (icon) {
                                    master.icons.push(iconService.loadIcon(entry[field.attribute], field) + ' ' + field.attribute);
                                }
                                break;
                        }
                    });

                    //add the master info
                    entry.master = master;
                });
            };
            
            $scope.displayDetails = function (entry) {
                $scope.getDetailDatamap = entry;
            };

            $scope.getDetailDisplayables = function () {
                return $scope.compositionschemadefinition.schemas.detail.displayables;
            };

            $scope.getDetailSchema = function () {
                return $scope.compositionschemadefinition.schemas.detail;
            };

            $scope.getListSchema = function () {
                return $scope.compositionschemadefinition.schemas.list;
            };

            $scope.getFormattedValue = function (value, column, datamap) {
                var formattedValue = formatService.format(value, column, datamap);
                if (formattedValue == "-666") {
                    //this magic number should never be displayed! 
                    //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                    return null;
                }
                return formattedValue;
            };

            $scope.hasRecords = function () {
                return $scope.compositiondata.length > 0;
            };

            $scope.showDetailField = function (fieldMetadata) {
                if ($scope.getDetailDatamap == undefined) {
                    return;
                }

                //don't show if hidden
                if (fieldMetadata.isHidden) {
                    return false;
                }

                //don't show if field is empty
                var dataValue = $scope.getDetailDatamap[fieldMetadata.attribute];
                return dataValue != undefined || dataValue != null;
            };

            /*Directvie Methods*/
            //this.function = function () {

            //    return something;
            //}

            //init();

            //add master info to the record data
            $scope.mapMaster($scope.compositiondata, $scope.getListSchema());

            //display the first record
            $scope.getDetailDatamap = $scope.compositiondata[0];

            log.debug($scope, $scope.compositiondata);
        }
    };
});
