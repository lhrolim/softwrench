describe('Association Service Test', function () {


    //declare used services
    var associationService, crudContextHolderService;

    //instantiate used modules
    beforeEach(module('sw_layout'));

    //inject services
    beforeEach(inject(function (_associationService_, _crudContextHolderService_) {
        associationService = _associationService_;
        crudContextHolderService = _crudContextHolderService_;
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
            
        const results = associationService.getEagerLookupOptions(lookupDTO).resultObject;

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

        const results = associationService.getEagerLookupOptions(lookupDTO).resultObject;

        expect(results.totalCount).toBe(3);
        var associationData = results.associationData;
        expect(associationData[0]).toBe(ob1);
        expect(associationData[1]).toBe(ob2);
        expect(associationData[2]).toBe(ob11);

    });



});