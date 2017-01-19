describe('Lookup Service Test', function () {

    //declare used services
    var associationService, crudContextHolderService, $rootScope, $timeout, lookupService, $httpBackend;

    //instantiate used modules
    beforeEach(module('sw_layout'));

    //inject services
    beforeEach(inject(function (_associationService_, _crudContextHolderService_, _$rootScope_, _$timeout_, _lookupService_, _$httpBackend_) {
        associationService = _associationService_;
        crudContextHolderService = _crudContextHolderService_;
        $rootScope = _$rootScope_;
        $timeout = _$timeout_;
        lookupService = _lookupService_;
        $httpBackend = _$httpBackend_;
    }));


    it('getEagerLookupOptions test', function () {
        const quickSearchDTO = new QuickSearchDTO("test1");
        quickSearchDTO.schemaId = "test";
        const lookupDTO = new LookupDTO(FieldMetadataPojo.ForAssociation("test"), quickSearchDTO);

        spyOn(crudContextHolderService, "isShowingModal").and.returnValue(true);

        const ob1 = { value: "test1", label: "test1 label" };
        const ob2 = { value: "test2", label: "test2 label" };
        const ob11 = { value: "test11", label: "test11 label" };

        spyOn(crudContextHolderService, "fetchEagerAssociationOptions").and.returnValue([ob1, ob2, ob11]);

        const results = lookupService.getEagerLookupOptions(lookupDTO, quickSearchDTO).resultObject;

        expect(results.totalCount).toBe(2);
        var associationData = results.associationData;
        expect(associationData[0]).toBe(ob1);
        expect(associationData[1]).toBe(ob11);

    });


    it('getEagerLookupOptions no filter test', function () {
        const lookupDTO = new LookupDTO(FieldMetadataPojo.ForAssociation("test"), null);

        spyOn(crudContextHolderService, "isShowingModal").and.returnValue(true);

        const ob1 = { value: "test1", label: "test1 label" };
        const ob2 = { value: "test2", label: "test2 label" };
        const ob11 = { value: "test11", label: "test11 label" };

        spyOn(crudContextHolderService, "fetchEagerAssociationOptions").and.returnValue([ob1, ob2, ob11]);

        const results = lookupService.getEagerLookupOptions(lookupDTO).resultObject;

        expect(results.totalCount).toBe(3);
        var associationData = results.associationData;
        expect(associationData[0]).toBe(ob1);
        expect(associationData[1]).toBe(ob2);
        expect(associationData[2]).toBe(ob11);

    });

    it("test init lookupmodal", done => {
        const schema = SchemaPojo.WithId("detailtest");
        const lookupDTO = new LookupDTO(FieldMetadataPojo.ForAssociation("asset_", "assetnum"));
        const datamap = DatamapPojo.BaseSRItem();

        crudContextHolderService.currentSchema(null, schema);

        $httpBackend.expectPOST((url) => assertHttp(url, "/api/generic/Association/GetLookupOptions", {
            associationFieldName: "asset_",
            parentKey: {
                applicationName: "sr",
                schemaId: "detailtest"
            },
            searchDTO: {
                addPreSelectedFilters: true
            }
        }
            ), datamap)
            .respond(ResponsePojo.LookpOptionsResult());

        lookupService.initLookupModal(lookupDTO, datamap).then(l => {
            expect(l.options).toBeDefined();
            expect(l.options.length).toBe(3);
        }).catch(err => {
            console.log(err);
            expect(true).toBeFalsy();
        }).finally(done);
        $rootScope.$digest();
        $httpBackend.flush();

    });

    it("test search lookup", done => {
        const schema = SchemaPojo.WithId("detailtest");
        const lookupDTO = new LookupDTO(FieldMetadataPojo.ForAssociation("asset_", "assetnum"));
        const searchDTO = new SearchDTO({ pageNumber: 5, quickSearchDTO: new QuickSearchDTO("test") });

        const datamap = DatamapPojo.BaseSRItem();

        crudContextHolderService.currentSchema(null, schema);

        $httpBackend.expectPOST((url) => assertHttp(url, "/api/generic/Association/GetLookupOptions", {
            associationFieldName: "asset_",
            parentKey: {
                applicationName: "sr",
                schemaId: "detailtest"
            },
            searchDTO: {
                quickSearchDTO: {
                    quickSearchData: "test"
                },
                pageNumber: 5
            }}
            ), datamap)
            .respond(ResponsePojo.LookpOptionsResult());

        lookupService.getLookupOptions(lookupDTO,searchDTO, datamap).then(l => {
            expect(l.options).toBeDefined();
            expect(l.options.length).toBe(3);
        }).catch(err => {
            console.log(err);
            expect(true).toBeFalsy();
        }).finally(done);

        $rootScope.$digest();
        $httpBackend.flush();

    });


   

});