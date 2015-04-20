﻿describe('Application Test', function () {

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
            resultObject: [{ fields: { id: "100" } }, { fields: { id: "200" } }],

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

        var data = [{ fields: { id: "100" } }, { fields: { id: "200" } }];
        var schema ={idFieldName:"id",displayables:[],properties: {} }
        
        mockScope.toListSchema(data,schema);

        expect(mockScope.datamap).toEqual(data);
        expect(mockScope.schema).toEqual(schema);
        expect(mockScope.isList).toBeTruthy();
        
    });


});