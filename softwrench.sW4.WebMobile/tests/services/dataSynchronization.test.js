describe("data synchronization suite",()=>{

    var dataSynchronizationService;

    var offlineCompositionService,dispatcherService,$q,swdbDAO, entities, attachmentDataSynchronizationService;


    beforeEach(module("softwrench"));
  
    beforeEach(inject(function (_swdbDAO_, _$q_, _$rootScope_, _dispatcherService_, _offlineEntities_,_attachmentDataSynchronizationService_) {
        swdbDAO = _swdbDAO_;
        $q = _$q_;
        $rootScope = _$rootScope_
        dispatcherService = _dispatcherService_;
        entities = _offlineEntities_;
        attachmentDataSynchronizationService = _attachmentDataSynchronizationService_;


    
        // clear statements and store them after the promise is resolved for comparing
     
    }));

    beforeEach(inject(function (_dataSynchronizationService_) {
        dataSynchronizationService = _dataSynchronizationService_;
    }));


    it("should append composition queries to array",(done)=>{


        var data = readJSON('tests/resources/jsons/syncdata/sampleresponse.json');

        let compositionCount =0;

        data.compositionData.forEach(element => {
            compositionCount+= element.newdataMaps.length;
            compositionCount+= element.insertOrUpdateDataMaps.length;
        });

        //one deletion array per composition, to wipe old data
        compositionCount+= data.compositionData.length;

        let topApplicationCount =0;
        data.topApplicationData.forEach(element => {
            topApplicationCount+= element.newdataMaps.length;
            topApplicationCount+= element.insertOrUpdateDataMaps.length;
        });


        spyOn(attachmentDataSynchronizationService, "generateAttachmentsQueryArray").and.callFake(function (arr) {
            return $q.when().then(function () {
                return [];
            });
        });

        dataSynchronizationService.generateQueriesPromise({data}).then((results)=>{
            //53
            expect(results.numberOfDownloadedItems).toEqual(topApplicationCount);
            //379
            expect(topApplicationCount + compositionCount).toEqual(results.queryArray.length);
        }).finally(done);
        $rootScope.$digest();
    });

});