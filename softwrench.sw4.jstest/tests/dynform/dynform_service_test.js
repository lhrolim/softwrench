describe('DynamicFormService Test', function () {


    //declare used services
    var dynFormService, crudContextHolderService;

    //instantiate used modules
    beforeEach(module('sw_layout'));

    beforeEach(inject(function (_dynFormService_, _crudContextHolderService_) {
        dynFormService = _dynFormService_;
        crudContextHolderService = _crudContextHolderService_;
    }));


    it("testing creating a section inside of a section", () => {
        
        const f1 = FieldMetadataPojo.Ordinary("f1");
        const f2 = FieldMetadataPojo.Ordinary("f2");
        const f3 = FieldMetadataPojo.Ordinary("f3");

        const verticalSection = SectionPojo.WithDisplayables([f1, f2, f3], "v1");
        var schema = SchemaPojo.WithIdAndDisplayables("detail",[verticalSection]);
        crudContextHolderService.currentSchema(null, schema);

        dynFormService.toggleSectionSelection(f1);
        dynFormService.toggleSectionSelection(f2);

        const modalData = {
            headerlabel: "h1",
            sectionorientation:"horizontal"
        }

        try {
            schema = dynFormService.doCreateEnclosingSection(modalData);
            expect(schema.displayables.length).toBe(1);
            //outer vertical section
            expect(schema.displayables[0].displayables.length).toBe(2);
            //inner horizontal section with 2 fields
            expect(schema.displayables[0].displayables[0].type).toBe("ApplicationSection");
            expect(schema.displayables[0].displayables[0].displayables.length).toBe(2);
            //remaining field
            expect(schema.displayables[0].displayables[1].type).toBe("ApplicationFieldDefinition");
        } finally {
            dynFormService.resetUpdateMode();
        }
        



    });


    it("testing creating a section at root schema", () => {

        const f1 = FieldMetadataPojo.Ordinary("f1");
        const f2 = FieldMetadataPojo.Ordinary("f2");
        const f3 = FieldMetadataPojo.Ordinary("f3");

//        const verticalSection = SectionPojo.WithDisplayables([f1, f2, f3], "v1");
        var schema = SchemaPojo.WithIdAndDisplayables("detail", [f1,f2,f3]);
        crudContextHolderService.currentSchema(null, schema);

        
        dynFormService.toggleSectionSelection(f1);
        dynFormService.toggleSectionSelection(f2);

        const modalData = {
            headerlabel: "h1",
            sectionorientation: "horizontal"
        }
        try {
            schema = dynFormService.doCreateEnclosingSection(modalData);
            //output shall be one horizontal section and one remaining field
            expect(schema.displayables.length).toBe(2);
            //outer horizontal section
            expect(schema.displayables[0].displayables.length).toBe(2);
            expect(schema.displayables[0].orientation).toBe("horizontal");
            //remaining field
            expect(schema.displayables[1].type).toBe("ApplicationFieldDefinition");
        } finally {
            dynFormService.resetUpdateMode();
        }

    });


   
  


  


});