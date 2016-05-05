describe('Grid Filter Test', function () {

    var mockScope;
    var controller;
    var _crudContextHolderService;
    var _searchService;

    //init app --> first action usually
    beforeEach(angular.mock.module('sw_layout'));

    beforeEach(angular.mock.inject(function ($rootScope, $controller, searchService, crudContextHolderService) {
        mockScope = $rootScope.$new();
        _crudContextHolderService = crudContextHolderService;
        _searchService = searchService;
        controller = $controller("GridFilterController", {
            $scope: mockScope,
            crudContextHolderService: _crudContextHolderService,
            searchService : _searchService
        });
    }));

    it("Clear Filter", function () {
        mockScope.panelid = "myPanelId";

        //creating mocks
        spyOn(_crudContextHolderService, "setSelectedFilter");
        spyOn(_searchService, "refreshGrid");
        spyOn(mockScope, "applyFilter");

        //simulating the user removed the filter from selection
        mockScope.selectedfilter = null;
        mockScope.filterChanged();

        //services should have been called
        expect(_crudContextHolderService.setSelectedFilter).toHaveBeenCalledWith(null, mockScope.panelid);
        expect(_crudContextHolderService.setSelectedFilter.calls.count()).toBe(1);
        expect(_searchService.refreshGrid).toHaveBeenCalled();
        expect(_searchService.refreshGrid.calls.count()).toBe(1);
        expect(mockScope.applyFilter.calls.count()).toBe(0);
    });

    it("Select Filter", function () {
        var filter = { alias: "myfilter" };
        mockScope.panelid = "myPanelId";

        //creating mocks
        spyOn(_crudContextHolderService, "setSelectedFilter");
        spyOn(_searchService, "refreshGrid");
        spyOn(mockScope, "applyFilter");

        //simulating the user selecting a filter
        mockScope.selectedfilter = filter;
        mockScope.filterChanged();

        //services should have been called
        expect(_crudContextHolderService.setSelectedFilter).toHaveBeenCalledWith(filter, mockScope.panelid);
        expect(_crudContextHolderService.setSelectedFilter.calls.count()).toBe(1);
        expect(_searchService.refreshGrid.calls.count()).toBe(0);
        expect(mockScope.applyFilter).toHaveBeenCalled();
        expect(mockScope.applyFilter.calls.count()).toBe(1);
    });


});