
class FieldMetadataPojo {

    static ForAssociation(associationKey,target) {

        return {
            associationKey: associationKey,
            target: target || associationKey,
        };
    }



   
}