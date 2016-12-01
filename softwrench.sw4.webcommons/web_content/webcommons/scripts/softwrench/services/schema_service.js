(function (modules, angular) {
    "use strict";

    modules.webcommons.factory('schemaService', ["$q", "fieldService", "expressionService", "schemaCacheService", "restService", function ($q, fieldService, expressionService, schemaCacheService, restService) {

        //#region private methods

        /// <summary>
        /// builds a cache of the grid qualified displayables to show on small grids
        /// </summary>
        /// <param name="schema"></param>
        function buildQualifierCache(schema) {
            schema.jscache = schema.jscache || {};
            if (schema.jscache.griddisplayables) {
                //already cached
                return;
            }
            schema.jscache.qualifiercache = {};
            var displayables = schema.displayables;
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                if (displayable.qualifier) {
                    schema.jscache.qualifiercache[displayable.qualifier] = displayable;
                }
            }
        };

        //#endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaKey">a string representing a full schema, with or without reference to the application</param>
        /// <returns type="">an object with the following properties
        /// app: the application name (could be null)
        /// schemaId: the schema Id (can´t be null)
        /// mode: the mode of the schema(or null)
        /// 
        /// </returns>
        function parseAppAndSchema(schemaKey) {
            if (schemaKey == null) {
                return null;
            }

            var keys = schemaKey.split('.');
            if (keys.length == 1) {
                //in this case we are passing only the schemaId  
                return { app: null, schemaId: schemaKey, mode: null };
            }
            var mode = null;
            var application = keys[0];
            var schemaId = keys[1];
            if (keys.length == 3) {
                mode = keys[2];
            }
            return { app: application, schemaId: schemaId, mode: mode };
        };

        function getId(datamap, schema) {
            return datamap[schema.idFieldName];
        };

        /**
         * Returns a jquery element (section) considering the path passed as parameter.
         * 
         * That is needed on circumstances in which you want to search for elements inside of a given section.For example:
         * 
         * You need to update the assets lookup element inside of a composition row
         * 
         * 
         * 
         * @param {} schema 
         * @param {} datamap
         * @param {} path 
         * @returns {} 
         */
        function locateJquerySectionElementByApp(schema, datamap, path) {

        }

        function nonTabFields(schema) {
            return flattenDisplayables(fieldService.nonTabFields(schema.displayables, true));
        };

        function hasAnyFieldOnMainTab(schema) {
            if (!schema) {
                return false;
            }

            schema.jscache = schema.jscache || {};
            if (schema.jscache.hasAnyFieldOnMainTab) {
                return schema.jscache.hasAnyFieldOnMainTab;
            }
            var fields = fieldService.nonTabFields(schema.displayables, false);
            var result = fields.length > 0;
            schema.jscache.hasAnyFieldOnMainTab = result;
            return result;
        };

        function isPropertyTrue(schema, propertyName) {
            if (!schema) {
                return false;
            }
            return schema.properties && "true" == schema.properties[propertyName];
        };

        function getProperty(schema, propertyName) {
            if (!schema) {
                return false;
            }
            schema.properties = schema.properties || {};
            return schema.properties[propertyName];
        };

        function buildApplicationKey(schema) {
            var basekey = schema.applicationName + "." + schema.schemaId;
            if (schema.mode && schema.mode !== "none") {
                basekey += "." + schema.mode;
            }
            return basekey;
        };

        function buildApplicationMetadataSchemaKey(schema) {
            return {
                applicationName: schema.applicationName,
                schemaId: schema.schemaId,
                mode: schema.mode,
                platform: platform()
            }
        };

        function getTitle(schema, datamap, smallDevices) {
            if (!schema) {
                return null;
            }

            if (!datamap) {
                return "New " + schema.title;
            }
            var fields = datamap;

            if (schema.properties['detail.titleexpression'] != null) {
                return expressionService.evaluate(schema.properties['detail.titleexpression'], fields);
            }
            var titleId = schema.idDisplayable;
            if (titleId == null) {
                return schema.title;
            }
            var result;
            var idValue = fields[schema.userIdFieldName];
            if (!idValue) {

                idValue = "New " + schema.title;
            }
            if (smallDevices) {
                result = idValue;
            } else {
                result = titleId + " " + idValue;
            }

            return result;
        };

        function getSummary(schema, datamap, smallDevices) {
            if (!schema || !datamap) {
                return null;
            }
            const field = locateDisplayableByQualifier(schema, "summary");
            if (!field) {
                return datamap.description;
            }
            return datamap[field.attribute] || datamap.description;
        };

        function locateDisplayableByQualifier(schema, qualifier) {
            schema.jscache = schema.jscache || {};
            if (schema.jscache.qualifiercache) {
                //already cached
                return schema.jscache.qualifiercache[qualifier];
            }
            buildQualifierCache(schema);
            return schema.jscache.qualifiercache[qualifier];
        };

        function hasEditableProperty(schema) {
            if (schema.mode === "input") {
                return true;
            }
            schema.jscache = schema.jscache || {};
            if (schema.jscache.editable) {
                return schema.jscache.editable;
            }

            var editable = allDisplayables(schema).some(function (displayable) {
                return fieldService.isPropertyTrue(displayable, "editable");
            });

            schema.jscache.editable = editable;
            return editable;
        }

        /**
         * @param {} schema 
         * @param Array<String>|String values 
         * @param {} emptyAsTrue (optional: defaults to false) whether or not a null/undefined/empty/"None" stereotype should be considered as the one being evaluated
         * @returns Boolean whether or not the schema is any of the values stereotype
         */
        function isStereotype(schema, values, emptyAsTrue) {
            var stereotype = schema.stereotype;
            if (!stereotype || stereotype === "None") return emptyAsTrue || false;
            if (!angular.isArray(values)) return stereotype.contains(values);
            return values.some(function (value) {
                return stereotype.contains(value);
            });
        }

        /**
         * @param {} schema 
         * @param Boolean emptyAsTrue (optional: defaults to false) whether or not a null/undefined/empty/"None" stereotype should be considered as detail
         * @returns Boolean whether or not the schema is of detail stereotype
         */
        function isDetail(schema, emptyAsTrue) {
            return isStereotype(schema, ["detail", "Detail"], emptyAsTrue);
        }

        /**
         * @param {} schema 
         * @param Boolean emptyAsTrue (optional: defaults to false) whether or not a null/undefined/empty/"None" stereotype should be considered as list
         * @returns Boolean whether or not the schema is of list stereotype
         */
        function isList(schema, emptyAsTrue) {
            return isStereotype(schema, ["list", "List"], emptyAsTrue);
        }

        function areTheSame(schema1, schema2) {
            if (schema1 == null) {
                return schema2 == null;
            }

            else if (schema2 == null) {
                return false;
            }

            return this.buildApplicationKey(schema1) === this.buildApplicationKey(schema2);
        }

        /**
         * @param {} schemaA
         * @param {} schemaB
         * @returns Boolean whether or not the schemas have same applicationName and schemaId
         */
        function isSameSchema(schemaA, schemaB) {
            return schemaA.applicationName === schemaB.applicationName && schemaA.schemaId === schemaB.schemaId;
        }

        function flattenDisplayables(fields, context, datamap) {
            if (!fields) return [];
            context = context || [];
            fields.forEach(function (f) {
                if (fieldService.isNullInvisible(f, datamap)) {
                    return;
                }
                if (angular.isArray(f.displayables)) {
                    flattenDisplayables(f.displayables, context, datamap);
                } else {
                    context.push(f);
                }
            });
            return context;
        }

        /**
         * Returns all displayable fields in a single array i.e. expands all sections.
         * 
         * @param {} schema 
         * @returns Array<FieldMetadata> 
         */
        function allDisplayables(schema) {
            return flattenDisplayables(schema.displayables);
        }

        function getSchema(application, schemaId) {
            const cachedSchema = schemaCacheService.getCachedSchema(application, schemaId);
            if (cachedSchema) {
                return $q.when(cachedSchema);
            }

            const parameters = {
                applicationName: application,
                targetSchemaId: schemaId
            }
            const promise = restService.getPromise("Metadata", "GetSchemaDefinition", parameters);
            return promise.then(function (result) {
                schemaCacheService.addSchemaToCache(result.data);
                return result.data;
            });
        }

        return {
            areTheSame: areTheSame,
            buildApplicationKey: buildApplicationKey,
            buildApplicationMetadataSchemaKey: buildApplicationMetadataSchemaKey,
            getId: getId,
            getProperty: getProperty,
            getSummary: getSummary,
            getTitle: getTitle,
            hasAnyFieldOnMainTab: hasAnyFieldOnMainTab,
            hasEditableProperty: hasEditableProperty,
            isPropertyTrue: isPropertyTrue,
            locateDisplayableByQualifier: locateDisplayableByQualifier,
            locateJquerySectionElementByApp: locateJquerySectionElementByApp,
            nonTabFields: nonTabFields,
            parseAppAndSchema: parseAppAndSchema,
            isStereotype: isStereotype,
            isDetail: isDetail,
            isList: isList,
            isSameSchema: isSameSchema,
            allDisplayables: allDisplayables,
            flattenDisplayables: flattenDisplayables,
            getSchema: getSchema
        };

    }]);

})(modules, angular);