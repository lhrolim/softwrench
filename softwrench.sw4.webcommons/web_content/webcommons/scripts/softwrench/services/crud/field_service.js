(function (angular, modules) {
    "use strict";

    modules.webcommons.service('fieldService', ["$injector", "$log", "$filter", "expressionService", "eventService", "userService", "formatService", function ($injector, $log, $filter, expressionService, eventService, userService, formatService) {

        var isFieldHidden = function (datamap, schema, fieldMetadata) {
            fieldMetadata.jscache = fieldMetadata.jscache || {};
            if (fieldMetadata.jscache.isHidden != undefined) {
                return fieldMetadata.jscache.isHidden;
            }
            const baseHidden = fieldMetadata.isHidden || (fieldMetadata.type != "ApplicationSection" && isIdFieldAndNotReadOnly(fieldMetadata, schema));
            const isTabComposition = fieldMetadata.type == "ApplicationCompositionDefinition" && !fieldMetadata.inline;
            if (baseHidden || isTabComposition) {
                fieldMetadata.jscache.isHidden = true;
                return true;
            } else if (fieldMetadata.type === "ApplicationSection" && fieldMetadata.resourcepath == null &&
                (fieldMetadata.displayables.length === 0 || $.grep(fieldMetadata.displayables, function (e) {
                      return !isFieldHidden(datamap, schema, e);
            }).length == 0)) {

                //            fieldMetadata.jscache.isHidden = true;
                return true;
            }
            //the opposite of the expression: showexpression --> hidden
            const result = !expressionService.evaluate(fieldMetadata.showExpression, datamap);
            if (schema.stereotype == "List") {
                //list schemas can be safely cached since if the header is visible the rest would be as well
                fieldMetadata.jscache.isHidden = result;
            }
            return result;
        };

        var isIdField = function (fieldMetadata, schema) {
            return fieldMetadata.attribute == schema.idFieldName;
        };

        var isIdFieldAndNotReadOnly = function (fieldMetadata, schema) {
            if (!isIdField(fieldMetadata, schema)) {
                return false;
            }
            return (schema.stereotype == "Detail" && schema.mode == "input" || schema.stereotype == "DetailNew") && !fieldMetadata.isReadOnly;
        }



        var api = {
            isFieldHidden: function (datamap, application, fieldMetadata) {
                if (fieldMetadata == null) {
                    return false;
                }
                return isFieldHidden(datamap, application, fieldMetadata);
            },

            getParameter : function(fieldMetadata, key) {
                return fieldMetadata.rendererParameters[key];
            },

            isFieldRequired: function (fieldMetadata, datamap) {
                if (fieldMetadata.type === "ApplicationSection" && fieldMetadata.parameters) {
                    return "true" === fieldMetadata.parameters["required"];
                }
                const requiredExpression = fieldMetadata.requiredExpression;
                if (requiredExpression != undefined) {
                    return expressionService.evaluate(requiredExpression, datamap);
                }
                return requiredExpression;
            },

            isFieldReadOnly: function (datamap, application, fieldMetadata, scope) {
                //test the metadata read-only property
                const isReadOnly = fieldMetadata.isReadOnly;

                //check if field is diable via other means
                if (isReadOnly) {
                    return true;
                }
                if (fieldMetadata.enableExpression == null) {
                    return false;
                }
                return !expressionService.evaluate(fieldMetadata.enableExpression, datamap, scope);
            },

            fieldHasValue: function (datamap, fieldMetadata) {
                //if the message field doesn't have a value
                if (fieldMetadata.attribute === 'message' && !!datamap.message && datamap.message === '\n                                    ') {
                    return false;
                }

                return !!datamap[fieldMetadata.attribute];
            },

            isPropertyTrue: function (field, propertyName) {
                if (!field) {
                    return false;
                }
                return field.rendererParameters && "true" == field.rendererParameters[propertyName];
            },

            nonTabFields: function (displayables, includeHiddens) {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="displayables"></param>
                /// <param name="includeHiddens">whether hidden fields should be included or not</param>
                /// <returns type=""></returns>
                includeHiddens = includeHiddens == undefined ? true : includeHiddens;
                const result = [];
                for (let i = 0; i < displayables.length; i++) {
                    const displayable = displayables[i];
                    if (!this.isTabOrComposition(displayable)) {
                        if (includeHiddens || !displayable.isHidden) {
                            result.push(displayable);
                        }
                    }
                }
                return result;
            },

            fillDefaultValues: function (displayables, datamap, scope) {
                $.each(displayables, function (key, value) {
                    const target = value.attribute;
                    if (value.displayables && value.displayables.length > 0) {
                        //section
                        api.fillDefaultValues(value.displayables, datamap, scope);
                    }

                    //Only continues if datmap for the current attribute is null
                    if (target != undefined && datamap[target] == null) {
                        let expressionResult = null;
                        const displayable = displayables[key];
                        if (displayable.evalExpression != null) {
                            expressionResult = expressionService.evaluate(displayable.evalExpression, datamap, scope, displayable);
                            datamap[target] = expressionResult;
                        } else if (displayable.defaultExpression != null) {
                            expressionResult = expressionService.evaluate(displayable.defaultExpression, datamap, scope, displayable);
                            datamap[target] = expressionResult;
                        }
                        if (expressionResult == null && value.defaultValue != null) {
                            if (value.defaultValue == "@now") {
                                datamap[target] = formatService.format("@now", value, null);
                                //                            datamap[target] = $filter('date')(new Date(), dateFormat)
                            } else {
                                let parsedUserValue = userService.readProperty(value.defaultValue);
                                if (displayable.rendererType == "numericinput" && parsedUserValue) {
                                    parsedUserValue = parseInt(parsedUserValue);
                                }
                                if (displayable.rendererType === "checkbox" && parsedUserValue) {
                                    parsedUserValue = parseBooleanValue(parsedUserValue);
                                }
                                datamap[target] = parsedUserValue;
                            }
                        }
                    }
                });
                return datamap;
            },

            isAssociation: function (displayable) {
                const type = displayable.type;
                return type === "ApplicationAssociationDefinition";
            },

            isLazyAssociation: function (displayable) {
                return this.isAssociation && displayable.schema.isLazyLoaded;
            },

            isTabComposition: function (displayable) {
                const type = displayable.type;
                return type === "ApplicationCompositionDefinition" && !displayable.inline;
            },

            isSection:function(displayable) {
                const type = displayable.type;
                return type === "ApplicationSection";
            },

            isInlineComposition: function (displayable) {
                const type = displayable.type;
                return type === "ApplicationCompositionDefinition" && displayable.inline;
            },

            isListOnlyComposition: function (displayable) {
                const type = displayable.type;
                return type === "ApplicationCompositionDefinition" && displayable.schema.schemas.detail == null;
            },

            isTabOrComposition: function (displayable) {
                return this.isTab(displayable) || this.isTabComposition(displayable);
            },

            isTab: function (displayable) {
                if (displayable == null) {
                    return false;
                }
                const type = displayable.type;
                return type === "ApplicationTabDefinition";
            },

            getDisplayableByKey: function (schema, key) {
                schema.jscache = schema.jscache || {};
                schema.jscache.fieldsByKey = schema.jscache.fieldsByKey || {};
                if (schema.jscache.fieldsByKey[key] != undefined) {
                    return schema.jscache.fieldsByKey[key];
                }
                const displayables = schema.displayables;
                for (let i = 0; i < displayables.length; i++) {
                    const displayable = displayables[i];
                    if ((displayable.attribute && displayable.attribute == key) || (displayable.tabId && displayable.tabId == key)) {
                        schema.jscache.fieldsByKey[key] = displayable;
                        return displayable;
                    }
                    if (displayable.displayables != undefined) {
                        const innerDisplayables = this.getDisplayableByKey(displayable, key);
                        if (innerDisplayables != undefined && ((innerDisplayables.attribute && innerDisplayables.attribute == key) || (innerDisplayables.tabId && innerDisplayables.tabId == key))) {
                            schema.jscache.fieldsByKey[key] = innerDisplayables;
                            return innerDisplayables;
                        }
                    }
                }
                return null;
            },

            getDisplayablesByAssociationKey: function (schema, associationKey) {
                schema.jscache = schema.jscache ||{};
                const cacheEntry = schema.jscache.displayablesByAssociation = schema.jscache.displayablesByAssociation || {};
                if (cacheEntry[associationKey] != undefined) {
                    return cacheEntry[associationKey];
                }

                var result = [];
                var fn = this;
                $.each(schema.displayables, function (index, value) {
                    if (value.associationKey == associationKey) {
                        result.push(value);
                    } else if (value.displayables != undefined) {
                        const innerDisplayables = fn.getDisplayablesByAssociationKey(value, associationKey);
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
                for (let i = 0; i < displayables.length; i++) {
                    if (!this.isFieldHidden(datamap, application, displayables[i])) {
                        count++;
                    }
                }
                return count;
            },

            getVisibleDisplayables: function (datamap, schema) {
                const displayables = schema.displayables;
                const result = [];
                for (let i = 0; i < displayables.length; i++) {
                    if (!this.isFieldHidden(datamap, schema, displayables[i])) {
                        result.push(displayables[i]);
                    }
                }
                return result;
            },

            getDisplayableIndexByKey: function (schema, key) {
                const displayables = schema.displayables;
                for (let i = 0; i < displayables.length; i++) {
                    //is this the current field?
                    const fieldMetadata = displayables[i];
                    if (fieldMetadata.attribute && fieldMetadata.attribute == key) {
                        return i;
                    }
                }
                return -1;
            },

            getVisiableDisplayableIdxByKey: function (schema, attribute) {
                /// <summary>
                /// Get the index for the supplied attribute key, skipping hidden fields.
                /// </summary>
                if (!schema) {
                    return -1;
                }
                schema.jscache = schema.jscache || {};
                const results = this.getLinearDisplayables(schema);
                for (let i = 0; i < results.length; i++) {
                    const result = results[i];
                    if (result.associationKey === attribute || result.target === attribute || result.attribute === attribute && !result.isHidden) {
                        return i;
                    }
                }
                return -1;
            },

            getLinearDisplayables: function (container) {
                /// <summary>
                /// gets a list of all the displayables of the current schema/section in a linear mode, excluding any sections/tabs themselves.
                /// </summary>
                /// <param name="container">either a schema or a section</param>
                /// <returns type=""></returns>
                container.jscache = container.jscache || {};
                if (container.jscache.alldisplayables) {
                    return container.jscache.alldisplayables;
                }
                const displayables = container.displayables;
                var result = [];
                for (let i = 0; i < displayables.length; i++) {
                    const displayable = displayables[i];
                    if (displayable.displayables) {
                        //at this point displayable is a section, calling recursively
                        result = result.concat(this.getLinearDisplayables(displayable));
                    } else {
                        result.push(displayable);
                    }
                }
                container.jscache.alldisplayables = result;
                return result;
            },

            getNextVisibleDisplayableIdx: function (datamap, schema, key) {
                //all fields, regardless of sections
                const displayables = this.getLinearDisplayables(schema);
                const fieldIdx = this.getVisiableDisplayableIdxByKey(schema, key);
                if (fieldIdx == -1 || fieldIdx == displayables.length) {
                    //no such field, or last field
                    return -1;
                }

                for (let i = fieldIdx + 1; i < displayables.length; i++) {
                    //is this the current field?
                    const fieldMetadata = displayables[i];

                    // also verifies if the field is actualy displayed on screen
                    if ($('[data-field="' + fieldMetadata.attribute + '"]').is(":hidden")) {
                        continue;
                    }

                    //if the current field is found, get the next visible and editable field
                    if (!this.isFieldHidden(datamap, schema, fieldMetadata) && !this.isFieldReadOnly(datamap, schema, fieldMetadata)) {
                        $log.getInstance("fieldService#getNextVisibleDisplayable").debug('found', fieldMetadata.attribute, fieldMetadata);
                        return i;

                    }
                }
                return -1;
            },

            getRequiredDisplayables: function (schema) {
                const displayables = schema.displayables;
                const result = [];
                for (let i = 0; i < displayables.length; i++) {
                    if (displayables[i].required) {
                        result.push(displayables[i]);
                    }
                }
                return result;
            },

            getId: function (datamap, schema) {
                return datamap[schema.idFieldName];
            },

            getDisplayablesOfTypes: function (displayables, types) {
                var result = [];
                var fn = this;
                $.each(displayables, function (key, value) {
                    const type = value.type;
                    if ($.inArray(type, types) != -1) {
                        result.push(value);
                    }
                    if (value.displayables != undefined) {
                        const innerDisplayables = fn.getDisplayablesOfTypes(value.displayables, types);
                        result = result.concat(innerDisplayables);
                    }
                });

                return result;
            },

            getDisplayablesOfRendererTypes: function (displayables, types) {
                var result = [];
                var fn = this;
                $.each(displayables, function (key, value) {
                    const type = value.rendererType;
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
            },

            onFieldChange: function (fieldMetadata, event) {
                $log.getInstance("sw4.fieldservice#onFieldChange").debug("Invoking before field change event");
                const result = eventService.beforechange(fieldMetadata, event);
                if (result === null) {
                    event.continue();
                    return;
                }

                //sometimes the event might be syncrhonous, returning either true of false
                if (result != undefined && result == false) {
                    event.interrupt();
                } else if (result != undefined && result == true) {
                    event.continue();
                }
            },

            postFieldChange: function (field, scope, oldValue, newValue) {
                const parameters = {
                    fields: scope.datamap,
                    target: field,
                    scope: scope,
                    oldValue: oldValue,
                    newValue: newValue
                };
                $log.get("sw4.fieldservice#postfieldchange", ["event", "field", "afterchange"]).debug("Invoking post field change event.");
                eventService.afterchange(field, parameters);
            },

            /// <summary>
            /// simple function to allow mocking of dates on unittests
            /// </summary>
            currentDate: function () {
                return new Date();
            },

            getQualifier: function(field, datamap) {
                const qualifier = field.qualifier;
                if (!qualifier || !qualifier.startsWith("expression=")) return qualifier;

                var expression = qualifier.replace("expression=", "");
                expression = replaceAll(expression, "\'", "\"");

                var expressionObject;
                try {
                    expressionObject = JSON.parse(expression);
                } catch (e) {
                    console.error("Invalid qualifier expression for field", field, ":", expression);
                    throw e;
                }
                const currentValue = datamap[field.attribute];

                // immediate lookup
                if (expressionObject.hasOwnProperty(currentValue)) {
                    return expressionObject[currentValue];
                }

                for (let key in expressionObject) {
                    if (!expressionObject.hasOwnProperty(key)) continue;
                    let attribute = key.match(/@.+/g);
                    if (!attribute) continue;

                    // number of '!' is even (or zero)
                    const affirmative = (key.match(/!/g) || []).length % 2 === 0;

                    // attribute name from expression (without @ and !)
                    // TODO: add support for expressions with boolean operators (===, >=, <=, etc)
                    attribute = attribute[0].replace("@", "");
                    attribute = replaceAll(attribute, "!", "");
                    let evaluated = datamap[attribute];
                    evaluated = [false, 0, undefined, null, "false"].indexOf(evaluated) >= 0 ? false : true;
                    const value = expressionObject[key];
                    if (affirmative && evaluated) {
                        return value;
                    } else if (!affirmative && !evaluated) {
                        return value;
                    }
                }

                return expressionObject["#default"];
            }
        };



        return api;

    }]);

})(angular, modules);