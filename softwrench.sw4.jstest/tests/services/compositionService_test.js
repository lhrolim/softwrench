describe('Composition Service Test', function () {


    //declare used services
    var compositionService;

    //instantiate used modules
    beforeEach(module('sw_layout'));

    //inject services
    beforeEach(inject(function (_compositionService_) {
        compositionService = _compositionService_;
    }));


    it('generate batch item', function () {
        const assetNumDisplayable = FieldMetadataPojo.ForAssociation("asset", "assetnum");
        const hiddenDisplayable = FieldMetadataPojo.Hidden("assetnum");
        const hiddenDisplayable2 = FieldMetadataPojo.Hidden("hidden2");
        const schema = SchemaPojo.WithIdAndDisplayables("test", [assetNumDisplayable,hiddenDisplayable,hiddenDisplayable2]);
        const result = compositionService.generateBatchItemDatamap(0,schema);
        expect(result["id"]).toBeDefined();
        expect(result["assetnum"]).toBeNull();
        expect(result["hidden2"]).toBeUndefined();

    });




});