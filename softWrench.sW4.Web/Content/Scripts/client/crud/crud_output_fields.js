﻿(function (app, angular) {
    "use strict";

app.directive('sectionElementOutput', function ($compile) {
    "ngInject";

    return {
        restrict: "E",
        replace: true,
        scope: {
            schema: '=',
            datamap: '=',
            displayables: '=',
            extraparameters: '=',
            rendererParameters: '=',
            orientation: '@',
            hideempty: '=',
            forprint: "="
        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.displayables)) {
                element.append(
                    "<crud-output-fields schema='schema'" +
                                    "datamap='datamap'" +
                                    "displayables='displayables'" +
                                    "section-parameters='rendererParameters'" +
                                    "orientation='{{orientation}}' hideempty='hideempty' forprint='forprint'></crud-output-fields>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});

app.directive('crudOutputFields', function (contextService) {
    "ngInject";

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_output_fields.html'),
        scope: {
            extraparameters: '=',
            schema: '=',
            datamap: '=',
            displayables: '=',
            sectionParameters: '=',
            orientation: '@',
            hideempty: '=',
            forprint: "="
        },

        controller: function ($scope, $injector, formatService, printService, tabsService, fieldService, commandService, redirectService, i18NService, expressionService, richTextService, layoutservice, associationService, crudContextHolderService) {
            $scope.$name = 'crud_output_fields';

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.i18NLabel = $scope.i18NLabel || function (fieldMetadata) {
                var label = i18NService.getI18nLabel(fieldMetadata, $scope.schema);
                if (label != undefined && label != "") {
                    label = label.replace(':', '');
                }
                return label;
            };

            $scope.getLabelOutputClass = function(fieldMetadata) {
                return fieldMetadata.rendererParameters["outputclass"];
            }

            $scope.getChildrenExpanded = function (attribute) {
                var root = datamap[attribute];
                var result = [];
                if (!root.children) {
                    return result;
                }
                for (var i = 0; i < root.children.length; i++) {
                    
                }
            },

            $scope.initField = function (fieldMetadata) {
                $scope.bindEvalExpression(fieldMetadata);
                return null;
            };

            $scope.handleDefaultValue = function (data, column) {
                var key = column.target ? column.target : column.attribute;

                if (column.defaultValue != null && data[key] == null) {
                    if (column.enableDefault != null && expressionService.evaluate(column.enableDefault, data)) {
                        data[key] = column.defaultValue;
                    }
                }
            }

            $scope.getFormattedValue = function (value, column, datamap) {
                if (column.type === 'ApplicationAssociationDefinition') {
                    //interpolation does not work with promises
                    var result = associationService.getLabelText(column.associationKey, value, { avoidPromise: true });
                    return result;
                }

                return formatService.format(value, column, datamap);
            };


            $scope.getSectionStyle = function (fieldMetadata) {
                var style = {};

                if (fieldMetadata.parameters != null) {
                    for (i in fieldMetadata.parameters) {
                        style[i] = fieldMetadata.parameters[i];
                    }
                }

                if (fieldMetadata.rendererParameters != null) {
                    for (i in fieldMetadata.rendererParameters) {
                        style[i] = fieldMetadata.rendererParameters[i];
                    }
                }

                if (style.width == null && !$scope.isVerticalOrientation() && $scope.countVisibleDisplayables > 0) {
                    style.width = (100 / $scope.countVisibleDisplayables) + '%';
                }

                return style;
            };



            $scope.bindEvalExpression = function (fieldMetadata) {
                if (fieldMetadata.evalExpression == null) {
                    return;
                }
                var variables = expressionService.getVariablesForWatch(fieldMetadata.evalExpression, $scope.datamap, $scope);
                $scope.$watchCollection(variables, function (newVal, oldVal) {
                    if (newVal != oldVal) {
                        $scope.datamap[fieldMetadata.attribute] = expressionService.evaluate(fieldMetadata.evalExpression, $scope.datamap, $scope);
                    }
                });
            }

            $scope.getHeaderStyle = function (fieldMetadata) {
                var style = {};

                if (fieldMetadata.header != null && fieldMetadata.header.parameters != null) {
                    for (i in fieldMetadata.header.parameters) {
                        style[i] = fieldMetadata.header.parameters[i];
                    }
                }

                return style;
            };

            $scope.hasLabelOrHeader = function (fieldMetadata) {
                return fieldMetadata.header || fieldMetadata.label;
            }

            $scope.initRichtextField = function (fieldMetadata) {
                var content = $scope.datamap[fieldMetadata.attribute];
                $scope.datamap[fieldMetadata.attribute] = richTextService.getDecodedValue(content);
            }

            $scope.showField = function (application, fieldMetadata) {
                ////always show sections
                //if (fieldMetadata.type === 'ApplicationSection') {
                //    return false;
                //}

                //always hide hidden fields
                var hidden = $scope.isFieldHidden(application, fieldMetadata);
                if (hidden) {
                    return hidden;
                }

                //if not hidding empty fields, show fields
                if (!$scope.hideempty) {
                    return false;
                }

                //if no value, hide empty fields
                var hasValue = !$scope.fieldHasValue(fieldMetadata);
                return hasValue;
            };

            function init() {
                $scope.countVisibleDisplayables = fieldService.countVisibleDisplayables($scope.datamap, $scope.schema, $scope.displayables);
                $injector.invoke(BaseController, this, {
                    $scope: $scope,
                    i18NService: i18NService,
                    fieldService: fieldService,
                    formatService: formatService,
                    layoutservice: layoutservice,
                    expressionService: expressionService
                });
            }

            init();

            $scope.selectNodeLabel = function () {
                //workaround to avoid treeview node to be selected
            };
        }
    };
});

})(app, angular);