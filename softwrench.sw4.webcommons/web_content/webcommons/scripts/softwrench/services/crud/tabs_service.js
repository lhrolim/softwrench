(function (angular,modules) {
    "use strict";
        
    modules.webcommons.service('tabsService', ["fieldService", "i18NService", function (fieldService, i18NService) {

    var buildTabObjectForPrint = function (datamap, tabSchema, schema) {
        const result = {};        

        result.items = [];
        result.items.push(datamap);
        result.schema = tabSchema;
        result.title = i18NService.getTabLabel(tabSchema, schema);

        return result;
    };

    const isTabSection = function(section) {
        return section.rendererParameters && section.rendererParameters["tabsection"] === "true";
    }

    return {

        hasCount: function (tab) {
            if (!tab) {
                return false;
            }

            return tab.type === "ApplicationCompositionDefinition" ||
                (tab.type === "ApplicationTabDefinition" && !!tab.countRelathionship);
        },

        getCompositionSchema: function (baseSchema, compositionKey, schemaId) {
            var schemas = this.nonInlineCompositionsDict(baseSchema);
            const thisSchema = schemas[compositionKey];
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
            const length = this.tabsDisplayables(schema).length;
            schema.hasTabs = length > 0;
            return length;
        },

        tabsDisplayablesForPrinting: function (schema, datamap) {
            if (schema.tabsDisplayablesForPrinting != undefined) {
                //cache
                return schema.tabsDisplayablesForPrinting;
            }
            var resultList = [];
            const displayables = this.tabsDisplayables(schema);
            $.each(displayables, function (key, displayable) {
                const value = datamap[displayable.relationship];
                if (value != undefined && value.length > 0) {
                    resultList.push(displayable);
                }
            });
            schema.tabsDisplayablesForPrinting = resultList;
            return resultList;
        },

        filterTabs: function (displayables, includeSubTabs = false) {
            var resultList = [];
            angular.forEach(displayables, function (displayable) {
                if (fieldService.isTabOrComposition(displayable) && !displayable.isHidden) {
                    resultList.push(displayable);
                    if (includeSubTabs) {
                        // only add subtabs if includeSubTabs equals true
                        resultList = resultList.concat(this.tabsDisplayables(displayable, includeSubTabs));
                    }
                } else if (fieldService.isSection(displayable) && (!isTabSection(displayable) || includeSubTabs)) {
                    // only add subtabs if includeSubTabs equals true
                    resultList = resultList.concat(this.tabsDisplayables(displayable, includeSubTabs));
                }
            }, this);
            return resultList;
        },

        /**
         * 
         * @param {} container a DisplayableContainer, either a schema or a section or any other implementation
         * @returns {} 
         */
        tabsDisplayables: function (container, includeSubTabs = false) {
            if (!includeSubTabs && container.tabsDisplayables != undefined) {
                //cache
                return container.tabsDisplayables;
            }
            if (includeSubTabs && container.allTabsDisplayables != undefined) {
                //cache
                return container.allTabsDisplayables;
            }

            const resultList = this.filterTabs(container.displayables, includeSubTabs);

            if (!includeSubTabs) {
                container.tabsDisplayables = resultList;
            } else {
                container.allTabsDisplayables = resultList;
            }
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

            // all tabs, including subtabs
            const allTabs = this.tabsDisplayables(schema, true);
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

                const displayable = fieldService.getDisplayableByKey(schema, key);

                if (fieldService.isTab(displayable)) {
                    notExpansible.push({ key: key, tabObject: buildTabObjectForPrint(datamap, displayable, schema) });
                    return;
                }

                const compositionData = datamap[key];

                if (compositionData == undefined) {
                    //this happens when the composition data has not been fetch yet,due to a lazy strategy
                    resultString += key + "=lazy,,,";
                    return;
                }

                //now, we are retrieving data for printing
                const currentSchema = self.getCompositionSchema(schema, key, obj.schema);
                if (currentSchema.properties.expansible != undefined && currentSchema.properties.expansible == "false") {
                    if (notExpansible != undefined && compositionData.length > 0) {
                        //only adding if there´s actual at least one element of this nonExpansible composition
                        notExpansible.push({ key: key, schema: currentSchema });
                    }
                    return;
                }

                const compositionIdField = self.getCompositionIdName(schema, key, schemaId);
                const compositionIdArray = [];

                for (let i = 0; i < compositionData.length; i++) {
                    const composition = compositionData[i];
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
            const schemas = nonInlineCompositionsDict(baseSchema);
            const thisSchema = schemas[compositionKey];
            return thisSchema.schema.schemas.print;
        },

        getTitle: function (baseSchema, compositionKey) {
            const schemas = nonInlineCompositionsDict(baseSchema);
            const thisSchema = schemas[compositionKey];
            return thisSchema.label;
        },

        hasTab: function(baseSchema, tabid) {
            const displayable = fieldService.getDisplayableByKey(baseSchema, tabid);
            if (!displayable) {
                return false;
            }
            return fieldService.isTabOrComposition(displayable);
        }

    };
}]);

})(angular,modules);