describe("Rowstamp service test", function () {

    //mocked values
    var swdbDAO, rowstampConstant, $q;

    //entity to test
    var rowStampService;

    beforeEach(module("sw_mobile_services"));

    beforeEach(function () {
        module(function ($provide) {
            //mocking constant value to 2 instead to make tests easier
            $provide.constant('rowstampConstants', { maxItemsForFullStrategy: 2 });
        });
    });

    beforeEach(module("softwrench"));


    beforeEach(inject(function (_rowstampService_, _swdbDAO_, _rowstampConstants_, _$q_) {
        rowStampService = _rowstampService_;
        swdbDAO = _swdbDAO_;
        $q = _$q_;
    }));

    it("Testing Huge DataSets", function (done) {
        spyOn(swdbDAO, "countByQuery").and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(2);
            return deferred.promise;
        });

        var failTest = function (error) {
            expect(error).toBeUndefined();
        };

        rowStampService.generateRowstampMap("asset").then(function (result) {
                expect(result).toBe({ rowstampMap: 20000 });
            }).catch(failTest).finally(done);

    });

});