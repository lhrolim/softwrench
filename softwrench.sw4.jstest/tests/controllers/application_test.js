describe('Application Test', function () {

    var mockScope;
    var controller;
    var redirectService;
    var detailService;
    var _contextService;
    var _validationService;
    var _crudlistViewmodel;
    var schemaCacheService;

    //init app --> first action usually
    beforeEach(angular.mock.module('sw_layout'));

    beforeEach(angular.mock.inject(function ($rootScope, $controller, contextService, validationService, crudlistViewmodel, _redirectService_, _schemaCacheService_, _detailService_) {
        mockScope = $rootScope.$new();
        mockScope.resultObject = {
            redirectURL: "Application.html", 
        };
        _contextService = contextService;
        _validationService = validationService;
        _crudlistViewmodel = crudlistViewmodel;
        redirectService = _redirectService_;
        schemaCacheService = _schemaCacheService_;
        detailService = _detailService_;
        controller = $controller('ApplicationController', {
            $scope: mockScope,
            contextService: _contextService,
            validationService: _validationService,
            crudlistViewmodel: _crudlistViewmodel
        });
        _contextService.clearContext();
    }));

    it("Receiving List Result From Server", function () {
        const serverResult = {
            type: "ApplicationListResult",
            resultObject: [{ id: "100" }, { id: "200" }],
        };
        spyOn(mockScope, "$broadcast");
        spyOn(_crudlistViewmodel, "initGridFromServerResult");

        mockScope.renderData(serverResult);

        expect(mockScope.datamap).toEqual(serverResult.resultObject);
        expect(mockScope.previousdata).toEqual({});
        expect(mockScope.isList).toBeTruthy();
        expect(_crudlistViewmodel.initGridFromServerResult).toHaveBeenCalledWith(serverResult, null);
    });

    it("Returning From Detail to Grid (Cancel)", function () {
        const data = [{ id: "100" }, { id: "200" }];
        const schema = { idFieldName: "id", displayables: [], properties: {} };
        mockScope.schema = schema;

        spyOn(mockScope, "$broadcast");
        spyOn(_crudlistViewmodel, "initGridFromServerResult");

        mockScope.toListSchema(data, schema);

        expect(mockScope.datamap).toEqual(data);
        expect(mockScope.schema).toEqual(schema);
        expect(mockScope.isList).toBeTruthy();
        const eventData = {
            //here we have to reproduce that the request is coming from the server, so use resultObject as the name.
            //check crud_list#gridRefreshed
            resultObject: data,
            schema: schema,
            pageResultDto: {},
        };
        expect(_crudlistViewmodel.initGridFromServerResult).toHaveBeenCalledWith(eventData, null);

    });

    it("Init On Detail Browser (F5)", function () {
        spyOn(redirectService, "redirectViewWithData").and.callThrough();
        spyOn(detailService, "detailLoaded");

        const detailResponse = ResponsePojo.CrudDetailResponse();
        const schema = SchemaPojo.WithId("editdetail", "workorder");
        schemaCacheService.addSchemaToCache(schema);

        mockScope.doInit(detailResponse);
        

        
        expect(redirectService.redirectViewWithData).toHaveBeenCalledWith(detailResponse);
        expect(mockScope.datamap).toEqual(detailResponse.resultObject);
        expect(detailService.detailLoaded).toHaveBeenCalled();
        expect(mockScope.schema).toEqual(schema);

    });


    it("Init On List Browser (F5)", function () {
        spyOn(redirectService, "redirectViewWithData").and.callThrough();
        spyOn(_crudlistViewmodel, "initGridFromServerResult").and.callThrough();

        const listResponse = ResponsePojo.ListResponse();
        const schema = SchemaPojo.WithId("list", "servicerequest");
        schemaCacheService.addSchemaToCache(schema);

        mockScope.doInit(listResponse);

        expect(redirectService.redirectViewWithData).toHaveBeenCalledWith(listResponse);
        expect(mockScope.schema).toEqual(schema);
        expect(mockScope.datamap).toEqual(listResponse.resultObject);
        expect(_crudlistViewmodel.initGridFromServerResult).toHaveBeenCalledWith(listResponse, null);

    });

    it("Init On Dashboard screen", function () {
        spyOn(redirectService, "redirectViewWithData").and.callThrough();

        const dashboardResponse = ResponsePojo.DashboardResponse();

        mockScope.doInit(dashboardResponse);

        expect(redirectService.redirectViewWithData).toHaveBeenCalledWith(dashboardResponse);
        expect(mockScope.crudsubtemplate).toBe("/Content/Shared/dashboard/templates/Dashboard.html");


    });


});