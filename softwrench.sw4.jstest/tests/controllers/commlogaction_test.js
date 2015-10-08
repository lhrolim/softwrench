describe('Comm Log Actions test', function () {


    var mockScope;
    var controller, _contextService, _fieldService, _applicationService, _rootScope;
    var $q;


    var clientdefaultemail = "default@default.com";
    var useremail = "useremail@a.com";
    


    //init app --> first action usually
    beforeEach(angular.mock.module('sw_layout'));



    beforeEach(function () {
        module(function ($provide) {
            //mocking a simpler constant
            $provide.constant('commlog_messagheader', "From:{0} To:{1} CC:{2} Subject:{3} Message:{4}");
        });
    });


    beforeEach(angular.mock.inject(function ($rootScope, $controller, contextService, fieldService, applicationService, _$q_) {

        _rootScope = $rootScope;
        mockScope = $rootScope.$new();
        _contextService = contextService;
        _fieldService = fieldService;
        _applicationService = applicationService;
        _fieldService = fieldService;
        $q = _$q_;

        mockScope.compositiondetailschema = {
            displayables: []
        };

        controller = $controller('CommLogActionsController', {
            $scope: mockScope,
            contextService: _contextService,
            applicationService: _applicationService,
            fieldService: _fieldService,
        });
    }));

    function testSetup(inputData, outputData, actionfn,hasSystemDefault) {
        spyOn(mockScope, "$emit");

        spyOn(_contextService, "getUserData").and.returnValue({ email: useremail });

        spyOn(_applicationService, "getApplicationDataPromise").and.callFake(function (application, schema, parameters) {
            return $q.when(inputData);
        });

        spyOn(_fieldService, "fillDefaultValues").and.callFake(function (displayables, clonedItem, scope) {
            if (hasSystemDefault) {
                clonedItem["sendfrom"] = clientdefaultemail;
            }
            return clonedItem;
        });

        //let´s pretend the date is a string, to make tests easier but it will be the current date!!
        spyOn(_fieldService, "currentDate").and.returnValue("mockeddate!");


        mockScope[actionfn]({});
        //this is needed to trigger the promises resolutions!
        _rootScope.$digest();
        expect(mockScope.$emit).toHaveBeenCalledWith('sw.composition.edit', outputData);
    }

    it("Reply All, with default email not present, with system default", function () {

        var mockedResult1 = {
            data: {
                resultObject: {
                    fields: {
                        "sendfrom": 'origfrom@a.com',
                        "sendto": 'origto@a.com,origto2@a.com',
                        "cc": 'cc1@a.com,cc2@a.com',
                        "subject": 'test subject',
                        "message": 'original message',
                        "commloguid": 10
                    }
                }
            }
        }

        var resultCompositionData = {
            "sendfrom": clientdefaultemail,
            "sendto": ['origfrom@a.com', 'origto@a.com', 'origto2@a.com'],
            "cc": ['cc1@a.com', 'cc2@a.com'],
            "subject": 'Re: test subject',
            "message": "From:origfrom@a.com To:origto@a.com,origto2@a.com CC:cc1@a.com,cc2@a.com Subject:test subject Message:original message",
            "commloguid": null,
            "createdate": "mockeddate!"
        }

        testSetup(mockedResult1, resultCompositionData, "replyAll",true);

    });

    it("Reply All, with default email present with system default --> from email not carried into to", function () {

        var mockedResult1 = {
            data: {
                resultObject: {
                    fields: {
                        "sendfrom": 'origfrom@a.com',
                        "sendto": 'origto@a.com,origto2@a.com,default@default.com',
                        "cc": 'cc1@a.com,cc2@a.com',
                        "subject": 'test subject',
                        "message": 'original message',
                        "commloguid": 10
                    }
                }
            }
        }

        var resultCompositionData = {
            "sendfrom": clientdefaultemail,
            "sendto": ['origfrom@a.com', 'origto@a.com', 'origto2@a.com'],
            "cc": ['cc1@a.com', 'cc2@a.com'],
            "subject": 'Re: test subject',
            "message": "From:origfrom@a.com To:origto@a.com,origto2@a.com,default@default.com CC:cc1@a.com,cc2@a.com Subject:test subject Message:original message",
            "commloguid": null,
            "createdate": "mockeddate!"
        }

        testSetup(mockedResult1, resultCompositionData, "replyAll",true);

    });

    it("Reply All, with default email not present, with no system default", function () {

        var mockedResult1 = {
            data: {
                resultObject: {
                    fields: {
                        "sendfrom": 'origfrom@a.com',
                        "sendto": 'origto@a.com,origto2@a.com,default@default.com',
                        "cc": null,
                        "subject": 'test subject',
                        "message": 'original message',
                        "commloguid": 10
                    }
                }
            }
        }

        var resultCompositionData = {
            "sendfrom": useremail,
            "commloguid": null,
            "sendto": ['origfrom@a.com', 'origto@a.com', 'origto2@a.com', 'default@default.com'],
            "cc": null,
            "subject": 'Re: test subject',
            "message": "From:origfrom@a.com To:origto@a.com,origto2@a.com,default@default.com CC: Subject:test subject Message:original message",
            "createdate": "mockeddate!"
        }

        testSetup(mockedResult1, resultCompositionData, "replyAll");

    });

    it("Reply with default email not present, with no system default", function () {

        var mockedResult1 = {
            data: {
                resultObject: {
                    fields: {
                        "sendfrom": 'origfrom@a.com',
                        "sendto": 'origto@a.com,origto2@a.com,default@default.com',
                        "cc": null,
                        "subject": 'test subject',
                        "message": 'original message',
                        "commloguid": 10
                    }
                }
            }
        }

        var resultCompositionData = {
            "sendfrom": useremail,
            "sendto": ['origfrom@a.com'],
            "cc": null,
            "subject": 'Re: test subject',
            "message": "From:origfrom@a.com To:origto@a.com,origto2@a.com,default@default.com CC: Subject:test subject Message:original message",
            "commloguid": null,
            "createdate": "mockeddate!"
        }

        testSetup(mockedResult1, resultCompositionData, "reply");

    });

    it("Forward with system default", function () {

        var mockedResult1 = {
            data: {
                resultObject: {
                    fields: {
                        "sendfrom": 'origfrom@a.com',
                        "sendto": 'origto@a.com,origto2@a.com,default@default.com',
                        "cc": 'a@b.com',
                        "subject": 'test subject',
                        "message": 'original message',
                        "commloguid": 10
                    }
                }
            }
        }

        var resultCompositionData = {
            "sendfrom": useremail,
            "sendto": null,
            "cc": null,
            "subject": 'Fw: test subject',
            "message": "From:origfrom@a.com To:origto@a.com,origto2@a.com,default@default.com CC:a@b.com Subject:test subject Message:original message",
            "commloguid": null,
            "createdate": "mockeddate!"
        }

        testSetup(mockedResult1, resultCompositionData, "forward");

    });

});