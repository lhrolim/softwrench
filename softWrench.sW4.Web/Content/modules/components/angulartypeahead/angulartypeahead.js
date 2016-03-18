
(function (angular, Bloodhound) {
    'use strict';




    function angularTypeahead(restService, $timeout, $log,
        contextService, associationService, crudContextHolderService, schemaService, datamapSanitizeService,compositionService) {
        /// <summary>
        /// This directive integrates with bootsrap-typeahead 0.10.X
        /// </summary>
        /// <returns type=""></returns>

        function beforeSendPostJsonDatamap(jqXhr, settings, datamap, isComposition) {
            if (datamap) {
                var datamapToSend = datamapSanitizeService.sanitizeDataMapToSendOnAssociationFetching(datamap);
                if (isComposition) {
                    datamapToSend = compositionService.buildMergedDatamap(datamapToSend, crudContextHolderService.rootDataMap());
                }
                var jsonString = angular.toJson(datamapToSend);
                settings.type = 'POST';
                settings.data = jsonString;
                settings.hasContent = true;
                jqXhr.setRequestHeader("Content-type", "application/json");
            }
            return true;
        }

        var configureSearchEngine = function (attrs, schema, provider, attribute, rateLimit,datamap,mode) {

            var applicationName = schema.applicationName;

            var parameters = {
                key: schemaService.buildApplicationMetadataSchemaKey(schema),
                labelSearchString: "%QUERY",
            };

            parameters.associationKey = provider;
//            parameters.filterAttribute = attribute;

            var urlToUse = restService.getActionUrl("Association", "GetFilteredOptions", parameters);
            urlToUse = replaceAll(urlToUse, '%25', "%");
            var isComposition = "composition" === mode;

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
                    },
                    ajax: {
                        beforeSend: function (jqXhr, settings) {

                            beforeSendPostJsonDatamap(jqXhr, settings, datamap, isComposition);
                        }
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

            var label = associationService.getLabelText(associationOption, scope.hideDescription, attributeValue);
            scope.log.debug("setting initial text of typeahead component {0} to {1}".format(scope.displayablepath, label));
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
                crudContextHolderService.updateLazyAssociationOption(scope.provider, datum, true);
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
            var log = $log.getInstance('angulartypeahed',['association','lookup']);
            //setting defaults
            var rateLimit = scope.rateLimit || 500;

            var element = $($('input.typeahead', el)[0]);
            var parElement = element.parent();

            scope.jelement = element;
            scope.log = log;

            var attribute = scope.attribute;
            var provider = scope.provider;
            var schema = scope.schema;

            log.debug("initing angulartypeahead for attribute {0}, provider {1}".format(attribute, provider));

            $timeout(function () {
                //let´s put this little timeout to delay the bootstrap-typeahead initialization to the next digest loop so that
                //it has enough to time to render itself on screen
                var engine = configureSearchEngine(attrs, schema, provider, attribute, rateLimit, scope.datamap,scope.mode);
                log.debug("configuring (after timeout) angulartypeahead for attribute {0}, provider {1}".format(attribute, provider));
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
            template: '<div class="input-group lazy-search">' +
                '<input type="search" class="hidden-phone form-control typeahead" placeholder="Find {{placeholder}}" ' +
                'data-association-key="{{provider}}" data-displayablepath="{{displayablepath}}"/>' +
                '<span class="input-group-addon last" ng-click="executeMagnetSearch()">' +
                '<i class="fa fa-search"></i>' +
                '</span>'+
            '<div></div><!--stop addon from moving on hover-->'+
            '</div>',
            scope: {
                schema: '=',
                datamap: '=',
                //the 
                attribute: '=',
                provider: '=',
                //full path of this displayable, considering eventual inline compositions (ex: #global_.asset, multiassetlocci_[1540].asset, multiassetlocci_[1550].asset where 1540, 1550 are the ids of the composition item)
                displayablepath: '=',
                placeholder: '=',
                //variable to expose the search text to outer scope, and allow programtic changes on the component
                searchText: '=',

                hideDescription: '@',
                mode: '@',

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

    angular.module('sw_typeahead', []).directive('angulartypeahead', ['restService', '$timeout', '$log', 'contextService', 'associationService', 'crudContextHolderService', 'schemaService', 'datamapSanitizeService', 'compositionService', angularTypeahead]);

})(angular, Bloodhound);

