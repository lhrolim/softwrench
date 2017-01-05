(function (modules) {
    "use strict";
        
    modules.webcommons.service('i18NService', ["$rootScope", "contextService", function ($rootScope, contextService) {

    var verifyKeyInAllCatalogsButEn = function (key) {
        const catalogs = $rootScope['sw_i18ncatalogs'];
        var all = true;
        $.each(catalogs, function (language, catalog) {
            if (language == 'en') {
                return;
            }
            all = all && hasKey(key, catalog);
        });
        return all;
    }

    var hasKey = function (key, catalog) {
        var catalogValue = null;
        if (catalog != null) {
            catalogValue = JsonProperty(catalog, key);
        }
        return catalogValue != null;
    };

    var doGetValue = function (key, defaultValue, isMenu) {
        if (!nullOrUndef(contextService.retrieveFromContext('currentmodule')) && !isMenu) {
            return defaultValue;
        }
        const catalog = $rootScope['sw_currentcatalog'];
        var catalogValue = null;
        if (catalog != null) {
            catalogValue = JsonProperty(catalog, key);
        }
        if (catalogValue != null) {
            return catalogValue;
        }
        if (contextService.isLocal() && $rootScope.i18NRequired == true) {
            if (defaultValue == null) {
                return null;
            }

            if (sessionStorage['ignorei18n'] == undefined && !verifyKeyInAllCatalogsButEn(key)) {
                return "??" + defaultValue + "??";
            }
        }
        return defaultValue;
    };

    var valueConsideringSchemas = function (value, schema) {
        if (value == undefined) {
            return null;
        }

        if (typeof value == "string") {
            //single option goes here
            return value;
        }
        //but we can have multiple definitions, one for each schema...
        if (value.hasOwnProperty(schema.schemaId)) {
            return value[schema.schemaId];
        }
        //default declaration
        return value["_"];
    };

    //??
    function fillattr(fieldMetadata) {
        var attr;
        if (fieldMetadata.attribute == null && fieldMetadata.label != undefined) {
            attr = fieldMetadata.label.replace(":", "");
        } else {
            attr = fieldMetadata.attribute;
        }
        return attr;
    }

    return {

        getI18nLabel: function (fieldMetadata, schema) {
            if (fieldMetadata.type === "ApplicationCompositionDefinition" || fieldMetadata.type === "ApplicationSection") {
                const headerLabel = this.getI18nSectionHeaderLabel(fieldMetadata, fieldMetadata.header, schema);
                if (headerLabel != null && headerLabel !== "") {
                    return headerLabel;
                }
            }

            if (fieldMetadata.isHidden) {
                return "";
            }

            const applicationName = schema.applicationName;
            const attr = fillattr(fieldMetadata);
            var key = applicationName + "." + attr;
            if (fieldMetadata.type === "OptionField") {
                key += "._label";
            }
            const value = doGetValue(key, fieldMetadata.label);
            return valueConsideringSchemas(value, schema);
        },

        getI18nLabelTooltip: function (fieldMetadata, schema) {
            const applicationName = schema.applicationName;
            const attr = fillattr(fieldMetadata);
            var key = applicationName + "." + attr + "._tooltip";
            if (!hasKey(key, $rootScope['sw_currentcatalog'])) {
                //fallbacks to default label strategy
                key = applicationName + "." + attr;
                if (fieldMetadata.type == "OptionField") {
                    key += "._label";
                }
            }
            var defaultValue = fieldMetadata.toolTip;
            if (defaultValue == undefined) {
                defaultValue = fieldMetadata.label;
            }
        
            var value = doGetValue(key, defaultValue);

            //if the tooltip and label are the same, don't show tooltips
            if (value === fieldMetadata.label) {
                value = '';
            }

            return valueConsideringSchemas(value, schema);
        },

        getI18nOptionField: function (option, fieldMetadata, schema) {
            if (fieldMetadata.type != "OptionField" || fieldMetadata.providerAttribute != null) {
                //case there´s a providerattribute, 118N makes no sense
                return option.label;
            }
            const applicationName = schema.applicationName;
            const attr = fieldMetadata.attribute;
            const val = option.value == '' ? option.label : option.value;
            const key = applicationName + "." + attr + "." + val;
            const value = doGetValue(key, option.label);
            return valueConsideringSchemas(value, schema);
        },

        getI18nCommandLabel: function (command, schema) {
            const applicationName = schema.applicationName;
            const key = applicationName + "._commands." + command.id;
            const value = doGetValue(key, command.label);
            return valueConsideringSchemas(value, schema);
        },

        getI18nInputLabel: function (fieldMetadata, schema) {
            var label = this.getI18nLabel(fieldMetadata, schema);
            if (label === "") {
                return "";
            }
            const lastChar = label.charAt(label.length - 1);
            if (lastChar === ":" || lastChar === "?" || lastChar === "#" || fieldMetadata.type === 'ApplicationSection') {
                return label;
            }

            label = label + ':';
            return label;
        },

        getI18nMenuLabel: function (menuitem, tooltip) {
            if (nullOrUndef(menuitem.id)) {
                return tooltip ? menuitem.tooltip : menuitem.title;
            }
            var defaultValue = menuitem.title;
            var key = "_menu." + menuitem.id;
            if (tooltip) {
                key += "_tooltip";
                defaultValue = menuitem.tooltip;
            }
            return doGetValue(key, defaultValue, true);
        },
        getI18nMenuIcon: function (menuitem) {
            if (nullOrUndef(menuitem.id)) {
                return menuitem.icon;
            }
            const defaultValue = menuitem.icon;
            const key = "_menu." + menuitem.id + "_icon";
            return doGetValue(key, defaultValue, true);
        },

        getI18nPlaceholder: function (fieldMetadata) {
            const label = fieldMetadata.label;
            const lastChar = label.charAt(label.length - 1);
            if (lastChar != ":") {
                return label;
            }
            const placeholder = label.substr(0, label.length - 1);
            return placeholder
        },

        getI18nRecordLabel: function (schema, datamap) {
            if (datamap == null || schema == null) {
                return "";
            }
            const userIdFieldName = schema.userIdFieldName;
            const userId = datamap[userIdFieldName];
            if (schema.idDisplayable && userId != null) {
                return '{0} {1}'.format(schema.idDisplayable, userId);

                //TODO: evaluate returning the userId as fallback, unless a schema property is set to hide the id lable
                //if (userId != null) {
                //    if (schema.idDisplayable) {
                //        return '{0} {1}'.format(schema.idDisplayable, userId);
                //    }
                //    return ' ' + userId;
            }
            return "";
        },

        getI18nTitle: function (schema) {
            const applicationName = schema.applicationName;
            const key = applicationName + "._title." + schema.schemaId;
            return doGetValue(key, schema.title);
        },

        get18nValue: function (key, defaultValue, paramArray) {
            const isHeaderMenu = (key!=null && key.indexOf("_headermenu") > -1) ? true : false;
            const unformatted = doGetValue(key, defaultValue, isHeaderMenu);
            if (paramArray == undefined) {
                return unformatted;
            }
            const formatFn = unformatted.format;
            return formatFn.apply(unformatted, paramArray);
        },

        getI18nSectionHeaderLabel: function (section, header, schema) {
            if (header == undefined) {
                return "";
            }
            const applicationName = schema.applicationName;
            section = !nullOrUndef(section.id) ? section.id : section.relationship;
            const key = applicationName + "." + section + "._header";
            const value = doGetValue(key, header.label);
            return valueConsideringSchemas(value, schema);
        },

        getTabLabel: function (tab, schema) {
            const applicationName = tab.applicationName;
            const key = applicationName + "." + tab.id + "._title";
            const value = doGetValue(key, tab.label);
            return valueConsideringSchemas(value, schema);
        },

        getLookUpDescriptionLabel: function(fieldMetadata){
                
            if (fieldMetadata && fieldMetadata.rendererParameters && fieldMetadata.rendererParameters.columnlabel) {
                return fieldMetadata.rendererParameters.columnlabel.toUpperCase();
            }
            
            return this.get18nValue('general.description', 'Description', null);
            
        },
       
        load: function (jsonString, language) {
            const languages = JSON.parse(jsonString);
            var catalogs = {};
            $.each(languages, function (key, value) {
                catalogs[key] = value;
            });
            $rootScope['sw_i18ncatalogs'] = catalogs;
            this.changeCurrentLanguage(language);
        },

        getCurrentLanguage: function () {
            return $rootScope['sw_userlanguage'];
        },

        changeCurrentLanguage: function (language) {
            $rootScope['sw_userlanguage'] = language;
            const normalizedLanguage = language != null ? language.toLowerCase() : '';
            $rootScope['sw_currentcatalog'] = $rootScope['sw_i18ncatalogs'][normalizedLanguage];
            //broadcast language changed event to update filter label translations.
            $rootScope.$broadcast("sw_languageChanged", normalizedLanguage);
        }


    };

}]);

})(modules);