describe('Field Service Test', function () {


    //declare used services
    var fieldService;

    //instantiate used modules
    beforeEach(module('sw_layout'));

    beforeEach(inject(function (_fieldService_) {
        fieldService = _fieldService_;
    }));


    it("testing finding outer vertical section", () => {
        var schema = SchemaPojo.WithId("detail");
        const fieldToSearch = FieldMetadataPojo.Ordinary("f1", "f1");
        //section
        schema.displayables[0] =
            SectionPojo.HorizontalWithDisplayables([fieldToSearch, FieldMetadataPojo.Ordinary("f2", "f2")]);
        
        const result = fieldService.locateFirstOuterVerticalSection(schema, fieldToSearch);
        expect(result.container).toBe(schema);
        expect(result.idx).toBe(0);

    });


    it("testing finding outer vertical section 2 levels", () => {
        var schema = SchemaPojo.WithId("detail");
        const fieldToSearch = FieldMetadataPojo.Ordinary("f1", "f1");
        const innerHorizontalSection = SectionPojo.HorizontalWithDisplayables([fieldToSearch, FieldMetadataPojo.Ordinary("f2", "f2")]);
        const verticalSection = SectionPojo.WithDisplayables([innerHorizontalSection]);
        //section
        schema.displayables[0] = verticalSection;
            

        const result = fieldService.locateFirstOuterVerticalSection(schema, fieldToSearch);
        expect(result.container).toBe(verticalSection);
        expect(result.idx).toBe(0);
    });


    it("testing finding outer vertical section 2 levels not first field", () => {
        var schema = SchemaPojo.WithId("detail");
        const fieldToSearch = FieldMetadataPojo.Ordinary("f1", "f1");
        const innerHorizontalSection = SectionPojo.HorizontalWithDisplayables([fieldToSearch, FieldMetadataPojo.Ordinary("f2", "f2")]);
        const innerHorizontalSection2 = SectionPojo.HorizontalWithDisplayables([FieldMetadataPojo.Ordinary("f3", "f3"), FieldMetadataPojo.Ordinary("f4", "f4") ]);
        const verticalSection = SectionPojo.WithDisplayables([innerHorizontalSection2,innerHorizontalSection]);
        //section
        schema.displayables[0] = verticalSection;

        const result = fieldService.locateFirstOuterVerticalSection(schema, fieldToSearch);
        expect(result.container).toBe(verticalSection);
        expect(result.idx).toBe(1);
    });

    it("testing finding outer 2 levels not first field", () => {
        var schema = SchemaPojo.WithId("detail22","test");

        const fieldToSearch = FieldMetadataPojo.Ordinary("f1", "f1");
        const innerHorizontalSection = SectionPojo.HorizontalWithDisplayables([fieldToSearch, FieldMetadataPojo.Ordinary("f2", "f2")]);
        const innerHorizontalSection2 = SectionPojo.HorizontalWithDisplayables([FieldMetadataPojo.Ordinary("f3", "f3"), FieldMetadataPojo.Ordinary("f4", "f4")]);
        const verticalSection = SectionPojo.WithDisplayables([innerHorizontalSection2, innerHorizontalSection]);
        //section
        schema.displayables[0] = verticalSection;

        const result = fieldService.locateOuterSection(schema, fieldToSearch);
        expect(result.container).toBe(innerHorizontalSection);
        expect(result.idx).toBe(0);
    });

    it("testing finding outer 2 levels not first field second position", () => {
        var schema = SchemaPojo.WithId("detail22", "test");

        const fieldToSearch = FieldMetadataPojo.Ordinary("f1", "f1");
        const innerHorizontalSection = SectionPojo.HorizontalWithDisplayables([FieldMetadataPojo.Ordinary("f2", "f2"), fieldToSearch]);
        const innerHorizontalSection2 = SectionPojo.HorizontalWithDisplayables([FieldMetadataPojo.Ordinary("f3", "f3"), FieldMetadataPojo.Ordinary("f4", "f4")]);
        const verticalSection = SectionPojo.WithDisplayables([innerHorizontalSection2, innerHorizontalSection]);
        //section
        schema.displayables[0] = verticalSection;

        const result = fieldService.locateOuterSection(schema, fieldToSearch);
        expect(result.container).toBe(innerHorizontalSection);
        expect(result.idx).toBe(1);
    });


    it("testing finding common container across several fields", () => {
        var schema = SchemaPojo.WithId("detail22", "test");

        const f2 = FieldMetadataPojo.Ordinary("f2", "f2");
        const f1 = FieldMetadataPojo.Ordinary("f1", "f1");
        const innerHorizontalSection = SectionPojo.HorizontalWithDisplayables([f2, f1]);
        const f3 = FieldMetadataPojo.Ordinary("f3", "f3");
        const f4 = FieldMetadataPojo.Ordinary("f4", "f4");

        const innerHorizontalSection2 = SectionPojo.HorizontalWithDisplayables([f3, f4]);
        const verticalSection = SectionPojo.WithDisplayables([innerHorizontalSection2, innerHorizontalSection]);

        //this first fied is just to test whether we return the correct index, should be skipped
        const spaceField = FieldMetadataPojo.Ordinary("space", "space");

        schema.displayables[0] = spaceField;

        schema.displayables[1] = verticalSection;

        //regardless of the order
        let result = fieldService.locateCommonContainer(schema, [f1,f4,f3,f2]);
        expect(result.container).toBe(verticalSection);
        expect(result.idx).toBe(1);


        result = fieldService.locateCommonContainer(schema, [f1, f4]);
        expect(result.container).toBe(verticalSection);
        expect(result.idx).toBe(1);
    });



    it("testing finding common container across several fields --> returning base schema", () => {
        var schema = SchemaPojo.WithId("detail22", "test");

        const f2 = FieldMetadataPojo.Ordinary("f2", "f2");
        const f1 = FieldMetadataPojo.Ordinary("f1", "f1");
        const innerHorizontalSection = SectionPojo.HorizontalWithDisplayables([f2, f1]);
        const f3 = FieldMetadataPojo.Ordinary("f3", "f3");
        const f4 = FieldMetadataPojo.Ordinary("f4", "f4");

        const innerHorizontalSection2 = SectionPojo.HorizontalWithDisplayables([f3, f4]);
        const verticalSection = SectionPojo.WithDisplayables([innerHorizontalSection2, innerHorizontalSection]);

        //this first fied is just to test whether we return the correct index, should be skipped
        const spaceField = FieldMetadataPojo.Ordinary("space", "space");

        schema.displayables[0] = spaceField;

        schema.displayables[1] = verticalSection;

        const result = fieldService.locateCommonContainer(schema, [spaceField, f1, f2, f3, f4]);
        expect(result.container).toBe(schema);
        expect(result.idx).toBe(0);
    });

    it("testing finding common container across several fields and no sections --> returning base schema", () => {

        //this first fied is just to test whether we return the correct index, should be skipped
        const spaceField = FieldMetadataPojo.Ordinary("space", "space");
        const f2 = FieldMetadataPojo.Ordinary("f2", "f2");
        const f1 = FieldMetadataPojo.Ordinary("f1", "f1");
        const f3 = FieldMetadataPojo.Ordinary("f3", "f3");
        const f4 = FieldMetadataPojo.Ordinary("f4", "f4");

        var schema = SchemaPojo.WithIdAndDisplayables("detail22",[spaceField,f1,f2,f3,f4], "test");
        const result = fieldService.locateCommonContainer(schema, [f1, f2, f3, f4]);
        expect(result.container).toBe(schema);
        expect(result.idx).toBe(1);
    });

    it("testing field sorting", () => {

        //this first fied is just to test whether we return the correct index, should be skipped
        const spaceField = FieldMetadataPojo.Ordinary("space", "space");
        const f2 = FieldMetadataPojo.Ordinary("f2", "f2");
        const f1 = FieldMetadataPojo.Ordinary("f1", "f1");
        const f3 = FieldMetadataPojo.Ordinary("f3", "f3");
        const f4 = FieldMetadataPojo.Ordinary("f4", "f4");

        var schema = SchemaPojo.WithIdAndDisplayables("detail22", [spaceField, f1, f2, f3, f4], "test");
        const result = fieldService.sortBySchemaIdx(schema, [f2, f1, f4, f3]);
        expect(result.map(a => a.attribute)).toEqual([f1,f2,f3,f4].map(a=>a.attribute));
    });
  


  


});