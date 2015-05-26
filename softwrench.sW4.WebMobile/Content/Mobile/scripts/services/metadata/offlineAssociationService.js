mobileServices.factory('offlineAssociationService', function (swdbDAO) {

    
    


    return {

        filterPromise: function (parentSchema, associationName, filterText) {
            if (associationName.endsWith("_")) {
                associationName = associationName.substring(0, associationName.length-1);
            }
            return swdbDAO.findByQuery("AssociationData", "application = '{0}' and datamap like '%{1}%'".format(associationName, filterText),{projectionFields:["remoteid","datamap"]});
        }

    }
});