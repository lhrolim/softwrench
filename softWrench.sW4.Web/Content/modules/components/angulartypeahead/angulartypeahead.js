
(function (angular, Bloodhound) {
    'use strict';




    function angularTypeahead(restService, contextService) {
        /// <summary>
        /// This directive integrates with bootsrap-typeahead 0.10.X
        /// </summary>
        /// <returns type=""></returns>

        function link(scope, el, attrs) {

            //createing defaults
            var minLength = scope.minLength || 2;
            var rateLimit = scope.rateLimit || 500;

            var element = $($('input.typeahead', el)[0]);

            var attribute = scope.attribute;
            var provider = scope.provider;
            var schema = scope.schema;

            var applicationName = schema.applicationName;
            var parameters = {
                key: {
                    schemaId: schema.schemaId,
                    mode: schema.mode,
                    platform: "web"
                },
                labelSearchString: "%QUERY",
                application: applicationName
            };

            parameters.filterProvider = provider;
            parameters.filterAttribute = attribute;

            var urlToUse = restService.getActionUrl("FilterData", "GetFilterOptions", parameters);
            urlToUse = replaceAll(urlToUse, '%25', "%");


            var engine = new Bloodhound({
                datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
                queryTokenizer: Bloodhound.tokenizers.whitespace,
                limit: 30,
                remote: {
                    url: urlToUse,
                    rateLimitFn: 'debounce',
                    rateLimitWait: rateLimit,
                    filter: function (parsedResponse) {
                        if (angular.isDefined(attrs.externalFilter)) {
                            //if we are on filterMode, we´ll use only the Bloodhound engine, but rather have an external control for the list instead of the typeahead default
                            return scope.externalFilter(parsedResponse);
                        }
                        return parsedResponse;
                    }
                }
            });

            engine.initialize();

            element.typeahead({ minLength: minLength }, {
                //TODO: allow more flexible setup than label
                displayKey: 'label',
                source: engine.ttAdapter()
            });

            element.on("typeahead:selected typeahead:autocompleted", function (e, datum) {
                scope.itemSelected({ item: datum });
//                if (datamap) {
//                    datamap[dataTarget] = datum.value;
//                    scope.associationOptions[associationKey] = [{ value: datum.value, label: datum.label, extrafields: datum.extrafields }];
//                    scope.$digest();
//                } else {
//                    //going down
//                    scope.$broadcast("sw.autocompleteserver.selected", e, datum, dataTarget);
//
//                }
            });

//            $(jelement.parent()).data('initialized', true);


            // Configure autocompletes layout
            $('span.twitter-typeahead', element).css('width', '100%');
            $('input.tt-hint', element).addClass('form-control');
            $('input.tt-hint', element).css('width', '96%');
            $('input.tt-query', element).css('width', '97%');


        };

        var directive = {
            link: link,
            restrict: 'E',
            templateUrl: contextService.getResourceUrl("/Content/modules/components/angulartypeahead/templates/angulartypeahead.html"),
            scope: {
                schema: '=',
                //the 
                attribute: '=',
                provider: '=',
                placeholder:'=',

                //min number of characters to start searching. Depending on the size of the dataset might be wise to put a higher number here.
                minLength: '@',

                //the limit in milliseconds to debounce the remote calls, avoiding multiple calls to occur repeatedly
                rateLimit: '@',
                //function to be executed upon any return from the server side. That function should receive an array a results and handle it properly. 
                // It can return a blank array to avoid the typeahead for showing any value on screen.
                //If not defined, the default function will get applied which will parse the result of the server based on the result of FilterDataController#GetFilterOptions
                externalFilter: '&',

                //function to be called when an item gets selected
                itemSelected: '&',

                isEnabled:'&',

                //function to handle corresponding jquery event
                keyup: '&',
                //function to handle corresponding jquery event
                keydown: '&',


            }
        };



        return directive;
    }

    angular.module('sw_typeahead', []).directive('angulartypeahead', ['restService', 'contextService', angularTypeahead]);

})(angular, Bloodhound);

