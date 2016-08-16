!(function (angular) {
    "use strict";


    function searchIndexService(dispatcherService, offlineSchemaService) {


        var indexColumnCache = {}; // cache of attribute -> index column
        const orderByBlackList = ["asc", "desc", "is", "null"]; // list of ignored terms on order by parsing
        //key = application name , value = array of columns
        const applicationSortColumnCache = new Map();


        //#region utils

        // build a string with two digits 2 -> 02, 10 -> 10
        const padToTwo = number => number <= 99 ? (`0${number}`).slice(-2) : number;

        // create a cache of attribute -> index column
        const parseIndexColumn = function (cache, indexList, prefix) {
            angular.forEach(indexList, (indexAttribute, i) => {
                var trimmed = indexAttribute.trim();
                if (!trimmed) {
                    return;
                }
                cache[trimmed] = "`root`." + prefix + padToTwo(i + 1);
            });
        }

        // create a cache of attribute -> index column
        const buildIndexColumnCache = function (listSchema) {
            const appCache = {};
            if (!listSchema || !listSchema.properties) {
                return appCache;
            }

            const textIndexes = listSchema.properties["list.offline.text.indexlist"];
            if (textIndexes) {
                parseIndexColumn(appCache, textIndexes.split(","), "textindex");
            }

            const numericIndexes = listSchema.properties["list.offline.numeric.indexlist"];
            if (numericIndexes) {
                parseIndexColumn(appCache, numericIndexes.split(","), "numericindex");
            }

            const dateIndexes = listSchema.properties["list.offline.date.indexlist"];
            if (dateIndexes) {
                parseIndexColumn(appCache, dateIndexes.split(","), "dateindex");
            }

            return appCache;
        }

        // parses a term of order by replacing attributes for index columns
        const parseOrderByTerm = function (appName, listSchema, orderByTerm) {
            const tokens = orderByTerm.split(/[\s]+/);
            const parsedTokens = [];
            angular.forEach(tokens, token => {
                const trimmed = token.trim();
                if (!trimmed) {
                    return;
                }
                if (orderByBlackList.indexOf(trimmed) >= 0) {
                    parsedTokens.push(trimmed);
                    return;
                }
                var columnSet = applicationSortColumnCache.get(appName);
                if (!columnSet) {
                    columnSet = new Set();
                    applicationSortColumnCache.set(appName, columnSet);
                }
                columnSet.add(trimmed);
                var indexColumn = getIndexColumn(appName, listSchema, trimmed);
                parsedTokens.push(indexColumn || trimmed);
            });

            return parsedTokens.join(" ");
        }

        // builds a search where clause for a single search field
        const buildAttributeQuery = function (appName, listSchema, gridSearch, searchValue, attribute) {
            const indexColumn = getIndexColumn(appName, listSchema, attribute);
            if (!indexColumn) {
                return "";
            }

            const field = gridSearch.searchFields[attribute];
            if (!field) {
                return "";
            }

            if (field.type === "MetadataDateTimeFilter" || field.dataType === "date") {
                if (!searchValue.startUTC && !searchValue.endUTC) {
                    return "";
                }

                let query = "";
                if (searchValue.startUTC) {
                    query += ` and ${indexColumn} >= ${searchValue.startUTC.getTime()} `;
                }
                if (searchValue.endUTC) {
                    query += ` and ${indexColumn} <= ${searchValue.endUTC.getTime()} `;
                }
                return query;
            }

            if (field.type === "MetadataOptionFilter") {
                if (!searchValue.value || !searchValue.value.trim()) {
                    return "";
                }
                const terms = searchValue.value.split(";");
                const termQueries = [];
                angular.forEach(terms, term => {
                    var trimmed = term.trim();
                    if (!trimmed) {
                        return;
                    }

                    if (!field.whereClause) {
                        termQueries.push(`${indexColumn} = '${term}'`);
                        return;
                    }

                    if (field.whereClause.startsWith("@")) {
                        const serviceString = field.whereClause.substring(1);
                        termQueries.push(dispatcherService.invokeServiceByString(serviceString, [term]));
                    }

                    // TODO: add support for option filter whereclauses that are not services
                });

                return ` and (${termQueries.join(" or ")}) `;
            }

            if (!searchValue.value || !searchValue.value.trim()) {
                return "";
            }

            return ` and ${indexColumn} like '%${searchValue.value}%' `;
        }
        //#endregion

        // builds an array of index values parameters to be stored on client db
        const buildIndexes = function (textIndexes, numericIndexes, dateIndexes, newDataMap) {
            const indexesData = {
                t1: null,
                t2: null,
                t3: null,
                t4: null,
                t5: null,
                n1: null,
                n2: null,
                d1: null,
                d2: null,
                d3: null
            };

            if (textIndexes) {
                angular.forEach(textIndexes, (indexName, i) => {
                    indexesData[`t${i + 1}`] = newDataMap[indexName] || null;
                });
            }

            if (numericIndexes) {
                angular.forEach(numericIndexes, (indexName, i) => {
                    indexesData[`n${i + 1}`] = newDataMap[indexName] || null;
                });
            }

            if (dateIndexes) {
                angular.forEach(dateIndexes, (indexName, i) => {
                    const value = newDataMap[indexName];
                    var convertedValue;
                    if (!value) {
                        convertedValue = null;
                    } else {
                        convertedValue = new Date(value).getTime();
                    }
                    indexesData[`d${i + 1}`] = convertedValue;
                });
            }

            return indexesData;
        };

        // gets the index columns given the attribute, or all of them if the attribute is no passed
        const getIndexColumn = function (appName, listSchema, attribute) {



            if (!indexColumnCache[appName]) {
                indexColumnCache[appName] = buildIndexColumnCache(listSchema);
            }
            if (!attribute) {
                //to retrieve the list of all indexes
                return indexColumnCache[appName];
            }

            if (attribute.startsWith("#") && attribute.contains(".")) {
                //this means we are referring to a related joined schema that needs to be resolved on the offline
                //let´s locate the appropriate index on that schema instead
                const entityData = offlineSchemaService.findRelatedEntityName(attribute);
                const entityName = entityData.entityName;
                const relatedSchema = offlineSchemaService.locateRelatedListSchema(listSchema, entityName);
                const relatedAppName = relatedSchema.applicationName;
                if (!indexColumnCache[relatedAppName]) {
                    indexColumnCache[relatedAppName] = buildIndexColumnCache(relatedSchema);
                }
                return indexColumnCache[relatedAppName][entityData.attribute].replace("`root`","`" + entityName+ "`");
            }

            return indexColumnCache[appName][attribute];
        }

        // builds the default order by replacing attributes for index columns
        const buildDefaultOrderBy = function (appName, listSchema, defaultOrderBy) {
            if (!defaultOrderBy) {
                return "";
            }

            const terms = defaultOrderBy.split(",");
            const parsedTerms = [];

            angular.forEach(terms, term => {
                if (!term) {
                    return;
                }
                parsedTerms.push(parseOrderByTerm(appName, listSchema, term));
            });

            return parsedTerms.join(", ");
        }

        // builds the search where clause
        const buildSearchQuery = function (appName, listSchema, gridSearch) {
            if (!gridSearch.searchValues) {
                return "";
            }

            var query = " ";
            angular.forEach(gridSearch.searchValues, (searchValue, attribute) => {
                if (!gridSearch.searchValues.hasOwnProperty(attribute)) {
                    return;
                }
                query += buildAttributeQuery(appName, listSchema, gridSearch, searchValue, attribute) + " ";
            });

            return query;
        }

        // builds the sort where clause
        const buildSortQuery = function (appName, listSchema, gridSearch) {
            if (gridSearch.sort && gridSearch.sort.attribute && listSchema) {
                const indexColumn = getIndexColumn(appName, listSchema, gridSearch.sort.attribute);
                if (indexColumn) {
                    const direction = gridSearch.sort.direction;
                    if (indexColumn.startsWith("text")) {
                        return ` order by \`root\`.isDirty desc, ${indexColumn} is null ${direction}, lower(${indexColumn}) ${direction} `;
                    }
                    return ` order by \`root\`.isDirty desc, ${indexColumn} is null ${direction}, ${indexColumn} ${direction} `;
                }
            }

            if (listSchema && listSchema.properties) {
                const orderBy = listSchema.properties["list.defaultorderby"];
                if (orderBy) {
                    const parsedOrderBy = buildDefaultOrderBy(appName, listSchema, orderBy);
                    return ` order by \`root\`.isDirty desc, ${parsedOrderBy}`;
                }
            }
            return " order by \`root\`.isDirty desc, \`root\`.rowstamp is null desc, \`root\`.rowstamp desc ";
        }

        const parseWhereClause = function (whereClause, datamap) {
            const parameterRegex = /@[\w]+/g;
            const parameters = parameterRegex.exec(whereClause);
            if (!parameters) {
                return whereClause;
            }

            angular.forEach(parameters, parameter => {
                const parameterName = parameter.substring(1);
                const value = datamap[parameterName] || "";
                whereClause = whereClause.replace(new RegExp(parameter, "g"), `'${value}'`);
            });

            return whereClause;
        }

        const refreshIndexCaches = function () {
            applicationSortColumnCache.clear();
            indexColumnCache = {};
        }

        const getSearchColumnsByApp = function (applicationName) {
            const set = applicationSortColumnCache.get(applicationName);
            return set ? Array.from(set) : [];
        }

        const service = {
            buildIndexes,
            getIndexColumn,
            buildDefaultOrderBy,
            buildSearchQuery,
            buildSortQuery,
            getSearchColumnsByApp,
            parseWhereClause,
            refreshIndexCaches
        };

        return service;

    }

    mobileServices.factory("searchIndexService", ["dispatcherService", "offlineSchemaService", searchIndexService]);
})(angular);