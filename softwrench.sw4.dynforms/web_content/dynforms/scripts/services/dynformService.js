
(function (angular) {
    "use strict";

    const formsInfo = {
        app: "_FormMetadata",
        schemaid: "listselection",
    }

    const cloneModalInfo = {
        app: "_FormMetadata",
        schemaid: "detail",
    }


    let isEditingSection = false;
    let isPreviewMode = false;
    let currentSelectedFields = [];
    let buildSectionHeader;


    class dynFormService {

        constructor($q, $timeout, $rootScope, schemaCacheService, restService, modalService, redirectService, applicationService, crudContextHolderService, fieldService, alertService, schemaService, associationService, checkListTableBuilderService) {
            this.$q = $q;
            this.$timeout = $timeout;
            this.$rootScope = $rootScope;
            this.modalService = modalService;
            this.schemaCacheService = schemaCacheService;
            this.restService = restService;
            this.redirectService = redirectService;
            this.applicationService = applicationService;
            this.crudContextHolderService = crudContextHolderService;
            this.fieldService = fieldService;
            this.alertService = alertService;
            this.associationService = associationService;
            this.checkListTableBuilderService = checkListTableBuilderService;

            function restoreData() {
                currentSelectedFields = [];
                isEditingSection = false;
                isPreviewMode = false;
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

            var that = this;

            this.$rootScope.$on(JavascriptEventConstants.FormDoubleClicked, (aEvent, mouseEvent, layoutDispatch) => {
                const cs = crudContextHolderService.currentSchema();
                const dm = crudContextHolderService.rootDataMap();
                if (cs.properties["dynforms.editionallowed"] !== "true") {
                    //not on a edition schema mode
                    return;
                }
                const fields = schemaService.allNonHiddenDisplayables(dm, cs);
                const lastField = fields[fields.length - 1];
                that.addDisplayable(lastField, 'down', true).then(r => {
                    $rootScope.$broadcast(JavascriptEventConstants.ReevalDisplayables);
                });

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
            return this.schemaCacheService.fetchSchema(info.app, info.schemaid);
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





            return displayable;
        }

        addDisplayable(currentField, direction, showposition = false) {
            var that = this;
            const schema = this.crudContextHolderService.currentSchema();
            const rootDm = this.crudContextHolderService.rootDataMap();
            return this.schemaCacheService.fetchSchema("_FormMetadata", "fieldEditModal").then(schema => {
                let dm = {}
                if (showposition) {
                    dm['showposition'] = showposition;
                    dm['refposition'] = 'down';
                    dm['reffield'] = currentField.role;
                }

                return that.modalService.showPromise(schema, dm, {
                    cssclass: 'largemodal', onloadfn: () => {
                        this.$timeout(() => {
                            this.$rootScope.$broadcast("dynform.checklist.loaddata");
                        },100,false);

                    }
                });
            }).then(savedData => {
                if (savedData['reffield']) {
                    const selectedField = this.fieldService.getDisplayableByKey(schema, savedData['reffield']);
                    if (!selectedField) {
                        return this.alertService.alert(`Field ${savedData['reffield']} not found `);
                    }
                    currentField = selectedField;
                    direction = savedData['refposition'];
                }

                return that.doAddDisplayable(currentField, savedData, direction);
            }).then(resultDisplayable => {
                if (resultDisplayable.type === "OptionField" && resultDisplayable.rendererType === "combo") {
                    const fieldQualifier = resultDisplayable.qualifier;
                    const provider = resultDisplayable.providerAttribute;

                    return that.restService.get("FormMetadata", "EagerLoadOptions", { fieldQualifier }).then(httpResult => {
                        that.crudContextHolderService.updateEagerAssociationOptions(provider, httpResult.data, null, null);
                    });
                }
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

            let fieldType = modalData.fieldtype;
            let rendererType = "default";
            if (fieldType.indexOf("#") !== -1) {
                const types = fieldType.split("#");
                fieldType = types[0];
                rendererType = types[1];
            }

            if (fieldType === "checklisttable") {
                const tableMetadata = this.checkListTableBuilderService.createTable(modalData);
                this.$rootScope.$broadcast("dynform.checklist.onsavemodal", modalData, tableMetadata);
                return tableMetadata;
            }

            const rendererParameters = {};

            if (!modalData.fcheckontop && rendererType === "checkbox") {
                rendererParameters["layout"] = "left";
            }

            if (fieldType === "OptionField") {
                rendererType = modalData["ofrenderer"];
            }

            const resultOb = {
                //has to be first field, until we´re able to migrate to newtonsoft 10.0.0 and use https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_MetadataPropertyHandling.htm
                "$type": `softwrench.sW4.Shared2.Metadata.Applications.Schema.${fieldType}, softwrench.sw4.Shared2`,
                attribute: modalData.fattribute,
                role: modalData.fattribute,
                label: modalData.flabel,
                requiredExpression: modalData.frequired ? "true" : "false",
                isReadOnly: modalData.freadonly,
                extraparameters: { "dynforms.editionallowed": true },
                renderer: {
                    rendererType,
                    parameters: rendererParameters
                },
                rendererType,
                rendererParameters: rendererParameters,
                showExpression: "true",
                enableExpression: modalData.freadonly ? "false" : "true",
                "type": fieldType
            };

            if (fieldType === "OptionField") {
                resultOb.qualifier = modalData.fprovider;
                resultOb.providerAttribute = "formMetadataOptionsProvider.GetAvailableOptions";
            }

            return resultOb;
        }

        editDisplayable(fieldMetadata) {
            var that = this;
            const convertedDatamap = {
                fieldtype: fieldMetadata.type,
                fattribute: fieldMetadata.attribute,
                flabel: fieldMetadata.label,
                frequired: fieldMetadata.requiredExpression === "true",
                freadonly: !!fieldMetadata.isReadOnly,
                "#isEditing": true
            }
            if (fieldMetadata.type === "TableDefinition") {
                //TODO: allow other kinds of table
                convertedDatamap.fieldtype = "checklisttable";
                convertedDatamap.optionsLabel = fieldMetadata.headers[2];
                convertedDatamap["#checklistrows"] =
                    this.checkListTableBuilderService.convertRowsIntoArray(fieldMetadata);
            } else if (fieldMetadata.rendererType === "checkbox") {
                convertedDatamap.fieldtype += "#checkbox";
                convertedDatamap.fcheckontop = "left" !== fieldMetadata.rendererParameters["layout"];
            } else if (fieldMetadata.rendererType === "label") {
                convertedDatamap.fieldtype += "#label";
            }


            return this.schemaCacheService.fetchSchema("_FormMetadata", "fieldEditModal").then(schema => {
                return that.modalService.showPromise(schema, convertedDatamap, {
                    cssclass: 'largemodal', onloadfn: () => {
                        this.$rootScope.$broadcast("dynform.checklist.loaddata");
                    }
                });
            }).then(savedData => {
                if (savedData.fattribute.indexOf(' ') >= 0) {
                    this.alertService.alert("Attribute names cannot contain spaces");
                }

                return that.doAddDisplayable(fieldMetadata, savedData, "edit");
            });
        }

        afterChangeType(event) {
            const dm = this.crudContextHolderService.rootDataMap("#modal");
            if (dm.fieldtype === "checklisttable") {
                this.$rootScope.$broadcast("dynform.checklist.loaddata");
            }

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
            return this.alertService.confirm(`Are you sure you want to remove field ${key}`).then(() => {
                return this.fieldService.replaceOrRemoveDisplayableByKey(cs, key);
            });
        }


        loadFormDetailEdition() {
            const id = this.crudContextHolderService.rootDataMap()["name"];
            return this.redirectService.goToApplication("_FormMetadata", "newformbuilder", { id });
        }

        clone() {
            const dm = this.crudContextHolderService.rootDataMap();
            const originalId = dm["name"];
            return this.getFormSchema(cloneModalInfo).then((schema) => {
                const clonedDm = {
                    name: originalId + "_clone",
                    formtitle: dm["formtitle"] + "_clone",
                    formstatus: 'Draft'
                }
                return this.modalService.showPromise(schema, clonedDm);
            }).then(modalData => {
                const id = modalData["name"];
                modalData["#originalid"] = originalId;
                //TODO: remove the need for this dispatchedByModal:false flag, but without it the real method isn´t invoked at the submitservice.js
                return this.applicationService.save({ datamap: modalData, operation: "clone", dispatchedByModal: false }).then(r => {
                    return this.applicationService.loadItem({ id });
                });
            });
        }

        publish() {
            const dm = this.crudContextHolderService.rootDataMap();
            dm["formstatus"] = "Published";
            return this.applicationService.save({ datamap: dm, operation: "crud_update" });
        }

        remove() {

            this.alertService.confirm("Are you sure you want to delete this form").then(r => {
                return this.applicationService.save({ operation: "crud_delete" });
            })


        }


        saveDetailForm() {
            const extraparameters = {
                "realschema": "newformbuilder"
            }
            const dm = this.crudContextHolderService.rootDataMap();
            const cs = this.crudContextHolderService.currentSchema();
            //preserving first 2 items, which are the blank element section and the id
            //.net cannot convert flawlessly these items, so we´ll pass them as a JSON and use a custom deserialization process
            //            const newFields = cs.displayables.slice(2);
            const newFields = cs.displayables;
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
            if (datamap["#isEditing"]) {
                return true;
            }

            const cs = this.crudContextHolderService.currentSchema();
            const displ = this.fieldService.getDisplayableByKey(cs, newAttribute);
            if (displ) {
                this.alertService.alert(`attribute ${newAttribute} already present at the form. No duplicates allowed`);
                return false;
            }

        }

        applyPostValidationRules(schema, datamap, parameters) {
            if (datamap.fieldtype === "checklisttable") {
                this.$rootScope.$broadcast("dynform.checklist.onsavemodal");
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

        isPreviewMode() {
            return isPreviewMode;
        }

        setPreviewMode(previewMode) {
            isPreviewMode = "true"===previewMode;
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


    dynFormService["$inject"] = ["$q", "$timeout", "$rootScope", "schemaCacheService", "restService", "modalService", "redirectService", "applicationService", "crudContextHolderService", "fieldService", "alertService", "schemaService", "associationService", "checkListTableBuilderService"];

    angular.module("sw_layout").service("dynFormService", dynFormService);

})(angular);