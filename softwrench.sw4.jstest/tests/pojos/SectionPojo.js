
class SectionPojo {


    static WithDisplayables(displayables,label) {
        return FieldMetadataPojo.Section(displayables,"vertical",label);
        
    }

    static HorizontalWithDisplayables(displayables) {
        return FieldMetadataPojo.Section(displayables,"horizontal");
    }

}