
(function (angular) {
    'use strict';



    function cmpfacade($timeout, $log, cmpComboDropdown, cmpCombo, cmplookup, cmpAutocompleteClient, cmpAutocompleteServer, screenshotService, fieldService, crudContextHolderService, compositionService) {


        function unblock(displayable, scope) {
            var log = $log.getInstance('cmpfacade#unblock');
            var rendererType = displayable.rendererType;
            var attribute = displayable.attribute;
            log.debug('unblocking association {0} of type {1}'.format(attribute, rendererType));
            if (rendererType == 'autocompleteclient') {
                cmpAutocompleteClient.unblock(displayable);
            }
            else if (rendererType == 'combodropdown') {
                cmpComboDropdown.unblock(attribute);
            }
            //            else if (rendererType == 'autocompleteserver') {
            //                cmpAutocompleteServer.unblock(displayable, scope);

            this.digestAndrefresh(displayable, scope);
        };

        function block(displayable, scope) {
            var log = $log.getInstance('cmpfacade#block');
            var rendererType = displayable.rendererType;
            var attribute = displayable.attribute;
            log.debug('block association {0} of type {1}'.format(attribute, rendererType));
            if (rendererType == 'autocompleteclient') {
                cmpAutocompleteClient.block(displayable);
            }
                //            else if (rendererType == 'autocompleteserver') {
                //                cmpAutocompleteServer.unblock(displayable, scope);
            else if (rendererType == 'combodropdown') {
                cmpComboDropdown.block(displayable.associationKey);
            }
            this.digestAndrefresh(displayable, scope);
        };

        function updateEagerOptions(scope, displayable, options, contextData, datamapId) {
            var log = $log.getInstance("cmpfacade#updateEagerOptions",["association"]);
            var attribute = displayable.attribute;
            var rendererType = displayable.rendererType;
            if (!contextData) {
                contextData = scope.ismodal === "true" ? { schemaId: "#modal" } : null;
            }
            if (!options) {
                options = crudContextHolderService.fetchEagerAssociationOptions(displayable.associationKey, contextData);
            }

            var fn = function doRefresh() {
                log.debug("updating list for component {0}".format(attribute));
                if (rendererType === 'autocompleteclient') {
                    var value = scope.datamap[displayable.target];
                    cmpAutocompleteClient.refreshFromAttribute(scope, displayable, value, options, datamapId);
                } else if (rendererType === 'combodropdown') {
                    cmpComboDropdown.refreshList(attribute);
                } else if (rendererType === 'combo') {
                    var value = scope.datamap[displayable.target];
                    cmpCombo.refreshFromAttribute(value, options);
                }
            }

            try {
                scope.$digest();
                fn();
            } catch (e) {
                $timeout(
                    function () {
                        fn();
                        try {
                            scope.$digest();
                        } catch (e) {
                            $log.getInstance('componentfacade#digestandrefresh').warn('validating this is actually being thrown. if u see this, remove this log' + e);
                            //nothing
                        }
                    }, 0, false);
            }

        }

        function digestAndrefresh(displayable, scope, newValue, datamapId) {
            var rendererType = displayable.rendererType;
            if (rendererType !== 'autocompleteclient' && rendererType !== 'autocompleteserver' && rendererType !== 'combodropdown' && rendererType !== 'lookup' && rendererType !== 'modal') {
                return;
            }
            try {
                scope.$digest();
                this.refresh(displayable, scope, true, newValue, datamapId);
            } catch (e) {
                //nothing to do, just checking if digest was already in place or not, because we need angular to update screen first of all
                //if inside a digest already, exception would be thrown --> force a timeout with false flag
                var fn = this;
                $timeout(
                    function () {
                        fn.refresh(displayable, scope, true, newValue, datamapId);
                        try {
                            scope.$digest();
                        } catch (e) {
                            $log.getInstance('componentfacade#digestandrefresh').warn('validating this is actually being thrown. if u see this, remove this log' + e);
                            //nothing
                        }
                    }, 0, false);
            }
        };

        function focus(displayable) {
            var log = $log.getInstance('cmpfacade#focus');

            var attribute = displayable.attribute;
            var associationKey = displayable.associationKey;
            var rendererType = displayable.rendererType;
            //TODO: test will all render types
            //works with: default, autocompleteclient
            //not working with: richtext, lookup (works with direct call)
            if (rendererType == 'autocompleteclient') {
                cmpAutocompleteClient.focus(displayable);
            } else if (rendererType == "combo" || rendererType == "lookup") {
                var selector = '[data-field="' + associationKey + '"]';
                $(selector).focus();
            }
            else {
                //default case
                selector = '[data-field="' + attribute + '"]';
                $(selector).focus();
            }

            log.debug('change focus to {0}'.format(attribute));
        };

        function refresh(displayable, scope, fromDigestAndRefresh, newValue, datamapId) {
            var attribute = displayable.attribute;

            var log = $log.getInstance('cmpfacade#refresh',["association"]);
            var rendererType = displayable.rendererType;
            var msg = fromDigestAndRefresh ? 'calling digest and refresh for field {0}, component {1} | value {2}' : 'calling refresh for field {0}, component {1} | value {2}';
            var valueToLog = newValue ? newValue : scope.datamap[displayable.target];

            log.debug(msg.format(displayable.attribute, rendererType, valueToLog));

            if (rendererType === 'autocompleteclient') {
                var contextData = null;
                // special case of a composition list
                if (compositionService.isCompositionListItem(scope.datamap)) {
                    contextData = compositionService.buildCompositionListItemContext(contextData, scope.datamap, scope.schema);
                }
                var options = crudContextHolderService.fetchEagerAssociationOptions(displayable.associationKey, contextData);
                cmpAutocompleteClient.refreshFromAttribute(scope, displayable, valueToLog, options, datamapId);
            } else if (rendererType === 'autocompleteserver') {
                cmpAutocompleteServer.refreshFromAttribute(displayable, scope);
            } else if (rendererType === 'combodropdown') {
                cmpComboDropdown.refreshFromAttribute(attribute);
            } else if (rendererType === 'lookup' || rendererType === 'modal') {
                cmplookup.refreshFromAttribute(displayable, scope.datamap, datamapId, newValue);
            }
        };

        function init(bodyElement, scope) {
            var datamap = scope.datamap;
            var schema = scope.schema;
            cmpComboDropdown.init(bodyElement,schema);
            cmpAutocompleteClient.init(bodyElement, datamap, schema, scope);
            //deprecating autocompleteservers
//            cmpAutocompleteServer.init(bodyElement, datamap, schema, scope);
            screenshotService.init(bodyElement, datamap);
        };

        function blockOrUnblockAssociations(scope, newValue, oldValue, association) {
            if (oldValue == newValue) {
                return;
            }
            var displayables;
            var fn;
            if (oldValue == true && newValue == false) {
                //this means that this association has been unblocked
                displayables = fieldService.getDisplayablesByAssociationKey(scope.schema, association.associationKey);
                fn = this;
                $.each(displayables, function (idx, value) {
                    fn.unblock(value, scope);
                });
            } else if ((oldValue == false || oldValue == undefined) && newValue == true) {
                displayables = fieldService.getDisplayablesByAssociationKey(scope.schema, association.associationKey);
                fn = this;
                $.each(displayables, function (idx, value) {
                    fn.block(value, scope);
                });
            }
        }

        return {
            unblock: unblock,
            block: block,
            digestAndrefresh: digestAndrefresh,
            updateEagerOptions: updateEagerOptions,
            focus: focus,
            refresh: refresh,
            init: init,
            blockOrUnblockAssociations: blockOrUnblockAssociations
        };;
    }

    angular
      .module('sw_layout')
      .factory('cmpfacade', ['$timeout', '$log', 'cmpComboDropdown', 'cmpCombo', 'cmplookup', 'cmpAutocompleteClient', 'cmpAutocompleteServer', 'screenshotService', 'fieldService', 'crudContextHolderService', 'compositionService', cmpfacade]);

})(angular);




