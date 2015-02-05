﻿var app = angular.module('sw_layout');

app.factory('fieldService', function ($injector, $log, expressionService, eventService) {

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

        fillDefaultValues: function (displayables, datamap, scope) {
            $.each(displayables, function (key, value) {
                var target = value.attribute;
                
                //Only continues if datmap for the current attribute is null
                if (target != undefined && datamap[target] == null) {
                    var expressionResult = null;
                    if (displayables[key].evalExpression != null) {
                        expressionResult = expressionService.evaluate(displayables[key].evalExpression, datamap, scope);
                        datamap[target] = expressionResult;
                    } else if(displayables[key].defaultExpression != null) {
                        expressionResult = expressionService.evaluate(displayables[key].defaultExpression, datamap, scope);
                        datamap[target] = expressionResult;
                    }
                    if (expressionResult == null && value.defaultValue != null) {
                        //TODO: extract a service here, to be able to use @user, @person, @date, etc...
                        datamap[target] = value.defaultValue;
                   }
                }
            });
            return datamap;
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

        getVisibleDisplayables: function (datamap, schema) {
            var displayables = schema.displayables;
            var result = [];
            for (var i = 0; i < displayables.length; i++) {
                if (!this.isFieldHidden(datamap, schema, displayables[i])) {
                    result.push(displayables[i]);
                }
            }
            return result;
        },

        getRequiredDisplayables: function (schema) {
            var displayables = schema.displayables;
            var result = [];
            for (var i = 0; i < displayables.length; i++) {
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
        },

        onFieldChange: function (fieldMetadata, event) {
            var eventType = "beforechange";
            var beforeChangeEvent = fieldMetadata.events[eventType];
            var fn = eventService.loadEvent(fieldMetadata, eventType);
            if (fn == null) {
                event.continue();
                return;
            }
            $log.getInstance('sw4.fieldservice#onFieldChange').debug('invoking before field change service {0} method {1}'.format(beforeChangeEventservice, beforeChangeEvent.method));
            var result = fn(event);
            //sometimes the event might be syncrhonous, returning either true of false
            if (result != undefined && result == false) {
                event.interrupt();
            } else if (result != undefined && result == true) {
                event.continue();
            }
        },
        postFieldChange: function (field, scope) {
            var eventType = "afterchange";
            var afterChangeEvent = field.events[eventType];
            var fn = eventService.loadEvent(field, eventType);
            if (fn == null) {
                return;
            }
            var fields = scope.datamap;
            if (scope.datamap.fields != undefined) {
                fields = scope.datamap.fields;
            }

            var afterchangeEvent = {
                fields: fields,
                scope: scope
            };
            $log.getInstance('sw4.fieldservice#postfieldchange').debug('invoking post field change service {0} method {1}'.format(afterChangeEvent.service, afterChangeEvent.method));
            fn(afterchangeEvent);
        }
    };

});


