(function (angular) {
    "use strict";


    // service.$inject = [];
    class compositionCommons {
        
        constructor(datamapSanitizeService) {
            this.datamapSanitizeService = datamapSanitizeService;
        }


        buildInlineDefinition(definition) {
            //this means that we recevived only the list schema, for inline compositions
            definition.schemas = {
                list: definition.compositionschemadefinition
            };
            definition.inline = true;
            definition.collectionProperties = {
                allowInsertion: "false",
                allowUpdate: "false",
                autoCommit:false
            }
            return definition;
        }

        /**
       * merges the parent datamap with a specific composition row data, making sure that, 
       * in case of conflicts, the composition data is sent and not the parent one (ex: both have assets)
       * @param {} datamap 
       * @param {} parentdata 
       * @returns {} 
       */
        buildMergedDatamap (datamap, parentdata) {
            const toClone = parentdata;
            let clonedDataMap = angular.copy(toClone);
            clonedDataMap = this.datamapSanitizeService.sanitizeDataMapToSendOnAssociationFetching(clonedDataMap);

            if (datamap) {
                const item = datamap;
                for (let prop in item) {
                    if (item.hasOwnProperty(prop)) {
                        clonedDataMap[prop] = item[prop];
                    }
                }
            }
            return clonedDataMap;
        }
    }

    compositionCommons.$inject = ["datamapSanitizeService"];

    angular.module("webcommons_services").service("compositionCommons", ['datamapSanitizeService', compositionCommons]);


})(angular);
