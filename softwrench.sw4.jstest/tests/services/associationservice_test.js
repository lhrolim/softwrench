describe('Association Service Test', function () {


    //declare used services
    var associationService, crudContextHolderService, $rootScope, $timeout, restService, $q;

    //instantiate used modules
    beforeEach(module('sw_layout'));

    //inject services
    beforeEach(inject(function (_associationService_, _crudContextHolderService_, _$rootScope_, _$timeout_, _restService_, _$q_) {
        associationService = _associationService_;
        crudContextHolderService = _crudContextHolderService_;
        $rootScope = _$rootScope_;
        $timeout = _$timeout_;
        restService = _restService_;
        $q = _$q_;
    }));


    it("updateFromServerSchemaLoadResult for list", done => {
        spyOn(crudContextHolderService, "markAssociationsResolved");
        associationService.updateFromServerSchemaLoadResult(ResponsePojo.ListSchemaLoadResult(), null, true).then(r => {
            expect(crudContextHolderService.markAssociationsResolved).toHaveBeenCalledWith(null);
        }).finally(done);
        $rootScope.$digest();
        $timeout.flush();

    });


    it("updateFromServerSchemaLoadResult for detail", done => {
        spyOn(crudContextHolderService, "markAssociationsResolved");
        spyOn(crudContextHolderService, "updateEagerAssociationOptions").and.callThrough();
        associationService.updateFromServerSchemaLoadResult(ResponsePojo.DetailSchemaLoadResult(), null, true).then(r => {
            expect(crudContextHolderService.updateEagerAssociationOptions).toHaveBeenCalledWith("classification", [{ value: "100", label: "label 100" }, { value: "101", label: "label 101" }],null);
            expect(crudContextHolderService.markAssociationsResolved).toHaveBeenCalledWith(null);
        }).finally(done);
        $rootScope.$digest();
        $timeout.flush();

    });

    it("update underlying fields test --> update extra fields", () => {

        const metadata = FieldMetadataPojo.ForAssociation("asset_", "assetnum", ["location"]);
        const dto = new AssociationOptionDTO({ value:"100", label:"asset 100", type:"MultiValueAssociationOption", extrafields:{ location: "location" }});

        crudContextHolderService.currentSchema(null, SchemaPojo.WithIdAndDisplayables("detail", [metadata]));
        
        const datamap = associationService.updateUnderlyingAssociationObject(metadata, dto, { datamap: {} });
        expect(datamap["asset_.location"]).toBe("location");
        expect(crudContextHolderService.fetchLazyAssociationOption("asset_", 100)).toBe(dto);

    });

    it("lookup single association by value --> update lazy options", done => {
        const metadata = FieldMetadataPojo.ForAssociation("asset_", "assetnum", ["location"]);
        crudContextHolderService.currentSchema(null, SchemaPojo.WithIdAndDisplayables("detail", [metadata]));

        const dto = new AssociationOptionDTO({ value: "100", label: "asset 100", type: "MultiValueAssociationOption", extrafields: { location: "location" } });

        spyOn(restService, "postPromise").and.returnValue($q.when({ "data": dto }));

        associationService.lookupSingleAssociationByValue("asset_", "100").then(r => {
            expect(crudContextHolderService.fetchLazyAssociationOption("asset_", 100)).toBe(dto);
        }).finally(done);
        $rootScope.$digest();
    });


});