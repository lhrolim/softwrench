
(function (angular, Bloodhound) {
    'use strict';




    function angularTypeahead(restService, $timeout, $log, $rootScope,
        contextService, associationService, lookupService, crudContextHolderService, schemaService, datamapSanitizeService, compositionService, expressionService, fieldService, spinService) {
        /// <summary>
        /// This directive integrates with bootsrap-typeahead 0.10.X
        /// </summary>
        /// <returns type=""></returns>

        function beforeSendPostJsonDatamap(jqXhr, settings) {
            const datamap = this.datamap;
            const isComposition = this.mode === "composition";
            this.loading = true;
            //            if (!this.spinnerElement) {
            //                //caching spinner
            //                this.spinerElement = $('[data-field="spinner_' + this.provider + '"]');
            //            }

            this.spinner = spinService.start({ savingDetail: true });

            if (datamap) {
                let datamapToSend = datamapSanitizeService.sanitizeDataMapToSendOnAssociationFetching(datamap);
                if (isComposition) {
                    datamapToSend = compositionService.buildMergedDatamap(datamapToSend, crudContextHolderService.rootDataMap());
                }
                const jsonString = angular.toJson(datamapToSend);
                settings.type = 'POST';
                settings.data = jsonString;
                settings.hasContent = true;
                jqXhr.setRequestHeader("Content-type", "application/json");
            }
            return true;
        }

        function filter(parsedResponse) {
            this.loading = false;
            spinService.stop();
            if (angular.isDefined(this.attrs.externalFilter)) {
                //if we are on filterMode, we´ll use only the Bloodhound engine, but rather have an external control for the list instead of the typeahead default
                return this.externalFilter(parsedResponse);
            }
            return parsedResponse;
        }




        var configureSearchEngine = function (scope, schema, provider, attribute, rateLimit, datamap, mode, fieldMetadata) {
            const isDataEagerFetched = fieldMetadata.type === "OptionField";
            const parameters = {
                key: schemaService.buildApplicationMetadataSchemaKey(schema),
                labelSearchString: "%QUERY"
            };
            parameters.associationKey = provider;
            //            parameters.filterAttribute = attribute;

            var urlToUse = restService.getActionUrl("Association", "GetFilteredOptions", parameters);
            urlToUse = replaceAll(urlToUse, '%25', "%");

            const bloodHoundOptions = {
                datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
                queryTokenizer: Bloodhound.tokenizers.whitespace,
                limit: 30
            };

            const beforeSendPostJsonDatamapFn = beforeSendPostJsonDatamap.bind(scope);
            const filterFn = filter.bind(scope);


            if (!isDataEagerFetched) {
                bloodHoundOptions.remote = {
                    url: urlToUse,
                    rateLimitFn: 'debounce',
                    rateLimitWait: rateLimit,
                    filter: filterFn,
                    ajax: {
                        beforeSend: beforeSendPostJsonDatamapFn
                    }
                }
            } else {
                bloodHoundOptions.local = function () {
                    const eagerLookupOptions = lookupService.getEagerLookupOptions(new LookupDTO(fieldMetadata, null));
                    return eagerLookupOptions == null ? [] : eagerLookupOptions.resultObject.associationData;
                }
            }


            const engine = new Bloodhound(bloodHoundOptions);
            engine.initialize();
            return engine;
        }

        var configureStyles = function (element, parElement) {
            // Configure autocompletes layout
            const typeAhead = $('span.twitter-typeahead', parElement);
            typeAhead.css('width', '100%');
            typeAhead.css('display', 'block');
            const ttHint = $('input.tt-hint', parElement);
            ttHint.addClass('form-control');
            ttHint.css('width', '96%');
            $('input.tt-query', parElement).css('width', '97%');
            //var dropDownMenu = $('span.tt-dropdown-menu', parElement);
            //var width = element.outerWidth();
            //dropDownMenu.width(width);
        }

        var setInitialText = function (element, scope) {
            const attributeValue = scope.datamap[scope.attribute];
            if (attributeValue == null) {
                //no initial value present
                return;
            }
            associationService.getLabelText(scope.provider, scope.datamap[scope.attribute], { hideDescription: scope.hideDescription, allowTransientValue: scope.allowCustomValue === "true", isEager: !!scope.fieldMetadata.providerAttribute })
                .then(function (label) {
                    scope.log.debug("setting initial text of typeahead component {0} to {1}".format(scope.displayablepath, label));
                    element.typeahead('val', label);
                });

        }

        var configureJqueryHooks = function (scope, element, engine, fieldMetadata) {
            let minLength = scope.minLength || 2;
            if (!scope.shouldShowModal()) {
                minLength = 0;
            }

            const engineSourceFn = engine.ttAdapter();
            const sourceFn = fieldMetadata.type === "OptionField" ?
                function (query, cb) {
                    const results = lookupService.getEagerLookupOptions(new LookupDTO(fieldMetadata), new QuickSearchDTO(query));
                    if (results == null) {
                        return cb([]);
                    }
                    return cb(results.resultObject.associationData);
                } : engineSourceFn;


            //initing typeahead itself
            element.typeahead({ minLength: minLength, highlight: true }, {
                displayKey: function (item) {
                    return associationService.parseLabelText(item, { hideDescription: scope.hideDescription, allowTransientValue: scope.allowCustomValue === "true" });
                },
                source: sourceFn
            });


            element.on("typeahead:selected typeahead:autocompleted", function (e, datum) {
                const datamap = scope.datamap;
                if (datamap) {
                    datamap[scope.attribute] = datum.value;
                }
                associationService.updateUnderlyingAssociationObject(scope.fieldMetadata, datum, scope);
                scope.itemSelected({ item: datum });
                $rootScope.$digest();
                scope.jelement.blur();
            });

            element.on("focus", function (e) {
                $timeout(function () {
                    element.typeahead("moveToBegin");
                    element.typeahead("highlight");
                    $log.get("angulartypeahed#foucushandler", ["typeahead"]).trace("focus handler called moving to beginning and highlighting");
                }, 0, false);
            });


            element.on("typeahead:opened", function () {
                var initial = element.val(),
                    ev = $.Event("keydown");
                ev.keyCode = ev.which = 40;
                $(this).trigger(ev);
                if (element.val() !== initial) {
                    element.val(initial);
                }
                $log.get("angulartypeahed#typeaheadopened", ["typeahead"]).trace("opeend method called, triggering key down");
                return true;
            });

            element.on("keyup", function (e) {
                scope.searchText = scope.jelement.val();
                if (scope.searchText === "") {
                    const datamap = scope.datamap;
                    if (datamap) {
                        $log.getInstance("angulartypeahead#keyup").debug("cleaning datamap");
                        datamap[scope.attribute] = null;
                        //rootScope needed if datamap is change to reeval any related expressions that were bound to that particular item
                        $rootScope.$digest();
                    }
                } else {
                    //if filter is applied, let´s not show recently used filters
                    //scope digest is enough if we´re not clearing nor selecting an entry (i.e, not changing the datamap)
                    scope.$digest();
                }
            });
        }

        function link(scope, el, attrs) {

            scope.shouldShowModal = function () {
                return scope.fieldMetadata.rendererType === "lookup" && !fieldService.isPropertyTrue(scope.fieldMetadata, "disablemodal");
            }

            scope.name = "angulartypeahead";
            var log = $log.getInstance('angulartypeahed', ['association', 'lookup','typeahead']);
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

            scope.attrs = attrs;

            $timeout(function () {
                //let´s put this little timeout to delay the bootstrap-typeahead initialization to the next digest loop so that
                //it has enough to time to render itself on screen
                const engine = configureSearchEngine(scope, schema, provider, attribute, rateLimit, scope.datamap, scope.mode, scope.fieldMetadata);
                scope.engine = engine;
                log.debug("configuring (after timeout) angulartypeahead for attribute {0}, provider {1}".format(attribute, provider));
                configureJqueryHooks(scope, element, engine, scope.fieldMetadata);

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

            scope.$on('sw_cleartypeaheadtext', function (event) {
                if (scope && scope.jelement) {
                    scope.jelement.typeahead('val', '');
                }
            });

            scope.expandSearch = function () {
                const jel = scope.jelement;
                if (jel.typeahead("isOpen")) {
                    console.log("expand close");
                    jel.typeahead("close");
                } else {
                    console.log("expand open");
                    const val = jel.typeahead('val');
                    jel.typeahead('val', '');
                    jel.typeahead("open");
                    jel.typeahead('val', val);
                    jel.typeahead("highlight");
                }

            }

            function clearCache(jelement, engine) {
                jelement.typeahead("clearCache");
                engine.clearRemoteCache();
                engine.clearPrefetchCache();
                engine.clear();
            }

            scope.$on(JavascriptEventConstants.ClearAutoCompleteCache, (event, associationKey) => {
                if (scope.provider !== associationKey) {
                    //not the right handler... ignoring
                    return;
                }
                $log.get("angulartypeahead#clearcache", ["detail"]).debug(`clearing autocomplete cache for ${associationKey}`);
                clearCache(scope.jelement, scope.engine);
            });

            scope.$on(JavascriptEventConstants.DetailLoaded, function(event) {
                if (scope.engine) {
                    $log.get("angulartypeahead#clearcache", ["detail"]).debug(`clearing autocomplete cache for ${scope.provider}`);
                    //upon detail navigation
                    clearCache(scope.jelement, scope.engine);
                }
            });

            scope.$on(JavascriptEventConstants.CrudSaved, function (event) {
                if (scope.engine) {
                    $log.get("angulartypeahead#clearcache", ["detail"]).debug(`clearing autocomplete cache for ${scope.provider}`);
                    //once the crud has been saved we might also need to clear it, since different rules may apply
                    clearCache(scope.jelement, scope.engine);
                }
            });

            scope.$on(JavascriptEventConstants.AssociationResolved, function (event, panelid) {
                if (panelid != scope.panelid && !(panelid == null && scope.panelid === "")) {
                    //keep != to avoid errors
                    log.debug("ignoring event sw_associationsresolved for panelid {0} since we are on {1}".format(panelid, scope.panelid));
                    return;
                }
                setInitialText(element, scope);
            });



            scope.isModifiableEnabled = function (fieldMetadata) {
                const result = expressionService.evaluate(fieldMetadata.enableExpression, scope.datamap, scope);
                return result;
            };

        };

        const directive = {
            link: link,
            restrict: 'E',
            replace: true,
            template:
                '<div class="input-group lazy-search">' +
                '<input type="search" class="hidden-phone form-control typeahead" ng-enabled="isModifiableEnabled(fieldMetadata)" placeholder="Find {{placeholder}}" ' +
                'data-association-key="{{provider}}" data-displayablepath="{{displayablepath}}" data-field="{{provider}}"/>' +
                '<span class="input-group-addon last" ng-click="!isModifiableEnabled(fieldMetadata) || executeMagnetSearch()" ng-if="shouldShowModal()">' +
                '<i class="fa fa-search"></i>' +
                '</span>' +
                '<span class="input-group-addon last " ng-click="expandSearch()" ng-if="!shouldShowModal()">' +
                '<i class="fa fa-caret-down"></i>' +
                '</span>' +
                '<div></div><!--stop addon from moving on hover-->' +
                '</div>',
            scope: {
                schema: '=',
                datamap: '=',
                parentdata: '=',
                fieldMetadata: "=",
                panelid: '@',
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

                //isEnabled: '&',

                //function to handle corresponding jquery event
                keyup: '&',
                //function to handle corresponding jquery event
                keydown: '&',
                onFocus: '&',
                // if the a value is set that is not an option adds it anyway (with the value as label)
                allowCustomValue: '@'
            }
        };
        return directive;
    }

    angular.module('sw_typeahead', []).directive('angulartypeahead', ['restService', '$timeout', '$log', '$rootScope', 'contextService', 'associationService', 'lookupService', 'crudContextHolderService', 'schemaService', 'datamapSanitizeService', 'compositionService', 'expressionService', 'fieldService', 'spinService', angularTypeahead]);

})(angular, Bloodhound);

