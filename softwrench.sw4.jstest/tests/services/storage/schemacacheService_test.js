describe('SchemaCache Service Test', function () {


    //declare used services
    var schemaCacheService;

    //instantiate used modules
    beforeEach(module('sw_layout'));

    //inject services
    beforeEach(inject(function (_schemaCacheService_) {
        schemaCacheService = _schemaCacheService_;
    }));


    it("test wipe schema --> clear all keys",()=> {
        const schema = SchemaPojo.WithId("detail");
        schemaCacheService.addSchemaToCache(schema);
        let cachedSchema = schemaCacheService.getCachedSchema("sr", "detail");
        expect(cachedSchema).toBe(schema);
        expect(schemaCacheService.getSchemaCacheKeys()).toBe(";systeminitMillis;sr.detail;");
        schemaCacheService.wipeSchemaCacheIfNeeded(true);
        cachedSchema = schemaCacheService.getCachedSchema("sr", "detail");
        expect(cachedSchema).toBeNull();
        expect(schemaCacheService.getSchemaCacheKeys()).toBe(";systeminitMillis;");
    });
   


});