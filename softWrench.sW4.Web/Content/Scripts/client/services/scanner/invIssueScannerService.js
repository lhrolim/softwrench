(function (angular) {
    "use strict";

    var validateOnlyOneOptionFound;

    class InvIssueScannerService {

        constructor($q, scanningCommonsService, applicationService, contextService, alertService, lookupService, associationService, fieldService) {
            this.$q = $q;
            this.scanningCommonsService = scanningCommonsService;
            this.applicationService = applicationService;
            this.contextService = contextService;
            this.alertService = alertService;
            this.lookupService = lookupService;
            this.associationService = associationService;
            this.fieldService = fieldService;

            validateOnlyOneOptionFound = (lookupObj) => {
                if (lookupObj.options === null || lookupObj.options.length !== 1) {
                    // Exit if more than one record is returned
                    this.alertService.alert("{0} is not a valid option for the {1} field".format(lookupObj.code, lookupObj.fieldMetadata.label));
                    return this.$q.reject();
                }
                return lookupObj;
            };

        }


        initInvIssueDetailListener(scope, schema, datamap, parameters) {
            // Set the avgTimeByChar to the correct value depending on if using mobile or desktop

            return this.scanningCommonsService.registerScanCallBackOnSchema(parameters)
                .then(data => {
                    //If the user scan's the string '%SUBMIT%', then the sw_submitdata event
                    //will be called using the default submit functions/process
                    if (data === '%SUBMIT%') {
                        return this.applicationService.save({ selecteditem: datamap });
                    }

                    const scanOrderString = this.contextService.retrieveFromContext(schema.schemaId + "ScanOrder");
                    const scanOrder = scanOrderString.split(",");
                    for (let attribute in scanOrder) {
                        if (!scanOrder.hasOwnProperty(attribute)) {
                            continue;
                        }

                        // Loop through the scan order, checking the corresponding fields in the datamap
                        // to see if they have a value
                        const currentAttribute = scanOrder[attribute];
                        if (datamap[currentAttribute] !== '' && !!datamap[currentAttribute]) {
                            //if the value is already set we perform no operation
                            continue;
                        }
                            
                        // Update the associated values
                        const fieldMetadata = this.fieldService.getDisplayableByKey(schema, currentAttribute);
                        // Update the associated values using the new scanned data
                        const lookupObj = new LookupDTO(fieldMetadata);

                        const searchObj = new SearchDTO({searchParams:currentAttribute , searchValues:data});

                        // Exit the loop once we have set a value from the scan
                        return this.lookupService.getLookupOptions(lookupObj, searchObj, datamap)
                            .then(validateOnlyOneOptionFound)
                            .then(lookupObj => {
                                if (!lookupObj) {
                                    return null;
                                }
                                datamap[lookupObj.fieldMetadata.target] = lookupObj.options[0].value;
                                //this method expects a dictionary as a first parameter, hence we need to adapt it a bit
                                const adaptedParameter = {
                                    [lookupObj.fieldMetadata.associationKey]: {
                                        associationData: lookupObj.options
                                    }
                                };
                                return this.associationService.updateAssociationOptionsRetrievedFromServer(adaptedParameter, datamap, currentSchema);
                            });
                    };
                });

        }

    }



    InvIssueScannerService.$inject = ['$q', 'scanningCommonsService', 'applicationService', 'contextService', 'alertService', 'lookupService', 'associationService', 'fieldService'];

    try {
        angular.module('sw_scan').service('invIssueScannerService', InvIssueScannerService);
    } catch (e) {
        angular.module('sw_scan', []);
        angular.module('sw_scan').service('invIssueScannerService', InvIssueScannerService);
    }


})(angular);