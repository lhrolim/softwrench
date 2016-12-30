﻿describe('Composition List Test', function () {

    var mockScope;
    var validationService;
    var $rootScope;
    var modalService;
    var $q;
    var crudContextHolderService;
    var redirectService;
    var applicationService;
    var crud_inputcommons;

    //init app --> first action usually
    beforeEach(() => {


        angular.mock.module('sw_layout');
        angular.mock.inject(function (_$rootScope_, $controller, _validationService_, _modalService_, _$q_, _crudContextHolderService_, _redirectService_, _applicationService_, _crud_inputcommons_) {
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

            mockScope.relationship = "worklog_";
            mockScope.compositiondata = [];
            mockScope.compositionschemadefinition = new ApplicationCompositionSchemaDTO(new CompositionSchemas(SchemaPojo.CompositionListSchema(), SchemaPojo.CompositionDetailSchema()));

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
        const datamap = {};

        spyOn(validationService, "validatePromise").and.callThrough();
        spyOn(redirectService, "redirectToTab").and.returnValue($q.when());
        spyOn(modalService, "showPromise").and.returnValue($q.when());

        //setting parent data
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
        const rootDatamap = { "attr1": "x" };

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

        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
        const serverSideDefer = $q.defer();
        const rootDatamap = { "attr1": "x" };

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
        crudContextHolderService.rootDataMap(null, rootDatamap);
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
        mockScope.compositiondata = [{ "multiid": 10, siteid: "BEDFORD", "assetnum": "100", "location": "LOC_1", isprimary : true}, { "multiid": 11, siteid: "BEDFORD", "assetnum": "101", "location": "LOC_1", isprimary: false }];
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
   


});