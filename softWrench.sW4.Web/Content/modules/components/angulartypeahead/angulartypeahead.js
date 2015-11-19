
(function (angular, Bloodhound) {
    'use strict';




    function angularTypeahead(restService, $timeout, $log, contextService, associationService, crudContextHolderService, schemaService) {
        /// <summary>
        /// This directive integrates with bootsrap-typeahead 0.10.X
        /// </summary>
        /// <returns type=""></returns>


        var configureSearchEngine = function (attrs, schema, provider, attribute, rateLimit) {

            var applicationName = schema.applicationName;

            var parameters = {
                key: schemaService.buildApplicationMetadataSchemaKey(schema),
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
            return engine;
        }

        var configureStyles = function (element, parElement) {
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
        }

        var setInitialText = function (element, scope) {
            var attributeValue = scope.datamap[scope.attribute];
            if (attributeValue == null) {
                //no initial value present
                return;
            }
            var associationOption = crudContextHolderService.fetchLazyAssociationOption(scope.provider, scope.datamap[scope.attribute]);
            var label = associationService.getLabelText(associationOption, scope.hideDescription);
            element.typeahead('val', label);
        }

        var configureJqueryHooks = function (scope, element, engine) {

            var minLength = scope.minLength || 2;

            //initing typeahead itself
            element.typeahead({ minLength: minLength }, {
                displayKey: function (item) {
                    return associationService.getLabelText(item, scope.hideDescription);
                },
                source: engine.ttAdapter()
            });


            element.on("typeahead:selected typeahead:autocompleted", function (e, datum) {
                var datamap = scope.datamap;
                if (datamap) {
                    datamap[scope.attribute] = datum.value;
                    scope.$digest();
                }
                scope.itemSelected({ item: datum });
            });

            element.on("focus", function (e) {
                $timeout(function () {
                    element.typeahead("moveToBegin");
                    element.typeahead("highlight");
                }, 0, false);

            });

            element.on("keyup", function (e) {
                scope.searchText = scope.jelement.val();
                if (scope.searchText === "") {
                    var datamap = scope.datamap;
                    if (datamap) {
                        $log.getInstance("angulartypeahead#keyup").debug("cleaning datamap");
                        datamap[scope.attribute] = null;
                    }
                }
                scope.$digest();
                //if filter is applied, let´s not show recently used filters

            });
        }

        function link(scope, el, attrs) {
            scope.name = "angulartypeahead";
            var log = $log.getInstance('angulartypeahed');
            //setting defaults
            var rateLimit = scope.rateLimit || 500;

            var element = $($('input.typeahead', el)[0]);
            var parElement = element.parent();

            scope.jelement = element;

            var attribute = scope.attribute;
            var provider = scope.provider;
            var schema = scope.schema;

            log.debug("initing angulartypeahead for attribute {0}, provider {1}".format(attribute, provider));

            $timeout(function () {
                //let´s put this little timeout to delay the bootstrap-typeahead initialization to the next digest loop so that
                //it has enough to time to render itself on screen
                var engine = configureSearchEngine(attrs, schema, provider, attribute, rateLimit);
                configureJqueryHooks(scope, element, engine);

                if (crudContextHolderService.associationsResolved()) {
                    setInitialText(element, scope);
                }

                configureStyles(element, parElement);
            }, 0, false);


            scope.executeMagnetSearch = function () {
                scope.magnetClicked({ text: scope.getText() });
            }

            scope.getText = function () {
                return scope.jelement.typeahead("val");
            }

            scope.$on("sw_associationsresolved", function (event) {
                setInitialText(element, scope);
            });

        };



        var directive = {
            link: link,
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl("/Content/modules/components/angulartypeahead/templates/angulartypeahead.html"),
            scope: {
                schema: '=',
                datamap: '=',
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

                //function to be called when an item gets selected
                magnetClicked: '&',

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

    angular.module('sw_typeahead', []).directive('angulartypeahead', ['restService', '$timeout', '$log', 'contextService', 'associationService', 'crudContextHolderService', 'schemaService', angularTypeahead]);

})(angular, Bloodhound);

