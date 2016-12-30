describe('SubmitService Test', function () {


    var submitService;
    var validationService;
    var $rootScope;
    var $q;
    var crudContextHolderService;
    var submitServiceCommons;
    var compositionService;
    var redirectService;
    var schemaCacheService;

    var $httpBackend;


    beforeEach(() => {
        module('sw_layout');
        inject((_submitService_, _$rootScope_, _validationService_, _$q_, _$httpBackend_, _crudContextHolderService_, _submitServiceCommons_, _compositionService_, _redirectService_, _schemaCacheService_) => {
            submitService = _submitService_;
            $rootScope = _$rootScope_;
            validationService = _validationService_;
            $q = _$q_;
            $httpBackend = _$httpBackend_;
            crudContextHolderService = _crudContextHolderService_;
            submitServiceCommons = _submitServiceCommons_;
            compositionService = _compositionService_;
            redirectService = _redirectService_;
            schemaCacheService = _schemaCacheService_;
            

        });
    });





    it('Test Submission invalid data update entry', function (done) {
        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
        const deferred = $q.defer();

        spyOn(validationService, "validatePromise").and.returnValue(deferred.promise);
        deferred.reject(false);

        submitService.submit(schemaWithOneRequiredField, {}).then(() => {
            fail();
        }).catch(reason => {
            expect(reason).toEqual(SubmitResult.ClientValidationFailed);
            //expecting exception to be thrown
            expect(validationService.validatePromise).toHaveBeenCalled();
        }).finally(done);

        $rootScope.$digest();

    });

    it('Test Submission valid data update entry', function (done) {

        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
        const deferred = $q.defer();
        const datamap = { attr1: "x", id: "1" };
        schemaCacheService.addSchemaToCache(SchemaPojo.WithId("editdetail","workorder"));


        spyOn(validationService, "validatePromise").and.returnValue(deferred.promise);
        spyOn(crudContextHolderService, "afterSave");
        spyOn(submitServiceCommons, "applyTransformationsForSubmission").and.callThrough();
        deferred.resolve();

        const submissionData = {
            json: datamap,
            requestData: {
                id: "1",
                applicationName: "sr",
                batch: false,
                compositionData: undefined,
                platform: "web",
                currentSchemaKey: "detail.input.web",
                userId: undefined
            }
        };


        $httpBackend.expectPUT("/api/data/sr/", JSON.stringify(submissionData)).respond(ResponsePojo.CrudUpdateBaseResponse());

        submitService.submit(schemaWithOneRequiredField, datamap).then(() => {
            //expecting exception to be thrown
            expect(validationService.validatePromise).toHaveBeenCalled();
            expect(crudContextHolderService.afterSave).toHaveBeenCalled();
            expect(submitServiceCommons.applyTransformationsForSubmission).toHaveBeenCalled();
        }).catch(reason => {
            console.log(reason);
            fail();
        }).finally(done);

        $httpBackend.flush();
        $rootScope.$digest();

    });

    it('Test Submission valid data create entry', function (done) {

        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
        const deferred = $q.defer();
        const datamap = { attr1: "x"};

        crudContextHolderService.rootDataMap(null,datamap);
        crudContextHolderService.currentSchema(null,schemaWithOneRequiredField);

        spyOn(validationService, "validatePromise").and.returnValue(deferred.promise);
        spyOn(crudContextHolderService, "afterSave").and.callThrough();
        spyOn(submitServiceCommons, "applyTransformationsForSubmission").and.callThrough();
        spyOn(compositionService, "updateCompositionDataAfterSave");
        spyOn(redirectService, "redirectViewWithData").and.callThrough();
        deferred.resolve();

        const submissionData = {
            json: datamap,
            requestData: {
                applicationName: "sr",
                batch: false,
                compositionData: undefined,
                platform: "web",
                currentSchemaKey: "detail.input.web",
                userId: undefined
            }
        };

        const crudResponse = ResponsePojo.CrudCreateCachedBaseResponse();

        $httpBackend.expectPOST("/api/data/sr/", JSON.stringify(submissionData)).respond(crudResponse);

        submitService.submit(schemaWithOneRequiredField, datamap).then(() => {
            //expecting exception to be thrown
            expect(validationService.validatePromise).toHaveBeenCalled();
            expect(crudContextHolderService.afterSave).toHaveBeenCalled();
            expect(submitServiceCommons.applyTransformationsForSubmission).toHaveBeenCalled();
            expect(compositionService.updateCompositionDataAfterSave).toHaveBeenCalled();
            expect(redirectService.redirectViewWithData).toHaveBeenCalled();
            //asserting schema was changed to edit instead of create
            expect(crudContextHolderService.currentSchema().schemaId).toBe("editdetail");
            //asserting datamap was updated
            expect(crudContextHolderService.rootDataMap()).toEqual(crudResponse.resultObject);

        }).catch(reason => {
            console.log(reason);
            fail();
        }).finally(done);

        $httpBackend.flush();
        $rootScope.$digest();

    });

    it('test modal submission', function (done) {


        const schemaWithOneRequiredField = SchemaPojo.BaseWithSection();
        const datamap = { attr1: "x" };

        crudContextHolderService.rootDataMap(null, datamap);
        crudContextHolderService.currentSchema(null, schemaWithOneRequiredField);

        crudContextHolderService.registerSaveFn(() => { return $q.when({modalfunctionresult:true}); });
        crudContextHolderService.modalLoaded(schemaWithOneRequiredField,datamap);

        spyOn(validationService, "validatePromise").and.returnValue($q.when());

        submitService.submit(schemaWithOneRequiredField, datamap, { dispatchedByModal: true }).then((result) => {
            //making sure the modal function is getting invoked and it´s result is being returned
            expect(result.modalfunctionresult).toBeTruthy();
        }).catch(reason => {
            expect(false).toBeTruthy();
        }).finally(done);
        $rootScope.$digest();
    });


});
