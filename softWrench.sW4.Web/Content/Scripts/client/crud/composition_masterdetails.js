(function (angular, app) {
    "use strict";

    app.directive('compositionMasterDetails', function (contextService, $log) {
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

            controller: ["$scope", "$element", "$attrs", "formatService", "schemaService", "iconService", "eventService", "i18NService", "controllerInheritanceService", "fieldService", "$timeout",
                function ($scope, $element, $attrs, formatService, schemaService, iconService, eventService, i18NService, controllerInheritanceService, fieldService, $timeout) {

                    var log = $log.getInstance('sw4.composition.master/detail');

                    $scope.compositionlistschema = $scope.compositionschemadefinition.schemas.list;
                    $scope.compositiondetailschema = $scope.compositionschemadefinition.schemas.detail;

                    $scope.i18NInputLabel = function (fieldMetadata) {
                        return i18NService.getI18nInputLabel(fieldMetadata, $scope.compositiondetailschema);
                    };

                    $scope.displayDetails = function (entry) {
                        //close the current record
                        $scope.getDetailDatamap.open = false;

                        //update the first message if another message is viewed
                        if (!$scope.getDetailDatamap.read) {
                            $scope.onViewDetail($scope.getDetailDatamap);
                        }

                        //display the selected details
                        $scope.getDetailDatamap = entry;
                        entry.open = true;

                        if (!entry.read) {
                            $scope.onViewDetail(entry);
                        }

                        // scroll to the detail:
                        $(document.body).animate({ scrollTop: 0 });

                        log.debug('$scope.displayDetails', entry);

                        $timeout(function () {
                            $(window).resize();
                        });
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
                        if (!compositiondata) return;

                        //loop thru the records
                        compositiondata.forEach(function (entry) {
                            var master = {
                                icons: []
                            };

                            //loop thru the schema fields
                            schema.displayables.forEach(function (field) {
                                var qualifier = fieldService.getQualifier(field, entry);

                                if (!!qualifier) {
                                    master[qualifier] = formatEntryString(entry, field, qualifier);
                                }

                                //process the icon fields
                                if (field.rendererType === "icon") {
                                    var icon = iconService.loadIcon(entry[field.attribute], field);
                                    if (!!icon) {
                                        master.icons.push(icon + " " + field.attribute);
                                    }
                                }
                            });

                            //add the master info
                            entry.master = master;
                        });
                    };

                    $scope.onViewDetail = function (entry) {
                        log.debug('mark as read', entry);

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
                    function formatEntryString(entry, field, qualifier) {
                        var string = entry[field.attribute];
                        if (qualifier === "message") {
                            string = $("<p>" + string + "</p>").text();
                        }
                        if (string.length > 200) {
                            string = string.substring(0, 200) + "...";
                        }

                        string = $scope.getFormattedValue(string, field, entry);

                        if (string == 'null') {
                            string = '';
                        }

                        return string;
                    }

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

                    function prepareDataProxy(original, params, context) {
                        var result = original.apply(context, params);
                        if (result && angular.isFunction(result.then)) {
                            return result.then(prepareData);
                        }
                        return prepareData();
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
                            .scope($scope, "selectPage", prepareDataProxy)
                            .scope($scope, "onSaveError", prepareDataProxy)
                            .scope($scope, "onAfterCompositionResolved", prepareDataProxy);

                        log.debug($scope, $scope.compositiondata);
                    }
                    init(this);

                    $scope.$on("sw_compositiondataresolved", prepareData);
                }],

            link: function (scope, element, attrs) {
                var log = $log.getInstance('sw4.composition.master/detail.link');                
                var scrollPanes = {
                    master: null,
                    details: null
                };

                function getPaneHeight(scroll, available, subtract) {

                    //get the total height of the content
                    var contents = $('.jspPane', scroll).height();
                    if (contents == null) {
                        contents = scroll.height();
                    }

                    //if the height was not set (no content), exit
                    if (contents == 0) {
                        return null;
                    }

                    //set pane height to smallest, contents or available area
                    if (contents > available) {
                        scroll.height(available - subtract);
                    } else {
                        scroll.height(contents);
                    }

                    return scroll.height();
                }

                function initScrollPane(pane, scroll) {
                    if (pane == null) {
                        pane = scroll.jScrollPane().data('jsp');
                    } else {
                        pane.reinitialise();
                    }

                    return pane;
                }

                function setSrcollHeight() {
                    var composition = $('.composition.master-details:visible');

                    if (composition.position() != undefined) {
                        var scrollTop = composition.position().top;
                        var scrollHeight = $(window).height() - scrollTop;

                        //create the master scroll pane
                        var masterScroll = $('.master .scroll', element);
                        var masterMargins = $('.master', element).outerHeight(true) - $('.master', element).height();
                        var pagination = $('.master .swpagination', element).outerHeight(true);

                        var newHeight = getPaneHeight(masterScroll, scrollHeight, masterMargins + pagination);
                        //only try to create scroll pane if height is set
                        if (newHeight != null) {
                            scrollPanes.master = initScrollPane(scrollPanes.master, masterScroll);
                        }

                        //create the details scroll pane
                        var detailScroll = $('.details .scroll', element);
                        var commandbuttons = $('.master-details .commands:visible', element).outerHeight(true);
                        newHeight = getPaneHeight(detailScroll, scrollHeight, commandbuttons);
                        //only try to create scroll pane if height is set
                        if (newHeight != null) {
                            scrollPanes.details = initScrollPane(scrollPanes.details, detailScroll);
                        }

                        log.debug(scrollPanes);
                    }
                }

                var lazyLayout = window.debounce(setSrcollHeight, 200);
                $(window).resize(lazyLayout);
                //$(window).trigger('resize');
            }
        };
    });

})(angular, app);