(function (mobileServices) {
    "use strict";

    const compositionLoadedEvent = "sw:inline:compositions:resolved";

    class inlineCompositionService {
        constructor($rootScope, $q, offlineCompositionService, crudContextHolderService) {
            this.$rootScope = $rootScope;
            this.$q = $q;
            this.offlineCompositionService = offlineCompositionService;
            this.crudContextHolderService = crudContextHolderService;

            this.arrayToObj = function(array) {
                const obj = {};
                angular.forEach(array, (comp, idx) => {
                    obj[idx] = comp;
                });
                return obj;
            }

            this.objToArray = function (obj) {
                const array = [];
                angular.forEach(obj, (comp) => {
                    array.push(comp);
                });
                return array;
            }
        }

        loadInlineCompositions(item, datamap, allDisplayables, tabId = "main") {
            const promises = [];
            angular.forEach(allDisplayables, (displayable) => {
                if (displayable.type !== "ApplicationCompositionDefinition" || !displayable.inline || displayable.relationship in datamap) {
                    return;
                }

                const promise = this.offlineCompositionService.loadCompositionList(item, displayable);
                promises.push(promise);

                datamap[displayable.relationship] = [];
                promise.then((compositionList) => {
                    // array to obj to keep angular copy and catch changes to know if it's dirty
                    datamap[displayable.relationship] = this.arrayToObj(compositionList);
                });
            });
            return this.$q.all(promises).then(() => {
                const context = this.crudContextHolderService.getCrudContext();
                context.originalDetailItemDatamap = angular.copy(datamap);
                angular.forEach(allDisplayables, (displayable) => {
                    if (displayable.type === "ApplicationCompositionDefinition" && displayable.inline && datamap[displayable.relationship] && typeof datamap[displayable.relationship] === "object") {
                        datamap[displayable.relationship] = this.objToArray(datamap[displayable.relationship]);
                        context.originalDetailItemDatamap[displayable.relationship] = this.objToArray(context.originalDetailItemDatamap[displayable.relationship]);
                    }
                });
                return this.$rootScope.$broadcast(compositionLoadedEvent, tabId);
            });
        }

        cancelChanges() {
            this.$rootScope.$broadcast(compositionLoadedEvent);
        }

        compositionLoadedEventName() {
            return compositionLoadedEvent;
        }
    }

    inlineCompositionService["$inject"] = ["$rootScope", "$q", "offlineCompositionService", "crudContextHolderService"];

    mobileServices.service("inlineCompositionService", inlineCompositionService);

})(mobileServices)
