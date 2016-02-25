(function (angular) {
    "use strict";

mobileServices.factory('offlineAssociationService', ["swdbDAO", "fieldService", function (swdbDAO, fieldService) {

    function testEmptyExpression(label) {
        return "(!!" + label + " && " + label + " !== \'null\' && " + label + " !== \'undefined\')";
    }

    return {

        fieldValueExpression: function(fieldMetadata) {
            return "datamap." + fieldMetadata.valueField;
        },

        fieldLabelExpression: function(fieldMetadata) {
            var associationValueField = this.fieldValueExpression(fieldMetadata);
            if ("true" === fieldMetadata.hideDescription) {
                return associationValueField;
            }

            var label = "datamap." + fieldMetadata.labelFields[0];

            return "(" + testEmptyExpression(associationValueField) + " ? " + associationValueField + " : \'\' ) + " +
                    "(" + testEmptyExpression(label) + " ? (\' - \'  + " + label + ") : \'\')";
        },

        filterPromise: function (parentSchema, parentdatamap, associationName, filterText) {
            var displayable = fieldService.getDisplayablesByAssociationKey(parentSchema, associationName)[0];
            

            if (associationName.endsWith("_")) {
                associationName = associationName.substring(0, associationName.length-1);
            }
            var associationAsEntityName = displayable.entityAssociation.to;
            
            //the related application could have been downloaded using either the qualifier or the entity name, 
            //but it doesn´t matter here, as the other relationships will be used
            var baseQuery = " (application = '{0}' or application = '{1}' )".format(associationName, associationAsEntityName);
            if (!nullOrEmpty(filterText)) {
                baseQuery += " and datamap like '%{0}%' ".format(filterText);
            }

            angular.forEach(displayable.entityAssociation.attributes, function (attribute) {
                if (attribute.primary) {
                    return;
                }
                var allowsNull = false;
                var fromValue;
                if (attribute.literal) {
                    //siteid = 'SOMETHING'
                    fromValue = attribute.literal;
                } else {
                    //siteid = siteid
                    allowsNull = attribute.allowsNull;
                    fromValue = parentdatamap[attribute.from];
                }
                if (allowsNull) {
                    baseQuery += ' and ( datamap like \'%"{0}":"{1}"%\' or datamap like \'%"{0}":null%\' )'.format(attribute.to, fromValue);
                } else {
                    baseQuery += !!fromValue ? ' and datamap like \'%"{0}":"{1}"%\''.format(attribute.to, fromValue) : "";
                }
            });

            return swdbDAO.findByQuery("AssociationData", baseQuery, { projectionFields: ["remoteId", "datamap"] });
        }

    }
}]);

})(angular);