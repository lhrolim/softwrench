
(function (angular, Bloodhound) {
    'use strict';




    function angularTypeahead(restService, contextService, associationService) {
        /// <summary>
        /// This directive integrates with bootsrap-typeahead 0.10.X
        /// </summary>
        /// <returns type=""></returns>

        function link(scope, el, attrs) {

            //createing defaults
            var minLength = scope.minLength || 2;
            var rateLimit = scope.rateLimit || 500;

            var element = $($('input.typeahead', el)[0]);
            var parElement = element.parent();

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
                displayKey: function (item) {
                    return associationService.getLabelText(item, scope.hideDescription);
                },
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

            element.on("keyup", function (e) {
                scope.searchText = $(e.target).val();
                scope.$digest();
                //if filter is applied, let´s not show recently used filters

            });


            //            $(jelement.parent()).data('initialized', true);


            // Configure autocompletes layout
            var typeAhead = $('span.twitter-typeahead', parElement);
            typeAhead.css('width', '100%');
            typeAhead.css('display', 'block');
            var ttHint = $('input.tt-hint', parElement);
            ttHint.addClass('form-control');
            ttHint.css('width', '96%');
            $('input.tt-query', parElement).css('width', '97%');
            var dropDownMenu = $('span.tt-dropdown-menu', parElement);
            var width = element.outerWidth();
            dropDownMenu.width(width);


        };

        var directive = {
            link: link,
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl("/Content/modules/components/angulartypeahead/templates/angulartypeahead.html"),
            scope: {
                schema: '=',
                //the 
                attribute: '=',
                provider: '=',
                placeholder: '=',
                //variable to expose the search text to outer scope, and allow programtic changes on the component
                searchText: '=',

                hideDescription: '@',

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

                isEnabled: '&',

                //function to handle corresponding jquery event
                keyup: '&',
                //function to handle corresponding jquery event
                keydown: '&',
                onFocus: '&'


            }
        };



        return directive;
    }

    angular.module('sw_typeahead', []).directive('angulartypeahead', ['restService', 'contextService', 'associationService', angularTypeahead]);

})(angular, Bloodhound);

