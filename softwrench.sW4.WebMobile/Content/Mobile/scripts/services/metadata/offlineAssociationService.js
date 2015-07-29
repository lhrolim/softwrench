mobileServices.factory('offlineAssociationService', function (swdbDAO,fieldService) {

    
    


    return {

        filterPromise: function (parentSchema,parentdatamap, associationName, filterText) {
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
            var entityDeclarationAttributes = displayable.entityAssociation.attributes;
            
            for (var i = 0; i < entityDeclarationAttributes.length; i++) {
                var attribute = entityDeclarationAttributes[i];
                if (attribute.primary) {
                    continue;
                }
                var fromValue;
                if (attribute.literal) {
                    //siteid = 'SOMETHING'
                    fromValue = attribute.literal;
                } else {
                    //siteid = siteid
                    fromValue = parentdatamap[attribute.from];
                }
                baseQuery += ' and datamap like \'%"{0}":"{1}"%\''.format(attribute.to, fromValue);
            }

            return swdbDAO.findByQuery("AssociationData", baseQuery,{projectionFields:["remoteId","datamap"]});
        }

    }
});