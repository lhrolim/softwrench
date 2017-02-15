describe('SubmitService Commons Test', function () {


    var submitServiceCommons;

    beforeEach(() => {
        module('sw_layout');
        inject((_submitServiceCommons_) => {
            submitServiceCommons = _submitServiceCommons_;
        });
    });





    it('Test Attribute to server replacement', () =>{
        const field1 = FieldMetadataPojo.Required("attr1");
        field1.attributeToServer = "attr1toserver";

        const fieldForReplacement = FieldMetadataPojo.Ordinary("attr2");
        fieldForReplacement.attributeToServer = "attr2toserver";
        const schema= SchemaPojo.WithIdAndDisplayables("detail", [field1, FieldMetadataPojo.Section(fieldForReplacement)]);

        const translated = submitServiceCommons.applyTransformationsForSubmission(schema, { "attr1": "x", "attr2": "y" }, { "attr1": "x", "attr2": "y" });
        expect(translated["attr1toserver"]).toBe("x");
        expect(translated["attr2toserver"]).toBe("y");
        expect(translated["attr1"]).toBeUndefined();
        expect(translated["attr2"]).toBeUndefined();

    });

   


});
