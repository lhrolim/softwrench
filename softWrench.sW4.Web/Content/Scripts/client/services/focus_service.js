﻿(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .service('focusService', ["$rootScope", "fieldService", "schemaService", "cmpfacade", "$log", "crudContextHolderService", function ($rootScope, fieldService, schemaService, cmpfacade, $log, crudContextHolderService) {

            var currentFocusedIdx = 0;
            var currentModalFocusIdx = 0;

            $rootScope.$on(JavascriptEventConstants.HideModal, () => {
                currentModalFocusIdx = 0;
            });

            $rootScope.$on(JavascriptEventConstants.DetailLoaded, () => {
                currentModalFocusIdx = 0;
                currentFocusedIdx = 0;
            });

            function getCurrentFocusIdx() {
                if (crudContextHolderService.isShowingModal()) {
                    return currentModalFocusIdx;
                }
                return currentFocusedIdx;
            };

            function setCurrentFocusIdx(value) {
                if(crudContextHolderService.isShowingModal()) {
                    currentModalFocusIdx = value;
                } else {
                    currentFocusedIdx = value;
                }
            };


            return {

            



                getFirstFocusableFieldIdx: function (schema, datamap, acceptFilled) {
                    const realdatamap = datamap;
                    const displayables = fieldService.getLinearDisplayables(schema);
                    for (let i = 0; i < displayables.length; i++) {
                        const displayable = displayables[i];
                        if (!displayable.attribute) {
                            continue;
                        }
                        if (fieldService.isFieldHidden(realdatamap, schema, displayable) || fieldService.isFieldReadOnly(realdatamap, schema, displayable)) {
                            continue;
                        }
                        if (displayable.rendererParameters && "true" === displayable.rendererParameters['avoidautofocus']) {
                            continue;
                        }
                        if (!acceptFilled && (realdatamap[displayable.attribute] == null || realdatamap[displayable.attribute] === "" || realdatamap[displayable.attribute].length === 0)) {
                            return i;
                        } else if (acceptFilled) {
                            return i;
                        }
                    }
                    return -1;
                },

                setFocusToFirstField: function (schema, datamap) {
                    const log = $log.get("focusService#setFocusToFirstNonFilled");
                    const isEditing = schemaService.getId(datamap, schema);
                    const idx = this.getFirstFocusableFieldIdx(schema, datamap, isEditing);
                    if (idx != -1) {
                        this.setFocusOnIdx(schema, idx, { reset: true });
                    }
                    log.debug("reseting focus to first field");
                },

                resetFocusToCurrent: function (schema, attribute) {
                    if (!schema) {
                        return;
                    }
                    const log = $log.get("focusService#resetFocusToCurrent", ["detail", "focus"]);
                    log.debug("resetting focus to {0}".format(attribute));
                    const idx = fieldService.getVisibleDisplayableIdxByKey(schema, attribute);
                    setCurrentFocusIdx(idx);
                    this.setFocusOnIdx(schema, idx);
                },

                setFocusOnIdx: function (schema, idx, parameters = {}) {
                    if (idx === -1) {
                        return false;
                    }

                    const log = $log.get("focusService#setFocusOnIdx", ["detail", "focus"]);
                    if (schema.properties && schema.properties["detail.focus.allowmovingbackward"] === "true") {
                        parameters.allowmovingbackward = true;
                    }

                    if (parameters.reset) {
                        setCurrentFocusIdx(0);
                    }
                    const movingForward = idx >= getCurrentFocusIdx();
                    if (movingForward || parameters.allowmovingbackward) {
                        const nextField = fieldService.getLinearDisplayables(schema)[idx];
                        log.debug('call cmpfacade.focus', nextField);
                        setCurrentFocusIdx(idx);
                        return cmpfacade.focus(nextField);
                    }
                },


                /// <summary>
                /// move input focus to the next field
                ///  TODO: not working with lookups, e.g. updating SR Asset resultes in two updateAssociations, the first for asset
                /// the second for location, cause the asset to remain in focus
                /// </summary>
                /// <param name="triggerFieldName"></param>
                moveFocus: function (datamap, schema, attribute, params) {

                    if (schemaService.isPropertyTrue(schema, 'crud.disableautofocus')) {
                        return false;
                    }
                    const log = $log.get("focusService#moveFocus");
                    log.debug("moving focus to item next to {0}".format(attribute));
                    const nextFieldIdx = fieldService.getNextVisibleDisplayableIdx(datamap, schema, attribute);
                    if (nextFieldIdx === -1) {
                        return false;
                    }

                    return this.setFocusOnIdx(schema, nextFieldIdx, params);
                }
            };

        }]);

})(angular);