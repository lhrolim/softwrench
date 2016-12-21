describe('Application Test', function () {

    var mockScope;
    var controller;
    var _contextService;
    var _validationService;
    var _crudlistViewmodel;

    //init app --> first action usually
    beforeEach(angular.mock.module('sw_layout'));

    beforeEach(angular.mock.inject(function ($rootScope, $controller, contextService, validationService, crudlistViewmodel) {
        mockScope = $rootScope.$new();
        mockScope.resultObject = {
            redirectURL: "Application.html", 
        };
        _contextService = contextService;
        _validationService = validationService;
        _crudlistViewmodel = crudlistViewmodel;
        controller = $controller('ApplicationController', {
            $scope: mockScope,
            contextService: _contextService,
            validationService: _validationService,
            crudlistViewmodel: _crudlistViewmodel
        });
        _contextService.clearContext();
    }));

    it("Receiving List Result From Server", function () {

        var serverResult = {
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

        var data = [{ id: "100" }, { id: "200" }];
        var schema = { idFieldName: "id", displayables: [], properties: {} }
        mockScope.schema = schema;

        spyOn(mockScope, "$broadcast");
        spyOn(_crudlistViewmodel, "initGridFromServerResult");

        mockScope.toListSchema(data, schema);

        expect(mockScope.datamap).toEqual(data);
        expect(mockScope.schema).toEqual(schema);
        expect(mockScope.isList).toBeTruthy();

        var eventData = {
            //here we have to reproduce that the request is coming from the server, so use resultObject as the name.
            //check crud_list#gridRefreshed
            resultObject: data,
            schema: schema,
            pageResultDto: {},
        }

        expect(_crudlistViewmodel.initGridFromServerResult).toHaveBeenCalledWith(eventData, null);

    });


});