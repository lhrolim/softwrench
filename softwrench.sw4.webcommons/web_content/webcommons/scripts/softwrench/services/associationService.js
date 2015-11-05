var app = angular.module('sw_layout');

app.factory('associationService', function (dispatcherService, $http, $timeout, $log, $rootScope, submitService, fieldService, contextService, searchService) {

    var doUpdateExtraFields = function (associationFieldMetadata, underlyingValue, datamap) {
        var log = $log.getInstance('sw4.associationservice#doUpdateExtraFields');
        var key = associationFieldMetadata.associationKey;
        if (datamap.fields != undefined) {
            datamap = datamap.fields;
        }

        datamap[key] = {};
        datamap.extrafields = datamap.extrafields || {};
        if (associationFieldMetadata.extraProjectionFields == null) {
            return;
        }
        for (var i = 0; i < associationFieldMetadata.extraProjectionFields.length; i++) {
            var extrafield = associationFieldMetadata.extraProjectionFields[i];
            var valueToSet = underlyingValue == null ? null : underlyingValue.extrafields[extrafield];
            log.debug('updating extra field {0}.{1} | value={2}'.format(key, extrafield, valueToSet));
            if (extrafield.startsWith(key)) {
                //if the extrafield has the association we dont need to append anything (i.e: solution_x_y and relationship = solution_)
                //but we have to take care of a_resolution_.ldtext and reslationship=solution_ that could lead to false positive, thats why startsWith
                datamap[extrafield] = valueToSet;
            } else {
                var appendDot = extrafield.indexOf('.') === -1;
                var fullKey = key;
                if (appendDot) {
                    fullKey += ".";
                }
                fullKey += extrafield;
                FillRelationship(datamap, fullKey, valueToSet);
                datamap[GetRelationshipName(fullKey)] = valueToSet;
                datamap[key][extrafield] = valueToSet;

                FillRelationship(datamap.extrafields, fullKey, valueToSet);
                datamap.extrafields[GetRelationshipName(fullKey)] = valueToSet;
                datamap.extrafields[key][extrafield] = valueToSet;

            }
        };
    }




    var doGetFullObject = function (associationFieldMetadata, associationOptions, selectedValue) {
        if (selectedValue == null) {
            return null;
        } else if (Array.isArray(selectedValue)) {
            var ObjectArray = [];

            // Extract each item into an array object
            for (var i = 0; i < selectedValue.length; i++) {
                var Object = doGetFullObject(associationFieldMetadata, associationOptions, selectedValue[i]);
                ObjectArray = ObjectArray.concat(Object);
            }

            // Return results for multi-value selection
            return ObjectArray;
        }

        // we need to locate the value from the list of association options
        // we only have the "value" on the datamap 
        var key = associationFieldMetadata.associationKey;

        var listToSearch = associationOptions[key];

        if (listToSearch == null) {
            //if the list is lazy (ex: lookups, there´s nothing we can do, except for static option field )
            if (associationFieldMetadata.options != undefined) {
                //this means this is an option field with static options
                var resultArr = $.grep(associationFieldMetadata.options, function (option) {
                    return selectedValue.equalIc(option.value);
                });
                return resultArr == null ? null : resultArr[0];
            }

            return null;
        }
        for (var i = 0; i < listToSearch.length; i++) {
            var objectWithProjectionFields = listToSearch[i];
            if ((typeof selectedValue === 'string') && selectedValue.equalIc(objectWithProjectionFields.value)) {
                //recovers the whole object in which the value field is equal to the datamap
                return objectWithProjectionFields;
            }
        }
        return null;
    };

    return {

        getLabelText:function(item, hideDescription) {
            if (hideDescription) {
                return item.value;
            }
            return "(" + item.value + ")" + " - " + item.label;
        },

        getFullObject: function (associationFieldMetadata, datamap, associationOptions) {
            //we need to locate the value from the list of association options
            // we only have the "value" on the datamap 
            var target = associationFieldMetadata.target;
            var selectedValue = datamap[target];

            if (selectedValue == null) {
                return null;
            }
            var resultValue = doGetFullObject(associationFieldMetadata, associationOptions, selectedValue);
            if (resultValue == null) {
                $log.getInstance('associationService#getFullObject').warn('value not found in association options for {0} '.format(associationFieldMetadata.associationKey));
            }
            return resultValue;
        },

        ///this method is used to update associations which depends upon the projection result of a first association.
        ///it only makes sense for associations which have extraprojections
        updateUnderlyingAssociationObject: function (associationFieldMetadata, underlyingValue, scope) {

            //if association options have no fields, we need to define it as an empty array. 
            scope.associationOptions = scope.associationOptions || [];
                

            var key = associationFieldMetadata.associationKey;
            var fullObject = this.getFullObject(associationFieldMetadata, scope.datamap, scope.associationOptions);
            if (underlyingValue == null) {
                //we need to locate the value from the list of association options
                // we only have the "value" on the datamap
                underlyingValue = fullObject;
            } else if (fullObject == null) {
                //now, the object was not present in the array, let´s update it. For instance, we have a lookup with a single initial option, and changed it to another.
                //the array should contain 2 elements in the end. If the object was already there, no need to do anything
                //TODO: it would better to update it, but need some code to locate it by key first, update getFullObject to return the index also
                if (scope.associationOptions[key] == undefined) {
                    scope.associationOptions[key] = [];
                }
                scope.associationOptions[key].push(underlyingValue);
                $log.getInstance('associationService#updateUnderlyingAssociationObject').debug('updating association array {0} pushing new item'.format(key));
            }
            if (underlyingValue == null && scope.associationOptions[key] == null) {
                //the value remains null, but this is because the list of options is lazy loaded, nothing to do
                return;
            }

            if (associationFieldMetadata.extraProjectionFields.length > 0) {
                doUpdateExtraFields(associationFieldMetadata, underlyingValue, scope.datamap);
            }
        },

        ///
        ///
        ///dispatchedbyuser: the method could be called after a user action (changing a field), or internally after a value has been set programatically 
        postAssociationHook: function (associationMetadata, scope, triggerparams) {
            if (associationMetadata.events == undefined) {
                return;
            }
            var afterChangeEvent = associationMetadata.events['afterchange'];
            if (afterChangeEvent == undefined) {
                return;
            }
            var fn = dispatcherService.loadService(afterChangeEvent.service, afterChangeEvent.method);
            if (fn == undefined) {
                //this should not happen, it indicates a metadata misconfiguration
                return;
            }
            var fields = scope.datamap;
            if (scope.datamap.fields != undefined) {
                fields = scope.datamap.fields;
            }

            var afterchangeEvent = {
                fields: fields,
                scope: scope,
                parentdata:scope.parentdata,
                triggerparams: instantiateIfUndefined(triggerparams)
            };
            $log.getInstance('sw4.associationservice#postAssociationHook').debug('invoking post hook service {0} method {1} from association {2}|{3}'
                .format(afterChangeEvent.service, afterChangeEvent.method, associationMetadata.target,associationMetadata.associationKey));
            fn(afterchangeEvent);
        },

        markAssociationProcessComplete: function () {
            $rootScope.$broadcast("sw_optionsretrievedFromServerEnded");
        },

        updateAssociationOptionsRetrievedFromServer: function (scope, serverOptions, datamap) {
            /// <summary>
            ///  Callback of the updateAssociations call, in which the values returned from the server would update the scope variables, 
            /// to be shown on screen
            /// It would be called at the first time the detail screen is opened as well
            /// </summary>
            /// <param name="scope"></param>
            /// <param name="serverOptions"></param>
            /// <param name="datamap">could be null for list schemas.</param>
            var log = $log.getInstance('sw4.associationservice#updateAssociationOptionsRetrievedFromServer');
            scope.associationOptions = instantiateIfUndefined(scope.associationOptions);
            scope.blockedassociations = instantiateIfUndefined(scope.blockedassociations);
            scope.associationSchemas = instantiateIfUndefined(scope.associationSchemas);
            scope.disabledassociations = instantiateIfUndefined(scope.disabledassociations);
            for (var dependantFieldName in serverOptions) {

                //this iterates for list of fields which were dependant of a first one. 
                var array = serverOptions[dependantFieldName] || {};
                var zeroEntriesFound = (array.associationData == null || array.associationData.length === 0);
                var oneEntryFound = !zeroEntriesFound && array.associationData.length === 1;

                log.debug('updating association from server {0} length {1}'.format(dependantFieldName, array.associationData == null ? 0 : array.associationData.length));

                scope.associationOptions[dependantFieldName] = array.associationData;

                scope.blockedassociations[dependantFieldName] = zeroEntriesFound;
                scope.associationSchemas[dependantFieldName] = array.associationSchemaDefinition;

                var associationFieldMetadatas = fieldService.getDisplayablesByAssociationKey(scope.schema, dependantFieldName);
                if (associationFieldMetadatas == null) {
                    //should never happen, playing safe here
                    continue;
                }
                var fn = this;

                $.each(associationFieldMetadatas, function (index, value) {
                    if (zeroEntriesFound && (value.rendererType == "lookup" || value.rendererType == "autocompleteserver")) {
                        //this should never be blocked, but the server might still be returning 0 entries due to a reverse association mapping
                        scope.blockedassociations[dependantFieldName] = false;
                    }

                    if (value.reverse && !zeroEntriesFound) {
                        datamap[value.target] = scope.associationOptions[dependantFieldName][0].value;
                    }

                    if (value.target == null || datamap == null) {
                        //the datamap could be null if this method is called from a list schema
                        return;
                    }

                    var datamapTargetValue = datamap[value.target] == null ? null : datamap[value.target].toString();

                    if (isIe9()) {
                        //clear datamap for the association updated, restoring it later -->This is needed due to a IE9 issue
                        datamap[value.target] = null;
                    }

                    

                    if (array.associationData == null) {
                        //if no options returned from the server, nothing else to do
                        return;
                    }

                    if (oneEntryFound && fieldService.isFieldRequired(value,datamap)) {
                        //if there´s just one option available, and the field is required, let´s select it.
                        var entryReturned = array.associationData[0].value;
                        if (datamapTargetValue !== entryReturned) {
                            datamap[value.target] = entryReturned;
                        }
                    }

                    if (datamapTargetValue == null) {
                        //if there was no previous selected value, we don´t need to restore it
                        return;
                    }


                    //restoring previous selected value after the ng-options has changed
                    for (var j = 0; j < array.associationData.length; j++) {
                        var associationOption = array.associationData[j];
                        if (associationOption.value.toUpperCase() == datamapTargetValue.toUpperCase()) {
                            var fullObject = associationOption;

                            if (isIe9()) {
                                $timeout(function () {
                                    log.debug('restoring {0} to previous value {1}. '.format(value.target, datamapTargetValue));
                                    //if still present on the new list, setting back the value which was 
                                    //previous selected, but after angular has updadted the list properly
                                    datamap[value.target] = String(associationOption.value);
                                    doUpdateExtraFields(value, fullObject, datamap);
                                    if (fn.postAssociationHook) {
                                        fn.postAssociationHook(value, scope, { phase: 'initial', dispatchedbytheuser: false });
                                    }
                                    try {
                                        $rootScope.$digest();
                                    } catch (e) {
                                        //digest already in progress
                                    }
                                }, 0, false);
                            } else {
                                //let´s remove the complexity of non ie9 solutions, calling the code outside of the timeout since it´s not needed
                                datamap[value.target] = String(datamapTargetValue);
                                doUpdateExtraFields(value, fullObject, datamap);
                                if (fn.postAssociationHook) {
                                    fn.postAssociationHook(value, scope, { phase: 'initial', dispatchedbytheuser: false });
                                }
                            }

                            //no need to iterate over the rest of the associationData array
                            break;
                        }
                    }
                });
            }
            //let´s force a scope digest if not yet in place
            if (!isIe9()) {
                try {
                    scope.digest();
                } catch (e) {
                    //digest already in progress
                }
            }
        },

        getEagerAssociations: function (scope, options) {
            var associations = fieldService.getDisplayablesOfTypes(scope.schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            if (associations == undefined || associations.length == 0) {
                //no need to hit server in that case
                return;
            }

            scope.associationOptions = instantiateIfUndefined(scope.associationOptions);
            scope.blockedassociations = instantiateIfUndefined(scope.blockedassociations);
            scope.associationSchemas = instantiateIfUndefined(scope.associationSchemas);
            return this.updateAssociations({ attribute: "#eagerassociations" }, scope, options);
        },

        // This method is called whenever an association value changes, in order to update all the dependant associations 
        // of this very first association.
        // This would only affect the eager associations, not the lookups, because they would be fetched at the time the user opens it.
        // Ex: An asset could be filtered by the location, so if a user changes the location field, the asset should be refetched.
        updateAssociations: function (association, scope, options) {
            options = options || {};

            var log = $log.getInstance('sw4.associationservice#updateAssociations');

            var triggerFieldName = association.attribute;
            var schema = scope.schema;
            if (!schema) {
                schema = scope.compositionlistschema;
            }
            if (triggerFieldName !== "#eagerassociations" && $.inArray(triggerFieldName, schema.fieldWhichHaveDeps) === -1) {
                //no other asociation depends upon this first association, return here.
                //false is to indicate that no value has been updated
                log.debug('No associated dependants for {0}'.format(triggerFieldName));
                if (association.rendererType !== 'multiselectautocompleteclient') {
                    //if multiple selection, there´s no sense to move focus
                    $timeout(function () {
                        //this timeout is required because there´s already a digest going on, so this emit would throw an exception
                        //had to put a bigger timeout so that the watches doesn´t get evaluated.
                        //TODO: investigate it
                        scope.$emit("sw_movefocus", scope.datamap, scope.schema, triggerFieldName);
                    }, 300, false);
                }
                
                
                return false;
            }
            var updateAssociationOptionsRetrievedFromServer = this.updateAssociationOptionsRetrievedFromServer;
            var postAssociationHook = this.postAssociationHook;

            var applicationName = schema.applicationName;
            var fields = scope.datamap;

            if (scope.datamap.fields) {
                fields = scope.datamap.fields;
            }

            if (options.datamap) {
                fields = options.datamap;
            }

            var parameters = {
                application: applicationName,
                key: {
                    schemaId: schema.schemaId,
                    mode: schema.mode,
                    platform: platform(),
                },
                triggerFieldName: triggerFieldName,
                id: fields[schema.idFieldName]
            };
            var fieldsTosubmit = submitService.removeExtraFields(fields, true, scope.schema);
            var urlToUse = url("/api/generic/ExtendedData/UpdateAssociation?" + $.param(parameters));
            var jsonString = angular.toJson(fieldsTosubmit);

            log.info('going to server for dependent associations of {0}'.format(triggerFieldName));
            log.debug('Content: \n {0}'.format(jsonString));
            var config = {};
            if (options.avoidspin) {
                config.avoidspin = true;
            }
            return $http.post(urlToUse, jsonString,config).success(function (data) {
                var options = data.resultObject;
                log.info('associations returned {0}'.format($.keys(options)));
                updateAssociationOptionsRetrievedFromServer(scope, options, fields);
                if (association.attribute != "#eagerassociations") {
                    //this means we´re getting the eager associations, see method above
                    postAssociationHook(association, scope, { dispatchedbytheuser: true, phase: 'configured' });
                } else {
                    $timeout(function () {
                        //this needs to be marked for the next digest loop so that the crud_input_fields has the possibility to distinguish between the initial and configured phases, and so the listeners
                        contextService.insertIntoContext("associationsresolved", true, true);
                    }, 100, false);
                    scope.$broadcast("sw_associationsupdated", scope.associationOptions);

                    //TODO: Is this needed, I couldn't find where it's used, I was not able to test if needed
                    //move input focus to the next field
                    if (triggerFieldName !== "#eagerassociations") {
                        $timeout(function () {
                            //this timeout is required because there´s already a digest going on, so this emit would throw an exception
                            scope.$emit("sw_movefocus", scope.datamap, scope.schema, triggerFieldName);
                        }, 0, false);
                    } else {
                        scope.$emit("sw_setFocusToInitial", scope.schema, fields);
                    }
                }
            }).error(
            function data() {
            });
        },

        //This takes the lookupObj, pageNumber, and searchObj (dictionary of attribute (key) 
        //to its value that will filter the lookup), build a searchDTO, and return the post call to the
        //UpdateAssociations function in the ExtendedData controller.
        getAssociationOptions: function (schema,datamap, lookupObj, pageNumber, searchObj) {
            var fields = datamap;
            if (lookupObj.searchDatamap) {
                fields = lookupObj.searchDatamap;
            }

            var parameters = {};
            parameters.application = schema.applicationName;
            parameters.key = {};
            parameters.key.schemaId = schema.schemaId;
            parameters.key.mode = schema.mode;
            parameters.key.platform = platform();
            parameters.associationFieldName = lookupObj.fieldMetadata.associationKey;

            var lookupApplication = lookupObj.application;
            var lookupSchemaId = lookupObj.schemaId;
            if (lookupApplication != null && lookupSchemaId != null) {
                parameters.associationApplication = lookupApplication;
                parameters.associationKey = {};
                parameters.associationKey.schemaId = lookupSchemaId;
                parameters.associationKey.platform = platform();
            }

            var totalCount = 0;
            var pageSize = 30;
            if (lookupObj.modalPaginationData != null) {
                totalCount = lookupObj.modalPaginationData.totalCount;
                pageSize = lookupObj.modalPaginationData.pageSize;
            }
            if (pageNumber == null) {
                pageNumber = 1;
            }

            //If a schema is provided for the lookup, then searchvalues/operators can be populated to
            //filter the search
            if (lookupObj.schemaId != null) {
                var defaultLookupSearchOperator = searchService.getSearchOperationById("CONTAINS");
                var searchValues = searchObj;
                var searchOperators = {};
                for (var field in searchValues) {
                    searchOperators[field] = defaultLookupSearchOperator;
                }
                if (searchValues == null) {
                    searchValues = {};
                }
                parameters.hasClientSearch = true;
                parameters.SearchDTO = searchService.buildSearchDTO(searchValues, {}, searchOperators);
                parameters.SearchDTO.pageNumber = pageNumber;
                parameters.SearchDTO.totalCount = totalCount;
                parameters.SearchDTO.pageSize = pageSize;
            } else {
                parameters.valueSearchString = lookupObj.code == null ? "" : lookupObj.code;
                parameters.labelSearchString = lookupObj.description == null ? "" : lookupObj.description;
                parameters.hasClientSearch = true;
                parameters.SearchDTO = {
                    pageNumber: pageNumber,
                    totalCount: totalCount,
                    pageSize: pageSize
                };
            }

            var urlToUse = url("/api/generic/ExtendedData/UpdateAssociation?" + $.param(parameters));
            var jsonString = angular.toJson(fields);
            return $http.post(urlToUse, jsonString);
        },

        //Updates dependent association values for all association rendererTypes.
        //This includes the associationOptions, associationDescriptions, etc.
        updateDependentAssociationValues: function (scope, datamap, lookupObj, postFetchHook, searchObj) {
            var getAssociationOptions = this.getAssociationOptions;
            var updateAssociationOptionsRetrievedFromServer = this.updateAssociationOptionsRetrievedFromServer;
            lookupObj.searchDatamap = datamap;
            getAssociationOptions(scope.schema,scope.datamap, lookupObj, null, searchObj).success(function (data) {
                var result = data.resultObject;

                if (postFetchHook != null) {
                    var continueFlag = postFetchHook(result, lookupObj, scope, datamap);
                    if (!continueFlag) {
                        return;
                    }
                }

                var associationResult = result[lookupObj.fieldMetadata.associationKey];
                lookupObj.options = associationResult.associationData;
                lookupObj.schema = associationResult.associationSchemaDefinition;
                datamap[lookupObj.fieldMetadata.target] = lookupObj.options[0].value;

                if (!scope.lookupAssociationsCode) {
                    scope["lookupAssociationsCode"] = {};
                }
                if (!scope.lookupAssociationsDescription) {
                    scope["lookupAssociationsDescription"] = {};
                }
                scope.lookupAssociationsCode[lookupObj.fieldMetadata.attribute] = lookupObj.options[0].value;
                scope.lookupAssociationsDescription[lookupObj.fieldMetadata.attribute] = lookupObj.options[0].label;
                updateAssociationOptionsRetrievedFromServer(scope, result, datamap);
            }).error(function (data) {
            });
        },

        onAssociationChange: function (fieldMetadata, updateUnderlying, event) {

            if (fieldMetadata.events == undefined) {
                event.continue();
                return;
            }
            var beforeChangeEvent = fieldMetadata.events['beforechange'];
            if (beforeChangeEvent == undefined) {
                event.continue();
            } else {
                var fn = dispatcherService.loadService(beforeChangeEvent.service, beforeChangeEvent.method);
                if (fn == undefined) {
                    //this should not happen, it indicates a metadata misconfiguration
                    event.continue();
                    return;
                }
                var result = fn(event);
                //sometimes the event might be syncrhonous, returning either true of false
                if (result != undefined && result == false) {
                    event.interrupt();
                }
                event.continue();
            }
        },

        insertAssocationLabelsIfNeeded: function (schema, datamap, associationoptions) {
            if (schema.properties['addassociationlabels'] != "true") {
                return;
            }
            var associations = fieldService.getDisplayablesOfTypes(schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            var fn = this;
            $.each(associations, function (key, value) {
                var targetName = value.target;
                var labelName = "#" + targetName + "_label";
                var realValue = fn.getFullObject(value, datamap, associationoptions);
                if (realValue != null && Array.isArray(realValue)) {
                    datamap[labelName] = "";
                    // store result into a string with newline delimiter
                    for (var i = 0; i < realValue.length; i++) {
                        datamap[labelName] += "\\n" + realValue[i].label;
                    }
                }
                else if (realValue != null) {
                    datamap[labelName] = realValue.label;
                }
            });
        },


        lookupAssociation: function (displayables, associationTarget) {
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                if (displayable.target != undefined && displayable.target == associationTarget) {
                    return displayable;
                }
            }
            return null;
        }
    };

});


