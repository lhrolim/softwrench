describe('Comm Log Actions test', function () {


    var mockScope;
    var controller, _contextService, _fieldService, _applicationService, _rootScope;
    var $q;

    var mockedResult1 = {
        data: {
            resultObject: {
                fields: {
                    "sendfrom": 'origfrom@a.com',
                    "sendto": 'origto@a.com,origto2@a.com',
                    "cc": 'cc1@a.com,cc2@a.com',
                    "subject": 'test subject',
                    "message": 'original message',
                }
            }
        }
    }


    //init app --> first action usually
    beforeEach(angular.mock.module('sw_layout'));


    beforeEach(function () {
        module(function ($provide) {
            //mocking a simpler constant
            $provide.constant('commlog.messagetemplate', "{0} {1} {2} {3} {4}");
        });
    });


    beforeEach(angular.mock.inject(function ($rootScope, $scope, contextService, fieldService, applicationService, _$q_) {

        mockScope = $rootScope.$new();
        _rootScope = $rootScope;
        _contextService = contextService;
        _fieldService = fieldService;
        _applicationService = applicationService;
        _fieldService = fieldService;
        $q = _$q_;

        controller = $controller('CommLogActionsController', {
            $scope: mockScope,
            contextService: _contextService,
            applicationService: _applicationService,
            fieldService: _fieldService,
        });
    }));

    it("Reply all append all entries", function() {
        
        //this would be an entry on the list
        var mockedcommlog = {
            "commloguid" : 10  
        }

        //result.data.resultObject.fields

       

        spyOn(_applicationService).and.callFake(function(application, schema, parameters) {
            return _$q_.$when(mockedResult1);
        });

        //real call
        mockScope.replyAll(mockedcommlog);

        var resultEmail = {
            "sendfrom": 'origfrom@a.com',
            "sendto": 'origto@a.com,origto2@a.com',
            "cc": 'cc1@a.com,cc2@a.com',
            "subject": 'test subject',
            "message": 'original message',
        }
        
        expect(_rootScope.$emit).toHaveBeenCalledWith('sw.composition.edit', resultEmail);
        


    });


});