var app = angular.module('sw_layout');

app.factory('i18NService', function ($rootScope, contextService) {
    "ngInject";

    var verifyKeyInAllCatalogsButEn = function (key) {
        var catalogs = $rootScope['sw_i18ncatalogs'];
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

    var doGetValue = function (key, defaultValue, isMenu, languageToForce) {
        if (!languageToForce && !nullOrUndef(contextService.retrieveFromContext('currentmodule')) && !isMenu) {
            return defaultValue;
        }
        var catalog = $rootScope['sw_currentcatalog'];
        if (languageToForce && languageToForce != '') {
            catalog = $rootScope['sw_i18ncatalogs'][languageToForce];
        } 
        var catalogValue = null;
        if (catalog != null) {
            catalogValue = JsonProperty(catalog, key);
        }
        if (catalogValue != null) {
            return catalogValue;
        }
        if ($rootScope.isLocal && $rootScope.i18NRequired == true) {
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
            if (fieldMetadata.type == "ApplicationCompositionDefinition" || fieldMetadata.type == "ApplicationSection") {
                var headerLabel = this.getI18nSectionHeaderLabel(fieldMetadata, fieldMetadata.header, schema);
                if (headerLabel != null && headerLabel != "") {
                    return headerLabel;
                }
            }
            var applicationName = schema.applicationName;

            var attr = fillattr(fieldMetadata);
            var key = applicationName + "." + attr;
            if (fieldMetadata.type == "OptionField") {
                key += "._label";
            }
            var value = doGetValue(key, fieldMetadata.label);
            return valueConsideringSchemas(value, schema);
        },

        getI18nLabelTooltip: function (fieldMetadata, schema) {
            var applicationName = schema.applicationName;
            var attr = fillattr(fieldMetadata);
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
            return valueConsideringSchemas(value, schema);
        },


        getI18nOptionField: function (option, fieldMetadata, schema) {
            if (fieldMetadata.type != "OptionField" || fieldMetadata.providerAttribute != null) {
                //case there´s a providerattribute, 118N makes no sense
                return option.label;
            }
            var applicationName = schema.applicationName;
            var attr = fieldMetadata.attribute;
            var val = option.value == '' ? option.label : option.value;
            var key = applicationName + "." + attr + "." + val;
            var value = doGetValue(key, option.label);
            return valueConsideringSchemas(value, schema);
        },

        getI18nCommandLabel: function (command, schema) {
            var applicationName = schema.applicationName;
            var key = applicationName + "._commands." + command.id;
            var value = doGetValue(key, command.label);
            return valueConsideringSchemas(value, schema);
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

        getI18nTitle: function (schema) {
            var applicationName = schema.applicationName;
            var key = applicationName + "._title." + schema.schemaId;
            return doGetValue(key, schema.title);
        },

        get18nValue: function (key, defaultValue, paramArray, languageToForce) {
            var isHeaderMenu = (key.indexOf("_headermenu") > -1) ? true : false;
            var unformatted = doGetValue(key, defaultValue, isHeaderMenu, languageToForce);
            if (paramArray == undefined) {
                return unformatted;
            }
            var formatFn = unformatted.format;
            return formatFn.apply(unformatted, paramArray);
        },

        getI18nSectionHeaderLabel: function (section, header, schema) {
            if (header == undefined) {
                return "";
            }
            var applicationName = schema.applicationName;
            section = !nullOrUndef(section.id) ? section.id : section.relationship;
            var key = applicationName + "." + section + "._header";
            var value = doGetValue(key, header.label);
            return valueConsideringSchemas(value, schema);
        },

        getTabLabel: function (tab, schema) {
            var applicationName = tab.applicationName;
            var key = applicationName + "." + tab.id + "._title";
            var value = doGetValue(key, tab.label);
            return valueConsideringSchemas(value, schema);
        },

        load: function (jsonString, language) {
            var languages = JSON.parse(jsonString);
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
            if (language == null) {
                language = "en";
            }
            $rootScope['sw_userlanguage'] = language;
            $rootScope['sw_currentcatalog'] = $rootScope['sw_i18ncatalogs'][language.toLowerCase()];
            //broadcast language changed event to update filter label translations.
            $rootScope.$broadcast("sw_languageChanged", language.toLowerCase());
        }


    };

});


