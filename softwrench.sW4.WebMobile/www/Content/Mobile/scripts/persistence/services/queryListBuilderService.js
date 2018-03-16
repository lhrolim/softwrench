(function (angular) {
    "use strict";

    const day = 24 * 60 * 60 * 1000;

    function queryListBuilderService(offlineSchemaService, searchIndexService, metadataModelService, $log, securityService) {
        //#region Utils

        const log = $log.get("queryListBuilderService", ["list", "search"]);

        function getLaborCode() {
            //TODO: make more generic...
            const user = securityService.currentFullUser();
            if (user == null) {
                return securityService.logout();
            }
            let realLabor = user.properties["laborcode"];
            if (!realLabor) {
                //keeping here for compatibility backwards
                const personId = securityService.currentFullUser()["PersonId"];

                const dotIndex = personId.indexOf(".");
                if (dotIndex !== -1) {
                    realLabor = personId.substring(0, 1).toUpperCase() + personId.substring(dotIndex + 1, personId.length).toUpperCase();
                }
            }
            return realLabor;
        }

        function buildExtraLaborAttribute(childListSchema, childEntityName) {
            
            const realLabor = getLaborCode();


            const laborIdxName = searchIndexService.getIndexColumn(childListSchema.applicationName, childListSchema, "laborcode").replace("`root`", "`" + childEntityName + "`");

            return `${laborIdxName} LIKE '${realLabor}'`;

        }


        function doBuildLeftJoin(mainListSchema, childEntityName) {



            const entityAssociation = mainListSchema.offlineAssociations[childEntityName];
            const application = metadataModelService.getApplicationByName(entityAssociation.to, true);
            const childListSchema = application.data.schemasList.find(s => s.stereotypeAttr === "list");

            const childTable = application.association ? "AssociationData" : "DataEntry";

            let query = ` left join ${childTable} as \`${childEntityName}\` on (`;

            const primaryAttribute = entityAssociation.attributes.find(a => !!a.primary);

            const mainIdx = searchIndexService.getIndexColumn(mainListSchema.applicationName, mainListSchema, primaryAttribute.from);
            const leftJoinedIndex = searchIndexService.getIndexColumn(childListSchema.applicationName, childListSchema, primaryAttribute.to).replace("`root`", "`" + childEntityName + "`");


            let extraLaborQuery = "1=1";
            //TODO:review
            if (mainListSchema.applicationName.equalsAny("workorder", "todayworkorder", "pastworkorder")) {
                extraLaborQuery = buildExtraLaborAttribute(childListSchema, childEntityName);
            }

            //TODO: make it generic, cause now it�s all tied to assignment child application
            const associatioNameQuery = "`" + childEntityName + "`" + ".application= 'assignment'";

            const now = new Date();
            const todayTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0).getTime();
            const tomorrowTime = todayTime + day;

            //this handles SWOFF-342
            let duplicateQuery = "`" + childEntityName + "`.dateindex02 = (select max(b.dateindex02) from AssociationData b where b.textindex01 = `" + childEntityName + "`.textindex01 and b.application = 'assignment' and b.textindex02 = " + `'${getLaborCode()}'`;

            if (mainListSchema.applicationName === "todayworkorder") {
                duplicateQuery += ` and b.dateindex02 <= ${tomorrowTime}`
            }

            else if (mainListSchema.applicationName === "pastworkorder"){
                duplicateQuery += ` and b.dateindex02 < ${todayTime}`
            }

            duplicateQuery += ")";


            query += `${mainIdx} = ${leftJoinedIndex} and ${extraLaborQuery} and ${associatioNameQuery}  and ${duplicateQuery} )`;
            return query;
        }




        //#endregion

        //#region Public methods

        /**
         *  Builds eventual extra parameters related to left join entities for the query to be performed on the list page:
         *      1) additionalJoins --> the left join queries
         *      2) extraProjectionFields --> the fields to be selected from these joined entities (datamap basically)
         * 
         * @param {} listSchema current list schema
         * @param {} baseQuery 
         * @param {} queryObj 
         * @returns {} 
         */
        function buildJoinParameters(listSchema) {
            if (listSchema.applicationName === "otherworkorderunassigned"){
                return {};
            }


            const joinedfields = listSchema.displayables.filter(a => a.attribute.startsWith("#") && a.attribute.contains(".")).map(a => a.attribute);
            const joinedIndexes = searchIndexService.getSearchColumnsByApp(listSchema.applicationName).find(a => a.startsWith("#") && a.contains("."));

            const allAttributes = joinedIndexes ? new Set(joinedfields.concat(joinedIndexes)) : joinedfields;

            if (allAttributes.length === 0) {
                //no extra parameters needed
                return {};
            }
            log.debug("building left join parameters");

            const extraProjectionFields = new Map();
            const additionalJoins = [];

            const leftJoinEntities = new Set();

            allAttributes.forEach(attribute => {
                const entityName = offlineSchemaService.findRelatedEntityName(attribute).entityName;
                if (!extraProjectionFields.has(entityName)) {
                    // should be added only once
                    log.trace(`adding entity ${entityName} as a left join`);
                    extraProjectionFields.set(entityName, { field: "`" + entityName + "`" + ".datamap", alias: "`#datamap." + entityName + "`" });
                }
                leftJoinEntities.add(entityName);
            });


            leftJoinEntities.forEach(entity => {
                additionalJoins.push(doBuildLeftJoin(listSchema, entity));
            });

            return { additionalJoins, extraProjectionFields: Array.from(extraProjectionFields.values()),leftJoinEntities: Array.from(leftJoinEntities) };
        }


        //#endregion

        //#region Service Instance
        const service = {
            buildJoinParameters
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("persistence.offline").factory("queryListBuilderService", ["offlineSchemaService", "searchIndexService", "metadataModelService", "$log", "securityService", queryListBuilderService]);

    //#endregion

})(angular);