describe('crudContextService Test', function () {

    var mockCompositionResult = {
        "attachment_": {
            "list": [{}, {}, {}]
        },
        "commlog_": {
            "list": [{}]
        },
        "worklog_": {
            "list": [{}, {}]
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
        expect(attachmentLength).toBe(3);
        expect(commlogLength).toBe(1);
        expect(worklogLength).toBe(2);
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
        crudContextService.detailLoaded({});
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
