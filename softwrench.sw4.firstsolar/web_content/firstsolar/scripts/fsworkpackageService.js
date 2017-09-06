(function (angular, bootbox) {
    "use strict";

    let handleEngComponentSection, locatePreferredSectionIdx, generateOuterSection, generateTestSection, generateInlineFileComposition, generateInlineWorklogComposition;
    let worklogCompositionSchema, fileExplorerCompositionSchema, redimensionIntermediateSectionsInternalAdding, redimensionIntermediateSectionsInternalRemoving;
    let worklogKey, attachmentKey, testWithWorklogs, testWithAttachments, testsMap, wipeDynamicSections;

    const maxColumns = 1;

    class workPackageService {

        constructor($rootScope, $q, $log, alertService, applicationService, crudContextHolderService, redirectService, fieldService) {
            this.$rootScope = $rootScope;
            this.$q = $q;
            this.$log = $log;
            this.alertService = alertService;
            this.applicationService = applicationService;
            this.crudContextHolderService = crudContextHolderService;
            this.redirectService = redirectService;
            this.fieldService = fieldService;

            this.$rootScope.$on("sw.crud.body.crawlocurred", () => {
                wipeDynamicSections();
            });


            testsMap = {
                "GSU": ["gsuimmediatetests", "gsutests"],
                "SF6": ["sf6tests"],
                "VACUUM": ["vacuumtests"],
                "AIRS": ["airswitchertests"],
                "CAPB": ["capbanktests"],
                "BATTERY": ["batterytests"],
                "FEEDER": ["feedertests"],
                "RELAY": ["relaytests"]
            }

            //#region private functions
            worklogKey = (baseKey) => `#${baseKey}worklogs_`;
            attachmentKey = (baseKey) => `#${baseKey}fileexplorer_`;

            handleEngComponentSection = function (log, option, componentsTopSection, selecting) {
                if (selecting) {
                    //noop, since the component checkboxes are calculated via showexpressions, and no extra sections need to be rendered yet
                    return;
                }
                const sectionIdx = fieldService.getVisibleDisplayableIdxByKey(componentsTopSection, option.value.toLowerCase() + "section", true, true);
                if (sectionIdx !== -1) {
                    log.debug(`removing section for component ${option.label}`);
                    componentsTopSection.displayables.splice(sectionIdx, 1);
                }
            }

            wipeDynamicSections = function () {
                const componentsTopSection = fieldService.getDisplayableByKey(crudContextHolderService.currentSchema(), "components");
                if (componentsTopSection) {
                    //might be null due to security policies
                    componentsTopSection.displayables = [];
                }


            }


            locatePreferredSectionIdx = function (schema, componentsTopSection, selectedValue, optionField = null) {
                const engComponentsField = optionField ? optionField : fieldService.getDisplayableByKey(schema, "engcomponents");
                const options = engComponentsField.options;
                let preferredIdx = 0;
                for (let i = 0; i < options.length; i++) {
                    const option = options[i];
                    if (option.value.toUpperCase() === selectedValue.toUpperCase()) {
                        preferredIdx = i;
                        break;
                    }
                }

                if (!!optionField && optionField.attribute === 'gsutests') {
                    preferredIdx += 3;
                }

                let idxToInsert = 0;

                if (componentsTopSection.id === "components") {
                    //for the components outer section, there´ll be only vertical inner sections created, thus we locate the right one by iterating on each of its sections only

                    componentsTopSection.displayables.forEach(existingSection => {
                        if (existingSection.preferredIdx < preferredIdx) {
                            idxToInsert++;
                        }
                    });
                } else {
                    //each internal component section should have intermediate vertical sections <s1> , <s2> ... <sn>
                    //in order to keep the horizontal layout smooth, and a maximum of 4 horizontal sections, therefore we need to take that into account

                    for (let i = 0; i < componentsTopSection.displayables.length; i++) {
                        const intermediateSection = componentsTopSection.displayables[i];
                        intermediateSection.displayables.forEach(existingSection => {
                            if (existingSection.preferredIdx < preferredIdx) {
                                idxToInsert++;
                            } else {
                                return;
                            }
                        });


                    }


                }

                return { preferredIdx, idxToInsert }
            }




            generateOuterSection = function (schema, componentName, preferredIdx) {
                const engComponentsField = fieldService.getDisplayableByKey(schema, "engcomponents");
                const options = engComponentsField.options;
                let label = null;
                for (let i = 0; i < options.length; i++) {
                    const option = options[i];
                    if (option.value.toUpperCase() === componentName.toUpperCase()) {
                        label = option.label;
                        break;
                    }
                }


                //each intermediate section should hold only a maximum of 4 tests
                const intermediateSection = {
                    type: "ApplicationSection",
                    orientation: "horizontal",
                    attribute: componentName + "section1",
                    displayables: [],
                    rendererParameters: {},
                    showExpression: 'true',

                };

                //                const intermediateSection2 = angular.copy(intermediateSection1);
                //                intermediateSection2.attribute = componentName + "section2";
                //
                //                const intermediateSection3 = angular.copy(intermediateSection1);
                //                intermediateSection3.attribute = componentName + "section3";

                const resultSection = {
                    attribute: componentName + "section",
                    orientation: "vertical",
                    rendererParameters: {
                        //                        class: "borderedsection"
                    },
                    displayables: [
                        intermediateSection
                    ],
                    showExpression: 'true',
                    type: "ApplicationSection",
                    preferredIdx: preferredIdx,
                    //                    header: {
                    //                        label,
                    //                        displacement: "sameline",
                    //                        showExpression: "true",
                    //                        parameters: {
                    //                            fieldset: "true"
                    //                        }
                    //                    }
                };

                return resultSection;
            }

            generateTestSection = function (outerSectionName, selectedTest, preferredIdx) {

                const resultSection = {
                    id: outerSectionName + selectedTest.value + "section",
                    attribute: outerSectionName + selectedTest.value + "section",
                    rendererParameters: {
                        class: "borderedsection"
                    },
                    displayables: [],
                    type: "ApplicationSection",
                    showExpression: 'true',
                    orientation: "vertical",
                    preferredIdx: preferredIdx,
                    header: {
                        label: outerSectionName.toUpperCase() + " - " + selectedTest.label,
                        displacement: "sameline",
                        showExpression: "true",
                        parameters: {
                            fieldset: "true"
                        }
                    }
                };

                resultSection.displayables[0] = generateInlineFileComposition(selectedTest);
                resultSection.displayables[1] = generateInlineWorklogComposition(selectedTest);

                return resultSection;
            }


            generateInlineWorklogComposition = function (selectedTest) {

                //                <composition inline="true" relationship="#relayeventevaluations_" label="Engineering Evaluation - Event File" detailschema="worklog.workpackageview" printschema="">
                //                    <collectionproperties listschema="worklog.workpackagelist" autocommit="true" allowremoval="false" allowupdate="false" allowcreation="false" />
                //                    <!--<renderer type="TABLE" params="mode=batch;composition.inline.expandreadonly=true;composition.inline.addfunction=fsengineeringevaluationService.openModalNew;composition.inline.editfunction=fsengineeringevaluationService.openModalEdit;composition.inline.forcehideremove=true" />-->
                //                    <renderer type="TABLE" params="mode=batch;composition.inline.expandreadonly=true;composition.inline.addfunction=fsengineeringevaluationService.openModalNew;composition.inline.forcehideremove=true" />
                //                    </composition>
                const worklogComposition = {
                    type: "ApplicationCompositionDefinition",
                    inline: true,
                    relationship: worklogKey(selectedTest.value),
                    //                    label: "Engineering Evaluation " + selectedTest.label,
                    detailschema: 'worklog.workpackageview',
                    printschema: '',
                    showExpression: 'true',
                    schema: worklogCompositionSchema,
                    collection: true,
                    renderer: {
                        type: 'TABLE',
                        params: {
                            mode: "batch",
                            "composition.inline.expandreadonly": true,
                            "composition.inline.addfunction": "fsengineeringevaluationService.openModalNew",
                            "composition.inline.forcehideremove": true,
                            "composition.inline.avoidheader": true
                        }
                    },
                    collectionproperties: {
                        listschema: "worklog.workpackagelist",
                        autocommit: true,
                        allowremoval: false,
                        allowupdate: false,
                        allowcreation: "true"
                    },
                    rendererType: 'TABLE'
                }
                worklogComposition.schema.rendererParameters["composition.inline.avoidheader"] = "true";
                return worklogComposition;
            }

            generateInlineFileComposition = function (selectedTest) {

                //                <composition inline="true" relationship="#relayeventfiles" label="Relay Event Files" detailschema="">
                //                    <renderer type="fileexplorer" params="acceptedFileExtensions=sw_all_types" />
                //                    </composition>

                return {
                    type: "ApplicationCompositionDefinition",
                    inline: true,
                    relationship: attachmentKey(selectedTest.value),
                    //                    label: selectedTest.label,
                    schema: fileExplorerCompositionSchema,
                    showExpression: 'true',
                    collection: true,
                    renderer: {
                        type: 'fileexplorer',
                        params: {
                            acceptedFileExtensions: "sw_all_types",
                            deletefunction: "fsworkpackagefilesService.deleteFile"
                        }
                    },
                    rendererType: 'fileexplorer'
                }
            }

            redimensionIntermediateSectionsInternalAdding = function (componentSection, intermediateSectionIdx, intermediateSection, indexToConsider, itemToInsert) {

                const log = $log.get("workpackageservice#redimensionIntermediateSections", ["workpackage"]);

                intermediateSection.displayables.splice(indexToConsider, 0, itemToInsert);
                if (intermediateSection.displayables.length <= maxColumns) {
                    return;
                }
                //picking last index, and removing it from current section, since it will be overflown to the next
                const itemToOverFlow = intermediateSection.displayables[maxColumns];
                intermediateSection.displayables.splice(maxColumns, 1);

                if (intermediateSectionIdx + 1 >= componentSection.displayables.length) {
                    //if there was no next section, just create it and insert at the beginning
                    log.debug("generating fresh intermediate section");
                    intermediateSection = angular.copy(intermediateSection);
                    intermediateSection.displayables = [];
                    componentSection.displayables.push(intermediateSection);
                    intermediateSection.displayables.push(itemToOverFlow);
                } else {
                    const nextSection = componentSection.displayables[intermediateSectionIdx + 1];
                    redimensionIntermediateSectionsInternalAdding(componentSection, intermediateSectionIdx + 1, nextSection, 0, itemToOverFlow);
                }
            }


            redimensionIntermediateSectionsInternalRemoving = function (componentSection, intermediateSectionIdx, intermediateSection, indexToConsider) {

                const log = $log.get("workpackageservice#redimensionIntermediateSections", ["workpackage"]);

                intermediateSection.displayables.splice(indexToConsider, 1);

                if (intermediateSection.displayables.length === 0) {
                    //removing the intermediate section itself if is not the last one
                    if (componentSection.displayables.length > 1) {
                        componentSection.displayables.splice(intermediateSectionIdx, 1);
                    }
                    return null;
                }

                if (intermediateSectionIdx === componentSection.displayables.length - 1 || componentSection.displayables[intermediateSectionIdx + 1].displayables.length === 0) {
                    //either if we were on the last section, or the next section has no elements
                    log.trace("no need to redimension next sections");
                    return null;
                }



                //picking last index, and removing it from current section, since it will be overflown to the next
                const nextSection = componentSection.displayables[intermediateSectionIdx + 1];
                const firstItemofNext = nextSection.displayables[0];
                redimensionIntermediateSectionsInternalRemoving(componentSection, intermediateSectionIdx + 1, nextSection, 0);
                intermediateSection.displayables.push(firstItemofNext);
            }

            testWithWorklogs = function (dm, baseKey) {
                const worklog = worklogKey(baseKey);
                return dm[worklog] && dm[worklog].length > 0;
            }

            testWithAttachments = function (dm, baseKey) {
                const attach = attachmentKey(baseKey);
                return dm[attach] && dm[attach].length > 0;
            }

            //#endregion


        }

        redimensionIntermediateSections(componentSection, indexToConsider, itemToInsert) {
            const log = this.$log.get("workpackageservice#redimensionIntermediateSections", ["workpackage"]);
            const deletion = !itemToInsert;
            const intermediateSectionIdx = Math.floor(indexToConsider / maxColumns);
            indexToConsider = indexToConsider % maxColumns;


            let intermediateSection;

            if (intermediateSectionIdx < componentSection.displayables.length) {
                intermediateSection = componentSection.displayables[intermediateSectionIdx];
            } else {
                log.debug("generating fresh intermediate section");
                const originalintermediate = componentSection.displayables[intermediateSectionIdx - 1];
                intermediateSection = angular.copy(originalintermediate);
                intermediateSection.displayables = [];
                componentSection.displayables.push(intermediateSection);
            }

            if (!deletion) {
                redimensionIntermediateSectionsInternalAdding(componentSection, intermediateSectionIdx, intermediateSection, indexToConsider, itemToInsert);
            } else {
                redimensionIntermediateSectionsInternalRemoving(componentSection, intermediateSectionIdx, intermediateSection, indexToConsider);
            }
            return componentSection;
        }


        //afterchange
        onWorkorderSelected(parameters) {
            const workorderid = parameters.fields["workorder_.workorderid"];
            const currentDatamap = this.crudContextHolderService.rootDataMap();
            currentDatamap["workorderid"] = workorderid;
            if (workorderid == null) {
                //cleaning up
                Object.keys(currentDatamap).filter(f => f.startsWith("#workorder_")).forEach(k => {
                    delete currentDatamap[k];
                });
                delete currentDatamap["wpnum"];
                return null;
            }
            return this.applicationService.getApplicationDataPromise("workorder", "workpackageschema", { id: workorderid }).then(result => {
                var resultWo = result.data.resultObject;
                Object.keys(currentDatamap).forEach(k => {
                    if (k.startsWith("#workorder_.")) {
                        currentDatamap[k] = null;
                    }
                });
                Object.keys(resultWo).forEach(k => {
                    currentDatamap["#workorder_." + k] = resultWo[k];
                });
                const wonum = resultWo["wonum"];
                currentDatamap["wpnum"] = wonum && wonum.startsWith("NA") ? "WP" + wonum.substring(2) : wonum;
            });
        }

        innerReevaluateSections(field, option) {
            const log = this.$log.get("workpackageservice#reevaluateSections", ["workpackage"]);
            const dm = this.crudContextHolderService.rootDataMap();
            const schema = this.crudContextHolderService.currentSchema();
            const selectedValue = dm[field.attribute];
            const selecting = !!selectedValue && selectedValue.indexOf(option.value) !== -1;

            const componentsTopSection = this.fieldService.getDisplayableByKey(schema, "components");

            if (field.attribute === "engcomponents") {
                return handleEngComponentSection(log, option, componentsTopSection, selecting);
            }

            const outerSectionName = field.qualifier;
            const sectionIdx = componentsTopSection.displayables.findIndex(s => s.attribute === outerSectionName.toLowerCase() + "section");
            let outerSection = sectionIdx === -1 ? null : componentsTopSection.displayables[sectionIdx];
            if (selecting) {
                if (!outerSection) {
                    const idxData = locatePreferredSectionIdx(schema, componentsTopSection, outerSectionName);
                    //outer section didn´t exist up to this point --> first test selected for a given component
                    outerSection = generateOuterSection(schema, outerSectionName, idxData.preferredIdx);
                    const idxToInsert = idxData.idxToInsert;
                    componentsTopSection.displayables.splice(idxToInsert, 0, outerSection);
                    log.debug(`adding missing outer component section for ${outerSectionName} at position ${idxToInsert}`);
                }
                const idxData = locatePreferredSectionIdx(schema, outerSection, option.value, field);
                const generatedTestSection = generateTestSection(outerSectionName, option, idxData.preferredIdx);
                const idxToInsertInternalSection = idxData.idxToInsert;
                log.debug(`adding missing internal test section ${outerSectionName}${option.value} at position ${idxToInsertInternalSection} for ${outerSectionName} `);
                //                outerSection.displayables.splice(idxToInsertInternalSection,0,generatedTestSection);
                this.redimensionIntermediateSections(outerSection, idxToInsertInternalSection, generatedTestSection);

            } else {
                if (!outerSection) {
                    //shouldn´t happen, as we shouldn´t be able to unselect a test if the outer section is not yet available
                    return null;
                }

                let internalSectionIdx = 0;
                let found = false;

                for (let i = 0; i < outerSection.displayables.length; i++) {
                    if (found) {
                        break;
                    }
                    const inSection = outerSection.displayables[i];
                    for (let j = 0; j < inSection.displayables.length; j++) {
                        const section = inSection.displayables[j];
                        if (section.attribute === (outerSectionName + option.value + "section")) {
                            found = true;
                            break;
                        }
                        internalSectionIdx++;
                    }
                }

                log.debug(`removing internal test section ${option.value} at position ${internalSectionIdx} for ${outerSectionName} `);
                this.redimensionIntermediateSections(outerSection, internalSectionIdx);
                //                outerSection.displayables.splice(internalSectionIdx, 1);
            }
        }

        showOtherTestModal() {
            const deferred = this.$q.defer();

            var saveFormSt = $("#othertestsform").prop("outerHTML");
            saveFormSt = saveFormSt.replace("none", "");
            //change id of the filter so that it becomes reacheable via jquery
            saveFormSt = saveFormSt.replace("othertestsname", "othertestsname2");
            bootbox.dialog({
                templates: {
                    header:
                    "<div class='modal-header'>" +
                    "<i class='fa fa-question-circle'></i>" +
                    "<h4 class='modal-title'></h4>" +
                    "</div>"
                },
                message: saveFormSt,
                title: "Save Test",
                onEscape: true,
                buttons: {
                    cancel: {
                        label: 'cancel',
                        className: "btn btn-default",
                        callback: function () {
                            deferred.reject();
                        }
                    },
                    main: {
                        label: "Save",
                        className: "btn-primary",
                        callback: function (result) {
                            if (result) {
                                deferred.resolve($("#othertestsname2").val());
                            }
                        }
                    }
                },
                className: "smallmodal"
            });

            return deferred.promise;
        }

        addNewOption(field, newtest) {

            var existingOptions = field.options;
            if (existingOptions.some(o => o.value === newtest)) {
                this.alertService.alert(`Test "${newtest}" already exists.`);
                return null;
            }
            const newOption = { label: newtest, value: newtest };
            var indexToInsert = existingOptions.length >= 3 ? existingOptions.length - 3 : 0;
            existingOptions.splice(indexToInsert, 0, newOption);
            if (!!field.jscache) {
                delete field.jscache.grouppedcheckboxes;
            }


            return newOption;
        }

        //afterchange
        reevaluateSections(eventParameters) {
            const dm = this.crudContextHolderService.rootDataMap();
            const field = eventParameters.fieldMetadata;

            const removeOtherTest = (field) => {
                var idx = dm[field.attribute].findIndex(f => f.endsWith("othertest"));
                if (idx !== -1) {
                    dm[field.attribute].splice(idx, 1);
                }
            }

            const markasSelected = (field) => {
                var idx = dm[field.attribute].findIndex(f => f.endsWith("othertest"));
                if (idx !== -1) {
                    dm[field.attribute].splice(idx, 1);
                }
            }

            
            const option = eventParameters.option;
            if (option.value.endsWith("othertest")) {
                return this.showOtherTestModal()
                    .then(label => {
                        removeOtherTest(field);
                        dm[field.attribute].push(label);
                        const fakeOption = this.addNewOption(field, label);
                        if (!!fakeOption) {
                            this.innerReevaluateSections(field, fakeOption);
                        }
                    }).catch(() => {
                        removeOtherTest(field);
                    });
            }


            this.innerReevaluateSections(field, option);
            if (field.attribute !== "gsuimmediatetests") {
                return;
            }

            let pairOption = null;
            if (option.value === "gsucaptureoil") {
                pairOption = field.options.find((testOption) => testOption.value === "gsucapturemon");
            }
            if (option.value === "gsucapturemon") {
                pairOption = field.options.find((testOption) => testOption.value === "gsucaptureoil");
            }
            if (pairOption) {
                this.innerReevaluateSections(field, pairOption);
            }
        }

        testLoad(schema, datamap, test, component) {
            const values = datamap[test] || [];
            const fieldMetadata = this.fieldService.getDisplayableByKey(schema, test);
            let gsucaptureoilLoaded = false;
            let gsucapturemonLoaded = false;

            const load = (option) => {
                if (option.value === "gsucaptureoil") {
                    gsucaptureoilLoaded = true;
                }
                if (option.value === "gsucapturemon") {
                    gsucapturemonLoaded = true;
                }
                this.innerReevaluateSections(fieldMetadata, option);
            }

            

            values.forEach(val => {
                if (!fieldMetadata.options.some(o => o.value === val)) {
                    const existingOptions = fieldMetadata.options;
                    const newOption = { label: val, value: val };
                    var indexToInsert = existingOptions.length >= 3 ? existingOptions.length - 3 : 0;
                    existingOptions.splice(indexToInsert, 0, newOption);
                }
            });

            angular.forEach(fieldMetadata.options, (option) => {
                let onDatamap = false;
                angular.forEach(values, (value) => {
                    if (option.value === value) {
                        onDatamap = true;
                    }
                });

                // already on datamap just load the section
                if (onDatamap) {
                    load(option);
                    return;
                }

                const hasWorklogs = testWithWorklogs(datamap, option.value);
                const hasAttachs = testWithAttachments(datamap, option.value);
                if (!hasWorklogs && !hasAttachs) {
                    return;
                }

                // not on datamap but with evaluations or files
                // pobably the user added compositions to the wo but did not save the package
                // add the test to datamap and the component if needed
                if (!datamap[test]) {
                    datamap[test] = [];
                }
                datamap[test].push(option.value);

                if (!datamap["engcomponents"]) {
                    datamap["engcomponents"] = [];
                }
                let componentOnDm = false;
                angular.forEach(datamap["engcomponents"], (dmComponent) => {
                    if (dmComponent === component) {
                        componentOnDm = true;
                    }
                });
                if (!componentOnDm) {
                    datamap["engcomponents"].push(component);
                }

                load(option);
            });

            

            if (gsucaptureoilLoaded && !gsucapturemonLoaded) {
                datamap[test].push("gsucapturemon");
                this.innerReevaluateSections(fieldMetadata, fieldMetadata.options.find((testOption) => testOption.value === "gsucapturemon"));
            } else if (gsucapturemonLoaded && !gsucaptureoilLoaded) {
                datamap[test].push("gsucaptureoil");
                this.innerReevaluateSections(fieldMetadata, fieldMetadata.options.find((testOption) => testOption.value === "gsucaptureoil"));
            }
        }

        //beforechange
        checkSectionRefresh(event) {
            const field = event.fieldMetadata;
            const option = event.option;

            const dm = this.crudContextHolderService.rootDataMap();
            let selectedValue = dm[field.attribute];
            const selecting = selectedValue == undefined || (!!selectedValue && selectedValue.indexOf(option.value) === -1);


            if (selecting) {
                if (field.attribute !== "gsuimmediatetests") {
                    return this.$q.when();
                }
                if (!selectedValue) {
                    dm[field.attribute] = [];
                    selectedValue = dm[field.attribute];
                }
                if (option.value === "gsucaptureoil" && selectedValue.indexOf("gsucapturemon") === -1) {
                    selectedValue.push("gsucapturemon");
                }
                if (option.value === "gsucapturemon" && selectedValue.indexOf("gsucaptureoil") === -1) {
                    selectedValue.push("gsucaptureoil");
                }
                return this.$q.when();
            }

            const unselectTest = (testOption) => {
                const hasWorklogs = testWithWorklogs(dm, testOption.value);
                const hasAttachs = testWithAttachments(dm, testOption.label);
                if (!hasWorklogs && !hasAttachs) {
                    return this.$q.when();
                }
                const msg = `The test ${testOption.label} has ${hasWorklogs && hasAttachs ? "evaluations and related files" : (hasWorklogs ? "evaluations" : "related files")} and cannot be removed.`;
                this.alertService.alert(msg);
                return this.$q.reject();
            }

            if ("engcomponents" !== field.attribute) {
                return unselectTest(option).then(() => {
                    let pairOption = null;
                    if (option.value === "gsucaptureoil") {
                        pairOption = field.options.find((testOption) => testOption.value === "gsucapturemon");
                    }
                    if (option.value === "gsucapturemon") {
                        pairOption = field.options.find((testOption) => testOption.value === "gsucaptureoil");
                    }
                    if (pairOption) {
                        return unselectTest(pairOption).then(() => {
                            const idx = selectedValue.indexOf(pairOption.value);
                            if (idx >= 0) {
                                selectedValue.splice(selectedValue.indexOf(pairOption.value), 1);
                            }
                        });
                    }
                });
            }

            const invalid = testsMap[option.value].some((test) => {
                return dm[test] && dm[test].length > 0;
            });

            if (!invalid) {
                return this.$q.when();
            }

            this.alertService.alert(`The component ${option.label} has selected tests and cannot be removed.`);
            return this.$q.reject();
        }


        // onload
        onSchemaLoad(parameters) {
            const log = this.$log.get("workpackageservice#onSchemaLoad", ["workpackage"]);
            const rootSchema = this.crudContextHolderService.currentSchema();
            const datamap = this.crudContextHolderService.rootDataMap();
            const fileExplorerComp = this.fieldService.getDisplayableByKey(rootSchema, "#relayeventfileexplorer_");
            const worklogComp = this.fieldService.getDisplayableByKey(rootSchema, "#relayeventevaluations_");

            const dailyOutageMeeting = datamap["dailyOutageMeetings_"];
            const dailyOutageMeetingCount = dailyOutageMeeting ? dailyOutageMeeting.length : 0;
            this.crudContextHolderService.setTabRecordCount("dailyoutage", null, dailyOutageMeetingCount);

            if (worklogComp != null) {
                //might be null due to security policies
                worklogCompositionSchema = worklogComp.schema;
            }

            if (fileExplorerComp != null) {
                fileExplorerCompositionSchema = fileExplorerComp.schema;
            }

            if (rootSchema.id === "newdetail" && datamap["workorderid"]) {
                this.onWorkorderSelected({ fields: { "workorder_.workorderid": datamap["workorderid"] } });
            }


            log.debug("caching composition schemas");

            //to correct SWWEB-3012
            wipeDynamicSections();

            angular.forEach(testsMap, (tests, component) => {
                angular.forEach(tests, (test) => this.testLoad(rootSchema, datamap, test, component));
            });

        }






        //#region Public methods
        gridClick(datamap, fieldMap, gridSchema) {
            this.redirectService.goToApplication("_WorkPackage", "newdetail");
        }




    }

    workPackageService.$inject = ['$rootScope', '$q', '$log', 'alertService', 'applicationService', 'crudContextHolderService', 'redirectService', 'fieldService'];

    angular.module('sw_layout').service('fsworkpackageService', workPackageService);

})(angular, bootbox);