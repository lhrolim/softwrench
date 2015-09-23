describe("Rowstamp service test", function () {

    //mocked values
    var swdbDAO, rowstampConstant, $q, $rootScope, entities;

    //entity to test
    var rowStampService;

    beforeEach(module("sw_mobile_services"));

    beforeEach(function () {
        module(function ($provide) {
            //mocking constant value to 3 instead to make tests easier
            $provide.constant('rowstampConstants', { maxItemsForFullStrategy: 3 });
        });
    });

    beforeEach(module("softwrench"));


    beforeEach(inject(function (_rowstampService_, _swdbDAO_, _rowstampConstants_, _$q_, _$rootScope_, _offlineEntities_) {
        rowStampService = _rowstampService_;
        swdbDAO = _swdbDAO_;
        $q = _$q_;
        $rootScope = _$rootScope_;
        entities = _offlineEntities_;
    }));

    //use this done function to avoid the tests to finish before the promises were resolved
    it("Testing Huge DataSets", function (done) {

        spyOn(swdbDAO, 'countByQuery').and.callFake(function () {
            return $q.when(4);
        });

        spyOn(swdbDAO, 'findByQuery').and.callFake(function () {
            return $q.when([{datamap:'aaa', rowstamp:"20000" }]);
        });

        rowStampService.generateRowstampMap("asset").then(function (result) {
            expect(result).toEqual({ maxrowstamp: "20000" });
            expect(swdbDAO.countByQuery).toHaveBeenCalledWith('DataEntry', "application ='asset'");
            expect(swdbDAO.findByQuery).toHaveBeenCalledWith('DataEntry', null, { fullquery: entities.DataEntry.maxRowstampByAppQuery.format("asset") });
        }).finally(done);

        //this is needed to trigger the promises resolutions!
        $rootScope.$digest();

    });

    //use this done function to avoid the tests to finish before the promises were resolved
    it("Testing small DataSets", function (done) {

        spyOn(swdbDAO, 'countByQuery').and.callFake(function () {
            return $q.when(2);
        });

        spyOn(swdbDAO, 'findByQuery').and.callFake(function () {
            return $q.when([{ remoteId: '10', rowstamp: '100' }, { remoteId: '20', rowstamp: '200' }]);
        });

        rowStampService.generateRowstampMap("asset").then(function (result) {
            expect(result).toEqual({ items: [{ id: '10', rowstamp: '100' }, { id: '20', rowstamp: '200' }] });
            expect(swdbDAO.countByQuery).toHaveBeenCalledWith('DataEntry', "application ='asset'");
            expect(swdbDAO.findByQuery).toHaveBeenCalledWith('DataEntry', "application ='asset'", { projectionFields: ["remoteId", "rowstamp"] });
        }).finally(done);

        //this is needed to trigger the promises resolutions!
        $rootScope.$digest();

    });

});