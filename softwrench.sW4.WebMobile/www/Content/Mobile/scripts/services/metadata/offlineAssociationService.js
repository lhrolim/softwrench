(function (angular, _) {
    "use strict";

    mobileServices.factory('offlineAssociationService', ["swdbDAO", "fieldService", "crudContextHolderService", "$log", "searchIndexService", "dispatcherService",
        function (swdbDAO, fieldService, crudContextHolderService, $log, searchIndexService, dispatcherService) {

        function testEmptyExpression(label) {
            return `(!!${label} && ${label} !== \'null\' && ${label} !== \'undefined\')`;
        }

        function fieldValueExpression(fieldMetadata) {
            return `datamap.${fieldMetadata.valueField}`;
        };

        function fieldLabelExpression(fieldMetadata) {
            const associationValueField = this.fieldValueExpression(fieldMetadata);
            if ("true" === fieldMetadata.hideDescription) {
                return associationValueField;
            }
            const label = `datamap.${fieldMetadata.labelFields[0]}`;

            return _.contains([false, "false", "False", 0, "0"], fieldMetadata.rendererParameters["showCode"])
                // show only label/description
                ? `(${testEmptyExpression(label)} ? ${label} : \'\')`
                // show <code> - <label>
                : `(${testEmptyExpression(associationValueField)} ? ${associationValueField} : \'\' ) + (${testEmptyExpression(label)} ? (\' - \'  + ${label}) : \'\')`;
        };

        function getWhereClause(displayable, parentdatamap) {
            var whereClause = displayable.schema.dataProvider.whereClause;
            if (whereClause.startsWith("@")) {
                whereClause = dispatcherService.invokeServiceByString(whereClause.substring(1));
            }
            return searchIndexService.parseWhereClause(whereClause, parentdatamap);
        }

        function filterPromise(parentSchema, parentdatamap, associationName, filterText, preCalcDisplayable, pageNumber, useWhereClause, currentValue) {
            const log = $log.get("offlineAssociationService#filterPromise", ["association", "query"]);

            const displayable = preCalcDisplayable || fieldService.getDisplayablesByAssociationKey(parentSchema, associationName)[0];

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
                    if (!fromValue) {
                        log.info(`field ${attribute.from} could not be found on the datamap ignoring it`);
                    } else {
                        baseQuery += ' and datamap like \'%"{0}":"{1}"%\''.format(attribute.to, fromValue);
                    }

                    
                }
            });

            if (displayable.schema && displayable.schema.dataProvider && displayable.schema.dataProvider.whereClause && useWhereClause) {
                baseQuery += ` and (${getWhereClause(displayable, parentdatamap)}) `;
            }

            if (currentValue) {
                baseQuery += ` order by (datamap like "%${currentValue}%") desc `;
            }

            pageNumber = pageNumber || 1;
            return swdbDAO.findByQuery("AssociationData", baseQuery, { pagesize: 10, pageNumber: pageNumber, projectionFields: ["datamap"] });
        }

        function updateExtraProjections(associationDataEntry, associationKey) {
            const log = $log.get("offlineAssociationService#updateExtraProjections", ["association"]);
            const isComposition = !crudContextHolderService.isOnMainTab();
            const detailSchema = isComposition
                ? crudContextHolderService.getCompositionDetailSchema()
                : crudContextHolderService.currentDetailSchema();
            const associationMetadata = fieldService.getDisplayablesByAssociationKey(detailSchema, associationKey)[0];
            if (!associationMetadata || !associationMetadata.extraProjectionFields) {
                log.trace(`no extraprojectionfields to update for ${associationKey}`);
                return;
            }
            
            const dm = crudContextHolderService.currentDetailItemDataMap();
            const associationDataMap = associationDataEntry.datamap;
            log.info(`updating extraprojection fields for ${associationKey}`);
            associationMetadata.extraProjectionFields.forEach(item => {
                const projectedKey = associationKey + "." + item;
                const value = associationDataMap[item];
                log.debug(`updating ${projectedKey} to ${value}`);
                dm[projectedKey] = value;
            });

        }

        function updateExtraProjectionsForOptionField(optionFieldEntry, associationKey) {
            const log = $log.get("offlineAssociationService#updateExtraProjections", ["association"]);
            const isComposition = !crudContextHolderService.isOnMainTab();
            const detailSchema = isComposition
                ? crudContextHolderService.getCompositionDetailSchema() 
                : crudContextHolderService.currentDetailSchema();
            const associationMetadata = fieldService.getDisplayablesByAssociationKey(detailSchema, associationKey)[0];
            const extrafields = optionFieldEntry.extrafields;

            if (!associationMetadata || !extrafields) {
                log.trace(`no extraprojectionfields to update for ${associationKey}`);
                return;
            }
            const dm = crudContextHolderService.currentDetailItemDataMap();

            log.info(`updating extraprojection fields for ${associationKey}`);
            Object.keys(extrafields).forEach((item )=> {
                const projectedKey = associationKey + "_." + item;
                const value = extrafields[item];
                log.debug(`updating ${projectedKey} to ${value}`);
                dm[projectedKey] = value;
            });

        }


        const api = {
            filterPromise,
            fieldLabelExpression,
            fieldValueExpression,
            updateExtraProjections,
            updateExtraProjectionsForOptionField,
        }
        return api;

    }]);

})(angular, _);