(function (angular) {
    "use strict";

    const app = angular.module('sw_layout');

    app.directive('compositionListWrapper', function ($compile, i18NService, $log, $timeout, compositionService, spinService, crudContextHolderService) {
        "ngInject";

        return {
            restrict: 'E',
            replace: true,
            template: "<div></div>",
            scope: {
                metadata: '=',
                parentschema: '=',
                parentdata: '=',
                cancelfn: '&',
                previousschema: '=',
                previousdata: '=',
                mode: '@',
                inline: '@',
                ismodal: '@',
                tabid: '@',
                startlazyloading: "@"
            },
            link: function (scope, element, attrs) {

                scope.$name = 'compositionlistwrapper';

                var doLoad = function () {
                    $log.getInstance('compositionlistwrapper#doLoad').debug('loading composition {0}'.format(scope.tabid));
                    const metadata = scope.metadata;
                    scope.tabLabel = i18NService.get18nValue(metadata.schema.schemas.list.applicationName + '._title', metadata.label);

                    scope.compositiondata = scope.parentdata[scope.metadata.relationship];

                    if (!scope.compositiondata) {
                        const arr = [];
                        //this modifies the original datamap causing a dirty checking issue
                        //SWWEB-2992
                        crudContextHolderService.clearDetailDataResolved();
                        scope.parentdata[scope.metadata.relationship] = arr;

                        //a blank array if nothing exists, scenario for selfcompositions
                        scope.compositiondata = arr;

                    }

                    scope.compositionschemadefinition = metadata.schema;
                    scope.compositionlistschema = scope.compositionschemadefinition.schemas.list;
                    scope.compositiondetailschema = scope.compositionschemadefinition.schemas.detail;
                    scope.relationship = metadata.relationship;

                    //display the list composition by default
                    if (scope.compositionschemadefinition.schemas.list.properties.masterdetail === "true") {
                        element.append(
                            "<composition-master-details data-cancelfn='cancel(data,schema)' " +
                            "data-compositiondata='compositiondata' data-compositionschemadefinition='compositionschemadefinition' " +
                            "data-parentdata='parentdata' parentschema='parentschema' " +
                            "mode='{{mode}}' " +
                            "data-relationship='{{relationship}}' data-title='{{tabLabel}}' />");
                    } else {
                        element.append("<composition-list data-title='{{tabLabel}}' ismodal='{{ismodal}}'" +
                            "compositionschemadefinition='compositionschemadefinition' " +
                            "relationship='{{relationship}}' " +
                            "compositiondata='compositiondata' " +
                            "metadatadeclaration='metadata' " +
                            "parentschema='parentschema' " +
                            "mode='{{mode}}' " +
                            "parentdata='parentdata' " +
                            "cancelfn='cancel(data,schema)' " +
                            "previousschema='previousschema' " +
                            "previousdata='previousdata' />");
                    }

                    $compile(element.contents())(scope);

                    $timeout(() => {
                        crudContextHolderService.setDetailDataResolved();
                    },0,false);
                    //controls tab lazy loading
                    scope.loaded = true;
                }
                const custom = scope.metadata.schema.renderer.rendererType == 'custom';
                const isInline = scope.metadata.inline;
                if (scope.metadata.type == "ApplicationCompositionDefinition" && isInline && !custom) {
                    //inline compositions should load automatically
                    doLoad();

                }

                scope.cancel = function (data, schema) {
                    scope.cancelfn({ data: data, schema: schema });
                }


                scope.$on(JavascriptEventConstants.ModalShown, function (event, modalData) {
                    if (scope.ismodal === "true") {
                        //inline compositions inside of the modal need to be refreshed (relinked)
                        const datamap = modalData.datamap;
                        scope.parentdata = datamap;
                        doLoad();
                    }
                });

                scope.$on(JavascriptEventConstants.HideModal, function (event) {
                    if (scope.ismodal === "true") {
                        $log.get('compositionlistwrapper#doLoad', ["composition", "inline"]).debug('wiping <composition-list> directive due to modal disposal');
                        //inline compositions inside of the modal need to be refreshed (relinked)
                        element.empty();
                    }
                });

                const lazyLoad = function () {
                    if (!compositionService.isCompositionLodaded(scope.tabid)) {
                        spinService.start({ compositionSpin: true });
                    }
                    if (!scope.loaded) {
                        doLoad();
                    }
                }

                scope.$on("sw_lazyloadtab", function (event, tabid) {
                    if (scope.tabid == tabid) {
                        lazyLoad();
                    }
                });

                if (scope.startlazyloading === "true") {
                    lazyLoad();
                }
            }
        }
    });


    //#region legacy directives

    app.directive('expandedItemOutput', function ($compile) {
        "ngInject";

        return {
            restrict: "E",
            replace: true,
            scope: {
                displayables: '=',
                schema: '=',
                datamap: '=',
                cancelfn: '&'
            },
            template: "<div></div>",
            link: function (scope, element, attrs) {
                if (angular.isArray(scope.displayables)) {
                    element.append(
                        "<crud-output schema='schema'" +
                        "datamap='datamap'" +
                        "displayables='displayables'" +
                        "cancelfn='cancelfn()'></crud-output>"
                    );
                    $compile(element.contents())(scope);
                }
            }
        }
    });

    app.directive('expandedItemInput', function ($compile) {
        "ngInject";

        return {
            restrict: "E",
            replace: true,
            scope: {
                displayables: '=',
                schema: '=',
                datamap: '=',
                savefn: '&',
                cancelfn: '&'
            },
            template: "<div></div>",
            link: function (scope, element, attrs) {
                if (angular.isArray(scope.displayables)) {
                    element.append(
                        "<crud-input schema='schema'" +
                        "datamap='datamap'" +
                        "displayables='displayables'" +
                        "savefn='savefn()'" +
                        "cancelfn='cancelfn()'></crud-input>"
                    );
                    $compile(element.contents())(scope);
                }
            }
        }
    });

    app.directive('newItemInput', function ($compile, fieldService) {
        "ngInject";

        return {
            restrict: "E",
            replace: true,
            scope: {
                displayables: '=',
                elementid: '=',
                schema: '=',
                datamap: '=',
                cancelfn: '&',
                savefn: '&',
                parentdata: '=',
                parentschema: '=',
                previousdata: '=',
                previousschema: '='

            },
            template: "<div></div>",
            link: function (scope, element, attrs) {
                if (angular.isArray(scope.displayables)) {
                    fieldService.fillDefaultValues(scope.displayables, scope.datamap, scope);
                    element.append(
                        "<crud-input schema='schema' " +
                        "datamap='datamap' " +
                        "displayables='displayables' " +
                        "elementid='crudInputNewItemComposition' " +
                        "blockedassociations='blockedassociations' " +
                        "parentdata='parentdata' " +
                        "parentschema='parentschema' " +
                        "previousdata='previousdata' " +
                        "previouschema='previousschema' " +
                        "savefn='savefn()' " +
                        "cancelfn='cancelfn()' " +
                        "composition='true'></crud-input>"
                    );
                    $compile(element.contents())(scope);
                }
            }
        }
    });

    //#endregion

})(angular);