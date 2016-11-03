
class FieldMetadataPojo {

    static ForAssociation(associationKey,target) {

        return {
            associationKey: associationKey,
            target: target || associationKey,
        };
    }

    static Hidden(attribute) {

        return {
            attribute: attribute,
            isHidden:true
        };
    }



   
}