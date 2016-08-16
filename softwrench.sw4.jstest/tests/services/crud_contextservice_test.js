describe('crudContextService Test', function () {

    var mockCompositionResult = {
        "attachment_": {
            "paginationData": {
                "totalCount": 5
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

    var mockedSchemaWithReverseAssociation = {
        displayables: [
            {
                displayables: [
                    {
                        associationKey: "reverse_",
                        target: "reversetarget",
                        reverse: true,
                        type: "ApplicationAssociationDefinition"
                    }
                ],
                type: "ApplicationSection"

            },
            {
                associationKey: "other_",
                target: "_othertarget",
                reverse: true,
                type: "ApplicationAssociationDefinition"
            }
        ]
    };

    var attachmentTab = { "tabId": "attachment_" };
    var commlogTab = { "tabId": "commlog_" };
    var worklogTab = { "tabId": "worklog_" };

    var crudContextService;
    beforeEach(module('sw_layout'));
    beforeEach(inject(function (crudContextHolderService) {
        crudContextService = crudContextHolderService;
    }));
    beforeEach(function () { crudContextService.compositionsLoaded(mockCompositionResult); });

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

    it('should remove record counts on gridLoaded', function () {
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

    it("clears selected buffer on app change", function () {
        var row1 = { a: "1" };
        var row2 = { a: "2" };

        // add rows
        crudContextService.addSelectionToBuffer("1", row1);
        crudContextService.addSelectionToBuffer("2", row2);

        // verify buffer
        var buffer = crudContextService.getSelectionModel().selectionBuffer;
        expect(Object.keys(buffer).length).toBe(2);
        expect(buffer["1"].a).toBe("1");
        expect(buffer["2"].a).toBe("2");

        // updates context
        crudContextService.updateCrudContext({}, {});

        // verify buffer

        buffer = crudContextService.getSelectionModel().selectionBuffer;
        expect(Object.keys(buffer).length).toBe(2);
        expect(buffer["1"].a).toBe("1");
        expect(buffer["2"].a).toBe("2");

        // chage application
        crudContextService.applicationChanged({}, {});

        // verify buffer
        buffer = crudContextService.getSelectionModel().selectionBuffer;
        expect(Object.keys(buffer).length).toBe(0);
    });

    it("independent selected buffer from different panels", function () {
        var row1 = { a: "1" };
        var row2 = { a: "2" };
        var row3 = { a: "3" };

        // add rows
        crudContextService.addSelectionToBuffer("1", row1);
        crudContextService.addSelectionToBuffer("2", row2);
        crudContextService.addSelectionToBuffer("3", row3, "#modal");

        // verify buffers
        var buffer = crudContextService.getSelectionModel().selectionBuffer;
        expect(Object.keys(buffer).length).toBe(2);
        expect(buffer["1"].a).toBe("1");
        expect(buffer["2"].a).toBe("2");
        buffer = crudContextService.getSelectionModel("#modal").selectionBuffer;
        expect(Object.keys(buffer).length).toBe(1);
        expect(buffer["3"].a).toBe("3");

        // clear modal context
        crudContextService.clearCrudContext("#modal");

        // verify buffers
        buffer = crudContextService.getSelectionModel().selectionBuffer;
        expect(Object.keys(buffer).length).toBe(2);
        expect(buffer["1"].a).toBe("1");
        expect(buffer["2"].a).toBe("2");
        buffer = crudContextService.getSelectionModel("#modal").selectionBuffer;
        expect(Object.keys(buffer).length).toBe(0);
    });

    it("update lazy options, fill datamap when reverse, FIX for SWWEB-2013", function () {

        var dm = {
        }
        crudContextService.updateCrudContext(mockedSchemaWithReverseAssociation, dm);
        dm = crudContextService.rootDataMap();
        expect(dm["reversetarget"]).toBe(undefined);
        crudContextService.updateLazyAssociationOption("reverse_", { value: "xxx", label: "yyy" }, true);
        dm = crudContextService.rootDataMap();
        expect(dm["reversetarget"]).toBe("xxx");
        var result = crudContextService.fetchLazyAssociationOption("reverse_", "xxx");
        expect(result).toEqual({ value: "xxx", label: "yyy" });
    });
});
