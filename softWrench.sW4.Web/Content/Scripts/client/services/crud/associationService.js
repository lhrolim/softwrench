(function (angular) {
    "use strict";


    function associationService(dispatcherService, $http, $q, $timeout, $log, $rootScope, restService, submitServiceCommons, fieldService, contextService, searchService, crudContextHolderService, schemaService, datamapSanitizeService, compositionService, eventService) {


        /**
           * Array to avoid multiple calls to the server upon 
           */
        var lazyAssociationsBeingResolved = [];

        var doUpdateExtraFields = function (associationFieldMetadata, underlyingValue, datamap) {
            const log = $log.getInstance('sw4.associationservice#doUpdateExtraFields');
            const key = associationFieldMetadata.associationKey;
            const isOptionField = associationFieldMetadata.type === "OptionField";

            if (!isOptionField) {
                datamap[key] = {};
            }
            datamap.extrafields = datamap.extrafields || {};
            if (associationFieldMetadata.extraProjectionFields == null) {
                return;
            }
            for (let i = 0; i < associationFieldMetadata.extraProjectionFields.length; i++) {
                const extrafield = associationFieldMetadata.extraProjectionFields[i];
                const valueToSet = (underlyingValue == null || !underlyingValue.extrafields) ? null : underlyingValue.extrafields[extrafield];

                if (isOptionField) {
                    log.debug(`updating extra field ${extrafield} from option field ${associationFieldMetadata.attribute} | value=${valueToSet}`);
                    const extraFieldKey = associationFieldMetadata.attribute + "." + extrafield;
                    datamap.extrafields[extraFieldKey] = valueToSet;
                    continue;
                }

                log.debug('updating extra field {0}.{1} | value={2}'.format(key, extrafield, valueToSet));
                if (extrafield.startsWith(key)) {
                    //if the extrafield has the association we dont need to append anything (i.e: solution_x_y and relationship = solution_)
                    //but we have to take care of a_resolution_.ldtext and reslationship=solution_ that could lead to false positive, thats why startsWith
                    datamap[extrafield] = valueToSet;
                } else {
                    const appendDot = extrafield.indexOf('.') === -1;
                    let fullKey = key;
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

        var updateExtraFields = function (associations, datamap, schema, contextData) {
            if (!associations || associations.length === 0) {
                return;
            }
            const log = $log.get("associationService#updateExtraFields");
            associations.forEach(function (association) {
                if (!association || association.extraProjectionFields == null || association.extraProjectionFields.length === 0) {
                    return;
                }
                const optionValue = getFullObject(association, datamap, schema, contextData);
                doUpdateExtraFields(association, optionValue, datamap);
            });

            log.info("Extra fields of associations updated during schema load.");
        }

        var doGetFullObject = function (associationFieldMetadata, selectedValue, datamap, schema, contextData) {


            if (selectedValue == null) {
                return null;
            } else if (Array.isArray(selectedValue)) {
                // Extract each item into an array object
                const objectArray = selectedValue
                    .map(function (value) {
                        return doGetFullObject(associationFieldMetadata, value, datamap, schema, contextData);
                    })
                    .flatten(); // Return results for multi-value selection
                return objectArray;
            }

            // we need to locate the value from the list of association options
            // we only have the "value" on the datamap 
            const key = associationFieldMetadata.associationKey;
            if (associationFieldMetadata.schema && associationFieldMetadata.schema.isLazyLoaded) {
                return crudContextHolderService.fetchLazyAssociationOption(key, selectedValue);
            }

            // special case of a composition list
            if (compositionService.isCompositionListItem(datamap)) {
                contextData = compositionService.buildCompositionListItemContext(contextData, datamap, schema);
            }
            if (!contextData && crudContextHolderService.isShowingModal()) {
                contextData = { schemaId: "#modal" };
            }
            const listToSearch = crudContextHolderService.fetchEagerAssociationOptions(key, contextData);
            if (listToSearch == null) {
                //if the list is lazy (ex: lookups, there´s nothing we can do, except for static option field )
                if (associationFieldMetadata.options != undefined) {
                    //this means this is an option field with static options
                    const resultArr = $.grep(associationFieldMetadata.options, function (option) {
                        return selectedValue.equalIc(option.value);
                    });
                    return resultArr == null ? null : resultArr[0];
                }

                return null;
            }
            for (let i = 0; i < listToSearch.length; i++) {
                const objectWithProjectionFields = listToSearch[i];
                if ((typeof selectedValue === 'string') && selectedValue.equalIc(objectWithProjectionFields.value)) {
                    //recovers the whole object in which the value field is equal to the datamap
                    return objectWithProjectionFields;
                }
            }
            return null;
        };

        function lookupSingleAssociationByValue(associationKey, associationValue) {

            lazyAssociationsBeingResolved.push(associationKey);
            const panelId = crudContextHolderService.isShowingModal() ? "#modal" : null;
            const datamap = crudContextHolderService.rootDataMap(panelId);
            const schema = crudContextHolderService.currentSchema(panelId);
            const fieldsTosubmit = submitServiceCommons.removeExtraFields(datamap, true, schema);
            const key = schemaService.buildApplicationMetadataSchemaKey(schema);

            const parameters = {
                key,
                associationKey,
                associationValue
            };

            return restService.postPromise("Association", "LookupSingleAssociation", parameters, fieldsTosubmit, { avoidspin: true }).then(function (httpResponse) {
                if (httpResponse.data === "null" || httpResponse.data == null) {
                    //                    var fakeItem = {
                    //                        value: associationValue,
                    //                    };
                    //
                    //                    crudContextHolderService.updateLazyAssociationOption(associationKey, fakeItem, true);
                    return null;
                }
                const idx = lazyAssociationsBeingResolved.indexOf(associationKey);
                if (idx !== -1) {
                    lazyAssociationsBeingResolved.splice(idx, 1);
                }
                crudContextHolderService.updateLazyAssociationOption(associationKey, httpResponse.data, true);
                return httpResponse.data;
            });
        }


        function parseLabelText(item, options) {
            if (item == null) {
                //if item is null no need to perform any further action
                return null;
            }

            options = options || {};
            const hideDescription = options.hideDescription || false;
            if ("true" === hideDescription || true === hideDescription || item.label == null) {
                return item.value;
            }
            const result = "(" + item.value + ")" + " - " + item.label;
            return result;
        }


        /**
         * 
         * @param {} associationKey 
         * @param {} itemValue 
         * @param {} options 
         *   hideDescription --> whether to show only the label value, false by default
         *   allowTransientValue --> whether to allow the insertion of new values, which do not necessarily match a server value
         *   isEager --> whether or not this is an eagerly loaded association
         * 
         * @returns {} 
         */
        function getLabelText(associationKey, itemValue, options) {
            const log = $log.get("associationService#getLabelText", ["association"]);
            options = options || {};

            if (options.isEager) {
                const eager = crudContextHolderService.fetchEagerAssociationOption(associationKey, itemValue);
                return $q.when(!!eager ? eager.label : null);
            }

            var allowTransientValue = options.allowTransientValue || false;

            if (itemValue == null) {
                //if item is null no need to perform any further action
                return $q.when(null);
            }
            const item = crudContextHolderService.fetchLazyAssociationOption(associationKey, itemValue);
            if (options.hideDescription === "true" || options.hideDescription === true) {
                return $q.when(itemValue);
            }


            if (item == null) {
                if (!crudContextHolderService.associationsResolved()) {
                    log.debug("schema associations not resolved yet, waiting...");
                    return $q.when(itemValue);
                }

                if (lazyAssociationsBeingResolved.indexOf(associationKey) !== -1) {
                    log.debug("association was already requested, waiting");
                    return $q.when(itemValue);
                }

                log.debug("fetching association option from server");

                return this.lookupSingleAssociationByValue(associationKey, itemValue).then(function (association) {
                    if (association == null && allowTransientValue) {
                        return $q.when(itemValue);
                    }
                    return parseLabelText(association, options);
                });
            }

            return $q.when(this.parseLabelText(item, options));

        };

        function getFullObject(associationFieldMetadata, datamap, schema, contextData) {
            //we need to locate the value from the list of association options
            // we only have the "value" on the datamap 
            const target = associationFieldMetadata.target;
            const selectedValue = datamap[target];

            if (selectedValue == null) {
                return null;
            }

            const resultValue = doGetFullObject(associationFieldMetadata, selectedValue, datamap, schema, contextData);

            if (resultValue == null) {
                $log.getInstance('associationService#getFullObject').warn('value not found in association options for {0} '.format(associationFieldMetadata.associationKey));
            }

            return resultValue;
        };

        ///this method is used to update associations which depends upon the projection result of a first association.
        ///it only makes sense for associations which have extraprojections
        function updateUnderlyingAssociationObject(associationFieldMetadata, underlyingValue, scope) {

            if (!!associationFieldMetadata.providerAttribute) return;

            //if association options have no fields, we need to define it as an empty array. 
            scope.associationOptions = scope.associationOptions || [];
            const key = associationFieldMetadata.associationKey;
            const contextData = scope.ismodal === "true" ? { schemaId: "#modal" } : null;
            const fullObject = this.getFullObject(associationFieldMetadata, scope.datamap, scope.schema, contextData);
            crudContextHolderService.updateLazyAssociationOption(key, underlyingValue, true);

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

            if (!!associationFieldMetadata.extraProjectionFields && associationFieldMetadata.extraProjectionFields.length > 0) {
                doUpdateExtraFields(associationFieldMetadata, underlyingValue, scope.datamap);
            }
        };

        function updateOptionFieldExtraFields(optionFieldMetadata, scope) {
            if (!optionFieldMetadata.extraProjectionFields || optionFieldMetadata.extraProjectionFields.length === 0) {
                return;
            }
            const contextData = scope.ismodal === "true" ? { schemaId: "#modal" } : null;
            const fullObject = this.getFullObject(optionFieldMetadata, scope.datamap, scope.schema, contextData);
            doUpdateExtraFields(optionFieldMetadata, fullObject, scope.datamap);
        }


        ///dispatchedbyuser: the method could be called after a user action (changing a field), or internally after a value has been set programatically 
        function postAssociationHook(associationMetadata, scope, triggerparams ={}) {
            var fields = triggerparams.fields;
            if (!fields) {
                fields = scope.datamap;
            }

            // clear the extra fields if new value is null
            if (fields && fields[associationMetadata.attribute] === null && associationMetadata.extraProjectionFields && associationMetadata.extraProjectionFields.length > 0) {
                doUpdateExtraFields(associationMetadata, null, fields);
            }

            const parameters = {
                fields,
                scope,
                parentdata: scope.parentdata || /* when trigerring event from modal */ crudContextHolderService.rootDataMap(),
                triggerparams
            };
            $log.get("sw4.associationservice#postAssociationHook", ["association", "detail"]).debug(`Post hook from association ${associationMetadata.target}|${associationMetadata.associationKey}`);
            return $q.when(eventService.afterchange(associationMetadata, parameters));
        };


        function updateAssociationOptionsRetrievedFromServer(scope, serverOptions, datamap, postAssociationHookFN) {
            /// <summary>
            ///  Callback of the updateAssociations call, in which the values returned from the server would update the scope variables, 
            /// to be shown on screen
            /// It would be called at the first time the detail screen is opened as well
            /// </summary>
            /// <param name="scope"></param>
            /// <param name="serverOptions"></param>
            /// <param name="datamap">could be null for list schemas.</param>
            var log = $log.getInstance('sw4.associationservice#updateAssociationOptionsRetrievedFromServer');
            scope.associationOptions = scope.associationOptions || {};
            scope.blockedassociations = scope.blockedassociations || {};
            scope.associationSchemas = scope.associationSchemas || {};
            scope.disabledassociations = scope.disabledassociations || {};
            postAssociationHookFN = postAssociationHookFN || this.postAssociationHookFN;

            for (var dependantFieldName in serverOptions) {

                //this iterates for list of fields which were dependant of a first one. 
                var array = serverOptions[dependantFieldName] || {};
                var zeroEntriesFound = (array.associationData == null || array.associationData.length === 0);
                var oneEntryFound = !zeroEntriesFound && array.associationData.length === 1;

                log.debug('updating association from server {0} length {1}'.format(dependantFieldName, array.associationData == null ? 0 : array.associationData.length));

                scope.associationOptions[dependantFieldName] = array.associationData;

                scope.blockedassociations[dependantFieldName] = zeroEntriesFound;
                scope.associationSchemas[dependantFieldName] = array.associationSchemaDefinition;
                let contextData = scope.ismodal === "true" ? { schemaId: "#modal" } : null; // special case of a composition list
                if (compositionService.isCompositionListItem(scope.datamap)) {
                    contextData = compositionService.buildCompositionListItemContext(contextData, scope.datamap, scope.schema);
                }

                crudContextHolderService.updateEagerAssociationOptions(dependantFieldName, array.associationData, contextData);
                const associationFieldMetadatas = fieldService.getDisplayablesByAssociationKey(scope.schema, dependantFieldName);
                if (associationFieldMetadatas == null) {
                    //should never happen, playing safe here
                    continue;
                }
                const fn = this;
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

                    if (oneEntryFound && fieldService.isFieldRequired(value, datamap)) {
                        //if there´s just one option available, and the field is required, let´s select it.
                        const entryReturned = array.associationData[0].value;
                        if (datamapTargetValue !== entryReturned) {
                            datamap[value.target] = entryReturned;
                        }
                    }

                    if (datamapTargetValue == null) {
                        //if there was no previous selected value, we don´t need to restore it
                        return;
                    }


                    //restoring previous selected value after the ng-options has changed
                    for (let j = 0; j < array.associationData.length; j++) {
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
                                    if (postAssociationHookFN) {
                                        postAssociationHookFN(value, scope, { phase: 'initial', dispatchedbytheuser: false });
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
                                if (postAssociationHookFN) {
                                    postAssociationHookFN(value, scope, { phase: 'initial', dispatchedbytheuser: false });
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
        };


        function getEagerAssociations(scope, options) {
            const associations = fieldService.getDisplayablesOfTypes(scope.schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            if (associations == undefined || associations.length === 0) {
                //no need to hit server in that case
                return $q.when();
            }

            scope.associationOptions = scope.associationOptions || {};
            scope.blockedassociations = scope.blockedassociations || {};
            scope.associationSchemas = scope.associationSchemas || {};
            return this.updateAssociations({ attribute: "#eagerassociations" }, scope, options);
        };

        function updateLazyOptions(lazyOptions) {
            angular.forEach(lazyOptions, function (value, key) {
                crudContextHolderService.updateLazyAssociationOption(key, value);
            });
        }

        function updateEagerOptions(eagerOptions, contextData) {
            angular.forEach(eagerOptions, function (value, key) {
                crudContextHolderService.updateEagerAssociationOptions(key, value, contextData);
            });
        }

        function updateEagerCompositionOptions(compositionData) {
        }

        function updateFromServerSchemaLoadResult(schemaLoadResult, contextData, allAssociationsResolved) {
            const log = $log.get("associationService#updateFromServerSchemaLoadResult", ["association"]);
            if (schemaLoadResult == null) {
                return $q.when();
            }

            log.debug("updating schema associations");

            updateLazyOptions(schemaLoadResult.preFetchLazyOptions);
            updateEagerOptions(schemaLoadResult.eagerOptions, contextData);
            updateEagerCompositionOptions(schemaLoadResult.eagerCompositionAssociations);
            if (allAssociationsResolved) {
                return $timeout(function () {
                    //this needs to be marked for the next digest loop so that the crud_input_fields has the possibility to distinguish between the initial and configured phases, 
                    //and so the listeners
                    crudContextHolderService.markAssociationsResolved(contextData != null ? contextData.panelId : null);
                    contextService.insertIntoContext("associationsresolved", true, true);
                }, 100, false);
            }
        }

        function loadSchemaAssociations(datamap, schema, options) {
            const log = $log.get("associationService#loadSchemaAssociations", ["association"]);
            datamap = datamap || {};

            var associations = fieldService.getDisplayablesOfTypes(schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            if (associations == undefined || associations.length === 0) {
                //no need to hit server in that case
                return $q.when();
            }

            options = options || {};
            const config = { avoidspin: options.avoidspin };
            const key = schemaService.buildApplicationMetadataSchemaKey(schema);
            const parameters = {
                key: key,
                showmore: options.showmore || false
            };
            const fields = datamap;
            const fieldsTosubmit = submitServiceCommons.removeExtraFields(fields, true, schema);
            const urlToUse = url("/api/generic/Association/GetSchemaOptions?" + $.param(parameters));
            const jsonString = angular.toJson(fieldsTosubmit);
            log.info('going to server for loading schema {0} associations '.format(schema.schemaId));
            if (log.isLevelEnabled("debug")) {
                log.debug('Content: \n {0}'.format(jsonString));
            }

            return $http.post(urlToUse, jsonString, config)
                .then(serverResponse => {
                    const result = updateFromServerSchemaLoadResult(serverResponse.data.resultObject, options.contextData, true);
                    updateExtraFields(associations, datamap, schema, options.contextData);
                    return result;
                }).catch(err => log.error(err));

        }


        // This method is called whenever an association value changes, in order to update all the dependant associations 
        // of this very first association.
        // This would only affect the eager associations, not the lookups, because they would be fetched at the time the user opens it.
        // Ex: An asset could be filtered by the location, so if a user changes the location field, the asset should be refetched.
        function updateAssociations(association, scope, options = {}) {

            var log = $log.getInstance('sw4.associationservice#updateAssociations', ['association']);

            var triggerFieldName = association.attribute;
            var schema = scope.schema;
            if (!schema) {
                schema = scope.compositionlistschema;
            }
            if (triggerFieldName !== "#eagerassociations" && $.inArray(triggerFieldName, schema.fieldWhichHaveDeps) === -1) {
                //no other asociation depends upon this first association, return here.
                //false is to indicate that no value has been updated
                log.debug('No associated dependants for {0}'.format(triggerFieldName));
                if (association.rendererType !== 'multiselectautocompleteclient' && options.phase !== "initial") {
                    //if multiple selection, there´s no sense to move focus
                    $timeout(() =>{
                        //this timeout is required because there´s already a digest going on, so this emit would throw an exception
                        //had to put a bigger timeout so that the watches doesn´t get evaluated.
                        //TODO: investigate it
                        scope.$emit(JavascriptEventConstants.MoveFocus, scope.datamap, scope.schema, triggerFieldName);
                    }, 300, false);
                }

                return false;
            }
            var updateAssociationOptionsRetrievedFromServer = this.updateAssociationOptionsRetrievedFromServer.bind(this);
            var postAssociationHook = this.postAssociationHook.bind(this);
            const applicationName = schema.applicationName;
            var fields = scope.datamap;

            if (options.datamap) {
                fields = options.datamap;
            }

            const parameters = {
                application: applicationName,
                key: {
                    schemaId: schema.schemaId,
                    mode: schema.mode,
                    platform: platform(),
                },
                triggerFieldName: triggerFieldName,
                id: fields[schema.idFieldName]
            };
            const fieldsTosubmit = submitServiceCommons.removeExtraFields(fields, true, scope.schema);
            const urlToUse = url("/api/generic/ExtendedData/UpdateAssociation?" + $.param(parameters));
            const jsonString = angular.toJson(fieldsTosubmit);
            log.info('going to server for dependent associations of {0}'.format(triggerFieldName));
            log.debug('Content: \n {0}'.format(jsonString));
            const config = {};
            if (options.avoidspin) {
                config.avoidspin = true;
            }
            return $http.post(urlToUse, jsonString, config).then(function (response) {
                const data = response.data;
                const options = data.resultObject;
                log.info('associations returned {0}'.format($.keys(options)));
                updateAssociationOptionsRetrievedFromServer(scope, options, fields, postAssociationHook);
                //clearDependantFieldValues(scope, triggerFieldName);
                if (association.attribute !== "#eagerassociations") {
                    clearDependantFieldValues(scope, triggerFieldName);
                    //this means we´re not getting the eager associations, see method above
                    //                    postAssociationHook(association, scope, { dispatchedbytheuser: true, phase: 'configured' });
                } else {
                    $timeout(function () {
                        //this needs to be marked for the next digest loop so that the crud_input_fields has the possibility to distinguish between the initial and configured phases, 
                        //and so the listeners
                        contextService.insertIntoContext("associationsresolved", true, true);
                    }, 100, false);
                    scope.$broadcast(JavascriptEventConstants.AssociationUpdated, scope.associationOptions);

                    //TODO: Is this needed, I couldn't find where it's used, I was not able to test if needed
                    //move input focus to the next field
                    if (triggerFieldName !== "#eagerassociations") {
                        $timeout(function () {
                            //this timeout is required because there´s already a digest going on, so this emit would throw an exception
                            scope.$emit(JavascriptEventConstants.MoveFocus, scope.datamap, scope.schema, triggerFieldName);
                        }, 0, false);
                    }
                }
            });
        };

        function buildLookupSearchDTO(fieldMetadata, lookupObj, searchObj, pageNumber, totalCount, pageSize) {
            const resultDTO = {};
            resultDTO.quickSearchDTO = lookupObj.quickSearchDTO;
            resultDTO.searchParams = searchObj.searchParams;
            resultDTO.searchValues = searchObj.searchValues;
            resultDTO.searchSort = searchObj.searchSort;
            resultDTO.SearchAscending = searchObj.SearchAscending;
            resultDTO.addPreSelectedFilters = searchObj.addPreSelectedFilters;

            resultDTO.pageNumber = pageNumber;
            resultDTO.totalCount = totalCount;
            resultDTO.pageSize = pageSize;

            return resultDTO;
        }

        function getEagerLookupOptions(lookupObj) {
            const isShowingModal = crudContextHolderService.isShowingModal();
            var contextData = null;
            if (isShowingModal) {
                contextData = {
                    schemaId: "#modal"
                };
            }
            var eagerOptions = crudContextHolderService.fetchEagerAssociationOptions(lookupObj.fieldMetadata.associationKey, contextData);
            if (!eagerOptions) {
                return null;
            }
            const quickSearchDTO = lookupObj.quickSearchDTO;
            if (quickSearchDTO) {
                const quickSearchData = quickSearchDTO.quickSearchData;
                eagerOptions = eagerOptions.filter(function (a) {
                    return a.value.containsIgnoreCase(quickSearchData) || a.label.containsIgnoreCase(quickSearchData);
                });
            }

            return {
                resultObject: {
                    associationData: eagerOptions,
                    totalCount: eagerOptions.length,
                    pageSize: eagerOptions.length,
                    pageCount: 1,
                    pageNumber: 1,
                    paginationOptions: [eagerOptions.length]
                }
            }
        }


        //This takes the lookupObj, pageNumber, and searchObj (dictionary of attribute (key) 
        //to its value that will filter the lookup), build a searchDTO, and return the post call to the
        //UpdateAssociations function in the ExtendedData controller.
        function getLookupOptions(schema, datamap, lookupObj, pageNumber, searchObj) {
            var fields = datamap;
            if (lookupObj.searchDatamap) {
                fields = lookupObj.searchDatamap;
            }
            const eagerOptions = getEagerLookupOptions(lookupObj);
            if (!!eagerOptions) {
                return $q.when(eagerOptions);
            }

            var totalCount = 0;
            var pageSize = 30;
            pageNumber = pageNumber || 1;

            if (lookupObj.modalPaginationData != null) {
                totalCount = lookupObj.modalPaginationData.totalCount;
                pageSize = lookupObj.modalPaginationData.pageSize;
            }

            //this should reflect LookupOptionsFetchRequestDTO.cs
            const parameters = {
                parentKey: schemaService.buildApplicationMetadataSchemaKey(schema),
                associationFieldName: lookupObj.fieldMetadata.associationKey,
            };
            parameters.searchDTO = buildLookupSearchDTO(lookupObj.fieldMetadata, lookupObj, searchObj, pageNumber, totalCount, pageSize);
            const urlToUse = url("/api/generic/Association/GetLookupOptions?" + $.param(parameters));
            const jsonString = angular.toJson(datamapSanitizeService.sanitizeDataMapToSendOnAssociationFetching(fields));
            return $http.post(urlToUse, jsonString).then(function (response) {
                return response.data;
            });
        };

        //Updates dependent association values for all association rendererTypes.
        //This includes the associationOptions, associationDescriptions, etc.
        function updateDependentAssociationValues(scope, datamap, lookupObj, postFetchHook, searchObj) {
            const getLookupOptions = this.getLookupOptions;
            var updateAssociationOptionsRetrievedFromServer = this.updateAssociationOptionsRetrievedFromServer;
            lookupObj.searchDatamap = datamap;
            getLookupOptions(scope.schema, scope.datamap, lookupObj, null, searchObj).then(function (data) {
                const result = data.resultObject;
                if (postFetchHook != null) {
                    const continueFlag = postFetchHook(result, lookupObj, scope, datamap);
                    if (!continueFlag) {
                        return;
                    }
                }
                const associationResult = result[lookupObj.fieldMetadata.associationKey];
                lookupObj.options = associationResult.associationData;
                lookupObj.schema = associationResult.associationSchemaDefinition;
                datamap[lookupObj.fieldMetadata.target] = lookupObj.options[0].value;

                updateAssociationOptionsRetrievedFromServer(scope, result, datamap);
            });
        };


        function onAssociationChange(fieldMetadata, updateUnderlying, event) {

            if (fieldMetadata.events == undefined) {
                event.continue();
                return true;
            }

            const result = eventService.beforechange(fieldMetadata, event); //sometimes the event might be syncrhonous, returning either true of false
            if (result != undefined && result == false) {
                event.interrupt();
                return false;
            }

            event.continue();
            return true;
        };

      

        function clearDependantFieldValues(scope, triggerFieldName) {
            const fieldsDependant = scope.displayables.filter(function (o) {
                return $.inArray(triggerFieldName, o.dependantFields) !== -1 && o.schema.isLazyLoaded;
            });
            $.each(fieldsDependant, function (key, value) {
                const attribute = value.attribute;
                scope.datamap[attribute] = null;
            });
        }


        function lookupAssociation(displayables, associationTarget) {
            for (let i = 0; i < displayables.length; i++) {
                const displayable = displayables[i];
                if (displayable.target != undefined && displayable.target === associationTarget) {
                    return displayable;
                }
            }
            return null;
        }

        const service = {
            clearDependantFieldValues,
            getLabelText,
            getFullObject,
            getLookupOptions,
            getEagerAssociations,
            getEagerLookupOptions,
            lookupAssociation,
            lookupSingleAssociationByValue,
            parseLabelText,
            onAssociationChange,
            postAssociationHook,
            updateAssociationOptionsRetrievedFromServer,
            updateAssociations,
            loadSchemaAssociations,
            updateDependentAssociationValues,
            updateFromServerSchemaLoadResult,
            updateUnderlyingAssociationObject,
            updateOptionFieldExtraFields
            };
        return service;
    }

    angular
    .module('sw_layout')
    .service('associationService', ['dispatcherService', '$http', '$q', '$timeout', '$log', '$rootScope', 'restService', 'submitServiceCommons', 'fieldService', 'contextService', 'searchService', 'crudContextHolderService', 'schemaService', 'datamapSanitizeService', 'compositionService', "eventService",
        associationService]);

})(angular);



