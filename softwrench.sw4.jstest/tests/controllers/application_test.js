describe('Application Test', function () {

    var mockScope;
    var controller;
    var _contextService;
    var _validationService;

    //init app --> first action usually
    beforeEach(angular.mock.module('sw_layout'));

    beforeEach(angular.mock.inject(function ($rootScope, $controller, contextService, validationService) {
        mockScope = $rootScope.$new();
        mockScope.resultObject = {
            redirectURL: "Application.html",
        };
        _contextService = contextService;
        _validationService = validationService;
        controller = $controller('ApplicationController', {
            $scope: mockScope,
            contextService: _contextService,
            validationService: _validationService
        });
        _contextService.clearContext();
    }));

    it("Receiving List Result From Server", function () {

        var serverResult = {
            type: "ApplicationListResult",
            resultObject: [{ id: "100" }, { id: "200" }],
        };

        spyOn(mockScope, "$broadcast");
        
        mockScope.renderData(serverResult);

        expect(mockScope.datamap).toEqual(serverResult.resultObject);
        expect(mockScope.previousdata).toEqual({});
        expect(mockScope.isList).toBeTruthy();
        expect(mockScope.$broadcast).toHaveBeenCalledWith("sw_gridchanged");
        expect(mockScope.$broadcast).toHaveBeenCalledWith("sw_gridrefreshed", serverResult, null);
    });

    it("Returning From Detail to Grid (Cancel)", function () {

        var data = [{ id: "100" }, { id: "200" }];
        var schema ={idFieldName:"id",displayables:[],properties: {} }
        mockScope.schema = schema;

        spyOn(mockScope, "$broadcast");

        mockScope.toListSchema(data,schema);

        expect(mockScope.datamap).toEqual(data);
        expect(mockScope.schema).toEqual(schema);
        expect(mockScope.isList).toBeTruthy();
        expect(mockScope.$broadcast).toHaveBeenCalledWith("sw_gridchanged");

        var eventData = {
            //here we have to reproduce that the request is coming from the server, so use resultObject as the name.
            //check crud_list#gridRefreshed
            resultObject: data,
            schema: schema,
            pageResultDto: {},
        }

        expect(mockScope.$broadcast).toHaveBeenCalledWith("sw_gridrefreshed", eventData, null);
        
    });


});