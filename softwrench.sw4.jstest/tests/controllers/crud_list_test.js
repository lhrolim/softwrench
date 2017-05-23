describe('Crud List Test', function () {

    var mockScope;
    var _eventService;
    var $httpBackend;
    var _contextService;
    var _searchService;
    var _checkPointService;

    beforeEach(module('sw_layout'));

    //init app --> first action usually
    beforeEach(function() {
        module("sw.templates");
        module('ngMockE2E');
    });
    
    beforeEach(inject(function ($injector, $rootScope, $compile, $q, contextService, searchService, checkpointService) {
        $httpBackend = $injector.get('$httpBackend');
        mockScope = $rootScope.$new();
        mockScope.schema = {
            schemaFilters : {}
        };
        mockScope.schema.commandSchema = {};
        mockScope.schema.properties = {};
        
        mockScope.datamap = {};
        _contextService = contextService;
        _checkPointService = checkpointService;

        _searchService = searchService;

        spyOn(searchService, "searchWithData").and.callFake(function () {
            //rejecting here on the before, so that the test runs for real later...
            //TODO: change this, this sucks
            return $q.reject();
        });

        spyOn(_checkPointService, "createGridCheckpointFromGridData");

        contextService.insertIntoContext("commandbars", {});
        const el = angular.element("<crud-list datamap='datamap' schema='schema' is-list='true' ismodal='false' timestamp='100' />");
        $compile(el)(mockScope);
        //mocking directive loading
        $httpBackend.when('GET', '/Content/Templates/directives/multiselectDropdown.html').respond({});
        mockScope.$digest();
        mockScope = el.isolateScope() || el.scope();

    }));

    it("Grid Refreshed By A Filter", function () {

        _contextService.insertIntoContext("crud_context", {
            list_elements: [{ id: "100" }, { id: "200" }, { id: "300" }],
            detail_next: { id: "0" },
            detail_previous: { id: "-1" },
            paginationData: {},
            previousData: [{}, {}, {}]
        });

        //simulating the data returned from the server
        const serverdata = {
            resultObject: [{ id: "100" }, { id: "200" }],
            schema: SchemaPojo.WithId("schema")
        };
        //real call
        mockScope.gridRefreshed(serverdata, null);

        expect(_checkPointService.createGridCheckpointFromGridData).toHaveBeenCalled();
        const crudContext = _contextService.fetchFromContext("crud_context", true);
        expect(crudContext).toBeDefined();
        expect(crudContext).toEqual({
            list_elements: [{ id: "100" }, { id: "200" }],
            detail_next: { id: "0" },
            detail_previous: { id: "-1" },
            paginationData: {},
            previousData: [{ id: "100" }, { id: "200" }],
            applicationName: 'sr'
        });
    });

    it("Grid Initial Load --> no checkpoint insertion", function () {

        _contextService.insertIntoContext("crud_context", {
            list_elements: [{ id: "100" }, { id: "200" }, { id: "300" }],
            detail_next: { id: "0" },
            detail_previous: { id: "-1" },
            paginationData: {},
            previousData: [{}, {}, {}]
        });

        //simulating the data returned from the server
        const serverdata = {
            resultObject: [{ id: "100" }, { id: "200" }],
            schema: SchemaPojo.WithId("schema")
        };
        //real call
        mockScope.gridRefreshed(serverdata, null,true);

        expect(_checkPointService.createGridCheckpointFromGridData).not.toHaveBeenCalled();
        const crudContext = _contextService.fetchFromContext("crud_context", true);
        expect(crudContext).toBeDefined();
        expect(crudContext).toEqual({
            list_elements: [{ id: "100" }, { id: "200" }],
            detail_next: { id: "0" },
            detail_previous: { id: "-1" },
            paginationData: {},
            previousData: [{ id: "100" }, { id: "200" }],
            applicationName: 'sr'
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
        mockScope.refreshGridRequested({}, null, { forcecleanup :true});

        expect(mockScope.searchSort).toBeDefined();
        expect(mockScope.searchSort).toEqual({});
        expect(mockScope.vm.quickSearchDTO).toBeDefined();
        expect(mockScope.vm.quickSearchDTO.compositionsToInclude).toEqual([]);
    });
});