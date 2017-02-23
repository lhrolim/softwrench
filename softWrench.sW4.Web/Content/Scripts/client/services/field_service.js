var app = angular.module('sw_layout');

app.factory('fieldService', function (expressionService,$log) {

    "ngInject";

    var isFieldHidden = function (datamap, application, fieldMetadata) {
        fieldMetadata.jscache = instantiateIfUndefined(fieldMetadata.jscache);
        if (fieldMetadata.jscache.isHidden != undefined) {
            return fieldMetadata.jscache.isHidden;
        }
        var baseHidden = fieldMetadata.isHidden || (fieldMetadata.type != "ApplicationSection" &&
              (fieldMetadata.attribute == application.idFieldName && application.stereotype == "Detail"
              && application.mode == "input" && !fieldMetadata.isReadOnly));
        var isTabComposition = fieldMetadata.type == "ApplicationCompositionDefinition" && !fieldMetadata.inline;
        if (baseHidden || isTabComposition) {
            fieldMetadata.jscache.isHidden = true;
            return true;
        } else if (fieldMetadata.type == "ApplicationSection" && fieldMetadata.resourcepath == null &&
            (fieldMetadata.displayables.length == 0 ||
             $.grep(fieldMetadata.displayables, function (e) {
                  return !isFieldHidden(datamap, application, e);
        }).length == 0)) {

            //            fieldMetadata.jscache.isHidden = true;
            return true;
        }
        //the opposite of the expression: showexpression --> hidden
        var result = !expressionService.evaluate(fieldMetadata.showExpression, datamap);
        if (application.stereotype == "List") {
            //list schemas can be safely cached since if the header is visible the rest would be as well
            fieldMetadata.jscache.isHidden = result;
        }
        return !expressionService.evaluate(fieldMetadata.showExpression, datamap);
    };


    return {
        isFieldHidden: function (datamap, application, fieldMetadata) {
            return isFieldHidden(datamap, application, fieldMetadata);
        },

        isFieldRequired: function (fieldMetadata, datamap) {
            if (fieldMetadata.type === "ApplicationSection" && fieldMetadata.parameters) {
                return "true" === fieldMetadata.parameters["required"];
            }
            var requiredExpression = fieldMetadata.requiredExpression;
            if (requiredExpression != undefined) {
                return expressionService.evaluate(requiredExpression, datamap);
            }
            return requiredExpression;
        },

        nonTabFields: function (displayables) {
            var result = [];
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                var type = displayable.type;
                var isTabComposition = this.isTabComposition(displayable);
                if (!isTabComposition && type != "ApplicationTabDefinition") {
                    result.push(displayable);
                }
            }
            return result;
        },

        isTabComposition: function (displayable) {
            var type = displayable.type;
            return type == "ApplicationCompositionDefinition" && !displayable.inline;
        },

        isTab: function (displayable) {
            if (displayable == null) {
                return;
            }
            else {
                var type = displayable.type;
                return type == "ApplicationTabDefinition";
            }
        },

        getDisplayableByKey: function (schema, key) {
            schema.jscache = instantiateIfUndefined(schema.jscache);
            schema.jscache.fieldsByKey = instantiateIfUndefined(schema.jscache.fieldsByKey);
            if (schema.jscache.fieldsByKey[key] != undefined) {
                return schema.jscache.fieldsByKey[key];
            }

            var displayables = schema.displayables;
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                if ((displayable.attribute && displayable.attribute == key) || (displayable.tabId && displayable.tabId == key)) {
                    schema.jscache.fieldsByKey[key] = displayable;
                    return displayable;
                }
                if (displayable.displayables != undefined) {
                    var innerDisplayables = this.getDisplayableByKey(displayable, key);
                    if (innerDisplayables != undefined && ((innerDisplayables.attribute && innerDisplayables.attribute == key) || (innerDisplayables.tabId && innerDisplayables.tabId == key))) {
                        schema.jscache.fieldsByKey[key] = innerDisplayables;
                        return innerDisplayables;
                    }
                }
            }
            return null;
        },

        getDisplayablesByAssociationKey: function (schema, associationKey) {
            schema.jscache = instantiateIfUndefined(schema.jscache);
            var cacheEntry = schema.jscache.displayablesByAssociation = instantiateIfUndefined(schema.jscache.displayablesByAssociation);
            if (cacheEntry[associationKey] != undefined) {
                return cacheEntry[associationKey];
            }

            var result = [];
            var fn = this;
            $.each(schema.displayables, function (index, value) {
                if (value.associationKey == associationKey) {
                    result.push(value);
                } else if (value.displayables != undefined) {
                    var innerDisplayables = fn.getDisplayablesByAssociationKey(value, associationKey);
                    if (innerDisplayables != null) {
                        result = result.concat(innerDisplayables);
                    }
                }
            });
            cacheEntry[associationKey] = result;
            return result;
        },

        countVisibleDisplayables: function (datamap, application, displayables) {
            var count = 0;
            for (var i = 0; i < displayables.length; i++) {
                if (!this.isFieldHidden(datamap, application, displayables[i])) {
                    count++;
                }
            }
            return count;
        },

        getId: function (datamap, schema) {
            return datamap[schema.idFieldName];
        },

        getDisplayablesOfTypes: function (displayables, types) {
            var result = [];
            var fn = this;
            $.each(displayables, function (key, value) {
                var type = value.type;
                if ($.inArray(type, types) != -1) {
                    result.push(value);
                }
                if (value.displayables != undefined) {
                    var innerDisplayables = fn.getDisplayablesOfTypes(value.displayables, types);
                    result = result.concat(innerDisplayables);
                }
            });

            return result;
        },

        getDisplayablesOfRendererTypes: function (displayables, types) {
            var result = [];
            var fn = this;
            $.each(displayables, function (key, value) {
                var type = value.rendererType;
                if ($.inArray(type, types) != -1) {
                    result.push(value);
                }
                if (value.displayables != undefined) {
                    result = result.concat(fn.getDisplayablesOfRendererTypes(value.displayables, types));
                }
            });

            return result;

        },

        getFilterDisplayables: function (displayables) {
            var result = [];
            var fn = this;
            $.each(displayables, function (key, value) {
                if (value.filter != null && value.filter.operation != null) {
                    result.push(value);
                }
                if (value.displayables != undefined) {
                    result = result.concat(fn.getFilterDisplayables(value.displayables));
                }
            });

            return result;

        },

        ///return if a field which is not on screen (but is not a hidden instance), and whose value is null from the datamap, avoiding sending useless (and wrong) data
        isNullInvisible: function (displayable, datamap) {
            if (displayable.showExpression == undefined || displayable.showExpression == "true" || displayable.isHidden) {
                return false;
            }
            return !expressionService.evaluate(displayable.showExpression, datamap);
        }
    };

});


