
class FieldMetadataPojo {

    static ForAssociation(associationKey,target) {

        return {
            associationKey,
            attribute: target,
            target: target || associationKey,
            type: "ApplicationAssociationDefinition",
            rendererType: "lookup"
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