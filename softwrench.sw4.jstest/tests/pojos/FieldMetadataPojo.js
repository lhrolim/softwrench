
class FieldMetadataPojo {

    static ForAssociation(associationKey,target) {

        return {
            associationKey,
            target: target || associationKey,
        };
    }

    static Hidden(attribute) {

        return {
            attribute,
            isHidden:true
        };
    }

    static Required(attribute,label) {
        label = label || attribute;

        return {
            attribute,
            label,
            requiredExpression: true
        };
    }

    static Ordinary(attribute,label) {
        label = label || attribute;

        return {
            attribute,
            label
        };
    }

    static Section(displayables) {

        if (!(displayables instanceof Array)) {
            displayables = [displayables];
        }

        return {
            displayables,
            type:"ApplicationSection"
        };
    }



   
}