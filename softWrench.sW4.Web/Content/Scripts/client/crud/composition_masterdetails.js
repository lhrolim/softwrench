(function (angular, app) {
    "use strict";

    app.directive('compositionMasterDetails', function (contextService) {
        return {
            restrict: 'E',
            replace: false,
            templateUrl: contextService.getResourceUrl('/Content/Templates/crud/composition_masterdetails.html'),
            scope: {
                cancelfn: '&',
                compositionschemadefinition: '=',
                compositiondata: '=',
                parentdata: '=',
                parentschema: '=',
                relationship: '@',
                title: '@'
            },

            controller: ["$scope", "$element", "$attrs", "formatService", "schemaService", "iconService", "eventService", "i18NService", "$log", "controllerInheritanceService",
                function ($scope, $element, $attrs, formatService, schemaService, iconService, eventService, i18NService, $log, controllerInheritanceService) {

                    var log = $log.getInstance('sw4.composition.master/detail');

                    $scope.compositionlistschema = $scope.compositionschemadefinition.schemas.list;

                    $scope.compositiondetailschema = $scope.compositionschemadefinition.schemas.detail;

                    $scope.i18NInputLabel = function (fieldMetadata) {
                        return i18NService.getI18nInputLabel(fieldMetadata, $scope.compositiondetailschema);
                    };

                    $scope.displayDetails = function (entry) {
                        //close the current record
                        $scope.getDetailDatamap.open = false;

                        //display the selected details
                        $scope.getDetailDatamap = entry;
                        entry.open = true;

                        if (!entry.read) {
                            //remove the read icon
                            angular.forEach(entry.master.icons, function (icon, index) {
                                if (icon.indexOf("read") >= 0) {
                                    entry.master.icons[index] = "";
                                }
                            });

                            //update the read flag on the server
                            var parameters = {};
                            parameters.compositionItemId = entry.commloguid;
                            parameters.compositionItemData = entry;
                            parameters.parentData = $scope.parentdata;
                            parameters.parentSchema = $scope.parentschema;
                            eventService.onviewdetail($scope.compositionschemadefinition, parameters);

                            entry.read = true;
                        }
                        // scroll to the detail:
                        $(document.body).animate({ scrollTop: 0 });

                        log.debug('$scope.displayDetails', entry);
                    };

                    $scope.getDetailDisplayables = function () {
                        return $scope.compositionschemadefinition.schemas.detail.displayables;
                    };

                    $scope.getListSchema = function () {
                        return $scope.compositionschemadefinition.schemas.list;
                    };

                    $scope.getFormattedValue = function (value, column, datamap) {
                        var formattedValue = formatService.format(value, column, datamap);
                        if (formattedValue === "-666" || formattedValue === -666) {
                            //this magic number should never be displayed! 
                            //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                            return null;
                        }
                        return formattedValue;
                    };

                    $scope.hasRecords = function () {
                        return $scope.compositiondata.length > 0;
                    };

                    $scope.i18N = function (key, defaultValue, paramArray) {
                        return i18NService.get18nValue(key, defaultValue, paramArray);
                    };

                    $scope.mapMaster = function (compositiondata, schema) {

                        //loop thru the records
                        compositiondata.forEach(function (entry) {
                            var master = {};
                            master.icons = [];

                            //loop thru the schema fields
                            schema.displayables.forEach(function (field) {

                                //map fields to master info
                                var string = entry[field.attribute];
                                var qualifier = field.qualifier;

                                //format fields
                                switch (qualifier) {
                                    case "message":
                                        //remove html tags
                                        string = $("<p>" + string + "</p>").text();

                                        //reduce string length
                                        if (string.length > 200) {
                                            string = string.substring(0, 200) + "...";
                                        }
                                        break;
                                    default:
                                        //reduce string length
                                        if (string.length > 80) {
                                            string = string.substring(0, 80) + "...";
                                        }
                                }

                                //remap fields
                                switch (qualifier) {
                                    case "personTo":
                                        //for comm logs, use if outbound
                                        if (!entry.inbound) {
                                            qualifier = "person";
                                        }
                                        break;
                                    case "personFrom":
                                        //for comm logs, use if inbound
                                        if (entry.inbound) {
                                            qualifier = "person";
                                        }
                                        break;
                                }

                                master[qualifier] = $scope.getFormattedValue(string, field, entry);

                                //process the icon fields
                                switch (field.rendererType) {
                                    case "icon":
                                        var icon = iconService.loadIcon(entry[field.attribute], field);

                                        if (icon) {
                                            master.icons.push(iconService.loadIcon(entry[field.attribute], field) + " " + field.attribute);
                                        }
                                        break;
                                }
                            });

                            //add the master info
                            entry.master = master;
                        });
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
                        return !!dataValue;
                    };

                    /* Directvie Methods */
                    this.getAddIcon = function () {
                        var iconCompositionAddbutton = $scope.compositionschemadefinition.schemas.list.properties['icon.composition.addbutton'];
                        if (!iconCompositionAddbutton) {
                            //use the same as the tab by default
                            iconCompositionAddbutton = $scope.compositionschemadefinition.schemas.list.properties['icon.composition.tab'];
                        }
                        return iconCompositionAddbutton;
                    }

                    this.getAddLabel = function () {
                        return $scope.i18N($scope.relationship + ".add", "Add " + $scope.title);
                    }

                    //init directive
                    function prepareData() {
                        //add master info to the record data
                        $scope.mapMaster($scope.compositiondata, $scope.getListSchema());

                        //display the first record
                        $scope.getDetailDatamap = $scope.compositiondata[0];

                        if ($scope.getDetailDatamap) {
                            $scope.getDetailDatamap.open = true;
                        }
                    }
                    function init(self) {
                        prepareData();

                        controllerInheritanceService
                            .begin(self)
                            .inherit(CompositionListController, {
                                $scope: $scope,
                                $element: $element,
                                $attrs: $attrs
                            })
                            .overrides()
                            .scope($scope, "selectPage", function (original, params, context) {
                                return original.apply(context, params).then(function () {
                                    prepareData();
                                });
                            })
                            .scope($scope, "setForm", function(original, params, context) {
                                console.log(original, params, context);
                            });

                        log.debug($scope, $scope.compositiondata);
                    }
                    init(this);
                }]
        };
    });

})(angular, app);