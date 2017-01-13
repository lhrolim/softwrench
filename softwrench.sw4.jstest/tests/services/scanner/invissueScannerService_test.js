describe('Lookup Service Test', function () {

    var invIssueScannerService;

    //declare used services
    var associationService,contextService, crudContextHolderService, $rootScope, $timeout, lookupService, $q, scanningCommonsService, applicationService;

    //instantiate used modules
    beforeEach(module('sw_layout'));

    //inject services
    beforeEach(inject(function (_associationService_, _crudContextHolderService_, _$rootScope_, _$timeout_, _lookupService_, _$q_, _scanningCommonsService_, _invIssueScannerService_, _applicationService_, _contextService_) {
        associationService = _associationService_;
        crudContextHolderService = _crudContextHolderService_;
        $rootScope = _$rootScope_;
        $timeout = _$timeout_;
        lookupService = _lookupService_;
        $q = _$q_;
        invIssueScannerService = _invIssueScannerService_;
        scanningCommonsService = _scanningCommonsService_;
        applicationService = _applicationService_;
        contextService = _contextService_;
    }));


    it('test scan complete ==> submit', done=> {

        spyOn(scanningCommonsService, "registerScanCallBackOnSchema").and.returnValue($q.when("%SUBMIT%"));
        spyOn(applicationService, "save").and.returnValue($q.when(ResponsePojo.CrudUpdateBaseResponse("10", "10")));

        const schema = SchemaPojo.WithId("edit");
        const datamap = DatamapPojo.BaseSRItem("10", "test", "test ld");

        invIssueScannerService.initInvIssueDetailListener({},schema,datamap).then(r => {
            expect(applicationService.save).toHaveBeenCalledWith({ selecteditem: datamap });
        }).finally(done);
        $rootScope.$digest();

    });

    it('test scan itemnum ==> populate entry after lookup result', done=> {

        spyOn(scanningCommonsService, "registerScanCallBackOnSchema").and.returnValue($q.when("1500"));
        spyOn(lookupService, "getLookupOptions").and.returnValue($q.when(ResponsePojo.LookpOptionsResult([{value:"10",label: "100"}])));

        contextService.set("newInvIssueDetailScanOrder", "itemnum");

        const schema = SchemaPojo.WithIdAndDisplayables("newInvIssueDetail", [FieldMetadataPojo.ForAssociation("item_", "itemnum")], "invissue");
        const datamap = DatamapPojo.BaseInvIssueItem("10", "test", "test ld");

        invIssueScannerService.initInvIssueDetailListener({}, schema, datamap).then(r => {
            expect(lookupService.getLookupOptions).toHaveBeenCalled();
        }).finally(done);
        $rootScope.$digest();

    });


  


});