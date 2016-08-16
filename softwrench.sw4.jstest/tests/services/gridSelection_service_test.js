describe("gridSelectionService Test", function () {

    var schema = {
        idFieldName: "id",
        properties: {
            "list.selectionstyle": "multiple"
        }
    };

    var schemaWithServerLoading = {
        idFieldName: "id",
        properties: {
            "list.selectionstyle": "multiple",
            "list.loadwithselection": "true"
        }
    };

    var selectionService;
    var crudContextService;
    beforeEach(module("sw_layout"));
    beforeEach(inject(function (crudContextHolderService, gridSelectionService) {
        crudContextService = crudContextHolderService;
        selectionService = gridSelectionService;
    }));

    it("grid data changed 1", function () {
        var row1 = { "_#selected": false, id: 1 };
        var row2 = { "_#selected": false, id: 2 };
        var datamap = [row1, row2];
        crudContextService.addSelectionToBuffer("1", row1);
        crudContextService.updateCrudContext(schema, datamap);

        selectionService.gridDataChanged(datamap, schema);

        var selectionModel = crudContextService.getSelectionModel();
        expect(selectionModel.pageSize).toBe(2);
        expect(selectionModel.onPageSelectedCount).toBe(1);
        expect(selectionModel.selectAllValue).toBe(false);
        expect(row1["_#selected"]).toBe(true);
        expect(row2["_#selected"]).toBe(false);
    });

    it("grid data changed 2", function () {
        var row1 = { "_#selected": false, id: 1 };
        var row2 = { "_#selected": false, id: 2 };
        var datamap = [row1, row2];
        crudContextService.addSelectionToBuffer("1", row1);
        crudContextService.addSelectionToBuffer("2", row1);
        crudContextService.updateCrudContext(schema, datamap);

        selectionService.gridDataChanged(datamap, schema);

        var selectionModel = crudContextService.getSelectionModel();
        expect(selectionModel.pageSize).toBe(2);
        expect(selectionModel.onPageSelectedCount).toBe(2);
        expect(selectionModel.selectAllValue).toBe(true);
        expect(row1["_#selected"]).toBe(true);
        expect(row2["_#selected"]).toBe(true);
    });

    it("grid data changed with server loading", function () {
        var row1 = { "_#selected": true, id: 1 };
        var row2 = { "_#selected": false, id: 2 };
        var datamap = [row1, row2];
        crudContextService.addSelectionToBuffer("2", row1);
        crudContextService.updateCrudContext(schema, datamap);

        selectionService.gridDataChanged(datamap, schemaWithServerLoading);

        var selectionModel = crudContextService.getSelectionModel();
        expect(selectionModel.pageSize).toBe(2);
        expect(selectionModel.onPageSelectedCount).toBe(2);
        expect(selectionModel.selectAllValue).toBe(true);
        expect(row1["_#selected"]).toBe(true);
        expect(row2["_#selected"]).toBe(true);
    });

    it("toggle selection", function () {
        var row1 = { "_#selected": false, id: 1 };
        var row2 = { "_#selected": false, id: 2 };
        var datamap = [row1, row2];
        var selectionModel = crudContextService.getSelectionModel();
        crudContextService.updateCrudContext(schema, datamap);

        selectionService.gridDataChanged(datamap, schema);
        selectionService.toggleSelection(row1, schema);

        var buffer = selectionModel.selectionBuffer;
        expect(Object.keys(buffer).length).toBe(1);
        expect(buffer["1"].id).toBe(1);
        expect(selectionModel.selectAllValue).toBe(false);
    });

    it("toggle selection triggers select all", function () {
        var row1 = { "_#selected": false, id: 1 };
        var row2 = { "_#selected": false, id: 2 };
        var datamap = [row1, row2];
        var selectionModel = crudContextService.getSelectionModel();
        crudContextService.updateCrudContext(schema, datamap);

        selectionService.gridDataChanged(datamap, schema);
        selectionService.toggleSelection(row1, schema);
        selectionService.toggleSelection(row2, schema);

        var buffer = selectionModel.selectionBuffer;
        expect(Object.keys(buffer).length).toBe(2);
        expect(buffer["1"].id).toBe(1);
        expect(buffer["2"].id).toBe(2);
        expect(selectionModel.selectAllValue).toBe(true);

        selectionService.toggleSelection(row2, schema);

        buffer = selectionModel.selectionBuffer;
        expect(Object.keys(buffer).length).toBe(1);
        expect(buffer["1"].id).toBe(1);
        expect(selectionModel.selectAllValue).toBe(false);
    });

    it("toggle selection", function () {
        var row1 = { "_#selected": false, id: 1 };
        var row2 = { "_#selected": false, id: 2 };
        var datamap = [row1, row2];
        var selectionModel = crudContextService.getSelectionModel();
        crudContextService.updateCrudContext(schema, datamap);

        selectionService.gridDataChanged(datamap, schema);
        selectionService.toggleSelection(row1, schema);

        var buffer = selectionModel.selectionBuffer;
        expect(Object.keys(buffer).length).toBe(1);
        expect(buffer["1"].id).toBe(1);
        expect(selectionModel.selectAllValue).toBe(false);
    });



    it("select all changed", function () {
        var row1 = { "_#selected": false, id: 1 };
        var row2 = { "_#selected": false, id: 2 };
        var datamap = [row1, row2];
        var selectionModel = crudContextService.getSelectionModel();
        crudContextService.updateCrudContext(schema, datamap);

        selectionModel.selectAllValue = true;
        selectionService.selectAllChanged(datamap, schema);


        var buffer = selectionModel.selectionBuffer;
        expect(Object.keys(buffer).length).toBe(2);
        expect(buffer["1"].id).toBe(1);
        expect(buffer["2"].id).toBe(2);
        expect(row1["_#selected"]).toBe(true);
        expect(row2["_#selected"]).toBe(true);
    });

    it("select all changed", function () {
        var row1 = { "_#selected": true, id: 1 };
        var row2 = { "_#selected": true, id: 2 };
        var datamap = [row1, row2];
        var selectionModel = crudContextService.getSelectionModel();
        crudContextService.updateCrudContext(schema, datamap);

        selectionModel.selectAllValue = false;
        selectionService.selectAllChanged(datamap, schema);

        var buffer = selectionModel.selectionBuffer;
        expect(Object.keys(buffer).length).toBe(0);
        expect(row1["_#selected"]).toBe(false);
        expect(row2["_#selected"]).toBe(false);
    });

    it("clear selection", function () {
        var row1 = { "_#selected": true, id: 1 };
        var row2 = { "_#selected": true, id: 2 };
        var datamap = [row1, row2];
        var selectionModel = crudContextService.getSelectionModel();
        crudContextService.updateCrudContext(schema, datamap);
        selectionModel.selectAllValue = true;

        selectionService.clearSelection(datamap, schema);

        expect(selectionModel.selectAllValue).toBe(false);
        var buffer = selectionModel.selectionBuffer;
        expect(Object.keys(buffer).length).toBe(0);
        expect(row1["_#selected"]).toBe(false);
        expect(row2["_#selected"]).toBe(false);
    });
});