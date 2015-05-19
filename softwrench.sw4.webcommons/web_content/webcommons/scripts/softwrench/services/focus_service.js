var app = angular.module('sw_layout');

app.factory('focusService', function ($rootScope, fieldService, schemaService, cmpfacade, $log) {

    var currentFocusedIdx = 0;


    return {



        setFocusToFirstNonFilled: function (schema, datamap) {
            var log = $log.get("focusService#setFocusToFirstNonFilled");
            var idx = schemaService.getFirstVisibleEditableNonFilledFieldIdx(schema, datamap);
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
        moveFocus: function (datamap,schema,attribute,params) {

            var log = $log.get("focusService#moveFocus");
            log.debug("moving focus to item next to {0}".format(attribute));

            var nextFieldIdx = fieldService.getNextVisibleDisplayableIdx(datamap, schema, attribute);
            if (nextFieldIdx == -1) {
                return;
            }

            this.setFocusOnIdx(schema, datamap, nextFieldIdx,params);

        }

    };

});


