(function (softwrench, angular, _) {
    "use strict";

    softwrench.directive('sectionElementInput', ["$compile", function ($compile) {
        return {
            restrict: "E",
            replace: true,
            scope: {
                schema: '=',
                datamap: '=',
                isDirty: '=',
                displayables: '=',
                associationOptions: '=',
                associationSchemas: '=',
                blockedassociations: '=',
                extraparameters: '=',
                elementid: '@',
                orientation: '@',
                islabelless: '@',
                lookupAssociationsCode: '=',
                lookupAssociationsDescription: '=',

            },
            template: "<div></div>",
            link: function (scope, element, attrs) {
                if (angular.isArray(scope.displayables)) {
                    element.append(
                    "<crud-input-fields displayables='displayables'" +
                    "schema='schema'" +
                    "datamap='datamap'" +
                    "is-dirty='isDirty'" +
                    "displayables='displayables'" +
                    "association-options='associationOptions'" +
                    "association-schemas='associationSchemas'" +
                    "blockedassociations='blockedassociations'" +
                    "elementid='{{elementid}}'" +
                    "orientation='{{orientation}}' insidelabellesssection='{{islabelless}}'" +
                    "outerassociationcode='lookupAssociationsCode' outerassociationdescription='lookupAssociationsDescription' issection='true'" +
                    "></crud-input-fields>"
                    );
                    $compile(element.contents())(scope);
                }
            }
        }
    }]);

    softwrench.directive('crudInputFields', [function () {

        return {
            restrict: 'E',
            replace: false,
            templateUrl: 'Content/Mobile/templates/directives/crud/crud_input_fields.html',
            scope: {
                schema: '=',
                datamap: '=',
                displayables: '=',
            },

            link: function (scope, element, attrs) {
                scope.name = "crud_input_fields";
            },

            controller: ["$scope", "offlineAssociationService", "crudContextService", "fieldService", "expressionService", "dispatcherService", "$timeout", "$log",
                function ($scope, offlineAssociationService, crudContextService, fieldService, expressionService, dispatcherService, $timeout, $log) {

                    $scope.associationSearch = function (query, componentId) {
                        return offlineAssociationService.filterPromise($scope.schema, $scope.datamap, componentId, query);
                    };

                    $scope.itemSelected = function (callback) {
                        return offlineAssociationService.updateExtraProjections(callback.item, callback.componentId);
                    }

                    $scope.optionFieldSelected = function (callback) {
                        return offlineAssociationService.updateExtraProjectionsForOptionField(callback.item, callback.componentId);
                    }

                    $scope.getAssociationLabelField = function (fieldMetadata) {
                        return offlineAssociationService.fieldLabelExpression(fieldMetadata);
                    }

                    $scope.getAssociationValueField = function (fieldMetadata) {
                        return offlineAssociationService.fieldValueExpression(fieldMetadata);
                    }

                    $scope.isFieldHidden = function (fieldMetadata) {
                        return fieldService.isFieldHidden($scope.datamap, $scope.schema, fieldMetadata);
                    }

                    $scope.isFieldRequired = function (requiredExpression) {
                        if (Boolean(requiredExpression)) {
                            return expressionService.evaluate(requiredExpression, $scope.datamap);
                        }
                        return requiredExpression;
                    };

                    class ChangeEventDispatcher {
                        constructor(fields, dispatcher, timeout, logger) {
                            this.fields = fields;
                            this.dispatcher = dispatcher;
                            this.timeout = timeout;
                            this.logger = logger;
                            this.eventDescriptors = this.fields.map((f, i) => ({
                                name: f.attribute,
                                shouldWatch: true,
                                event: f.events["afterchange"]
                            }));
                            //properties on the datamap that must be watched
                            this.expressions = _.sortBy(this.eventDescriptors, "position").map(e => `datamap.${e.name}`);
                        }
                        getEvent(expression) {
                            return this.eventDescriptors.find(e => `datamap.${e.name}` === expression);
                        }

                        handleIgnoreWatchScenario(descriptor, newValue, datamap, ignoreWatchIdx) {
                            descriptor.shouldWatch = false;
                            let realValue = newValue.substring(0, ignoreWatchIdx);
                            if (realValue === "null") {
                                realValue = null;
                            }

                            this.logger.debug(`setting real value ${realValue} for association ${descriptor.name}`);
                            const dm = crudContextService.currentDetailItemDataMap();
                            dm[descriptor.name] = realValue;
                            datamap[descriptor.name] = realValue;
                            try {
                                $scope.$digest();
                                descriptor.shouldWatch = true;
                            } catch (e) {
                                //nothing to do, just checking if digest was already in place or not
                                $timeout(() => descriptor.shouldWatch = true, 0, false);
                            }
                        }

                        dispatchEventFor(position, schema, datamap, newValue) {
                            const descriptor = this.getEvent(position);
                            if (!descriptor.shouldWatch) {
                                this.logger.trace(`ignoring event for ${descriptor.name}`);
                                return;
                            }
                            const ignoreWatchIdx = newValue == null ? -1 : newValue.indexOf("$ignorewatch");
                            if (ignoreWatchIdx >= 0) {
                                return this.handleIgnoreWatchScenario(descriptor, newValue, datamap, ignoreWatchIdx);
                            }

                            const service = descriptor.event.service;
                            const method = descriptor.event.method;
                            const handler = this.dispatcher.loadService(service, method);
                            const params = { schema, datamap, newValue };

                            this.logger.debug(`dispatching 'afterchange' event for field '${descriptor.name}'. Value: ${newValue}`);
                            this.timeout(() => handler(params))
                                .catch(e => this.logger.error(`Failed to execute ${service}.${method} on 'afterchange' of field '${descriptor.name}'`, e));
                        }
                    }

                    function init() {
                        // watching for changes to trigger afterchange event handlers
                        const watchableFields = $scope.displayables.filter(f => f.events.hasOwnProperty("afterchange") && !!f.events["afterchange"]);
                        if (!watchableFields || watchableFields.length <= 0) return;

                        const logger = $log.get("crud_input_fields", ["datamap", "event", "association"]);
                        const dispatcher = new ChangeEventDispatcher(watchableFields, dispatcherService, $timeout, logger);

                        logger.debug(`watching ${dispatcher.expressions}`);

                        dispatcher.expressions.forEach(expression => {
                            $scope.$watch(expression, (newValue, oldValue) => {
                                if (newValue === oldValue) {
                                    return;
                                }
                                dispatcher.dispatchEventFor(expression, $scope.schema, $scope.datamap, newValue);
                            });
                        });
                    }

                    init();

                }]
        }
    }]);

})(softwrench, angular, _);
