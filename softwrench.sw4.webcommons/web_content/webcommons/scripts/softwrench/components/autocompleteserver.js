
(function (angular, Bloodhound) {
    'use strict';


    function autocompleteServer($log, associationService, restService) {

        function beforeSendPostJsonDatamap(jqXhr, settings, datamap) {
            if (datamap) {
                var jsonString = angular.toJson(datamap);
                settings.type = 'POST';
                settings.data = jsonString;
                settings.hasContent = true;
                jqXhr.setRequestHeader("Content-type", "application/json");
            }
            return true;
        }

        return {
            refreshFromAttribute: function (associationFieldMetadata, scope) {
                //var contextData = scope.ismodal === "true" ? { schemaId: "#modal" } : null;
                var value = associationService.getFullObject(associationFieldMetadata, scope.datamap/*, contextData*/);
                var associationkey = associationFieldMetadata.associationKey;
                var label = value == null ? null : value.label;
                $log.getInstance('cmpAutocompleteServer#refreshFromAttribute').debug("update autocomplete-server {0} with value {1}".format(associationkey, label));
                $("input[data-association-key=" + associationkey + "]").typeahead('val', label);
            },

            init: function (bodyElement, datamap, schema, scope) {
                $('input.typeahead', bodyElement).each(function (index, element) {
                    var jelement = $(element);
                    if (true === $(jelement.parent()).data('initialized')) {
                        return;
                    }
                    var associationKey = element.getAttribute('data-association-key');
                    var dataTarget = element.getAttribute('data-target');
                    var filterProvider = element.getAttribute('data-filterprovider');
                    var filterMode = filterProvider != null;

                    $log.getInstance("cmpAutocompleteServer#init",["lookup"]).debug("init autocomplete {0}".format(associationKey));

                    var applicationName = schema.applicationName;
                    var parameters = {
                        key: {
                            schemaId: schema.schemaId,
                            mode: schema.mode,
                            platform: "web",
                            applicationName: applicationName
                        },
                        labelSearchString: "%QUERY",
                        application: applicationName
                    };

                    var urlToUse;
                    if (filterMode) {
                        parameters.filterProvider = filterProvider;
                        parameters.filterAttribute = dataTarget;
                        urlToUse = restService.getActionUrl("FilterData", "GetFilterOptions", parameters);
                    } else {
                        parameters.associationFieldName = associationKey;
                        urlToUse = restService.getActionUrl("Data", "UpdateAssociation", parameters);
                    }


                    //the %QUERY was being converted to %25QUERY breaking the component
                    urlToUse = replaceAll(urlToUse, '%25', "%");
                    var engine = new Bloodhound({
                        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
                        queryTokenizer: Bloodhound.tokenizers.whitespace,
                        limit: 30,
                        remote: {
                            url: urlToUse,
                            rateLimitFn: 'debounce',
                            rateLimitWait: 500,
                            filter: function (parsedResponse) {
                                if (filterMode) {
                                    //if we are on filterMode, we´ll use only the Bloodhound engine, but rather have an external control for the list instead of the typeahead default
                                    scope.$broadcast("sw.autocompleteserver.response", parsedResponse);
                                    return [];
                                }

                                if (Array.isArray(parsedResponse)) {
                                    return parsedResponse;
                                }

                                var options = parsedResponse.resultObject;
                                if (options[associationKey] != null) {
                                    return options[associationKey].associationData;
                                }
                                return null;
                            },
                            ajax: {
                                beforeSend: function (jqXhr, settings) {
                                    scope.$broadcast("sw.autocompleteserver.beforesend");
                                    beforeSendPostJsonDatamap(jqXhr, settings, datamap);
                                }
                            }

                        }
                    });
                    engine.initialize();

                    jelement.typeahead({ minLength: 2 }, {
                        displayKey: 'label',
                        source: engine.ttAdapter()
                    });

              

                    jelement.on("typeahead:selected typeahead:autocompleted", function (e, datum) {
                        if (datamap) {
                            datamap[dataTarget] = datum.value;
                            scope.associationOptions[associationKey] = [{ value: datum.value, label: datum.label, extrafields: datum.extrafields }];
                            scope.$digest();
                        } else {
                            //going down
                            scope.$broadcast("sw.autocompleteserver.selected", e, datum, dataTarget);
                            
                        }
                        
                    });

                    $(jelement.parent()).data('initialized', true);

                });

                // Configure autocompletes layout
                $('span.twitter-typeahead', bodyElement).css('width', '100%');
                $('input.tt-hint', bodyElement).addClass('form-control');
                $('input.tt-hint', bodyElement).css('width', '96%');
                $('input.tt-query', bodyElement).css('width', '97%');
            }

        }
    }

    angular.module('sw_layout').factory('cmpAutocompleteServer', ['$log', 'associationService', 'restService', autocompleteServer]);

})(angular, Bloodhound);


