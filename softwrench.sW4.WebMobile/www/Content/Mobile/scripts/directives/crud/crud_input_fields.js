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

        controller: ["$scope", "offlineAssociationService", "fieldService", "expressionService", "dispatcherService", "$timeout", "$log",  
            function ($scope, offlineAssociationService, fieldService, expressionService, dispatcherService, $timeout, $log) {
            
            $scope.associationSearch = function (query, componentId) {
                return offlineAssociationService.filterPromise($scope.schema, $scope.datamap, componentId, query);
            };

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
                        position: i,
                        name: f.attribute,
                        event: f.events["afterchange"]
                    }));
                    this.expressions = _.sortBy(this.eventDescriptors, "position").map(e => `datamap.${e.name}`);
                }
                getEvent(position) {
                    return this.eventDescriptors.find(e => e.position === position);
                }
                dispatchEventFor(position, schema, datamap) {
                    const descriptor = this.getEvent(position);
                    const service = descriptor.event.service;
                    const method = descriptor.event.method;
                    const handler = this.dispatcher.loadService(service, method);
                    const params = { schema, datamap };

                    this.logger.debug(`dispatching 'afterchange' event for field '${descriptor.name}'`);
                    this.timeout(() => handler(params))
                        .catch(e => this.logger.error(`Failed to execute ${service}.${method} on 'afterchange' of field '${descriptor.name}'`, e));
                }
            }

            function init() {
                // watching for changes to trigger afterchange event handlers
                const watchableFields = $scope.displayables.filter(f => f.events.hasOwnProperty("afterchange") && !!f.events["afterchange"]);
                if (!watchableFields || watchableFields.length <= 0) return;

                const logger = $log.get("crud_input_fields", ["datamap", "event"]);
                const dispatcher = new ChangeEventDispatcher(watchableFields, dispatcherService, $timeout, logger);

                logger.debug(`watching ${dispatcher.expressions}`);
                $scope.$watchGroup(dispatcher.expressions, (newValues, oldValues) => {
                    angular.forEach(newValues, (val, index) => {
                        if (val === oldValues[index]) return;
                        dispatcher.dispatchEventFor(index, $scope.schema, $scope.datamap);
                    });
                });
            }

            init();

        }]
    }
}]);

})(softwrench, angular, _);
