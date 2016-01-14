describe('Grid Filter Test', function () {

    var mockScope;
    var controller;
    var _contextService;
    var _searchService;

    //init app --> first action usually
    beforeEach(angular.mock.module('sw_layout'));

    beforeEach(angular.mock.inject(function ($rootScope, $controller, contextService, searchService) {
        mockScope = $rootScope.$new();
        _contextService = contextService;
        _searchService = searchService;
        controller = $controller('GridFilterController', {
            $scope: mockScope,
            contextService: _contextService,
            searchService : _searchService
        });
    }));

    it("Clear filters", function() {
        //simulating the user has added anything to screen
        mockScope.selectedfilter = {};
        mockScope.searchOperator = { a: 'a' };

        //creating mocks
        spyOn(_contextService, 'insertIntoContext');
        spyOn(_searchService, 'refreshGrid');

        //real call
        mockScope.clearFilter();

        //values should have been cleaned
        expect(mockScope.selectedfilter).toBeNull();
        expect(mockScope.searchOperator['a']).toBeUndefined();

        //services should have been called
        expect(_contextService.insertIntoContext).toHaveBeenCalledWith('selectedfilter', null, true);
        expect(_searchService.refreshGrid).toHaveBeenCalledWith({}, { panelid: undefined });


    });


});