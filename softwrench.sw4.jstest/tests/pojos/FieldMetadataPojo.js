
class FieldMetadataPojo {

    static ForAssociation(associationKey, target, extraFields = []) {

        return {
            associationKey,
            attribute: target,
            target: target || associationKey,
            type: "ApplicationAssociationDefinition",
            rendererType: "lookup",
            extraProjectionFields: extraFields
        };
    }

    static Hidden(attribute) {

        return {
            attribute,
            isHidden: true
        };
    }

    static Required(attribute, label) {
        label = label || attribute;

        return {
            attribute,
            label,
            requiredExpression: true,
            type: "ApplicationFieldDefinition"
        };
    }

    static Ordinary(attribute, label) {
        label = label || attribute;

        return {
            attribute,
            role:attribute,
            label,
            type: "ApplicationFieldDefinition"
        };
    }

    static Section(displayables, orientation = "vertical", label = null) {

        if (!(displayables instanceof Array)) {
            displayables = [displayables];
        }

        const section = {
            displayables,
            orientation,
            type: "ApplicationSection"
        };

        if (label) {
            section.header = {
                displacement: "ontop",
                label,
                showExpression: "true",
                parameters: {
                    fieldset: "true"
                }
            }
        }

        return section;
    }




}