var app = angular.module('sw_layout');

app.factory('cmpfacade', function ($timeout, $log, cmpComboDropdown, cmpAutocompleteClient, cmpAutocompleteServer, screenshotService, fieldService) {

    return {

        unblock: function (displayable, scope) {
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
        },

        block: function (displayable, scope) {
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
        },


        digestAndrefresh: function (displayable, scope) {
            var rendererType = displayable.rendererType;
            if (rendererType != 'autocompleteclient' && rendererType != 'autocompleteserver' && rendererType != 'combodropdown') {
                return;
            }
            try {
                scope.$digest();
                this.refresh(displayable, scope, true);
            } catch (e) {
                //nothing to do, just checking if digest was already in place or not, because we need angular to update screen first of all
                //if inside a digest already, exception would be thrown --> force a timeout with false flag
                var fn = this;
                $timeout(
                    function () {
                        fn.refresh(displayable, scope, true);
                    }, 0, false);
            }
        },

        refresh: function (displayable, scope, fromDigestAndRefresh) {
            var attribute = displayable.attribute;

            var log = $log.getInstance('cmpfacade#refresh');
            var rendererType = displayable.rendererType;
            if (fromDigestAndRefresh) {
                log.debug('calling digest and refresh for field {0}, component {1}'.format(displayable.attribute, rendererType));
            } else {
                log.debug('calling refresh for field {0}, component {1}'.format(displayable.attribute, rendererType));
            }

            if (rendererType == 'autocompleteclient') {
                cmpAutocompleteClient.refreshFromAttribute(attribute);
            } else if (rendererType == 'autocompleteserver') {
                cmpAutocompleteServer.refreshFromAttribute(displayable, scope);
            } else if (rendererType == 'combodropdown') {
                cmpComboDropdown.refreshFromAttribute(attribute);
            }
        },

        init: function (bodyElement, scope) {
            var datamap = scope.datamap;
            var schema = scope.schema;
            cmpComboDropdown.init(bodyElement);
            cmpAutocompleteClient.init(bodyElement, scope);
            cmpAutocompleteServer.init(bodyElement, datamap, schema, scope);
            screenshotService.init(bodyElement, datamap);
        },

        blockOrUnblockAssociations: function (scope, newValue, oldValue, association) {
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
            } else if (oldValue == false && newValue == true) {
                displayables = fieldService.getDisplayablesByAssociationKey(scope.schema, association.associationKey);
                fn = this;
                $.each(displayables, function (idx, value) {
                    fn.block(value, scope);
                });
            }
        }


    }

});


