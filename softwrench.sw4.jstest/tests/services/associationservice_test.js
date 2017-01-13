describe('Association Service Test', function () {


    //declare used services
    var associationService, crudContextHolderService, $rootScope, $timeout;

    //instantiate used modules
    beforeEach(module('sw_layout'));

    //inject services
    beforeEach(inject(function (_associationService_, _crudContextHolderService_, _$rootScope_, _$timeout_) {
        associationService = _associationService_;
        crudContextHolderService = _crudContextHolderService_;
        $rootScope = _$rootScope_;
        $timeout = _$timeout_;
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


});