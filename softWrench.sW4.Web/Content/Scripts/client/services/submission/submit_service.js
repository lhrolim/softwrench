
class SubmitResult {
        
    constructor(reason) {
        this.reason = reason;
    }

    static get ClientValidationFailed() {
        return new SubmitResult("ClientValidationFailed");
    }

    static get GenericError() {
        return new SubmitResult("GenericError");
    }

}



(function (angular) {
    'use strict';



    class submitService {

        constructor($rootScope, $log, $http, $q, fieldService, contextService, checkpointService,
            alertService, schemaService,  submitServiceCommons, compositionService, eventService, crudContextHolderService, spinService, validationService, redirectService, modalService) {
            this.$rootScope = $rootScope;
            this.$log = $log;
            this.$http = $http;
            this.$q = $q;
            this.fieldService = fieldService;
            this.contextService = contextService;
            this.checkpointService = checkpointService;
            this.alertService = alertService;
            this.schemaService = schemaService;
            this.submitServiceCommons = submitServiceCommons;
            this.compositionService = compositionService;
            this.eventService = eventService;
            this.crudContextHolderService = crudContextHolderService;
            this.spinService = spinService;
            this.validationService = validationService;
            this.redirectService = redirectService;
            this.modalService = modalService;
        }

        ///used for ie9 form submission
        submitFormForIe9(formToSubmit, parameters, jsonString, applicationName) {
            // remove from session the redirect url... the redirect url will be returned when the form submit response comes from server
            this.contextService.deleteFromContext("swGlobalRedirectURL");

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
            const savingMain = true === this.$rootScope.savingMain;
            this.spinService.startSpin(savingMain);

            // submit form
            formToSubmit.attr("action", url("/Application/Input"));
            formToSubmit.submit();
        }


        //TODO: move to specific service
        submitConfirmation() {
            return this.alertService.confirm("Are you sure you want to save changes to this record?");
        }


      

        setIdAfterCreation(serverData,schema,fields) {
            const log = this.$log.get("submitService#setIdAfterCreation",["submit","save"]);
            if (serverData && serverData.id && fields &&
                /* making sure not to update when it's not creation */
                (!fields.hasOwnProperty(schema.idFieldName) ||
                    !fields[schema.idFieldName])) {
                log.debug(`updating ids of created entry to ${serverData.id} and ${serverData.userId}`);
                //updating the id, useful when it´s a creation and we need to update value return from the server side
                fields[schema.idFieldName] = serverData.id;
                fields[schema.userIdFieldName] = serverData.userId;
            }
        }

        submitModal(schemaToSave,fields,modalSavefn) {

            const log = this.$log.get("submitService#submitModal",["submit","save"]);
            log.debug("submitting modal fn");
            const errorForm = this.crudContextHolderService.crudForm("#modal").$error;
            return this.validationService.validatePromise(schemaToSave, fields, errorForm).then(() => {
                log.debug("modal form validated. applying modal informed save fn");
                return this.$q.when(modalSavefn(fields, schemaToSave));
            });
        }

        submit(schemaToSave,datamap, parameters={}) {

            const log = this.$log.get("submitService#submit",["submit","save"]);

            const modalSavefn = this.modalService.getSaveFn(); //if there´s a custom modal service, let´s use it instead of the ordinary crud savefn
            const selecteditem = parameters.selecteditem;
            //selectedItem would be passed in the case of a composition with autocommit=true, in the case the target would accept only the child instance... not yet supported. 
            //Otherwise, fetching from the $scope.datamap
            const fromDatamap = selecteditem == null;
            const fields = fromDatamap ? datamap : selecteditem;

            if (modalSavefn && parameters.dispatchedByModal) {
                //if there's a custom save fn registered by the modal, lets invoke it
                return this.submitModal(schemaToSave,fields,modalSavefn);
            }

            log.debug("non-modal submission. validating");

            return this.validateSubmission(fields, schemaToSave, parameters).then(validationResult => {
                if (!validationResult) {
                    log.debug("validation rejected, returning");
                    return this.$q.reject(SubmitResult.ClientValidationFailed);
                }
                return this.doSubmitToServer(fields, schemaToSave, parameters).then(httpResult => {
                    return this.onServerResult(httpResult, parameters);
                }).catch(err => {
                    const exceptionData = err.data;
                    if (!exceptionData) {
                        //not really a server side error, but rather some sort of javascript exception happened
                        return this.$q.reject(err);
                    }

                    const resultObject = exceptionData.resultObject;
                    this.setIdAfterCreation(resultObject,schema,datamap);
                    return this.$q.reject(err);
                });
            });
        }


        onServerResult(httpResult, {dispatcherComposition,panelId}) {

            const log = this.$log.get("submitService#onServerResult",["submit","save"]);
            log.debug("onserverresult function start");

                
            const data = httpResult.data;

            if (data.fullRefresh) {
                log.debug("fullrefresh was requested, applying...");
                window.location.reload();
                return this.$q.when();
            }

                
            const datamap = this.crudContextHolderService.rootDataMap(panelId);
            const schema = this.crudContextHolderService.currentSchema(panelId);

            if (data.type.equalsAny(ResponseConstants.BlankApplicationResponse)) {
                this.crudContextHolderService.afterSave(panelId,datamap);
                return data;
            }


            this.setIdAfterCreation(data,schema,datamap);
            const responseDataMap = data.resultObject;

            

            // handle the case where the datamap had lazy compositions already fetched
            // and the response does not have them (for performance reasons)
            if (!data.type.equalsAny(ResponseConstants.ApplicationListResult)) {
                //--> hidratating the new returned datamap so that the previous composition values (if any) do not get lost
                this.compositionService.updateCompositionDataAfterSave(schema, datamap, responseDataMap);
            }

            // not necessary to update the complete datamap after a composition save
            if (!dispatcherComposition && (responseDataMap.type === null || responseDataMap.type !== "UnboundedDatamap")) {
                angular.extend(datamap, responseDataMap);
            }

            this.crudContextHolderService.afterSave(panelId,datamap);
            
            if (data.type === ResponseConstants.ActionRedirectResponse) {
                //we´ll not do a crud action on this case, so totally different workflow needed
                this.redirectService.redirectToAction(null, data.controller, data.action, data.parameters);
            } else if (!data.type.equalsAny(ResponseConstants.GenericApplicationResponse)) {
                //this helps for instance redirecting a brand new created entry to the edit schema
                this.redirectService.redirectViewWithData(data);
            }

            
            
            return data;
        }

      
        doSubmitToServer(transformedFields, schemaToSave,{originalDatamap,nextSchemaObj,isComposition,compositionData}) {
                
            const log = this.$log.get("submitService#doSubmitToServer",["submit","save"]);
            log.debug("doSubmit to server start... applying datamap transformations");

            originalDatamap = originalDatamap || this.crudContextHolderService.originalDatamap();

            //some fields might require special handling
            // applying transformations
            transformedFields = this.submitServiceCommons.applyTransformationsForSubmission(schemaToSave, originalDatamap, transformedFields);

            const applicationName = schemaToSave.applicationName;
            const id = transformedFields[schemaToSave.idFieldName];

            const submissionParameters = this.submitServiceCommons.createSubmissionParameters(transformedFields, schemaToSave, nextSchemaObj, id, compositionData);
            
            const jsonWrapper = {
                json: transformedFields,
                requestData: submissionParameters
            };

            const jsonString = angular.toJson(jsonWrapper);

            if (isIe9()) {
                log.debug("IE9 submission started (non-html5)");
                const formToSubmitId = this.submitServiceCommons.getFormToSubmitIfHasAttachement();
                if (formToSubmitId != null) {
                    const form = $(formToSubmitId);
                    this.submitService.submitFormForIe9(form, submissionParameters, jsonString, applicationName);
                    return this.$q.when();
                }
            }

            if ("true" === sessionStorage.logJSON) {
                //applying under default log
                this.$log.info(jsonString);
            }

            log.debug(jsonString);

            const urlToUse = url("/api/data/" + applicationName + "/");
            const command = id == null ? this.$http.post : this.$http.put;

            log.info(`Invoking server submission at ${urlToUse}`);
            return command(urlToUse, jsonString);

        }

        validateSubmission(transformedFields,schemaToSave,{originalDatamap}) {


         const log = this.$log.get("submitService#validateSubmission", ["save","submit","validation"]);
            originalDatamap = originalDatamap || this.crudContextHolderService.originalDatamap();


            const eventParameters = {
                originaldatamap: originalDatamap
            };

            //before the original validationService gets called
            //TODO: rethink of this solution
            const prevalidation = this.eventService.beforesubmit_prevalidation(schemaToSave, transformedFields, eventParameters);

            //3 phases: prevalidation, validation and post validation
            return this.$q.when(prevalidation).then(eventResult => {
                log.debug("prevalidation finished.. dispatching validation service");
                if (eventResult === false) {
                    log.debug('Validation failed, returning');
                    return false;
                }

                //todo: reconsider this event
                this.$rootScope.$broadcast("sw_beforesubmitprevalidate_internal", transformedFields);
                const crudForm = this.crudContextHolderService.crudForm();
                return this.validationService.validatePromise(schemaToSave, transformedFields, crudForm.$error);
            }).then(() => {
                log.debug("validation finished.. dispatching post validation hook");
                const postvalidation = this.eventService.beforesubmit_postvalidation(schemaToSave, transformedFields, eventParameters);
                return this.$q.when(postvalidation).then((eventResult) => {
                    if (eventResult === false) {
                        //this means that the custom postvalidator should call the continue method
                        log.debug('waiting on custom postvalidator to invoke the continue function');
                        return false;
                    }
                    log.debug("postvalidation finished");
                    return true;
                });
            }).catch(()=> false);
        }

    }
    

    submitService.$inject = [
        '$rootScope', '$log', '$http', '$q', 'fieldService', 'contextService', 'checkpointService', 'alertService', 'schemaService', 'submitServiceCommons', 'compositionService',
        'eventService', 'crudContextHolderService', 'spinService', 'validationService', 'redirectService', 'modalService'
    ];

    angular.module('sw_layout').service('submitService', submitService);


})(angular);

