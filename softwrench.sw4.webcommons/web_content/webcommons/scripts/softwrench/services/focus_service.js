﻿var app = angular.module('sw_layout');

app.factory('focusService', function ($rootScope, fieldService, schemaService, cmpfacade, $log) {

    var currentFocusedIdx = 0;


    return {

        getFirstFocusableFieldIdx: function (schema, datamap, acceptFilled) {
            var realdatamap = datamap.fields ? datamap.fields : datamap;

            var displayables = fieldService.getLinearDisplayables(schema);
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                if (displayable.attribute) {
                    if (!fieldService.isFieldHidden(realdatamap, schema, displayable) && !fieldService.isFieldReadOnly(realdatamap, schema, displayable)) {
                        if ("true" == displayable.rendererParameters['avoidautofocus']) {
                            continue;
                        }

                        if (!acceptFilled && realdatamap[displayable.attribute] == null) {
                            return i;
                        } else if (acceptFilled) {
                            return i;
                        }
                    }
                }
            }
            return -1;
        },


        setFocusToFirstField: function (schema, datamap) {
            var log = $log.get("focusService#setFocusToFirstNonFilled");
            var isEditing = schemaService.getId(datamap, schema);
            var idx = this.getFirstFocusableFieldIdx(schema, datamap, isEditing);

            if (idx != -1) {
                this.setFocusOnIdx(schema, datamap, idx, { reset: true });
            }
            log.debug("reseting focus to first field");
        },

        resetFocusToCurrent: function (schema, attribute) {
            var log = $log.get("focusService#resetFocusToCurrent");
            log.debug("resetting focus to {0}".format(attribute));
            var idx = fieldService.getDisplayableIdxByKey(schema, attribute);
            currentFocusedIdx = idx;
        },

        setFocusOnIdx: function (schema, datamap, idx, parameters) {
            var log = $log.get("focusService#setFocusOnIdx");
            parameters = parameters || {};
            if (parameters.reset) {
                currentFocusedIdx = 0;
            }
            var movingForward = idx >= currentFocusedIdx;

            if (movingForward || parameters.allowmovingbackward) {
                var nextField = fieldService.getLinearDisplayables(schema)[idx];
                log.debug('call cmpfacade.focus', nextField);
                cmpfacade.focus(nextField);
                currentFocusedIdx = idx;
            }
        },


        /// <summary>
        /// move input focus to the next field
        ///  TODO: not working with lookups, e.g. updating SR Asset resultes in two updateAssociations, the first for asset
        /// the second for location, cause the asset to remain in focus
        /// </summary>
        /// <param name="triggerFieldName"></param>
        moveFocus: function (datamap, schema, attribute, params) {

            var log = $log.get("focusService#moveFocus");
            log.debug("moving focus to item next to {0}".format(attribute));

            var nextFieldIdx = fieldService.getNextVisibleDisplayableIdx(datamap, schema, attribute);
            if (nextFieldIdx == -1) {
                return;
            }

            this.setFocusOnIdx(schema, datamap, nextFieldIdx, params);

        }

    };

});


