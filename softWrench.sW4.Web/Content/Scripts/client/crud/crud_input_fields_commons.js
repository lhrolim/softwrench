(function (angular) {
    'use strict';

    angular.module('sw_layout').factory('crud_inputcommons', factory);

    factory.$inject = ['$log', 'associationService', 'contextService', 'cmpfacade', 'fieldService', "$timeout", 'expressionService', 'dispatcherService', '$parse'];

    function factory($log, associationService, contextService, cmpfacade, fieldService, $timeout, expressionService, dispatcherService, $parse) {

        const api = {
            configureAssociationChangeEvents,
            initField
        };
        return api;


        function initField($scope, fieldMetadata, datamappropname = "datamap", idx) {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="$scope"></param>
            /// <param name="fieldMetadata"></param>
            /// <param name="datamappropname"></param>
            /// <param name="idx">if defined, it will refer to the line on a composition this field is inside</param>
            /// <returns type=""></returns>

            if (fieldMetadata.evalExpression != null) {
                const variables = expressionService.getVariablesForWatch(fieldMetadata.evalExpression);
                $scope.$watchCollection(variables, function (newVal, oldVal) {
                    if (newVal != oldVal) {
                        $scope[datamappropname][fieldMetadata.attribute] = expressionService.evaluate(fieldMetadata.evalExpression, $scope[datamappropname], $scope);
                    }
                });
            }
            var maxExpression = fieldMetadata.rendererParameters['max'];
            if (maxExpression != null) {
                const variablesToWatch = expressionService.getVariablesForWatch(maxExpression, datamappropname + ".");
                if (variablesToWatch == null) {
                    return null;
                }
                $scope.$watchCollection(variablesToWatch, function (newVal, oldVal) {
                    if (newVal != oldVal) {
                        const maxValue = parseInt(expressionService.evaluate(maxExpression, $parse(datamappropname)($scope)));
                        const selector = (idx != undefined) ? '[data-field="{0}{1}"]'.format(fieldMetadata.attribute, idx) : '[data-field="{0}"]'.format(fieldMetadata.attribute);
                        $(selector).spinner({
                            max: maxValue
                        });
                    }
                });
            }

            const isRequired= fieldService.isFieldRequired(fieldMetadata, $scope[datamappropname]);


            return null;
        };

        function configureAssociationChangeEvents($scope, datamappropertiesName, displayables, datamapId) {
            const associations = fieldService.getDisplayablesOfTypes(displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            var createdWatches = [];

            $.each(associations, function (key, association) {
                var shouldDoWatch = true;
                var isMultiValued = association.multiValued;
                createdWatches.push($scope.$watch('{0}["{1}"]'.format(datamappropertiesName, association.attribute), function (newValue, oldValue) {
                    if (oldValue == newValue || !shouldDoWatch) {
                        return;
                    }

                    var datamap = $parse(datamappropertiesName)($scope);
                    // If the datamap is undefined then we do not need to run any events for the items that are now gone
                    // Ex: ng-repeat over an array, an item is removed from the array, do not need to run events for the line that is now gone.
                    if (datamap == undefined) {
                        return;
                    }
                    if (!expressionService.evaluate(association.showExpression, datamap)) {

                        if (!association.rendererParameters || "true" !== association.rendererParameters["donotignoreeventsifhidden"]) {
                            //if the association is hidden, there´s no sense in executing any hook methods of it
                            $log.get("crud_inputcommons#configureAssociationChangeEvents").debug("ignoring hidden association {0}".format(association.associationKey));
                            return;
                        }
                    }


                    if (newValue != null && angular.isString(newValue)) {
                        //this is a hacky thing when we want to change a value of a field without triggering the watch
                        const ignoreWatchIdx = newValue.indexOf("$ignorewatch");
                        if (ignoreWatchIdx >= 0) {
                            shouldDoWatch = false;
                            $parse(datamappropertiesName)($scope)[association.attribute] = newValue.substring(0, ignoreWatchIdx);
                            try {
                                $scope.$digest();
                                shouldDoWatch = true;
                            } catch (e) {
                                //nothing to do, just checking if digest was already in place or not
                                $timeout(function () {
                                    shouldDoWatch = true;
                                }, 0, false);
                            }
                            return;
                        }
                    }
                    var fields = $parse(datamappropertiesName)($scope);
                    const eventToDispatch = {
                        oldValue: oldValue,
                        newValue: newValue,
                        fields: fields,
                        parentdata: $scope.parentdata,
                        displayables: displayables,
                        scope: $scope,
                        'continue': function () {
                            if ($scope.compositionlistschema) {
                                //workaround for compositions
                                $scope.datamap = datamap;
                                $scope.schema = $scope.compositionlistschema;
                            }
                            if (isMultiValued && association.rendererType !== 'lookup') {
                                associationService.updateUnderlyingAssociationObject(association, null, $scope);
                            }
                            if (association.type === "OptionField") {
                                associationService.updateOptionFieldExtraFields(association, $scope);
                            }
                            const resolved = contextService.fetchFromContext("associationsresolved", false, true);
                            const phase = resolved ? 'configured' : 'initial';
                            const dispatchedbytheuser = resolved ? true : false;
                            const hook = associationService.postAssociationHook(association, $scope, { phase: phase, dispatchedbytheuser: dispatchedbytheuser, fields: fields });
                            hook.then(function (hookResult) {
                                associationService.updateAssociations(association, $scope);
                                try {
                                    $scope.$digest();
                                } catch (ex) {
                                    //nothing to do, just checking if digest was already in place or not
                                }
                            });
                        },
                        interrupt: function () {
                            const originalOldValue = oldValue;
                            if (oldValue != null && !oldValue.endsWith("$ignorewatch")) {
                                oldValue = oldValue + "$ignorewatch";
                            }

                            $parse(datamappropertiesName)($scope)[association.attribute] = oldValue;

                            //to avoid infinite recursion here.
                            shouldDoWatch = false;


                            cmpfacade.digestAndrefresh(association, $scope, originalOldValue);
                            //turn it on for future changes
                            shouldDoWatch = true;

                        }
                    };
                    const result = associationService.onAssociationChange(association, isMultiValued, eventToDispatch);
                    if (!result) {
                        return;
                    }
                    if (newValue == undefined) {
                        //we will distinguish between null or undefined to know when the call was made in the sense of really setting it to null, 
                        //from the scenario where the list was changed first and the value was simply undefined due to the workflow
                        newValue = null;
                    }
                    cmpfacade.digestAndrefresh(association, $scope, newValue, datamapId);
                }));

                $log.getInstance("associationService#configureAssociationChangeEvents", ["association"]).debug("initing watchers for {0} ".format(association.attribute));

                $scope.$on(JavascriptEventConstants.Association_EagerOptionUpdated, function (event, associationKey, options, contextData) {
                    $timeout(function () {
                        if ($scope.schema) {
                            // if it is a composition list and have datamapId - the datamapId id from field should be the same from the event
                            if (datamapId && $scope.datamap && $scope.datamap[$scope.schema.idFieldName] !== datamapId) {
                                return;
                            }
                            const displayables = fieldService.getDisplayablesByAssociationKey($scope.schema, associationKey);
                            for (let i = 0; i < displayables.length; i++) {
                                cmpfacade.updateEagerOptions($scope, displayables[i], options, contextData, datamapId);
                            }
                        }
                    }, 0, false);
                });

                $scope.$watch('blockedassociations[\"' + association.associationKey + '\"]', function (newValue, oldValue) {
                    cmpfacade.blockOrUnblockAssociations($scope, newValue, oldValue, association);
                });



            });

            return createdWatches;


        }
    }
})(angular);
