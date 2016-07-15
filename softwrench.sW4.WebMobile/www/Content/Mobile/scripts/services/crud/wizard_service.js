!(function (angular) {
    "use strict";

    function wizardService(crudContextHolderService, crudContextService) {

        const wizardBehaviors = {
            SHOW: "show", // show wizards fields on last screen
            HIDE: "hide", // hide the wizard fileds on the last screen
            READ_ONLY: "readonly" // show wizards fields on last screen but read only
        }

        const allBehaviors = [];
        angular.forEach(wizardBehaviors, behavior => {
            allBehaviors.push(behavior);
        });

        const modelCache = {}

        const getBehavior = function (displayable) {
            const behavior = displayable.rendererParameters["wizardBehavior"];
            if (!behavior || allBehaviors.indexOf(behavior.toLowerCase()) < 0) {
                return wizardBehaviors.HIDE;
            }
            return behavior.toLowerCase();
        }

        const buildWizardModel = function (application, displayables) {
            const appModel = {
                states: [],
                allWizardFields: {}
            };
            modelCache[application] = appModel;
            if (!displayables) {
                return appModel;
            }

            angular.forEach(displayables, displayable => {
                if (!displayable.attribute) {
                    return;
                }
                const parameters = displayable.rendererParameters;
                if (!parameters) {
                    return;
                }
                const indexString = parameters["wizard"];
                if (nullOrUndef(indexString)) {
                    return;
                }
                const index = Number(indexString);
                if (isNaN(index)) {
                    return;
                }
                var state = appModel.states[index];
                if (!state) {
                    state = {
                        fields: {}
                    }
                    appModel.states[index] = state;
                }
                const fieldModel = {
                    attribute: displayable.attribute,
                    behavior: getBehavior(displayable)
                }

                state.fields[displayable.attribute] = fieldModel;
                appModel.allWizardFields[displayable.attribute] = fieldModel;
            });

            const nullSafeStates = [];
            angular.forEach(appModel.states, (state, index) => {
                if (appModel.states[index]) {
                    nullSafeStates.push(state);
                }
            });
            appModel.states = nullSafeStates;

            return appModel;
        }

        const getModel = function (application, displayables) {
            var model = modelCache[application];
            if (model) {
                return model;
            }
            model = buildWizardModel(application, displayables);
            return model;
        }

        const getModelAlso = function (displayables) {
            if (!crudContextService.isCreation()) {
                return null;
            }
            const application = crudContextHolderService.currentApplicationName();
            return getModel(application, displayables);
        }

        const nonWizardFields = function (application, displayables) {
            const appModel = getModel(application, displayables);
            if (appModel.states.length === 0) {
                return displayables;
            }
            
            const allWizardFields = appModel.allWizardFields;
            const nonWizardFieldsArray = [];
            angular.forEach(displayables, displayable => {
                if (!displayable.attribute) {
                    nonWizardFieldsArray.push(displayable);
                    return;
                }

                const fieldModel = allWizardFields[displayable.attribute];
                if (!fieldModel || fieldModel.behavior !== wizardBehaviors.HIDE) {
                    nonWizardFieldsArray.push(displayable);
                }
            });
            return nonWizardFieldsArray;
        }

        const innerGetWizardFields = function (displayables, stateIndex) {
            if (!crudContextService.isCreation() || !displayables) {
                return displayables;
            }

            const application = crudContextHolderService.currentApplicationName();

            const appModel = getModel(application, displayables);
            const state = appModel.states[stateIndex];
            if (!state) {
                return nonWizardFields(application, displayables);;
            }

            const wizardFields = [];
            angular.forEach(displayables, displayable => {
                if (!displayable || !displayable.attribute) {
                    return;
                }
                if (state.fields[displayable.attribute]) {
                    wizardFields.push(displayable);
                }
            });
            return wizardFields;
        }

        const getWizardFields = function (displayables) {
            const crudContext = crudContextService.getCrudContext();
            return innerGetWizardFields(displayables, crudContext.wizardStateIndex);
        }

        const next = function (displayables) {
            const crudContext = crudContextService.getCrudContext();
            crudContext.wizardStateIndex++;
            return innerGetWizardFields(displayables, crudContext.wizardStateIndex);
        }

        const previous = function (displayables) {
            const crudContext = crudContextService.getCrudContext();
            crudContext.wizardStateIndex--;
            if (crudContext.wizardStateIndex < 0) {
                crudContext.wizardStateIndex = 0;
            }
            return innerGetWizardFields(displayables, crudContext.wizardStateIndex);
        }

        const getWizardState = function(displayables) {
            const appModel = getModelAlso(displayables);
            if (!appModel) {
                return null;
            }
            const crudContext = crudContextService.getCrudContext();
            return appModel.states[crudContext.wizardStateIndex];
        }

        const isInWizardState = function (displayables) {
            return !!(getWizardState(displayables));
        }

        const canReturn = function () {
            if (!crudContextService.isCreation()) {
                return false;
            }
            const crudContext = crudContextService.getCrudContext();
            return crudContext.wizardStateIndex > 0;
        }

        const isReadOnly = function(displayable, allDisplayables) {
            if (!displayable.attribute) {
                return false;
            }
            const appModel = getModelAlso(allDisplayables);
            if (!appModel) {
                return false;
            }

            const crudContext = crudContextService.getCrudContext();
            const state =  appModel.states[crudContext.wizardStateIndex];
            if (state) {
                return false;
            }

            const fieldModel = appModel.allWizardFields[displayable.attribute];
            return fieldModel && fieldModel.behavior === wizardBehaviors.READ_ONLY;
        }

        const service = {
            getWizardFields,
            isInWizardState,
            canReturn,
            next,
            previous,
            isReadOnly
        };

        return service;
    }

    mobileServices.factory("wizardService", ["crudContextHolderService", "crudContextService", wizardService]);
})(angular);