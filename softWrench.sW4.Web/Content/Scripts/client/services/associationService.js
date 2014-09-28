var app = angular.module('sw_layout');

app.factory('associationService', function ($injector, $http, $timeout, $log, $rootScope, submitService, fieldService) {

    var doUpdateExtraFields = function (associationFieldMetadata, underlyingValue, datamap) {
        var log = $log.getInstance('sw4.associationservice#doUpdateExtraFields');
        var key = associationFieldMetadata.associationKey;
        datamap[key] = {};
        datamap.extrafields = instantiateIfUndefined(datamap.extrafields);
        if (associationFieldMetadata.extraProjectionFields == null) {
            return;
        }
        for (var i = 0; i < associationFieldMetadata.extraProjectionFields.length; i++) {
            var extrafield = associationFieldMetadata.extraProjectionFields[i];
            var valueToSet = underlyingValue == null ? null : underlyingValue.extrafields[extrafield];
            log.debug('updating extra field {0}.{1} | value={2}'.format(key, extrafield, valueToSet));
            if (extrafield.indexOf(key) > -1) {
                datamap[extrafield] = valueToSet;
            } else {
                var appendDot = extrafield.indexOf('.') == -1;
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
        }

        //we need to locate the value from the list of association options
        // we only have the "value" on the datamap 
        var key = associationFieldMetadata.associationKey;
        var listToSearch = associationOptions[key];
        if (listToSearch == null) {
            //if the list is lazy (ex: lookups, there´s nothing we can do, except for static option field )
            if (associationFieldMetadata.options != undefined) {
                //this means this is an option field with static options
                var resultArr = $.grep(associationFieldMetadata.options, function(option) {
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

            if (associationFieldMetadata.extraProjectionFields.length == 0) {
                //do nothing because it has no extraprojection fields
                return;
            }

            var key = associationFieldMetadata.associationKey;
            var i;
            if (underlyingValue == null) {
                //we need to locate the value from the list of association options
                // we only have the "value" on the datamap
                underlyingValue = this.getFullObject(associationFieldMetadata, scope.datamap, scope.associationOptions);
            }
            if (underlyingValue == null && scope.associationOptions[key] == null) {
                //the value remains null, but this is because the list of options is lazy loaded, nothing to do
                return;
            }

            doUpdateExtraFields(associationFieldMetadata, underlyingValue, scope.datamap);
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
            var service = $injector.get(afterChangeEvent.service);
            if (service == undefined) {
                //this should not happen, it indicates a metadata misconfiguration
                return;
            }
            //now let´s invoke the service
            var fn = service[afterChangeEvent.method];
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
                triggerparams: instantiateIfUndefined(triggerparams)
            };
            $log.getInstance('sw4.associationservice#postAssociationHook').debug('invoking post hook service {0} method {1}'.format(afterChangeEvent.service, afterChangeEvent.method));
            fn(afterchangeEvent);
        },

        //Callback of the updateAssociations call, in which the values returned from the server would update the scope variables, 
        //to be shown on screen
        ///It would be called at the first time the detail screen is opened as well
        updateAssociationOptionsRetrievedFromServer: function (scope, serverOptions, datamap) {
            var log = $log.getInstance('sw4.associationservice#updateAssociationOptionsRetrievedFromServer');
            scope.associationOptions = instantiateIfUndefined(scope.associationOptions);
            scope.blockedassociations = instantiateIfUndefined(scope.blockedassociations);
            scope.associationSchemas = instantiateIfUndefined(scope.associationSchemas);
            scope.disabledassociations = instantiateIfUndefined(scope.disabledassociations);
            for (var dependantFieldName in serverOptions) {

                //this iterates for list of fields which were dependant of a first one. 
                var array = instantiateIfUndefined(serverOptions[dependantFieldName]);

                log.debug('updating association from server {0} length {1}'.format(dependantFieldName, array.associationData == null ? 0 : array.associationData.length));

                scope.associationOptions[dependantFieldName] = array.associationData;
                scope.blockedassociations[dependantFieldName] = (array.associationData == null || array.associationData.length == 0);
                scope.associationSchemas[dependantFieldName] = array.associationSchemaDefinition;

                var associationFieldMetadatas = fieldService.getDisplayablesByAssociationKey(scope.schema, dependantFieldName);
                if (associationFieldMetadatas == null) {
                    //should never happen, playing safe here
                    continue;
                }
                var fn = this;
                $.each(associationFieldMetadatas, function (index, value) {
                    if (value.target == null) {
                        return;
                    }
                    //clear datamap for the association updated -->This is needed due to a IE9 issue
                    var previousValue = datamap[value.target];
                    datamap[value.target] = null;

                    if (array.associationData == null) {
                        //if no options returned from the server, nothing else to do
                        return;
                    }


                    if (previousValue != null) {
                        for (var j = 0; j < array.associationData.length; j++) {
                            if (array.associationData[j].value == previousValue) {
                                var fullObject = array.associationData[j];
                                $timeout(function () {
                                    log.debug('restoring {0} to previous value {1}. '.format(value.target, previousValue));
                                    //if still present on the new list, setting back the value which was 
                                    //previous selected, but after angular has updadted the list properly
                                    datamap[value.target] = previousValue;
                                    doUpdateExtraFields(value, fullObject, datamap);
                                    if (fn.postAssociationHook) {
                                        fn.postAssociationHook(value, scope, { phase: 'initial', dispatchedbytheuser: false });
                                    }
                                }, 0, false);
                                break;
                            }
                        }
                    }

                    //check if options is selected by default
                    //TODO: this name (isSelected) is a bit dangerous, place a # to avoid collisions with a column called isSelected
                    var selectedOptions = $.grep(array.associationData, function (option) {
                        if (option.extrafields != null && option.extrafields['isSelected'] != null) {
                            return option;
                        }
                    });

                    if (selectedOptions.length > 0) {
                        //TODO: why checkboxes aren´t the same (target)?
                        var targetString = value.rendererType == 'checkbox' ? value.associationKey : value.target;
                        // forces to clean previous association selection
                        datamap[targetString] = [];
                        for (var i = 0; i < selectedOptions.length; i++) {
                            if (selectedOptions[i].extrafields['isSelected'] == true) {
                                datamap[targetString].push(selectedOptions[i].value);
                            }
                        }
                    }
                });
            }
        },

        getEagerAssociations: function (scope) {
            var associations = fieldService.getDisplayablesOfTypes(scope.schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            if (associations == undefined || associations.length == 0) {
                //no need to hit server in that case
                return;
            }

            $rootScope.avoidspin = true;
            scope.associationOptions = instantiateIfUndefined(scope.associationOptions);
            scope.blockedassociations = instantiateIfUndefined(scope.blockedassociations);
            scope.associationSchemas = instantiateIfUndefined(scope.associationSchemas);
            return this.updateAssociations({ attribute: "#eagerassociations" }, scope);
        },

        //
        // This method is called whenever an association value changes, in order to update all the dependant associations 
        //of this very first association.
        // This would only affect the eager associations, not the lookups, because they would be fetched at the time the user opens it.
        //Ex: An asset could be filtered by the location, so if a user changes the location field, the asset should be refetched.
        updateAssociations: function (association, scope) {
            var triggerFieldName = association.attribute;
            var schema = scope.schema;
            if (triggerFieldName != "#eagerassociations" && $.inArray(triggerFieldName, schema.fieldWhichHaveDeps) == -1) {
                //no other asociation depends upon this first association, return here.
                //false is to indicate that no value has been updated
                return false;
            }
            var updateAssociationOptionsRetrievedFromServer = this.updateAssociationOptionsRetrievedFromServer;
            var postAssociationHook = this.postAssociationHook;

            var applicationName = schema.applicationName;
            var fields = scope.datamap;
            if (scope.datamap.fields) {
                fields = scope.datamap.fields;
            }


            var parameters = {
                application : applicationName ,
                key: {
                    schemaId: schema.schemaId,
                    mode: schema.mode,
                    platform: platform(),
                },
                triggerFieldName: triggerFieldName,
                id: fields[schema.idFieldName]
            };
            var fieldsTosubmit = submitService.removeExtraFields(fields, true, scope.schema);
            var urlToUse = url("/api/generic/Data/UpdateAssociation?" + $.param(parameters));
            var jsonString = angular.toJson(fieldsTosubmit);
            var log = $log.getInstance('sw4.associationservice#updateAssociations');
            log.info('going to server for dependat associations of {0}'.format(triggerFieldName));
            log.debug('Content: \n {0}'.format(jsonString));
            $http.post(urlToUse, jsonString).success(function (data) {
                var options = data.resultObject;
                log.info('associations returned {0}'.format($.keys(options)));
                updateAssociationOptionsRetrievedFromServer(scope, options, fields);
                if (association.attribute != "#eagerassociations") {
                    //this means we´re getting the eager associations, see method above
                    postAssociationHook(association, scope, { dispatchedbytheuser: true, phase: 'configured' });
                } else {
                    $rootScope.avoidspin = false;
                    $rootScope.$broadcast("sw_associationsupdated", scope.associationOptions);
                }
            }).error(
            function data() {
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
                var service = $injector.get(beforeChangeEvent.service);
                if (service == undefined) {
                    //this should not happen, it indicates a metadata misconfiguration
                    event.continue();
                    return;
                }
                //now let´s invoke the service
                var fn = service[beforeChangeEvent.method];
                if (fn == undefined) {
                    //this should not happen, it indicates a metadata misconfiguration
                    event.continue();
                    return;
                }
                var result = fn(event);
                //sometimes the event might be syncrhonous, returning either true of false
                if (result != undefined && result == false) {
                    event.interrupt();
                } else if (result != undefined && result == true) {
                    event.continue();
                }
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
                if (realValue != null) {
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


