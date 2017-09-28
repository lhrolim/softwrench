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
            return $q.when([{ datamap: 'aaa', rowstamp: "20000" }]);
        });

        rowStampService.generateRowstampMap("asset").then(function (result) {
            expect(result).toEqual({ application: "asset", maxrowstamp: "20000" });
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
            expect(swdbDAO.findByQuery).toHaveBeenCalledWith('DataEntry', "application ='asset'", { projectionFields: ["application", "remoteId", "rowstamp"] });
        }).finally(done);

        //this is needed to trigger the promises resolutions!
        $rootScope.$digest();

    });

    //use this done function to avoid the tests to finish before the promises were resolved
    it("Testing Small DataSets no app", function (done) {

        spyOn(swdbDAO, 'countByQuery').and.callFake(function () {
            return $q.when(2);
        });

        spyOn(swdbDAO, 'findByQuery').and.callFake(function () {
            return $q.when([{ remoteId: '10', rowstamp: '100', application: 'workorder' }, { remoteId: '20', rowstamp: '200', application: 'pastworkorder' }]);
        });

        rowStampService.generateRowstampMap().then(function (result) {
            expect(result).toEqual({
                applications: {
                    "workorder": {
                         items: [{ id: "10", rowstamp: '100' }]
                    },
                    "pastworkorder": {
                         items: [{ id: "20", rowstamp: '200' }]
                    }
                }
            });
            expect(swdbDAO.countByQuery).toHaveBeenCalledWith('DataEntry', "1=1");
            expect(swdbDAO.findByQuery).toHaveBeenCalledWith('DataEntry', "1=1", { projectionFields: ["application", "remoteId", "rowstamp"] });
        }).finally(done);

        //this is needed to trigger the promises resolutions!
        $rootScope.$digest();

    });

    //use this done function to avoid the tests to finish before the promises were resolved
    it("Testing Huge DataSets no app", function (done) {

        spyOn(swdbDAO, 'countByQuery').and.callFake(function () {
            return $q.when(4);
        });

        spyOn(swdbDAO, 'findByQuery').and.callFake(function () {
            return $q.when([{ application: 'workorder', rowstamp: "100" }, { application: 'pastworkorder', rowstamp: "200" }]);
        });

        rowStampService.generateRowstampMap().then(function (result) {
            expect(result).toEqual({
                applications: {
                    "workorder": '100',
                    "pastworkorder": '200'
                }
            });
            expect(swdbDAO.countByQuery).toHaveBeenCalledWith('DataEntry', "1=1");
            expect(swdbDAO.findByQuery).toHaveBeenCalledWith('DataEntry', null, { fullquery: entities.DataEntry.maxRowstampGeneralQuery });
        }).finally(done);

        //this is needed to trigger the promises resolutions!
        $rootScope.$digest();

    });

    //use this done function to avoid the tests to finish before the promises were resolved
    it("Testing Association Initial Load --> Bring uid instead of rowstamp", function (done) {

        spyOn(swdbDAO, 'findByQuery').and.callFake(function () {
            return $q.when([{ remoteid: '10', application: 'asset' }, { remoteid: '20', application: 'location' }]);
        });

        rowStampService.generateAssociationRowstampMap([], true).then(function (result) {

            const expectation = {
                associationmap: {
                    "asset": { "maximouid": '10' },
                    "location": { "maximouid": '20' }
                }
            };

            expect(result).toEqual(expectation);
            expect(swdbDAO.findByQuery).toHaveBeenCalledWith('AssociationData', null, { fullquery: entities.AssociationData.maxRemoteIdQueries });
        }).finally(done);

        //this is needed to trigger the promises resolutions!
        $rootScope.$digest();

    });

});