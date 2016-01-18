﻿describe("Test For BatchWOService", function () {



    var batchWorkorderService;
    var alertService;
    var $rootScope;
    var restService;
    var $httpBackend;
    var contextService;
    var redirectService;

    beforeEach(module("sw_layout"));
    beforeEach(module("firstsolar"));

    var sharedItem = {
        "location": 10,
        "description": "loc",
        "summary": "s"
    }

    beforeEach(inject(function ($injector, _alertService_, _$rootScope_, _restService_, _$httpBackend_, _contextService_, _redirectService_) {
        batchWorkorderService = $injector.getInstance("firstsolar.batchWorkorderService");
        alertService = _alertService_;
        $rootScope = _$rootScope_;
        restService = _restService_;
        $httpBackend = _$httpBackend_;
        contextService = _contextService_;
        redirectService = _redirectService_;
    }));

    it("Test Submission for Location Zero entries", function () {
        spyOn(alertService, "alert");

        //real call
        batchWorkorderService.submitBatch([], "location");

        expect(alertService.alert).toHaveBeenCalledWith("Please, select at least one entry to confirm the batch");
    });

    it("Test Submission for Location 2 entries no changes", (function (done) {

        contextService.set("batchshareddata", sharedItem, true);

        var items = [
        {
            fields: {
                "location": 10,
                "description": "loc",
                "summary": "s"
            }
        },
        {
            fields: {
                "location": 11,
                "description": "loc",
                "summary": "s"
            }
        }];

        var submissionData = {
            sharedData: sharedItem,
            specificData: {
                10: null,
                11: null
            }
        }

        var appResponse = { "applicationName": "workorder" };

        spyOn(redirectService, "redirectFromServerResponse")

        $httpBackend.expectPOST("/api/generic/FirstSolarWorkorderBatch/SubmitBatch?batchType=location", JSON.stringify(submissionData)).respond(appResponse);

        //real call
        batchWorkorderService.submitBatch(items, "location").then(function (result) {
            expect(redirectService.redirectFromServerResponse).toHaveBeenCalledWith(appResponse, "workorder");
        }).finally(done);



        //this is needed to trigger the promises resolutions!

        $httpBackend.flush();
        $rootScope.$digest();

    }));


    it("Test Submission for Location 2 entries one changed", (function (done) {

        contextService.set("batchshareddata", sharedItem, true);

        var items = [
        {
            fields: {
                "location": 10,
                "description": "loc",
                "summary": "s"
            }
        },
        {
            fields: {
                "location": 11,
                "description": "loc2",
                "summary": "s"
            }
        }];

        var submissionData = {
            sharedData: sharedItem,
            specificData: {
                10: null,
                11: { "description": "loc2" }
            }
        }

        var appResponse = { "applicationName": "workorder" };

        spyOn(redirectService, "redirectFromServerResponse");

        $httpBackend.expectPOST("/api/generic/FirstSolarWorkorderBatch/SubmitBatch?batchType=location", JSON.stringify(submissionData)).respond(appResponse);

        //real call
        batchWorkorderService.submitBatch(items, "location").then(function (result) {
            expect(redirectService.redirectFromServerResponse).toHaveBeenCalledWith(appResponse, "workorder");
        }).finally(done);



        //this is needed to trigger the promises resolutions!
        $httpBackend.flush();
        $rootScope.$digest();

    }));


});