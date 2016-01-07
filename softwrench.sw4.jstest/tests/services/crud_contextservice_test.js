describe('crudContextService Test', function () {

    var mockCompositionResult = {
        "attachment_": {
            "paginationData": {
                "totalCount" : 5
            },
            "list": [{}, {}, {}, {}, {}]
        },
        "commlog_": {
            "paginationData": {
                "totalCount": 11
            },
            "list": [{}, {}, {}, {}, {}, {}, {}, {}, {}, {}] // Only 10 object in the list because it is paginated to 10 per page
        },
        "worklog_": {
            "paginationData": {
                "totalCount": 10
            },
            "list": [{}, {}, {}, {}, {}, {}, {}, {}, {}, {}]
        }
    };

    var attachmentTab = { "tabId": "attachment_" };
    var commlogTab = { "tabId": "commlog_" };
    var worklogTab = { "tabId": "worklog_" };

    var crudContextService;
    beforeEach(module('sw_layout'));
    beforeEach(inject(function (crudContextHolderService) {
        crudContextService = crudContextHolderService;
    }));
    beforeEach(function() { crudContextService.compositionsLoaded(mockCompositionResult); });

    it('composition record counts added', function () {
        // Get the record counts for the compositions
        var attachmentLength = crudContextService.getTabRecordCount(attachmentTab);
        var commlogLength = crudContextService.getTabRecordCount(commlogTab);
        var worklogLength = crudContextService.getTabRecordCount(worklogTab);
        // Make sure they match the values from the mockCompositionResult
        expect(attachmentLength).toBe(5);
        expect(commlogLength).toBe(11);
        expect(worklogLength).toBe(10);
    });

    it('should remove record counts on disposeDetail', function () {
        // Dispose the values from the _crudContext
        crudContextService.disposeDetail();
        // Get the recond counts for the compositions
        var attachmentLength = crudContextService.getTabRecordCount(attachmentTab);
        var commlogLength = crudContextService.getTabRecordCount(commlogTab);
        var worklogLength = crudContextService.getTabRecordCount(worklogTab);
        // Make sure they have been removed
        expect(attachmentLength).toBe(0);
        expect(commlogLength).toBe(0);
        expect(worklogLength).toBe(0);
    });

    it('should remove record counts on gridLoaded', function() {
        // Call the gridLoaded function
        crudContextService.gridLoaded({});
        // Get the record counts for the compositions
        var attachmentLength = crudContextService.getTabRecordCount(attachmentTab);
        var commlogLength = crudContextService.getTabRecordCount(commlogTab);
        var worklogLength = crudContextService.getTabRecordCount(worklogTab);
        // Make sure they have been removed
        expect(attachmentLength).toBe(0);
        expect(commlogLength).toBe(0);
        expect(worklogLength).toBe(0);
    });

    it('should remove record counts on detailLoaded', function () {
        // Call the detailLoaded function
        debugger;
        crudContextService.detailLoaded();
        // Get the record counts for the compositions
        var attachmentLength = crudContextService.getTabRecordCount(attachmentTab);
        var commlogLength = crudContextService.getTabRecordCount(commlogTab);
        var worklogLength = crudContextService.getTabRecordCount(worklogTab);
        // Make sure they have been removed
        expect(attachmentLength).toBe(0);
        expect(commlogLength).toBe(0);
        expect(worklogLength).toBe(0);
    });

});
