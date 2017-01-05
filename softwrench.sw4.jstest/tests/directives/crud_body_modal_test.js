describe('Crud Body Modal Test', function () {

    var mockScope;
    var fieldService;
    var $rootScope;
    var modalService;
    var $q;
    var crudContextHolderService;
    var associationService;
    var commandService;

    var $httpBackend;

    //init app --> first action usually
    beforeEach(() => {


        angular.mock.module('sw_layout');
        angular.mock.inject(function (_$rootScope_, $controller, _$q_, _crudContextHolderService_, _modalService_, _associationService_, _fieldService_, _commandService_) {
            $rootScope = _$rootScope_;
            mockScope = $rootScope.$new();

            const element = angular.element('<div></div>');
            crudContextHolderService = _crudContextHolderService_;
            $q = _$q_;
            modalService = _modalService_;
            associationService = _associationService_;
            fieldService = _fieldService_;
            commandService = _commandService_;

            $controller('ExtractedCrudBodyModalController', {
                $scope: mockScope,
                $element: element
            });



        });

    });

    it("Load Modal--> Register primary command", done=> {


        const modalDm = WorklogPojo.DetailItem("10", "test", "test ld");
        const modalSchema = WorklogPojo.DetailSchema();
        const savefn = () => { return $q.when() };

        const modalData = {
            datamap: modalDm,
            schema: modalSchema,
            savefn
        }

        expect(crudContextHolderService.isShowingModal()).toBeFalsy();

        spyOn(associationService, "loadSchemaAssociations").and.returnValue($q.when());
        spyOn(fieldService, "fillDefaultValues");
        const saveCommand = { id: "save", label: "save", primary:true };

        spyOn(commandService, "getBarCommands").and.returnValue([{ id: "cancel", label: "cancel" }, saveCommand]);


        mockScope.showModal(modalData);

        expect(associationService.loadSchemaAssociations).toHaveBeenCalledWith(modalDm, modalSchema, { contextData: new ContextData("#modal") });
        expect(fieldService.fillDefaultValues).toHaveBeenCalled();

        expect(crudContextHolderService.isShowingModal()).toBeTruthy();
        expect(crudContextHolderService.rootDataMap("#modal")).toBe(modalDm);
        expect(crudContextHolderService.currentSchema("#modal")).toBe(modalSchema);
        expect(crudContextHolderService.getSaveFn()).toBe(savefn);
        expect(crudContextHolderService.getPrimaryCommand()).toBe(saveCommand);

        mockScope.closeModal();
        

        expect(crudContextHolderService.isShowingModal()).toBeFalsy();
        expect(crudContextHolderService.rootDataMap("#modal")).toBeNull();
        expect(crudContextHolderService.currentSchema("#modal")).toBeNull();

        expect(crudContextHolderService.getSaveFn()).toBeNull();
        expect(crudContextHolderService.getPrimaryCommand()).toBeUndefined();

        done();
        $rootScope.$digest();

    });


   



});