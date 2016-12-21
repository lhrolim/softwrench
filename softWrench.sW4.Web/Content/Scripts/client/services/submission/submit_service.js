
(function (angular) {
    'use strict';


    angular.module('sw_layout').service('submitService', [
        '$rootScope', '$log', '$http', '$q', 'fieldService', 'contextService', 'checkpointService', 'alertService', 'schemaService',  'associationService','submitServiceCommons','compositionService',
        'eventService', 'crudContextHolderService', 'spinService', 'validationService', 'redirectService','modalService', function submitService($rootScope, $log, $http, $q, fieldService, contextService, checkpointService,
            alertService, schemaService,  associationService,submitServiceCommons,compositionService, eventService, crudContextHolderService, spinService, validationService, redirectService,modalService) {


            ///used for ie9 form submission
            function submitForm(formToSubmit, parameters, jsonString, applicationName) {
                // remove from session the redirect url... the redirect url will be returned when the form submit response comes from server
                contextService.deleteFromContext("swGlobalRedirectURL");

                for (let i in parameters) {
                    if (parameters.hasOwnProperty(i)) {
                        formToSubmit.append("<input type='hidden' name='" + i + "' value='" + parameters[i] + "' />");
                    }
                }
                if (sessionStorage.mockmaximo === "true") {
                    formToSubmit.append("<input type='hidden' name='%%mockmaximo' value='true'/>");
                }


                formToSubmit.append("<input type='hidden' name='currentmodule' value='" + contextService.retrieveFromContext('currentmodule') + "' />");

                formToSubmit.append("<input type='hidden' name='application' value='" + applicationName + "' />");
                formToSubmit.append("<input type='hidden' name='json' value='" + replaceAll(jsonString, "'", "&apos;") + "' />");

                // start spin befor submitting form
                const savingMain = true === $rootScope.savingMain;
                spinService.startSpin(savingMain);

                // submit form
                formToSubmit.attr("action", url("/Application/Input"));
                formToSubmit.submit();
            };



            function defaultSuccessFunction(data) {
                if (data != null) {
                    if (data.type === 'ActionRedirectResponse') {
                        //we´ll not do a crud action on this case, so totally different workflow needed
                        redirectService.redirectToAction(null, data.controller, data.action, data.parameters);
                    } else if (data.type !== ResponseConstants.BlankApplicationResponse && data.type !== ResponseConstants.GenericApplicationResponse) {
                        $rootScope.$emit(JavascriptEventConstants.RenderViewWithData, data);
                    }
                }

            }

        


            //Updates fields that were "removed" from an existing record. If the value was originally not null, but is now null,
            //then we update the datamap to " ". This is because the MIF will ignore nulls, causing no change to that field on the ticket.
            function handleDatamapForMIF(schema, originalDatamap, datamap) {
                const displayableFields = fieldService.getDisplayablesOfTypes(schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
                for (var i = 0, len = displayableFields.length; i < len; i++) {
                    const key = displayableFields[i].target == null ? displayableFields[i].attribute : displayableFields[i].target;
                    if (originalDatamap && (datamap[key] == null || datamap[key] == undefined) && datamap[key] !== originalDatamap[key]) {
                        datamap[key] = " ";
                    }
                }
            }

            //TODO: move to specific service
            function submitConfirmation() {
                return alertService.confirm("Are you sure you want to save changes to this record?");
            }


            /**
             * GAthers server result, updating onscreen-datamap and dispatch several hook methods
             * @param {} httpResult 
             * @param {} dispatcherComposition the name of the composition that dispatched the original submission workflow or null if not present
             * @param {} successCbk 
             * @param {} panelId 
             * @returns {} 
             */
            function onServerResult(httpResult, {dispatcherComposition,successCbk,panelId}) {


                const log = $log.get("submitService#onServerResult",["submit","save"]);
                log.debug("onserverresult function start");

                
                const data = httpResult.data;

                if (data.fullRefresh) {
                    log.debug("fullrefresh was requested, applying...");
                    window.location.reload();
                    return $q.when();
                }

                
                const datamap = crudContextHolderService.rootDataMap(panelId);
                const schema = crudContextHolderService.currentSchema(panelId);

                this.setIdAfterCreation(data,schema,datamap);
                

                const responseDataMap = data.resultObject;
                if (!data.type.equalsAny(ResponseConstants.BlankApplicationResponse)) {

                    // handle the case where the datamap had lazy compositions already fetched
                    // and the response does not have them (for performance reasons)
                    if (!data.type.equalsAny(ResponseConstants.ApplicationListResult)) {
                        compositionService.updateCompositionDataAfterSave(schema, datamap, responseDataMap);
                    }

                    // not necessary to update the complete datamap after a composition save
                    if (!dispatcherComposition && (responseDataMap.type === null || responseDataMap.type !== "UnboundedDatamap")) {
                        angular.extend(datamap, responseDataMap);
                    }
                }

                //hooks
                successCbk ? successCbk(data) : defaultSuccessFunction(data);
                crudContextHolderService.afterSave(panelId,datamap);
                
                $rootScope.$broadcast(JavascriptEventConstants.CrudSaved, data);
                return data;
            }

            function submit(schemaToSave,datamap, parameters={}) {

                const log = $log.get("submitService#submit",["submit","save"]);


                const modalSavefn = modalService.getSaveFn(); //if there´s a custom modal service, let´s use it instead of the ordinary crud savefn

                var selecteditem = parameters.selecteditem;
                //selectedItem would be passed in the case of a composition with autocommit=true, in the case the target would accept only the child instance... not yet supported. 
                //Otherwise, fetching from the $scope.datamap
                const fromDatamap = selecteditem == null;
                const fields = fromDatamap ? datamap : selecteditem;

                //need an angular.copy to prevent beforesubmit transformation events from modifying the original datamap.
                //this preserves the datamap (and therefore the data presented to the user) in case of a submission failure
                const transformedFields = angular.copy(fields);

                if (modalSavefn && parameters.dispatchedByModal) {
                    log.debug("submitting modal fn");
                    const errorForm = crudContextHolderService.crudForm("#modal").$error;
                    return validationService.validatePromise(schemaToSave, transformedFields, errorForm).then(() => {
                        log.debug("modal form validated. applying modal informed save fn");
                        return $q.when(modalSavefn(transformedFields, schemaToSave)).then(() => {
                            log.debug("modal form promise chaining");
                            modalService.hide();
                        });
                    });
                }

                log.debug("non-modal submission. validating");

                return this.validateSubmission(fields, schemaToSave, parameters).then(validationResult => {
                    if (!validationResult) {
                        log.debug("validation rejected, returning");
                        return $q.reject();
                    }
                    return this.doSubmitToServer(selecteditem, transformedFields, schemaToSave, parameters).then(httpResult => {
                        return this.onServerResult(httpResult, parameters);
                    }).catch(err => {
                        const exceptionData = err.data;
                        const resultObject = exceptionData.resultObject;
                        this.setIdAfterCreation(resultObject,schema,datamap);
                        if (failureCbk) {
                            failureCbk(resultObject);
                        }
                        return $q.reject(err);
                    });
                });
            }


            function doSubmitToServer(selecteditem, transformedFields, schemaToSave,{originalDatamap,nextSchemaObj,isComposition,compositionData,failureCbk,successCbk}) {
                
                const log = $log.get("submitService#doSubmitToServer",["submit","save"]);
                log.debug("doSubmit to server start... applying datamap transformations");

                $rootScope.$broadcast("sw_beforesubmitpostvalidate_internal", transformedFields);
                originalDatamap = originalDatamap || crudContextHolderService.originalDatamap();

                //some fields might require special handling
                // applying transformations
                submitServiceCommons.removeNullInvisibleFields(schemaToSave.displayables, transformedFields);
                transformedFields = submitServiceCommons.removeExtraFields(transformedFields, true, schemaToSave);
                submitServiceCommons.translateFields(schemaToSave.displayables, transformedFields);
                associationService.insertAssocationLabelsIfNeeded(schemaToSave, transformedFields);
                this.handleDatamapForMIF(schemaToSave, originalDatamap, transformedFields);

                const applicationName = schemaToSave.applicationName;
                const idFieldName = schemaToSave.idFieldName;
                const id = transformedFields[idFieldName];
                const submissionParameters = submitServiceCommons.createSubmissionParameters(transformedFields, schemaToSave, nextSchemaObj, id, compositionData);
                const jsonWrapper = {
                    json: transformedFields,
                    requestData: submissionParameters
                };
                const jsonString = angular.toJson(jsonWrapper);
                $rootScope.savingMain = !isComposition;

                if (isIe9()) {
                    log.debug("IE9 submission started (non-html5)");
                    const formToSubmitId = submitServiceCommons.getFormToSubmitIfHasAttachement();
                    if (formToSubmitId != null) {
                        const form = $(formToSubmitId);
                        submitService.submitForm(form, submissionParameters, jsonString, applicationName);
                        return $q.when();
                    }
                }

                if ("true" === sessionStorage.logJSON) {
                    //applying under default log
                    $log.info(jsonString);
                }

                log.debug(jsonString);

                const urlToUse = url("/api/data/" + applicationName + "/");
                const command = id == null ? $http.post : $http.put;

                log.info(`Invoking server submission at ${urlToUse}`);
                return command(urlToUse, jsonString);

            }

               function validateSubmission(transformedFields,schemaToSave,parameters={}) {


                const log = $log.get("submitService#validateSubmission", ["save","submit","validation"]);
                const originalDatamap = parameters.originalDatamap || crudContextHolderService.originalDatamap();


                const eventParameters = {
                    originaldatamap: originalDatamap
                };

                //before the original validationService gets called
                //TODO: rethink of this solution
                const prevalidation = eventService.beforesubmit_prevalidation(schemaToSave, transformedFields, eventParameters);

                //3 phases: prevalidation, validation and post validation
                return $q.when(prevalidation).then(eventResult => {
                    log.debug("prevalidation finished.. dispatching validation service");
                    if (eventResult === false) {
                        log.debug('Validation failed, returning');
                        return false;
                    }

                    //todo: reconsider this event
                    $rootScope.$broadcast("sw_beforesubmitprevalidate_internal", transformedFields);
                    const crudForm = crudContextHolderService.crudForm();
                    return validationService.validatePromise(schemaToSave, transformedFields, crudForm.$error);
                }).then(() => {
                    log.debug("validation finished.. dispatching post validation hook");
                    const postvalidation = eventService.beforesubmit_postvalidation(schemaToSave, transformedFields, eventParameters);
                    return $q.when(postvalidation).then((eventResult) => {
                        if (eventResult === false) {
                            //this means that the custom postvalidator should call the continue method
                            log.debug('waiting on custom postvalidator to invoke the continue function');
                            return false;
                        }
                        log.debug("postvalidation finished");
                        return true;
                    });
                }).catch(()=> false);
            };

            function setIdAfterCreation(data,schema,datamap) {
                const log = $log.get("submitService#setIdAfterCreation",["submit","save"]);
                const fields = datamap;
                if (data && data.id && fields &&
                    /* making sure not to update when it's not creation */
                    (!fields.hasOwnProperty(schema.idFieldName) ||
                        !fields[schema.idFieldName])) {
                    log.debug(`updating ids of created entry to ${data.id} and ${data.userId}`);
                    //updating the id, useful when it´s a creation and we need to update value return from the server side
                    fields[schema.idFieldName] = data.id;
                    fields[schema.userIdFieldName] = data.userId;
                }
            }


            const service = {
                doSubmitToServer,
                handleDatamapForMIF,
                onServerResult,
                setIdAfterCreation,
                submitConfirmation,
                submitForm,
                submit,
                validateSubmission
            };
            return service;

        }
    ]);


})(angular);

