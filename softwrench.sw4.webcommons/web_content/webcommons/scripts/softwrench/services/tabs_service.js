(function (angular,modules) {
    "use strict";
        
    modules.webcommons.factory('tabsService', ["fieldService", "i18NService", function (fieldService, i18NService) {

    var buildTabObjectForPrint = function (datamap, tabSchema, schema) {
        var result = {};        

        result.items = [];
        result.items.push(datamap);
        result.schema = tabSchema;
        result.title = i18NService.getTabLabel(tabSchema, schema);

        return result;
    };

    return {

        getCompositionSchema: function (baseSchema, compositionKey, schemaId) {
            var schemas = this.nonInlineCompositionsDict(baseSchema);
            var thisSchema = schemas[compositionKey];
            schemas = thisSchema.schema.schemas;
            return schemaId === "print" ? schemas.print : schemas.list;
        },

        getCompositionIdName:  function (baseSchema, compositionKey, schemaId) {
            return this.getCompositionSchema(baseSchema, compositionKey, schemaId).idFieldName;
        },

        hasTabs: function (schema) {
            if (schema.hasTabs != undefined) {
                //cache
                return schema.hasTabs;
            }
            var length = this.tabsDisplayables(schema).length;
            schema.hasTabs = length > 0;
            return length;
        },

        tabsDisplayablesForPrinting: function (schema, datamap) {
            if (schema.tabsDisplayablesForPrinting != undefined) {
                //cache
                return schema.tabsDisplayablesForPrinting;
            }
            var resultList = [];
            var displayables = this.tabsDisplayables(schema);
            $.each(displayables, function (key, displayable) {
                var value = datamap[displayable.relationship];
                if (value != undefined && value.length > 0) {
                    resultList.push(displayable);
                }
            });
            schema.tabsDisplayablesForPrinting = resultList;
            return resultList;
        },

        /**
         * 
         * @param {} container a DisplayableContainer, either a schema or a section or any other implementation
         * @returns {} 
         */
        tabsDisplayables: function (container) {
            if (container.tabsDisplayables != undefined) {
                //cache
                return container.tabsDisplayables;
            }
            var resultList = [];

            angular.forEach(container.displayables, function(displayable) {
                if (fieldService.isTabOrComposition(displayable) && !displayable.isHidden) {
                    resultList.push(displayable);
                } else if (fieldService.isSection(displayable)) {
                    resultList = resultList.concat(this.tabsDisplayables(displayable));
                }
            }, this);


            container.tabsDisplayables = resultList;
            return resultList;
        },

        tabsPrintDisplayables: function (container) {
            return this.tabsDisplayables(container).filter(function(displayable) {
                return !displayable.isHidden && displayable.isPrintEnabled;
            });
        },

        nonInlineCompositionsDict: function (schema) {
            if (schema.nonInlineCompositionsDict != undefined) {
                //caching
                return schema.nonInlineCompositionsDict;
            }
            var resultDict = {};
            var allTabs = this.tabsDisplayables(schema);
            allTabs.forEach(function(tab) {
                resultDict[tab.relationship] = tab;
            });

            schema.nonInlineCompositionsDict = resultDict;
            return resultDict;
        },

        /*
        * param notExpansible = array of compositions that we do not need to hit the server, since they are not expandable
        *
        *
        * Returns a string in the same format the server expects for expanding the compositions on the Composition.ExpandCompositions method
        *
        */
        buildCompositionsToExpand: function (compositionsToExpandObj, schema, datamap, schemaId, notExpansible) {
            var resultString = "";
            if (compositionsToExpandObj == null) {
                return "";
            }
            var self = this;
            $.each(compositionsToExpandObj, function (key, obj) {
                if (obj.value == false) {
                    return;
                }

                var displayable = fieldService.getDisplayableByKey(schema, key);

                if (fieldService.isTab(displayable)) {
                    notExpansible.push({ key: key, tabObject: buildTabObjectForPrint(datamap, displayable, schema) });
                    return;
                }

                var compositionData = datamap[key];

                if (compositionData == undefined) {
                    //this happens when the composition data has not been fetch yet,due to a lazy strategy
                    resultString += key + "=lazy,,,";
                    return;
                }

                //now, we are retrieving data for printing
                var currentSchema = self.getCompositionSchema(schema, key, obj.schema);
                if (currentSchema.properties.expansible != undefined && currentSchema.properties.expansible == "false") {
                    if (notExpansible != undefined && compositionData.length > 0) {
                        //only adding if there´s actual at least one element of this nonExpansible composition
                        notExpansible.push({ key: key, schema: currentSchema });
                    }
                    return;
                }

                var compositionIdField = self.getCompositionIdName(schema, key, schemaId);
                var compositionIdArray = [];

                for (var i = 0; i < compositionData.length; i++) {
                    var composition = compositionData[i];
                    compositionIdArray.push(composition[compositionIdField]);
                }
                if (compositionIdArray.length > 0) {
                    resultString += key + "=" + compositionIdArray.join(",") + ",,,";
                }
            });
            if (resultString != "") {
                resultString = resultString.substring(0, resultString.length - 3);
            }
            return resultString;
        },

        locatePrintSchema: function (baseSchema, compositionKey) {
            var schemas = nonInlineCompositionsDict(baseSchema);
            var thisSchema = schemas[compositionKey];
            return thisSchema.schema.schemas.print;
        },

        getTitle: function (baseSchema, compositionKey) {
            var schemas = nonInlineCompositionsDict(baseSchema);
            var thisSchema = schemas[compositionKey];
            return thisSchema.label;
        },

        hasTab: function(baseSchema, tabid) {
            var displayable = fieldService.getDisplayableByKey(baseSchema, tabid);
            if (!displayable) {
                return false;
            }
            return fieldService.isTabOrComposition(displayable);
        }

    };
}]);

})(angular,modules);