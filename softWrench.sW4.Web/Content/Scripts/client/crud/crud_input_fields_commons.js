﻿(function () {
    'use strict';

    angular.module('sw_layout').factory('crud_inputcommons', factory);

    factory.$inject = ['$log','associationService', 'contextService', 'cmpfacade', 'fieldService', "$timeout", 'expressionService', '$parse'];

    function factory($log,associationService, contextService, cmpfacade, fieldService, $timeout, expressionService, $parse) {

        var api = {
            configureAssociationChangeEvents: configureAssociationChangeEvents,
            initField:initField
        };

        return api;


        function initField($scope, fieldMetadata, datamappropname, idx) {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="$scope"></param>
            /// <param name="fieldMetadata"></param>
            /// <param name="datamappropname"></param>
            /// <param name="idx">if defined, it will refer to the line on a composition this field is inside</param>
            /// <returns type=""></returns>
            datamappropname = datamappropname || "datamap";

            if (fieldMetadata.evalExpression != null) {
                var variables = expressionService.getVariablesForWatch(fieldMetadata.evalExpression);
                $scope.$watchCollection(variables, function (newVal, oldVal) {
                    if (newVal != oldVal) {
                        $scope[datamappropname][fieldMetadata.attribute] = expressionService.evaluate(fieldMetadata.evalExpression, $scope[datamappropname], $scope);
                    }
                });
            }
            var maxExpression = fieldMetadata.rendererParameters['max'];
            if (maxExpression != null) {
                var variablesToWatch = expressionService.getVariablesForWatch(maxExpression, datamappropname+".");
                if (variablesToWatch == null) {
                    return null;
                }
                $scope.$watchCollection(variablesToWatch, function (newVal, oldVal) {
                    if (newVal != oldVal) {
                        var maxValue = parseInt(expressionService.evaluate(maxExpression, $parse(datamappropname)($scope)));
                        var selector = (idx != undefined) ? '[data-field="{0}{1}"]'.format(fieldMetadata.attribute, idx) : '[data-field="{0}"]'.format(fieldMetadata.attribute);
                        $(selector).spinner({
                            max: maxValue
                        });
                    }
                });
            }

            return null;
        };

        function configureAssociationChangeEvents($scope, datamappropertiesName,displayables) {


            var associations = fieldService.getDisplayablesOfTypes(displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            $.each(associations, function (key, association) {
                var shouldDoWatch = true;
                var isMultiValued = association.multiValued;
                $scope.$watch('{0}["{1}"]'.format(datamappropertiesName, association.attribute), function (newValue, oldValue) {
                    if (oldValue == newValue || !shouldDoWatch) {
                        return;
                    }

                    var datamap = $parse(datamappropertiesName)($scope);

                    if (!expressionService.evaluate(association.showExpression,datamap)) {
                        //if the association is hidden, there´s no sense in executing any hook methods of it
                        $log.get("crud_inputcommons#configureAssociationChangeEvents").debug("ignoring hidden association {0}".format(association.associationKey));
                        return;
                    }


                    if (newValue != null) {
                        //this is a hacky thing when we want to change a value of a field without triggering the watch
                        var ignoreWatchIdx = newValue.indexOf('$ignorewatch');
                        if (ignoreWatchIdx != -1) {
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
                    var eventToDispatch = {
                        oldValue: oldValue,
                        newValue: newValue,
                        fields: $parse(datamappropertiesName)($scope),
                        parentdata: $scope.parentdata,
                        displayables: displayables,
                        scope: $scope,
                        'continue': function () {
                            if (isMultiValued && association.rendererType != 'lookup') {
                                associationService.updateUnderlyingAssociationObject(association, null, $scope);
                            }
                            var result = associationService.updateAssociations(association, $scope);
                            if (result != undefined && result == false) {
                                var resolved = contextService.fetchFromContext("associationsresolved", false, true);
                                var phase = resolved ? 'configured' : 'initial';
                                var dispatchedbytheuser = resolved ? true : false;
                                if ($scope.compositionlistschema) {
                                    //workaround for compositions
                                    
                                    $scope.datamap = datamap;
                                    $scope.schema = $scope.compositionlistschema;
                                }
                                associationService.postAssociationHook(association, $scope, { phase: phase, dispatchedbytheuser: dispatchedbytheuser });
                            }
                            try {
                                $scope.$digest();
                            } catch (ex) {
                                //nothing to do, just checking if digest was already in place or not
                            }
                        },
                        interrupt: function () {
                            $parse(datamappropertiesName)($scope)[association.attribute] = oldValue;
                            //to avoid infinite recursion here.
                            shouldDoWatch = false;
                            cmpfacade.digestAndrefresh(association, $scope);
                            //turn it on for future changes
                            shouldDoWatch = true;
                        }
                    };
                    associationService.onAssociationChange(association, isMultiValued, eventToDispatch);
                    if (newValue == undefined) {
                        //we will distinguish between null or undefined to know when the call was made in the sense of really setting it to null, 
                        //from the scenario where the list was changed first and the value was simply undefined due to the workflow
                        newValue = null;
                    }
                    cmpfacade.digestAndrefresh(association, $scope, newValue);
                });
                $scope.$watchCollection('associationOptions.' + association.associationKey, function (newvalue, old) {
                    if (newvalue == old) {
                        return;
                    }
                    $timeout(
                    function () {
                        cmpfacade.digestAndrefresh(association, $scope);
                    }, 0, false);
                });
                $scope.$watch('blockedassociations.' + association.associationKey, function (newValue, oldValue) {
                    cmpfacade.blockOrUnblockAssociations($scope, newValue, oldValue, association);
                });
            });

        }
    }
})();