describe('Composition List Test', function () {

    var mockScope;
    var validationService;
    var $rootScope;
    var modalService;
    var $q;
    var crudContextHolderService;
    var redirectService;
    var applicationService;
    var crud_inputcommons;
    var compositionService;
    var compositionListViewModel;
    var commandService;

    var $httpBackend;

    //init app --> first action usually
    beforeEach(() => {


        angular.mock.module('sw_layout');
        angular.mock.inject(function (_$rootScope_, $controller, _validationService_, _modalService_, _$q_,
            _crudContextHolderService_, _redirectService_, _applicationService_, _crud_inputcommons_, _compositionService_, _compositionListViewModel_, _$httpBackend_, _commandService_) {
            $rootScope = _$rootScope_;
            mockScope = $rootScope.$new();
            const element = angular.element('<div></div>');
            validationService = _validationService_;
            crudContextHolderService = _crudContextHolderService_;
            $q = _$q_;
            redirectService = _redirectService_;
            modalService = _modalService_;
            applicationService = _applicationService_;
            crud_inputcommons = _crud_inputcommons_;
            compositionService = _compositionService_;
            compositionListViewModel = _compositionListViewModel_;
            $httpBackend = _$httpBackend_;
            commandService = _commandService_;

            mockScope.relationship = "worklog_";
            mockScope.compositiondata = [];
            mockScope.compositionschemadefinition = new ApplicationCompositionSchemaDTO(new CompositionSchemas(SchemaPojo.CompositionDetailSchema(), SchemaPojo.CompositionListSchema()));

            const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
            const rootDatamap = { "attr1": "x" };

            $controller('ExtractedCompositionListController', {
                $scope: mockScope,
                $element: element
            });



        });

    });



    /**
     * Steps:
     * 1) Validate parent data before opening the modal
     * 2) If invalid do not open modal
     */
    it("Blank, Non inline, Composition List Add new Item--> parent invalid do not open the modal and redirect to main", done=> {

        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();


        spyOn(validationService, "validatePromise").and.callThrough();
        spyOn(redirectService, "redirectToTab").and.returnValue($q.when());
        spyOn(modalService, "showPromise").and.returnValue($q.when());

        //setting parent data
        const datamap = {};
        crudContextHolderService.rootDataMap(null, datamap);
        crudContextHolderService.currentSchema(null, schemaWithOneRequiredField);

        mockScope.newDetailFn().then(() => {
            expect(true).toBeFalsy();
        }).catch((err) => {
            expect(modalService.showPromise).not.toHaveBeenCalled();
            expect(redirectService.redirectToTab).toHaveBeenCalledWith('main');
            expect(validationService.validatePromise).toHaveBeenCalledWith(schemaWithOneRequiredField, datamap);
        }).finally(done);

        $rootScope.$digest();

    });


    /**
   * Steps:
   * 1) Validate parent data before showing the modal
   * 2) the modal savefunction would call the applicationService.save and validate the composition form. This step is mocked within this test scope
   * 3) If valid open modal and process submission, by calling applicationService.save with the right parameters. This step will be mocked and tested on the submitservice
   *
   */
    it("Blank, Non inline, Composition List Add new Item--> parent valid --> composition invalid --> assure parentdata is cleaned after save", done=> {

        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
        const serverSideDefer = $q.defer();


        const compositionDatamap = {
            _iscreation: true,
        }

        spyOn(validationService, "validatePromise").and.callThrough();
        spyOn(redirectService, "redirectToTab").and.returnValue($q.when());
        spyOn(modalService, "showPromise").and.returnValue($q.when(compositionDatamap));
        //simulating a submission failure, due to any reason
        spyOn(applicationService, "save").and.returnValue(serverSideDefer.promise);
        serverSideDefer.reject();

        //setting parent data
        const rootDatamap = { "attr1": "x" };
        crudContextHolderService.rootDataMap(null, rootDatamap);
        crudContextHolderService.currentSchema(null, schemaWithOneRequiredField);
        mockScope.parentdata = rootDatamap;


        mockScope.newDetailFn().then(() => {
            expect(true).toBeFalsy();
        }).catch((err) => {
            expect(redirectService.redirectToTab).not.toHaveBeenCalledWith('main');
            expect(modalService.showPromise).toHaveBeenCalled();
            expect(validationService.validatePromise).toHaveBeenCalledWith(schemaWithOneRequiredField, rootDatamap);
            expect(applicationService.save).toHaveBeenCalledWith({
                nextSchemaObj: { schemaId: "detail" },
                refresh: false,
                dispatchedByModal: false,
                compositionData: new CompositionOperation("crud_create", "worklog_", { _iscreation: true, "#isDirty": true })
            });
            //assuring that the composition was removed from the datamap in case of failure
            expect(mockScope.parentdata["worklog_"].length).toBe(0);
        }).finally(done);

        $rootScope.$digest();

    });


    /**
 * Steps:
 * 1) Validate parent data before showing the modal
 * 2) the modal savefunction would call the applicationService.save and validate the composition form. This step is mocked within this test scope
 * 3) If valid open modal and process submission, by calling applicationService.save with the right parameters. This step will be mocked and tested on the submitservice
 *
 */
    it("Blank, Non inline, Composition List Add new Item--> parent valid --> composition valid --> assure first save", done=> {


        const serverSideDefer = $q.defer();


        const compositionDatamap = {
            _iscreation: true
        }

        spyOn(validationService, "validatePromise").and.callThrough();
        spyOn(redirectService, "redirectToTab").and.returnValue($q.when());
        spyOn(modalService, "showPromise").and.returnValue($q.when(compositionDatamap));
        //simulating a submission failure, due to any reason
        spyOn(applicationService, "save").and.returnValue(serverSideDefer.promise);
        serverSideDefer.resolve();

        //setting parent data
        const rootDatamap = { "attr1": "x" };
        crudContextHolderService.rootDataMap(null, rootDatamap);
        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
        crudContextHolderService.currentSchema(null, schemaWithOneRequiredField);
        mockScope.parentdata = rootDatamap;


        mockScope.newDetailFn().then(() => {
            expect(redirectService.redirectToTab).not.toHaveBeenCalledWith('main');
            expect(modalService.showPromise).toHaveBeenCalled();
            expect(validationService.validatePromise).toHaveBeenCalledWith(schemaWithOneRequiredField, rootDatamap);
            expect(applicationService.save).toHaveBeenCalledWith({
                nextSchemaObj: { schemaId: "detail" },
                refresh: false,
                dispatchedByModal: false,
                compositionData: new CompositionOperation("crud_create", "worklog_", { _iscreation: true, "#isDirty": true })
            });
            //assuring that the composition was removed from the datamap in case of failure
            expect(mockScope.parentdata["worklog_"].length).toBe(1);

        }).catch((err) => {
            expect(true).toBeFalsy();
        }).finally(done);

        $rootScope.$digest();

    });

    it("Batch-Inline Composition Blank", done=> {

        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
        const rootDatamap = { "attr1": "x" };

        mockScope.relationship = "multiassetlocci_";
        mockScope.compositiondata = [];

        mockScope.compositionschemadefinition = CompositionDefinitionPojo.MultiAssetLocciBase();

        //setting parent data
        crudContextHolderService.rootDataMap(null, rootDatamap);
        crudContextHolderService.currentSchema(null, schemaWithOneRequiredField);

        mockScope.parentdata = rootDatamap;

        spyOn(crud_inputcommons, "configureAssociationChangeEvents");

        mockScope.init();

        expect(crud_inputcommons.configureAssociationChangeEvents).not.toHaveBeenCalled();


        expect(mockScope.compositionData().length).toBe(0);

        $rootScope.$digest();
        done();


    });


    it("Batch-Inline Composition Blank add new item if property says so. " +
        "Check Watchers", done=> {



            mockScope.relationship = "multiassetlocci_";
            mockScope.compositiondata = [];

            mockScope.compositionschemadefinition = CompositionDefinitionPojo.MultiAssetLocciBase("true");

            spyOn(crud_inputcommons, "configureAssociationChangeEvents").and.returnValue([]);

            mockScope.init();

            expect(crud_inputcommons.configureAssociationChangeEvents).toHaveBeenCalled();


            expect(mockScope.compositionData().length).toBe(1);
            const generatedItem = mockScope.compositionData()[0];

            const generatedId = generatedItem.id;
            expect(generatedId).not.toBeNull();
            expect(generatedItem[CompositionConstants.IsDirty]).toBeUndefined();
            expect(generatedItem["#datamaptype"]).toBe('compositionitem');


            expect(generatedId > 0).toBeFalsy();


            $rootScope.$digest();
            done();
        });



    it("Batch-Inline Composition with entries. Add and Remove item", done=> {

        mockScope.relationship = "multiassetlocci_";
        mockScope.compositiondata = [{ "multiid": 10, siteid: "BEDFORD", "assetnum": "100", "location": "LOC_1", isprimary: true }, { "multiid": 11, siteid: "BEDFORD", "assetnum": "101", "location": "LOC_1", isprimary: false }];
        mockScope.compositionschemadefinition = CompositionDefinitionPojo.MultiAssetLocciBase();

        spyOn(crud_inputcommons, "configureAssociationChangeEvents").and.callThrough();

        mockScope.init();



        expect(crud_inputcommons.configureAssociationChangeEvents).toHaveBeenCalled();

        expect(mockScope.compositionData().length).toBe(2);

        mockScope.addBatchItem();

        expect(mockScope.compositionData().length).toBe(3);

        expect(mockScope.compositionData()[2]["#isDirty"]).toBeUndefined();

        mockScope.$digest();

        mockScope.compositionData()[2]["assetnum"] = "1000";
        mockScope.$digest();
        expect(mockScope.compositionData()[2]["#isDirty"]).toBeTruthy();

        mockScope.removeBatchItem(2);

        expect(mockScope.compositionData().length).toBe(2);


        $rootScope.$digest();
        done();


    });

    it("Composition list toggle details (read-only) --> do not show modal", done=> {

        const compositionDatamap = { attr1: 'x', id: '100' };
        const compositionDetailDatamap = { attr1: 'x', detailHidden: "y", id: '100' };

        spyOn(validationService, "validatePromise").and.callThrough();
        spyOn(redirectService, "redirectToTab").and.returnValue($q.when());
        spyOn(modalService, "showPromise").and.returnValue($q.when(compositionDatamap));
        spyOn(compositionService, "getCompositionDetailItem").and.returnValue($q.when({ resultObject: compositionDetailDatamap }));
        spyOn(compositionListViewModel, "doToggle").and.callThrough();

        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();


        //setting parent data
        const datamap = {};
        crudContextHolderService.rootDataMap(null, datamap);
        crudContextHolderService.currentSchema(null, schemaWithOneRequiredField);

        expect(mockScope.detailData["100"]).toBeUndefined();

        mockScope.toggleDetails(compositionDatamap, FieldMetadataPojo.Required("attr1"), "arrow", { stopImmediatePropagation: () => { } }, 0).then(() => {
            //asserting that the main validation of the root datamap wasn´t called, and modal wasn´t show either
            expect(validationService.validatePromise).not.toHaveBeenCalled();
            expect(redirectService.redirectToTab).not.toHaveBeenCalledWith('main');
            expect(modalService.showPromise).not.toHaveBeenCalled();

            expect(compositionService.getCompositionDetailItem).toHaveBeenCalledWith("100", SchemaPojo.CompositionDetailSchema(), {});
            expect(compositionListViewModel.doToggle).toHaveBeenCalled();
            expect(mockScope.detailData["100"]).not.toBeUndefined();

            expect(mockScope.detailData["100"].expanded).toBeTruthy();

        }).then(() => {
            //switching back to non-expanded state
            return mockScope.toggleDetails(compositionDatamap, FieldMetadataPojo.Required("attr1"), "arrow", { stopImmediatePropagation: () => { } }, 0).then(() => {
                expect(mockScope.detailData["100"].expanded).toBeFalsy();
            });
        }).then(() => {
            compositionService.getCompositionDetailItem.calls.reset();
            return mockScope.toggleDetails(compositionDatamap, FieldMetadataPojo.Required("attr1"), "arrow", { stopImmediatePropagation: () => { } }, 0).then(() => {
                expect(compositionService.getCompositionDetailItem).not.toHaveBeenCalled();
                expect(mockScope.detailData["100"].expanded).toBeTruthy();
            });
        }).catch(err => {
            console.log(err);
            expect(false).toBeTruthy();
        }).finally(done);

        $rootScope.$digest();


    });

    it("test toggle details hit with service registered", done => {
        const compositionDatamap = { attr1: 'x', id: '100' };
        const compositionDetailDatamap = { attr1: 'x', detailHidden: "y", id: '100' };

        spyOn(validationService, "validatePromise").and.callThrough();
        spyOn(redirectService, "redirectToTab").and.returnValue($q.when());
        spyOn(commandService, "executeClickCustomCommand").and.returnValue($q.when(true));

        const listSchema = SchemaPojo.CompositionListSchema();
        listSchema.properties["list.click.service"] = "attachmentService.selectAttachment";

        const previousData = mockScope.compositionschemadefinition;



        mockScope.compositionschemadefinition = new ApplicationCompositionSchemaDTO(new CompositionSchemas(SchemaPojo.CompositionDetailSchema(), listSchema));
        mockScope.init();

        //setting parent data
        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
        const datamap = {};
        crudContextHolderService.rootDataMap(null, datamap);
        crudContextHolderService.currentSchema(null, schemaWithOneRequiredField);

        spyOn(compositionService, "getCompositionDetailItem").and.returnValue($q.when({ resultObject: compositionDetailDatamap }));


        mockScope.toggleDetails(compositionDatamap, FieldMetadataPojo.Required("attr1"), "row", { stopImmediatePropagation: () => { } }, 0).then(() => {
            //asserting that the main validation of the root datamap wasn´t called, and modal wasn´t show either
            expect(commandService.executeClickCustomCommand).toHaveBeenCalled();
            expect(compositionService.getCompositionDetailItem).not.toHaveBeenCalled();
        }).catch(e => {
            console.log(e);
            expect(true).toBeFalsy();
        }).finally(r => {
            done();
            mockScope.compositionschemadefinition = previousData;
        });

        $rootScope.$digest();
    });


    it("test expand all", done => {

        //setting parent data
        const rootDatamap = { "attr1": "x", "id": 10 };
        crudContextHolderService.rootDataMap(null, rootDatamap);
        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
        crudContextHolderService.currentSchema(null, schemaWithOneRequiredField);
        mockScope.parentdata = rootDatamap;

        mockScope.relationship = "worklog_";


        mockScope.compositionschemadefinition = new ApplicationCompositionSchemaDTO(new CompositionSchemas(WorklogPojo.DetailSchema(), WorklogPojo.ListSchema()));
        mockScope.compositiondata = [WorklogPojo.ListItem("1", "test1", "100", "sr"), WorklogPojo.ListItem("2", "test2", "100", "sr")];

        const detailCompsResult = { resultObject: { worklog_: [WorklogPojo.DetailItem("1", "test1", "ld 1", "100", "sr"), WorklogPojo.DetailItem("2", "test2", "ld 2", "100", "sr")] } }

        spyOn(compositionListViewModel, "doToggle").and.callThrough();

        mockScope.init();

        $httpBackend.expectGET("/api/generic/Composition/ExpandCompositions?options.printMode=true&options.compositionsToExpand=worklog_%3Dlazy&application=sr&detailRequest.key.schemaId=detail&detailRequest.key.mode=input&detailRequest.key.platform=web&detailRequest.id=10").respond(detailCompsResult);

        mockScope.expandAll().then(() => {
            expect(compositionListViewModel.doToggle).toHaveBeenCalled();
            //called twice
            expect(compositionListViewModel.doToggle.calls.count()).toBe(2);
            expect(mockScope.wasExpandedBefore).toBeTruthy();
        }).catch(err => { console.log(err); })
            .finally(done);


        $httpBackend.flush();

        $rootScope.$digest();
    })



});