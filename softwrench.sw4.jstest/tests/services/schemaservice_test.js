describe("schemaService", function () {

    var schemaService;
    beforeEach(module("sw_layout"));
    beforeEach(inject(function (_schemaService_) {
        schemaService = _schemaService_;
    }));

    it("schemaService#isStereotype", function () {
        // empty
        expect(schemaService.isStereotype({ stereotype: null }, "some")).toBe(false);
        expect(schemaService.isStereotype({ stereotype: undefined }, "stereotype")).toBe(false);
        expect(schemaService.isStereotype({ stereotype: "" }, ["and", "another"])).toBe(false);
        expect(schemaService.isStereotype({ stereotype: "None" }, ["one", "more", "please"])).toBe(false);
        // empty with default
        expect(schemaService.isStereotype({ stereotype: null }, "some", true)).toBe(true);
        expect(schemaService.isStereotype({ stereotype: undefined }, "stereotype", true)).toBe(true);
        expect(schemaService.isStereotype({ stereotype: "" }, ["and", "another"], true)).toBe(true);
        expect(schemaService.isStereotype({ stereotype: "None" }, ["one", "more", "please"], true)).toBe(true);
        // non-empty with default -> false
        expect(schemaService.isStereotype({ stereotype: "whatiwant" }, "notthis", true)).toBe(false);
        expect(schemaService.isStereotype({ stereotype: "real.stereotype.forsure" }, ["not", "this", "either"], true)).toBe(false);
        // non-empty false
        expect(schemaService.isStereotype({ stereotype: "whatiwant" }, "notthis")).toBe(false);
        expect(schemaService.isStereotype({ stereotype: "real.stereotype.forsure" }, ["not", "this", "either"])).toBe(false);
        // non-empty true
        expect(schemaService.isStereotype({ stereotype: "ZYxxxxYZ" }, "xxxx")).toBe(true);
        expect(schemaService.isStereotype({ stereotype: "something.XXbbbXX.stereotype" }, ["aaa", "bbb", "ccc"])).toBe(true);
    });

    it("schemaService#isDetail", function () {
        // empty
        expect(schemaService.isDetail({ stereotype: null })).toBe(false);
        expect(schemaService.isDetail({ stereotype: undefined })).toBe(false);
        expect(schemaService.isDetail({ stereotype: "" })).toBe(false);
        expect(schemaService.isDetail({ stereotype: "None" })).toBe(false);
        // empty with default
        expect(schemaService.isDetail({ stereotype: null }, true)).toBe(true);
        expect(schemaService.isDetail({ stereotype: undefined }, true)).toBe(true);
        expect(schemaService.isDetail({ stereotype: "" }, true)).toBe(true);
        expect(schemaService.isDetail({ stereotype: "None" }, true)).toBe(true);
        // non-empty with default -> false
        expect(schemaService.isDetail({ stereotype: "list" }, true)).toBe(false);
        expect(schemaService.isDetail({ stereotype: "List" }, true)).toBe(false);
        expect(schemaService.isDetail({ stereotype: "awesome" }, true)).toBe(false);
        expect(schemaService.isDetail({ stereotype: "!@#$%¨&*()/" }, true)).toBe(false);
        expect(schemaService.isDetail({ stereotype: "dLeItSaTil" }, true)).toBe(false);
        // non-empty false
        expect(schemaService.isDetail({ stereotype: "list" })).toBe(false);
        expect(schemaService.isDetail({ stereotype: "List" })).toBe(false);
        expect(schemaService.isDetail({ stereotype: "awesome" })).toBe(false);
        expect(schemaService.isDetail({ stereotype: "!@#$%¨&*()/" })).toBe(false);
        expect(schemaService.isDetail({ stereotype: "dLeItSaTil" })).toBe(false);
        // non-empty true
        expect(schemaService.isDetail({ stereotype: "detail" })).toBe(true);
        expect(schemaService.isDetail({ stereotype: "Detail" })).toBe(true);
        expect(schemaService.isDetail({ stereotype: "edit.detail.sw.rules" })).toBe(true);
        expect(schemaService.isDetail({ stereotype: "createDetailForTheWin" })).toBe(true);
    });

    it("schemaService#isList", function () {
        // empty
        expect(schemaService.isList({ stereotype: null })).toBe(false);
        expect(schemaService.isList({ stereotype: undefined })).toBe(false);
        expect(schemaService.isList({ stereotype: "" })).toBe(false);
        expect(schemaService.isList({ stereotype: "None" })).toBe(false);
        // empty with default
        expect(schemaService.isList({ stereotype: null }, true)).toBe(true);
        expect(schemaService.isList({ stereotype: undefined }, true)).toBe(true);
        expect(schemaService.isList({ stereotype: "" }, true)).toBe(true);
        expect(schemaService.isList({ stereotype: "None" }, true)).toBe(true);
        // non-empty with default -> false
        expect(schemaService.isList({ stereotype: "detail" }, true)).toBe(false);
        expect(schemaService.isList({ stereotype: "Detail" }, true)).toBe(false);
        expect(schemaService.isList({ stereotype: "awesome" }, true)).toBe(false);
        expect(schemaService.isList({ stereotype: "!@#$%¨&*()/" }, true)).toBe(false);
        expect(schemaService.isList({ stereotype: "dLeItSaTil" }, true)).toBe(false);
        // non-empty false
        expect(schemaService.isList({ stereotype: "detail" })).toBe(false);
        expect(schemaService.isList({ stereotype: "Detail" })).toBe(false);
        expect(schemaService.isList({ stereotype: "awesome" })).toBe(false);
        expect(schemaService.isList({ stereotype: "!@#$%¨&*()/" })).toBe(false);
        expect(schemaService.isList({ stereotype: "dLeItSaTiSl" })).toBe(false);
        // non-empty true
        expect(schemaService.isList({ stereotype: "list" })).toBe(true);
        expect(schemaService.isList({ stereotype: "List" })).toBe(true);
        expect(schemaService.isList({ stereotype: "crud.list.multiselect" })).toBe(true);
        expect(schemaService.isList({ stereotype: "gridListIsGreat" })).toBe(true);
    });
});