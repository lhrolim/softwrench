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
                allDisplayables: "="
            },

            link: function (scope, element, attrs) {
                scope.name = "crud_input_fields";
            },

            controller: ["$scope", "$rootScope", "offlineAssociationService", "crudContextService", "fieldService", "expressionService", "dispatcherService", "$timeout", "$log", "wizardService", "swdbDAO","searchIndexService","offlineSchemaService",
                function ($scope, $rootScope, offlineAssociationService, crudContextService, fieldService, expressionService, dispatcherService, $timeout, $log, wizardService, dao,searchIndexService, offlineSchemaService) {

                    $scope.associationSearch = function (query, componentId, pageNumber, useWhereClause, currentValue) {
                        return offlineAssociationService.filterPromise($scope.schema, $scope.datamap, componentId, query, null, pageNumber, useWhereClause, currentValue);
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

                    $scope.isReadOnly = function (field) {
                        return fieldService.isFieldReadOnly($scope.datamap, $scope.schema, field, $scope) || wizardService.isReadOnly(field, $scope.allDisplayables);
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

                    $scope.hasUseWhereClause = function (field) {
                        const params = field.rendererParameters;
                        if (!params) {
                            return false;
                        }
                        return "true" === params["hasUseWhereClause"];
                    }

                    //#region assosiations and options label init

                    /**
                     * $broadcast's "sw:association:resolved" event with the entity.AssociationData for setting initial labels in the $viewValues.
                     */
                    function triggerAssociationsInitialLabels() {
                        //TODO: move to offlineassociationService

                        const log = $log.get("crud_input_fields#triggerAssociationsInitialLabels", ["association", "detail"]);

                        // association fields that have a value set in the datamap
                        const associationFields = fieldService
                                                    .getDisplayablesOfTypes($scope.allDisplayables, ["ApplicationAssociationDefinition"])
                                                    .filter(field => $scope.datamap.hasOwnProperty(field.attribute) && !!$scope.datamap[field.attribute]);

                        if (associationFields.length <= 0) {
                            log.debug("no associationfields with value set in the current datamap");
                            return;
                        }

                        if (log.isLevelEnabled("debug")) log.debug(`fetching values for fields ${associationFields.map(f => f.attribute)}`);

                        const whereClauses = associationFields.map(f => {
                            const associationKey = f.associationKey;
                            const associationName = associationKey.endsWith("_")
                                ? associationKey.substring(0, associationKey.length - 1)
                                : associationKey;
                            const associationEntityName = f.entityAssociation.to;
                            const associationValue = $scope.datamap[f.attribute];

                            // local transient name cache to be used further down the promise chain
                            f["#associationLocalCache"] = { associationName, associationEntityName };

                            var listSchema = offlineSchemaService.locateSchemaByStereotype(associationName, "list");
                            var nameToUse = associationName;
                            if (!listSchema) {
                                //falling back to entityName instead, due to multiple possibilities on the sync process, depending on how the association is declared (qualifier or not)
                                listSchema = offlineSchemaService.locateSchemaByStereotype(associationEntityName, "list");
                                nameToUse = associationEntityName;
                            }

                            const idx = !!listSchema? searchIndexService.getIndexColumn(associationName, listSchema, f.valueField) : null;

                            if (!!idx) {
                                log.debug("applying index query");
                                return `( application = '${nameToUse}' and ${idx} = '${associationValue}')`;
                            }

                            log.warn(`applying non-indexed query consider adjusting your metadata to include the proper index for ${associationName}: ${f.valueField}`);
                            // fetching by application and by value
                            return `(application in('${associationName}','${associationEntityName}') and datamap like '%"${f.valueField}":"${associationValue}"%')`;
                        });

                        const query = whereClauses.join("or");

                        log.debug(`fetching labels with query '${query}'`);

                        dao.findByQuery("AssociationData", query).then(results => {
                            const values = results.map(association => {
                                // field that corresponds to the AssociationData fetched: same application and same value on the datamap 
                                const correspondingField = associationFields.find(field =>
                                    (association.application === field["#associationLocalCache"].associationName || association.application === field["#associationLocalCache"].associationEntityName)
                                    && association.datamap[field.valueField] === $scope.datamap[field.attribute]
                                );                                

                                return { associationKey: correspondingField.associationKey, item: association };
                            });

                            associationFields.forEach(f => delete f["#associationLocalCache"]);

                            // indexing by associationKey to facilitate lookup
                            const indexed = _.indexBy(values, "associationKey");

                            log.debug(`resolved items for labels: ${indexed}`);

                            $scope.$broadcast("sw:association:resolved", indexed);
                        });
                    }

                    //#endregion

                    //#region afterchangeevent dispatcher
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
                            this.expressions = this.eventDescriptors.map(e => `datamap.${e.name}`);
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
                            const ignoreWatchIdx = newValue === null || newValue === undefined || !angular.isFunction(newValue.indexOf)
                                ? -1
                                : newValue.indexOf("$ignorewatch");
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

                    function watchFields() {
                        // watching for changes to trigger afterchange event handlers
                        const watchableFields = $scope.allDisplayables.filter(f => f.events.hasOwnProperty("afterchange") && !!f.events["afterchange"]);
                        if (!watchableFields || watchableFields.length <= 0) return;

                        const logger = $log.get("crud_input_fields", ["datamap", "event", "association"]);
                        const dispatcher = new ChangeEventDispatcher(watchableFields, dispatcherService, $timeout, logger);

                        logger.debug(`watching ${dispatcher.expressions}`);

                        // flag that decides if change events should be dispatched
                        $rootScope.areChangeEventsEnabled = true;

                        dispatcher.expressions.forEach(expression => {
                            $scope.$watch(expression, (newValue, oldValue) => {
                                if (newValue === oldValue) {
                                    return;
                                }

                                if ($rootScope.areChangeEventsEnabled) {
                                    dispatcher.dispatchEventFor(expression, $scope.schema, $scope.datamap, newValue);
                                }
                            });
                        });
                    }
                    //#endregion

                    function init() {
                        $log.get("crud_input_fieldsl#init").debug("crud_input_fields init");
                        triggerAssociationsInitialLabels();
                        watchFields();
                    }

                    $scope.$on("sw_cruddetailrefreshed", () => {
                        $scope.datamap = crudContextService.currentDetailItemDataMap();
                        triggerAssociationsInitialLabels();
                    });

                    init();

                }]
        }
    }]);

})(softwrench, angular, _);
