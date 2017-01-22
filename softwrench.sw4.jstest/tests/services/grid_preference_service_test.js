describe('GridPreference Service Test', function () {



    var gridPreferenceService;
    var contextService;


    beforeEach(module('sw_layout'));
    beforeEach(inject(function (_gridPreferenceService_, _contextService_) {
        gridPreferenceService = _gridPreferenceService_;
        contextService = _contextService_;

    }));

    it('Test Loading Filters, no QuickSearch ones', function () {
        //creating mock user
        const result = gridPreferenceService.loadUserNonSharedFilters("sr", SchemaPojo.WithId("list"));
        expect(result.length).toBe(0);
    });

    it('Test Loading Filters, with a QuickSearch', function () {
        //creating mock user
        const schema = SchemaPojo.WithId("list");
        schema.schemaFilters.quickSearchFilters = [{ label: "test", whereClause: "xxx", id: "1" }];
        const result = gridPreferenceService.loadUserNonSharedFilters("sr", schema);
        expect(result.length).toBe(1);
        expect(result[0].deletable).toBeFalsy();
    });

    it('Test Loading Filters combining user and quicksearch', function () {
        //creating mock user
        const schema = SchemaPojo.WithId("list");
        spyOn(contextService, "getUserData").and.returnValue({ gridPreferences: { gridFilters: [{ "filter": { application: "sr", schema: "list" } }, { "filter": { application: "workorder", schema: "list" } }] } });

        schema.schemaFilters.quickSearchFilters = [{ label: "test", whereClause: "xxx", id: "1" }];
        const result = gridPreferenceService.loadUserNonSharedFilters("sr", schema);
        expect(result.length).toBe(2);
    });

   



});