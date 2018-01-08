﻿
(function (angular) {
    "use strict";

    const formsInfo = {
        app: "_FormMetadata",
        schemaid: "listselection",
    }


    let isEditingSection = false;
    let currentSelectedFields = [];
    let buildSectionHeader;


    class dynFormService {

        constructor($q, $rootScope, schemaCacheService, restService, modalService, redirectService, applicationService, crudContextHolderService, fieldService, alertService) {
            this.$q = $q;
            this.$rootScope = $rootScope;
            this.modalService = modalService;
            this.schemaCacheService = schemaCacheService;
            this.restService = restService;
            this.redirectService = redirectService;
            this.applicationService = applicationService;
            this.crudContextHolderService = crudContextHolderService;
            this.fieldService = fieldService;
            this.alertService = alertService;

            function restoreData() {
                currentSelectedFields = [];
                isEditingSection = false;
            }

            this.$rootScope.$on("sw.crud.body.crawlocurred", () => {
                restoreData();
            });

            this.$rootScope.$on(JavascriptEventConstants.REDIRECT_AFTER, () => {
                restoreData();
            });

            this.$rootScope.$on(JavascriptEventConstants.ApplicationRedirected, () => {
                restoreData();
            });

            buildSectionHeader = (label) => {
                return {
                    displacement: "ontop",
                    label: label,
                    showExpression: "true",
                    parameters: {
                        fieldset: "true"
                    }
                }
            }

        }



        //#region Utils
        getFormSchema(info) {
            const cachedSchema = this.schemaCacheService.getCachedSchema(info.app, info.schemaid);

            if (!!cachedSchema) {
                return this.$q.when(cachedSchema);
            }

            const parameters = {
                applicationName: info.app,
                targetSchemaId: info.schemaid
            };

            const promise = this.restService.getPromise("Metadata", "GetSchemaDefinition", parameters);
            return promise.then(result => {
                this.schemaCacheService.addSchemaToCache(result.data);
                return result.data;
            });
        }

        openFormsModal() {
            this.getFormSchema(formsInfo).then((schema) => {
                this.modalService.show(schema, {}, { cancelOnClickOutside: true });
            });
        }

        doAddDisplayable(currentField, modalData, direction) {
            const keyTouse = currentField.target ? "target" : "attribute";
            const displayable = this.buildDisplayable(modalData);
            const cs = this.crudContextHolderService.currentSchema();
            const foundSectionResult = this.fieldService.locateOuterSection(cs, currentField);
            const foundSection = foundSectionResult ? foundSectionResult.container : null;
            const containerToAdd = foundSection ? foundSection : cs;
            const currentIdx = foundSection.displayables.findIndex(a => a.role === currentField.role);

            if (direction === "down" || direction === "top") {
                let idxToSplice = direction === "down" ? currentIdx + 1 : currentIdx;
                if (foundSection && foundSection.orientation === "horizontal") {
                    const outerVerticalData = this.fieldService.locateFirstOuterVerticalSection(cs, foundSection);
                    idxToSplice = direction === "down" ? outerVerticalData.idx + 1 : outerVerticalData.idx;
                    outerVerticalData.container.displayables.splice(idxToSplice, 0, displayable);
                } else {
                    containerToAdd.displayables.splice(idxToSplice, 0, displayable);
                }

            }

            if (direction === "right" || direction === "left") {
                if (foundSection && foundSection.orientation === "horizontal") {
                    const idxToSplice = direction === "right" ? currentIdx + 1 : currentIdx;
                    if (containerToAdd.displayables.length === 4) {
                        return this.alertService.alert("Cannot add more than 4 items on a row");
                    }
                    containerToAdd.displayables.splice(idxToSplice, 0, displayable);
                } else {
                    const newSection = this.buildDisplayable("ApplicationSection");
                    newSection.orientation = "horizontal";
                    if (direction === "right") {
                        newSection.displayables = [
                            currentField,
                            displayable
                        ];
                    } else {
                        newSection.displayables = [
                            displayable,
                            currentField
                        ];
                    }
                    const originalIdx = containerToAdd.displayables.findIndex(a => a[keyTouse] === currentField[keyTouse]);
                    //replacing the current field for the newly created section
                    containerToAdd.displayables[originalIdx] = newSection;
                }
            }

            if (direction === "edit") {
//                const key = displayable.attribute ? displayable.attribute : displayable.target;
                this.fieldService.replaceOrRemoveDisplayableByKey(cs, currentField, displayable);
            }
            cs.jscache = {};
        }

        addDisplayable(currentField, direction) {
            var that = this;
            return this.schemaCacheService.fetchSchema("_FormMetadata", "fieldEditModal").then(schema => {
                return that.modalService.showPromise(schema, {}, { cssclass: 'largemodal' });
            }).then(savedData => {
                return that.doAddDisplayable(currentField, savedData, direction);
            });
        }

        toggleSectionSelection(fieldMetadata) {
            const idx = currentSelectedFields.findIndex(f => f.role === fieldMetadata.role);
            if (idx === -1) {
                currentSelectedFields.push(fieldMetadata);
            } else {
                currentSelectedFields.splice(idx, 1);
            }
        }

        buildDisplayable(modalData) {


            if (modalData === "ApplicationSection") {
                modalData = {
                    fattribute: null,
                    flabel: null,
                    frequired: false,
                    freadonly: false,
                    fieldtype: "ApplicationSection"
                }
            }

            return {
                //has to be first field, until we´re able to migrate to newtonsoft 10.0.0 and use https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_MetadataPropertyHandling.htm
                "$type": `softwrench.sW4.Shared2.Metadata.Applications.Schema.${modalData.fieldtype}, softwrench.sw4.Shared2`,
                attribute: modalData.fattribute,
                label: modalData.flabel,
                requiredExpression: modalData.frequired ? "true" : "false",
                isReadOnly: modalData.freadonly,
                extraparameters: { "dynforms.editionallowed": true },
                rendererParameters: {},
                showExpression: "true",
                enableExpression: modalData.freadonly ? "false" : "true",
                "type": modalData.fieldtype
            }
        }

        editDisplayable(fieldMetadata) {
            var that = this;
            const convertedDatamap = {
                fieldtype: fieldMetadata.type,
                fattribute: fieldMetadata.attribute,
                flabel: fieldMetadata.label,
                frequired: fieldMetadata.requiredExpression === "true",
                freadonly: !!fieldMetadata.isReadOnly
            }
            return this.schemaCacheService.fetchSchema("_FormMetadata", "fieldEditModal").then(schema => {
                return that.modalService.showPromise(schema, convertedDatamap, { cssclass: 'largemodal' });
            }).then(savedData => {
                if (savedData.fattribute.indexOf(' ') >= 0) {
                    this.alertService.alert("Attribute names cannot contain spaces");
                }

                return that.doAddDisplayable(fieldMetadata,savedData, "edit");
            });
        }

        isEditingSection() {
            return isEditingSection;
        }

        isNotEditingSection() {
            return !isEditingSection;
        }

        removeDisplayable(fieldMetadata) {
            const key = fieldMetadata.attribute ? fieldMetadata.attribute : fieldMetadata.target;
            const cs = this.crudContextHolderService.currentSchema();
            this.alertService.confirm(`Are you sure you want to remove field ${key}`).then(() => {
                this.fieldService.replaceOrRemoveDisplayableByKey(cs, key);
            });
        }


        loadFormDetailEdition() {
            const id = this.crudContextHolderService.rootDataMap()["name"];
            this.redirectService.goToApplication("_FormMetadata", "newformbuilder", { id }).then(data => {

            });
        }

        saveDetailForm() {
            const extraparameters = {
                "realschema": "newformbuilder"
            }
            const dm = this.crudContextHolderService.rootDataMap();
            const cs = this.crudContextHolderService.currentSchema();
            //preserving first 2 items, which are the blank element section and the id
            //.net cannot convert flawlessly these items, so we´ll pass them as a JSON and use a custom deserialization process
            const newFields = cs.displayables.slice(2);
            //to ensure $type is present at all fields every time
            this.fieldService.injectServerTypesIntoDisplayables({ displayables: newFields });
            dm["#newFieldsJSON"] = JSON.stringify(newFields);
            dm["#name"] = dm.name;


            return this.applicationService.save({ datamap: dm, skipValidation: true, operation: "save_editform" });
            //            this.restService.post("FormMetadata", "StoreDetailForm", { formName: dm.name }, payload).then(r => {
            //                console.log(r);
            //            });


        }

        validateEditModal(schema, datamap, parameters) {
            const newAttribute = datamap.fattribute;
            if (newAttribute.indexOf(' ') > 0) {
                this.alertService.alert("attribute field cannot contain spaces");
                return false;
            }
            const cs = this.crudContextHolderService.currentSchema();
            const displ = this.fieldService.getDisplayableByKey(cs, newAttribute);
            if (displ) {
                this.alertService.alert(`attribute ${newAttribute} already present at the form. No duplicates allowed`);
                return false;
            }

        }

        createEnclosingSection() {
            isEditingSection = true;
        }

        doCreateEnclosingSection(savedData) {
            const cs = this.crudContextHolderService.currentSchema();
            currentSelectedFields = this.fieldService.sortBySchemaIdx(cs, currentSelectedFields);

            const commonContainerResult = this.fieldService.locateCommonContainer(cs, currentSelectedFields);
            const container = commonContainerResult.container;
            const idxToAdd = commonContainerResult.idx;
            //<header displacement="ontop" label="Failure Reporting" params="fieldset=true" />
            if (container.type === "ApplicationSection") {
                container.header = buildSectionHeader(savedData.headerlabel);
            }
            else {
                const newSection = this.buildDisplayable("ApplicationSection");
                newSection.header = buildSectionHeader(savedData.headerlabel);
                newSection.displayables = currentSelectedFields;
                cs.displayables.splice(idxToAdd + 1, 0, newSection);
                currentSelectedFields.forEach(field => {
                    this.fieldService.replaceOrRemoveDisplayableByKey(cs, field);
                });
            }
        }

        finishSectionEdition() {
            return this.schemaCacheService.fetchSchema("_FormMetadata", "sectionenclosingmodal").then(schema => {
                return this.modalService.showPromise(schema, {}, { cssclass: 'largemodal' });
            }).then(savedData => {
                return this.doCreateEnclosingSection(savedData);
            }).then(() => {
                isEditingSection = false;
                currentSelectedFields = [];
            });

        }

        loadFormGridEdition() {


        }

        redirectToForm(form) {
            const applicationname = form.id;
            //            const customParameters = {
            //                applicationname
            //            };

            const customParameters = {};
            customParameters[0] = {};
            customParameters[0]["key"] = "formname";
            customParameters[0]["value"] = applicationname;

            this.redirectService.goToApplication("_FormDatamap", "detail", { "customParameters": customParameters })
                .then(f => {
                    return this.modalService.hide();
                });
            //returning false to interrupt default detail workflow
            return false;
        }


    }


    dynFormService["$inject"] = ["$q", "$rootScope", "schemaCacheService", "restService", "modalService", "redirectService", "applicationService", "crudContextHolderService", "fieldService", "alertService"];

    angular.module("sw_layout").service("dynFormService", dynFormService);

})(angular);