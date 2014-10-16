var app = angular.module('sw_layout');

app.factory('cmpAutocompleteServer', function ($log, associationService,restService) {

    function beforeSendPostJsonDatamap(jqXhr, settings, datamap) {
        var jsonString = angular.toJson(datamap);
        settings.type = 'POST';
        settings.data = jsonString;
        settings.hasContent = true;
        jqXhr.setRequestHeader("Content-type", "application/json");
        return true;
    }

    return {

        refreshFromAttribute: function (associationFieldMetadata, scope) {
            var value = associationService.getFullObject(associationFieldMetadata, scope.datamap, scope.associationOptions);
            var associationkey = associationFieldMetadata.associationKey;
            var label = value == null ? null : value.label;
            $log.getInstance('cmpAutocompleteServer#refreshFromAttribute').debug("update autocomplete-server {0} with value {1}".format(associationkey, label));
            $("input[data-association-key=" + associationkey + "]").typeahead('val', label);
        },

        init: function (bodyElement, datamap, schema,scope) {
            $('input.typeahead', bodyElement).each(function (index, element) {
                var jelement = $(element);
                if (true == $(jelement.parent()).data('initialized')) {
                    return;
                }
                var associationKey = element.getAttribute('data-association-key');
                var dataTarget = element.getAttribute('data-target');
                $log.getInstance("cmpAutocompleteServer#init").debug("init autocomplete {0}".format(associationKey));

                var applicationName = schema.applicationName;
                var parameters = {};
                parameters.key = {};
                parameters.key.schemaId = schema.schemaId;
                parameters.key.mode = schema.mode;
                parameters.key.platform = platform();
                parameters.associationFieldName = associationKey;
                parameters.labelSearchString = "%QUERY";
                parameters.applicationName = applicationName;

                var urlToUse = restService.getActionUrl("Data","UpdateAssociation",parameters);

                var engine = new Bloodhound({

                    datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
                    queryTokenizer: Bloodhound.tokenizers.whitespace,
                    limit: 30,
                    remote: {
                        url: urlToUse,
                        rateLimitFn: 'debounce',
                        rateLimitWait: 500,
                        filter: function (parsedResponse) {
                            var options = parsedResponse.resultObject;
                            if (options[associationKey] != null) {
                                return options[associationKey].associationData;
                            }
                            return null;
                        },
                        ajax: {
                            beforeSend: function (jqXhr, settings) {
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
                    datamap[dataTarget] = datum.value;
                    scope.associationOptions[associationKey] = [{ value: datum.value, label: datum.label,extrafields:datum.extrafields }];
                    scope.$digest();
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

});


