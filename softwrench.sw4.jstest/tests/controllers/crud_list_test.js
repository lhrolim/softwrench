describe('Crud List Test', function () {

    var mockScope;
    var _contextService;
    var _searchService;
    var $httpBackend;

    beforeEach(module('sw_layout'));

    //init app --> first action usually
    beforeEach(function() {
        module("sw.templates");
        module('ngMockE2E');
    });
    
    beforeEach(inject(function ($injector, $rootScope, $compile,$q, contextService,searchService) {
        $httpBackend = $injector.get('$httpBackend');
        mockScope = $rootScope.$new();
        mockScope.schema = {};
        mockScope.schema.commandSchema = {};
        mockScope.schema.properties = {};
        
        mockScope.datamap = {};
        _contextService = contextService;

        _searchService = searchService;

        spyOn(searchService, "searchWithData").and.callFake(function () {
            //rejecting here on the before, so that the test runs for real later...
            //TODO: change this, this sucks
            return $q.reject();
        });

        contextService.insertIntoContext("commandbars", {});
        var el = angular.element("<crud-list datamap='datamap' schema='schema' is-list='true' ismodal='false' timestamp='100' />");
        $compile(el)(mockScope);
        mockScope.$digest();
        mockScope = el.isolateScope() || el.scope();

    }));

    it("Grid Refreshed By A Filter", function () {

        _contextService.insertIntoContext("crud_context", {
            list_elements: ["100", "200", "300"],
            detail_next: "0",
            detail_previous: "-1",
            paginationData: {},
            previousData: [{}, {}, {}]
        });

        //simulating the data returned from the server
        var serverdata = {
            resultObject: [{fields: { id: "100" } }, { fields: { id: "200" } }],
            schema:{idFieldName:"id",displayables:[],properties: {} }
        }
        //real call
        mockScope.gridRefreshed(serverdata, null);

        var crudContext = _contextService.fetchFromContext("crud_context", true);

        expect(crudContext).toBeDefined();
        expect(crudContext).toEqual({
            list_elements: ["100", "200"],
            detail_next: "0",
            detail_previous: "-1",
            paginationData: {},
            previousData: [{ fields: { id: "100" } }, { fields: { id: "200" } }]
        });
    });

    it("Sorting should be cleared when grid is refreshed", function () {
        //Set mock data
        mockScope.paginationData = {};
        mockScope.searchSort = { field: "ticketid", order: "asc" };

        //mock the function call
        spyOn(mockScope, "selectPage").and.callFake(function () {
            return null;
        });

        //real call
        mockScope.refreshGridRequested({}, null, {});

        expect(mockScope.searchSort).toBeDefined();
        expect(mockScope.searchSort).toEqual({});
        expect(mockScope.vm.quickSearchDTO).toBeDefined();
        expect(mockScope.vm.quickSearchDTO.compositionsToInclude).toEqual([]);
    });
});