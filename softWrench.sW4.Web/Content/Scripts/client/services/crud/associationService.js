(function (angular) {
    "use strict";

    class associationService {

        constructor ($http, $q, $timeout, $log, $rootScope, restService, submitServiceCommons, fieldService, contextService,  
            crudContextHolderService, schemaService, datamapSanitizeService, compositionService, eventService) {
            this.$http = $http;
            this.$q = $q;
            this.$timeout = $timeout;
            this.$log = $log;
            this.$rootScope = $rootScope;
            this.restService = restService;
            this.submitServiceCommons = submitServiceCommons;
            this.fieldService = fieldService;
            this.contextService = contextService;
            this.crudContextHolderService = crudContextHolderService;
            this.schemaService = schemaService;
            this.datamapSanitizeService = datamapSanitizeService;
            this.compositionService = compositionService;
            this.eventService = eventService;
            /**
         * Array to avoid multiple calls to the server upon 
         */
            this.lazyAssociationsBeingResolved = [];
        }

        doUpdateExtraFields (associationFieldMetadata, underlyingValue, datamap) {
            const log = this.$log.getInstance('sw4.associationservice#doUpdateExtraFields',["association"]);
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
                    if (!!datamap.extrafields[key]) {
                        datamap.extrafields[key][extrafield] = valueToSet;    
                    }

                }
            };
        }

        updateExtraFields (associations, datamap, schema, contextData) {
            if (!associations || associations.length === 0) {
                return;
            }
            const log = this.$log.get("associationService#updateExtraFields");
            associations.forEach(function (association) {
                if (!association || association.extraProjectionFields == null || association.extraProjectionFields.length === 0) {
                    return;
                }
                const optionValue = this.getFullObject(association, datamap, schema, contextData);
                this.doUpdateExtraFields(association, optionValue, datamap);
            },this);

            log.info("Extra fields of associations updated during schema load.");
        }

        doGetFullObject (associationFieldMetadata, selectedValue, datamap, schema, contextData) {


            if (selectedValue == null) {
                return null;
            } else if (Array.isArray(selectedValue)) {
                // Extract each item into an array object
                const objectArray = selectedValue.map(value => {
                        return doGetFullObject(associationFieldMetadata, value, datamap, schema, contextData);
                    }).flatten(); // Return results for multi-value selection
                return objectArray;
            }

            // we need to locate the value from the list of association options
            // we only have the "value" on the datamap 
            const key = associationFieldMetadata.associationKey;
            if (associationFieldMetadata.schema && associationFieldMetadata.schema.isLazyLoaded) {
                return this.crudContextHolderService.fetchLazyAssociationOption(key, selectedValue);
            }

            // special case of a composition list
            if (this.compositionService.isCompositionListItem(datamap)) {
                contextData = this.compositionService.buildCompositionListItemContext(contextData, datamap, schema);
            }
            if (!contextData && this.crudContextHolderService.isShowingModal()) {
                contextData = { schemaId: "#modal" };
            }
            const listToSearch = this.crudContextHolderService.fetchEagerAssociationOptions(key, contextData);
            if (listToSearch == null) {
                //if the list is lazy (ex: lookups, there´s nothing we can do, except for static option field )
                if (associationFieldMetadata.options != undefined) {
                    //this means this is an option field with static options
                    const resultArr = $.grep(associationFieldMetadata.options, option => {
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
        }

        lookupSingleAssociationByValue(associationKey, associationValue) {

            this.lazyAssociationsBeingResolved.push(associationKey);
            const panelId = this.crudContextHolderService.isShowingModal() ? "#modal" : null;
            const datamap = this.crudContextHolderService.rootDataMap(panelId);
            const schema = this.crudContextHolderService.currentSchema(panelId);
            const fieldsTosubmit = this.submitServiceCommons.removeExtraFields(datamap, true, schema);
            const key = this.schemaService.buildApplicationMetadataSchemaKey(schema);
            const lazyAssociationsBeingResolvedLocal = this.lazyAssociationsBeingResolved;
            const updateLazyFn = this.crudContextHolderService.updateLazyAssociationOption.bind(this.crudContextHolderService);

            const parameters = {
                key,
                associationKey,
                associationValue
            };

            return this.restService.postPromise("Association", "LookupSingleAssociation", parameters, fieldsTosubmit, { avoidspin: true }).then(function (httpResponse) {
                if (httpResponse.data === "null" || httpResponse.data == null) {
                    return null;
                }
                const idx = lazyAssociationsBeingResolvedLocal.indexOf(associationKey);
                if (idx !== -1) {
                    lazyAssociationsBeingResolvedLocal.splice(idx, 1);
                }
                updateLazyFn(associationKey, httpResponse.data, true);
                return httpResponse.data;
            });
        }


        parseLabelText(item, options) {
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
         *   avoidPromise --> angular interpolation function does not unwrap promises automatically. Therefore, we need for some scenarios to recover the value
         * 
         * @returns {} 
         */
        getLabelText(associationKey, itemValue, options = {avoidPromise:false,hideDescription:false,isEager:false}) {
            const log = this.$log.get("associationService#getLabelText", ["association"]);

            if (options.isEager) {
                const eager = this.crudContextHolderService.fetchEagerAssociationOption(associationKey, itemValue);
                return this.$q.when(!!eager ? eager.label : null);
            }

            var allowTransientValue = options.allowTransientValue || false;

            if (itemValue == null) {
                if (options.avoidPromise) {
                    return itemValue;
                }
                //if item is null no need to perform any further action
                return this.$q.when(null);
            }
            const item = this.crudContextHolderService.fetchLazyAssociationOption(associationKey, itemValue);
            if (options.hideDescription === "true" || options.hideDescription === true) {
                if (options.avoidPromise) {
                    return itemValue;
                }

                return this.$q.when(itemValue);
            }

            const parseLabelTextFn = this.parseLabelText.bind(this);

            if (item == null) {
                if (!this.crudContextHolderService.associationsResolved()) {
                    log.debug("schema associations not resolved yet, waiting...");
                    if (options.avoidPromise) {
                        return itemValue;
                    }
                    return this.$q.when(itemValue);
                }

                if (this.contextService.get("anonymous", false, true) || options.avoidPromise) {
                    log.debug("cannot load extra association texts running on anonymous mode");
                    if (options.avoidPromise) {
                        return itemValue;
                    }
                    return this.$q.when(itemValue);
                }

                if (this.lazyAssociationsBeingResolved.indexOf(associationKey) !== -1) {
                    log.debug("association was already requested, waiting");
                    if (options.avoidPromise) {
                        return itemValue;
                    }
                    return this.$q.when(itemValue);
                }

                log.debug("fetching association option from server");



                return this.lookupSingleAssociationByValue(associationKey, itemValue).then(function (association) {
                    if (association == null && allowTransientValue) {
                        return this.$q.when(itemValue);
                    }
                    return parseLabelTextFn(association, options);
                });
            }
            const labelText = parseLabelTextFn(item, options);

            if (options.avoidPromise) {
                return labelText;
            }

            return this.$q.when(labelText);

        }

        getFullObject(associationFieldMetadata, datamap, schema, contextData) {
            //we need to locate the value from the list of association options
            // we only have the "value" on the datamap 
            const target = associationFieldMetadata.target;
            const selectedValue = datamap[target];

            if (selectedValue == null) {
                return null;
            }

            const resultValue = this.doGetFullObject(associationFieldMetadata, selectedValue, datamap, schema, contextData);

            if (resultValue == null) {
                this.$log.getInstance('associationService#getFullObject',["association"]).warn('value not found in association options for {0} '.format(associationFieldMetadata.associationKey));
            }

            return resultValue;
        }

        ///this method is used to update associations which depends upon the projection result of a first association.
        ///it only makes sense for associations which have extraprojections
        updateUnderlyingAssociationObject(associationFieldMetadata, underlyingValue, {datamap,schema, ismodal}) {

            this.$log.get("associationService#updateUnderlyingAssociationObject", ["association"]).debug("call updateUnderlyingAssociationObject");

            if (!!associationFieldMetadata.providerAttribute) return datamap;

            //if association options have no fields, we need to define it as an empty array. 
            
            const key = associationFieldMetadata.associationKey;
            const contextData = ismodal === "true" ? { schemaId: "#modal" } : null;
            if (!(underlyingValue instanceof AssociationOptionDTO)) {
                const fullObject = this.getFullObject(associationFieldMetadata, datamap, schema, contextData);    
                underlyingValue = underlyingValue || fullObject;
            }
            
            this.crudContextHolderService.updateLazyAssociationOption(key, underlyingValue, true);
            
            if (underlyingValue == null) {
                //the value remains null, but this is because the list of options is lazy loaded, nothing to do
                return datamap;
            }

            if (!!associationFieldMetadata.extraProjectionFields && associationFieldMetadata.extraProjectionFields.length > 0) {
                this.doUpdateExtraFields(associationFieldMetadata, underlyingValue, datamap);
            }
            return datamap;
        }

        updateOptionFieldExtraFields(optionFieldMetadata, scope) {
            if (!optionFieldMetadata.extraProjectionFields || optionFieldMetadata.extraProjectionFields.length === 0) {
                return;
            }
            const contextData = scope.ismodal === "true" ? { schemaId: "#modal" } : null;
            const fullObject = this.getFullObject(optionFieldMetadata, scope.datamap, scope.schema, contextData);
            this.doUpdateExtraFields(optionFieldMetadata, fullObject, scope.datamap);
        }


        ///dispatchedbyuser: the method could be called after a user action (changing a field), or internally after a value has been set programatically 
        postAssociationHook(associationMetadata, scope, triggerparams ={}) {
            const parentdata = this.crudContextHolderService.rootDataMap();
            const fields = triggerparams.fields || scope.fields || parentdata;

            // clear the extra fields if new value is null
            if (fields && fields[associationMetadata.attribute] === null && associationMetadata.extraProjectionFields && associationMetadata.extraProjectionFields.length > 0) {
                this.doUpdateExtraFields(associationMetadata, null, fields);
            }

            const parameters = {
                fields,
                parentdata: parentdata,
                triggerparams
            };
            this.$log.get("sw4.associationservice#postAssociationHook", ["association", "detail"]).debug(`Post hook from association ${associationMetadata.target}|${associationMetadata.associationKey}`);
            if (associationMetadata.type === "OptionField" && associationMetadata.rendererType === "checkbox") {
                //lets not deal with this type here, rather on the crud_input_fields.js, because we need the current option being selected as well
                return this.$q.when(true);
            }

            return this.$q.when(this.eventService.afterchange(associationMetadata, parameters));
        }


        updateAssociationOptionsRetrievedFromServer(serverOptions, datamap, schema) {
            /// <summary>
            /// It would be called at the first time the detail screen is opened as well
            /// </summary>
            /// <param name="serverOptions"></param>
            /// <param name="datamap">could be null for list schemas.</param>
            var log = this.$log.getInstance('sw4.associationservice#updateAssociationOptionsRetrievedFromServer',["association"]);
            
            
            const postAssociationHookFN = this.postAssociationHook.bind(this);
            

            //server could return the association itself plus a list of dependant associations of the first one.
            //for instance, after a location is selected, a list of asset could be available.
            //this (dependencies) would only make sense for eager associations though
            for (let associationKey in serverOptions) {
                if (!serverOptions.hasOwnProperty(associationKey)) {
                    continue;
                }
                    
                //this iterates for list of fields which were dependant of a first one ==> maps to BaseAssociationUpdateResult instances on the server side
                const array = serverOptions[associationKey] || {};

                const associationData = array.associationData;

                var zeroEntriesFound = (associationData == null || associationData.length === 0);
                var oneEntryFound = !zeroEntriesFound && associationData.length === 1;

                log.debug('updating association from server {0} length {1}'.format(associationKey, associationData == null ? 0 : associationData.length));

                this.crudContextHolderService.blockOrUnblockAssociations(associationKey, zeroEntriesFound);
                let contextData = this.crudContextHolderService.isShowingModal() ? { schemaId: "#modal" } : null;
                if (this.compositionService.isCompositionListItem(datamap)) {
                    // special case of a composition list
                    contextData = this.compositionService.buildCompositionListItemContext(contextData, datamap, schema);
                }

                this.crudContextHolderService.updateEagerAssociationOptions(associationKey, associationData, contextData);
                const associationFieldMetadatas = this.fieldService.getDisplayablesByAssociationKey(schema, associationKey);
                if (associationFieldMetadatas == null) {
                    //should never happen, playing safe here
                    continue;
                }
                
                associationFieldMetadatas.forEach((associationMetadata) => {

                    if (zeroEntriesFound && (associationMetadata.rendererType == "lookup" || associationMetadata.rendererType == "autocompleteserver")) {
                        //this should never be blocked, but the server might still be returning 0 entries due to a reverse association mapping
                        this.crudContextHolderService.blockOrUnblockAssociation(associationKey, false);
                    }

                    if (associationMetadata.reverse && !zeroEntriesFound) {
                        datamap[associationMetadata.target] = associationData[0].value;
                    }

                    if (associationMetadata.target == null || datamap == null) {
                        //the datamap could be null if this method is called from a list schema
                        return;
                    }

                    var datamapTargetValue = datamap[associationMetadata.target] == null ? null : datamap[associationMetadata.target].toString();

                    if (isIe9()) {
                        //clear datamap for the association updated, restoring it later -->This is needed due to a IE9 issue
                        datamap[associationMetadata.target] = null;
                    }


                    if (associationData == null) {
                        //if no options returned from the server, nothing else to do
                        return;
                    }

                    if (oneEntryFound && this.fieldService.isFieldRequired(associationMetadata, datamap)) {
                        //if there´s just one option available, and the field is required, let´s select it.
                        const entryReturned = associationData[0].value;
                        if (datamapTargetValue !== entryReturned) {
                            datamap[associationMetadata.target] = entryReturned;
                        }
                    }

                    if (datamapTargetValue == null) {
                        //if there was no previous selected value, we don´t need to restore it
                        return;
                    }


                    //restoring previous selected value after the ng-options has changed
                    for (let j = 0; j < associationData.length; j++) {
                        var associationOption = associationData[j];
                        if (associationOption.value.toUpperCase() == datamapTargetValue.toUpperCase()) {
                            var fullObject = associationOption;

                            if (isIe9()) {
                                this.$timeout(function() {
                                    log.debug('restoring {0} to previous value {1}. '.format(associationMetadata.target, datamapTargetValue));
                                    //if still present on the new list, setting back the value which was 
                                    //previous selected, but after angular has updadted the list properly
                                    datamap[associationMetadata.target] = String(associationOption.value);
                                    this.doUpdateExtraFields(associationMetadata, fullObject, datamap);
                                    if (postAssociationHookFN) {
                                        postAssociationHookFN(associationMetadata, { phase: 'initial', dispatchedbytheuser: false });
                                    }
                                    try {
                                        this.$rootScope.$digest();
                                    } catch (e) {
                                        //digest already in progress
                                    }
                                }, 0, false);
                            } else {
                                //let´s remove the complexity of non ie9 solutions, calling the code outside of the timeout since it´s not needed
                                datamap[associationMetadata.target] = String(datamapTargetValue);
                                this.doUpdateExtraFields(associationMetadata, fullObject, datamap);
                                if (postAssociationHookFN) {
                                    postAssociationHookFN(associationMetadata, { phase: 'initial', dispatchedbytheuser: false });
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
                    this.$rootScope.digest();
                } catch (e) {
                    //digest already in progress
                }
            }
        }


        getEagerAssociations(scope, options) {
            const associations = this.fieldService.getDisplayablesOfTypes(scope.schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            if (associations == undefined || associations.length === 0) {
                //no need to hit server in that case
                return this.$q.when();
            }

            scope.blockedassociations = scope.blockedassociations || {};
            scope.associationSchemas = scope.associationSchemas || {};
            return this.updateAssociations({ attribute: "#eagerassociations" }, scope, options);
        }


        updateFromServerSchemaLoadResult(schemaLoadResult, contextData, allAssociationsResolved) {
            const log = this.$log.get("associationService#updateFromServerSchemaLoadResult", ["association"]);
            if (schemaLoadResult == null) {
                return this.$q.when();
            }

            log.debug("updating schema associations");

            angular.forEach(schemaLoadResult.preFetchLazyOptions, (value, key) =>{
                this.crudContextHolderService.updateLazyAssociationOption(key, value);
            },this);

            angular.forEach(schemaLoadResult.eagerOptions, (value, key)=> {
                this.crudContextHolderService.updateEagerAssociationOptions(key, value, contextData);
            },this);

            if (allAssociationsResolved) {
                return this.$timeout(() =>{
                    //this needs to be marked for the next digest loop so that the crud_input_fields has the possibility to distinguish between the initial and configured phases, 
                    //and so the listeners
                    this.crudContextHolderService.markAssociationsResolved(contextData != null ? contextData.panelId : null);
                }, 100, false);
            }
        }

        loadSchemaAssociations(datamap, schema, options) {
            const log = this.$log.get("associationService#loadSchemaAssociations", ["association"]);
            datamap = datamap || {};

            var associations = this.fieldService.getDisplayablesOfTypes(schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            if (associations == undefined || associations.length === 0) {
                //no need to hit server in that case
                return this.$q.when();
            }

            options = options || {};
            const config = { avoidspin: options.avoidspin };
            const key = this.schemaService.buildApplicationMetadataSchemaKey(schema);
            const parameters = {
                key: key,
                showmore: options.showmore || false
            };
            const fields = datamap;
            const fieldsTosubmit = this.submitServiceCommons.removeExtraFields(fields, true, schema);
            const urlToUse = url("/api/generic/Association/GetSchemaOptions?" + $.param(parameters));
            const jsonString = angular.toJson(fieldsTosubmit);
            log.info('going to server for loading schema {0} associations '.format(schema.schemaId));
            if (log.isLevelEnabled("debug")) {
                log.debug('Content: \n {0}'.format(jsonString));
            }

            return this.$http.post(urlToUse, jsonString, config)
                .then(serverResponse => {
                    const result = this.updateFromServerSchemaLoadResult(serverResponse.data.resultObject, options.contextData, true);
                    this.updateExtraFields(associations, datamap, schema, options.contextData);
                    return result;
                }).catch(err => log.error(err));

        }


        // This method is called whenever an association value changes, in order to update all the dependant associations 
        // of this very first association.
        // This would only affect the eager associations, not the lookups, because they would be fetched at the time the user opens it.
        // Ex: An asset could be filtered by the location, so if a user changes the location field, the asset should be refetched.
        updateAssociations(association, scope, options = {}) {

            var log = this.$log.getInstance('sw4.associationservice#updateAssociations', ['association']);

            var triggerFieldName = association.attribute;
            let schema = scope.schema;
            if (!schema) {
                schema = scope.compositionlistschema;
            }
            if (triggerFieldName !== "#eagerassociations" && $.inArray(triggerFieldName, schema.fieldWhichHaveDeps) === -1) {
                //no other asociation depends upon this first association, return here.
                //false is to indicate that no value has been updated
                log.debug('No associated dependants for {0}'.format(triggerFieldName));
                if (association.rendererType !== 'multiselectautocompleteclient' && options.phase !== "initial") {
                    //if multiple selection, there´s no sense to move focus
                    this.$timeout(() =>{
                        //this timeout is required because there´s already a digest going on, so this emit would throw an exception
                        //had to put a bigger timeout so that the watches doesn´t get evaluated.
                        //TODO: investigate it
//                        scope.$emit(JavascriptEventConstants.MoveFocus, scope.datamap, scope.schema, triggerFieldName);
                    }, 300, false);
                }

                return false;
            }
            var updateAssociationOptionsRetrievedFromServer = this.updateAssociationOptionsRetrievedFromServer.bind(this);
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
            const fieldsTosubmit = this.submitServiceCommons.removeExtraFields(fields, true, scope.schema);
            const urlToUse = url("/api/generic/ExtendedData/UpdateAssociation?" + $.param(parameters));
            const jsonString = angular.toJson(fieldsTosubmit);
            log.info('going to server for dependent associations of {0}'.format(triggerFieldName));
            log.debug('Content: \n {0}'.format(jsonString));
            const config = {};
            if (options.avoidspin) {
                config.avoidspin = true;
            }
            return this.$http.post(urlToUse, jsonString, config).then(response=> {
                const data = response.data;
                const options = data.resultObject;
                log.info('associations returned {0}'.format($.keys(options)));
                updateAssociationOptionsRetrievedFromServer(options, fields,schema);
                if (association.attribute !== "#eagerassociations") {
                    this.clearDependantFieldValues(scope.displayables, scope.datamap, triggerFieldName);
                    //this means we´re not getting the eager associations, see method above
                    //                    postAssociationHook(association, scope, { dispatchedbytheuser: true, phase: 'configured' });
                } else {
                    this.$timeout(function () {
                        //this needs to be marked for the next digest loop so that the crud_input_fields has the possibility to distinguish between the initial and configured phases, 
                        //and so the listeners
                        this.contextService.insertIntoContext("associationsresolved", true, true);
                    }, 100, false);
                    scope.$broadcast(JavascriptEventConstants.AssociationUpdated);

                    //TODO: Is this needed, I couldn't find where it's used, I was not able to test if needed
                    //move input focus to the next field
                    if (triggerFieldName !== "#eagerassociations") {
                        this.$timeout(function () {
                            //this timeout is required because there´s already a digest going on, so this emit would throw an exception
//                            scope.$emit(JavascriptEventConstants.MoveFocus, scope.datamap, scope.schema, triggerFieldName);
                        }, 0, false);
                    }
                }
            });
        }

        onAssociationChange(fieldMetadata, updateUnderlying, event) {

            if (fieldMetadata.events == undefined || !event.dispatchedbytheuser) {
                //no sense to dispatch a before change event which was not dispatched by the user
                event.continue();
                return true;
            }
            event.fieldMetadata = fieldMetadata;
            if (fieldMetadata.type === "OptionField" && fieldMetadata.rendererType === "checkbox") {
                //lets not deal with this type here, rather on the crud_input_fields.js, because we need the current option being selected as well
                event.continue();
                return true;
            }

            const result = this.eventService.beforechange(fieldMetadata, event); //sometimes the event might be syncrhonous, returning either true of false
            if (result != undefined && result === false) {
                event.interrupt();
                return false;
            }

            event.continue();
            return true;
        }

        clearDependantFieldValues(displayables, datamap, triggerFieldName) {
            const fieldsDependant = displayables.filter(o=> {
                return $.inArray(triggerFieldName, o.dependantFields) !== -1 && o.schema.isLazyLoaded;
            });
            $.each(fieldsDependant, function (key, value) {
                const attribute = value.attribute;
                datamap[attribute] = null;
            });
        }


        lookupAssociation(displayables, associationTarget) {
            for (let i = 0; i < displayables.length; i++) {
                const displayable = displayables[i];
                if (displayable.target != undefined && displayable.target === associationTarget) {
                    return displayable;
                }
            }
            return null;
        }


    }

    associationService.$inject = ['$http', '$q', '$timeout', '$log', '$rootScope', 'restService', 'submitServiceCommons', 'fieldService', 'contextService',  'crudContextHolderService', 'schemaService', 'datamapSanitizeService', 'compositionService', "eventService"];

    angular.module('sw_layout').service('associationService', associationService);

})(angular);



