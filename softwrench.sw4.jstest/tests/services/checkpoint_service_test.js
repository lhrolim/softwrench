describe('CheckPoint Service Test', function () {



    var checkpointService;
    var crudContextHolderService;


    beforeEach(module('sw_layout'));
    beforeEach(inject(function (_crudContextHolderService_, _checkpointService_) {
        checkpointService = _checkpointService_;
        crudContextHolderService = _crudContextHolderService_;

    }));

    it('Test checkpoint --> prevent default if filter applied', function () {
        //creating mock user
        crudContextHolderService.setSelectedFilter({});
        checkpointService.createGridCheckpoint(SchemaPojo.WithId("list", "person"), new SearchDTO({ searchSort: "test desc" }));
        let filter = checkpointService.getCheckPointAsFilter("person", "list");
        expect(filter).toBeNull();
        crudContextHolderService.setSelectedFilter();
        checkpointService.createGridCheckpoint(SchemaPojo.WithId("list", "person"), new SearchDTO({ searchSort: "test desc" }));
        filter = checkpointService.getCheckPointAsFilter("person", "list");
        expect(filter).not.toBeNull();
        //TODO: refactor checkpointService, rethink of the need of pass this data
        // without this call other tests were being affected by the presence of a checkpoint
        checkpointService.clearCheckpoints();
    });



});