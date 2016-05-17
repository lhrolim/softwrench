describe('Crud Body Test', function () {

    var mockScope;
    var $httpBackend;
    var $rootScope;
    var eventService;
    var contextService;

    beforeEach(module('sw_layout'));

    //init app --> first action usually
    beforeEach(function () {
        module("sw.templates");
        module('ngMockE2E');
    });

    beforeEach(inject(function ($injector, _$rootScope_, $compile, _eventService_, _contextService_) {
        $httpBackend = $injector.get('$httpBackend');
        $rootScope = _$rootScope_;
        eventService = _eventService_;
        contextService = _contextService_;

        mockScope = $rootScope.$new();
        mockScope.schema = SchemaPojo.WithId("test");

        contextService.insertIntoContext("commandbars", {});
        contextService.set("grid_refreshdata", { panelid: "aaa" }, true);
        debugger;
        mockScope.datamap = {};
        var el = angular.element("<crud-body datamap='datamap' schema='schema' is-list='true' ismodal='false' timestamp='100' />");
        $compile(el)(mockScope);
        //mocking directive loading
        $httpBackend.when('GET', '/Content/Templates/directives/multiselectDropdown.html').respond({});
        mockScope.$digest();
        mockScope = el.isolateScope() || el.scope();

    }));

    it("Modal Shown --> dispatch load event with right schema", function () {

        mockScope.ismodal = "true";


        const schema1 = SchemaPojo.WithIdAndEvent("schema1", MetadataEventConstants.OnLoadEvent, "myservice", "mymethod");
        const schema2 = SchemaPojo.WithIdAndEvent("schema2", MetadataEventConstants.OnLoadEvent,"myservice2","mymethod2");

        let modalData1 = new ModalData(schema1);
        let modalData2 = new ModalData(schema2);

        spyOn(eventService, "dispatchEvent");

        $rootScope.$broadcast(JavascriptEventConstants.ModalShown, modalData1);
        expect(eventService.dispatchEvent).toHaveBeenCalledWith(schema1, "onschemafullyloaded");

        $rootScope.$broadcast(JavascriptEventConstants.ModalShown, modalData2);
        expect(eventService.dispatchEvent).toHaveBeenCalledWith(schema2, "onschemafullyloaded");

    });


});