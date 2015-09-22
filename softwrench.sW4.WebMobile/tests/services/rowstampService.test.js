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


    beforeEach(inject(function (_rowStampService_, _swdbDAO_, _rowstampConstants_, _$q_) {
        rowStampService = _rowStampService_;
        swdbDAO = _swdbDAO_;
        $q = _$q_;
        console.log('ok');
    }));

    it("Testing Huge DataSets", function () {
        spyOn(swdbDAO, "countByQuery").and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(2);
            return deferred.promise;
        });
    });

});