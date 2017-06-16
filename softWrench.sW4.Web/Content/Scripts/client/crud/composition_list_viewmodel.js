
(function (angular) {
    'use strict';

    class compositionListViewModel {



        constructor($q, $http, crud_inputcommons, fieldService, schemaService, tabsService, eventService, formatService, crudContextHolderService) {
            this.crud_inputcommons = crud_inputcommons;
            this.fieldService = fieldService;
            this.schemaService = schemaService;
            this.tabsService = tabsService;
            this.eventService = eventService;
            this.formatService = formatService;
            this.crudContextHolderService = crudContextHolderService;
            this.$q = $q;
            this.$http = $http;
        }



        filterForColumn(schema, column) {
            if (!schema || !schema.schemaFilters || !schema.schemaFilters.filters) {
                return null;
            }
            return schema.schemaFilters.filters.find((filter) => {
                return filter.attribute === column.attribute;
            });
        }

        buildExpandAllParams(relationship, parentdata, compositionlistschema, compositiondetailschema) {

            const parentSchema = this.crudContextHolderService.currentSchema();

            const key = {
                schemaId: parentSchema.schemaId,
                mode: parentSchema.mode,
                platform: platform()
            };

            const params = {
                options: { printMode: true },
                application: parentSchema.applicationName,
                detailRequest: {
                    key,
                    id: this.fieldService.getId(parentdata, parentSchema)
                }
            };

            const compositionsToExpand = {
                [relationship]: { schema: compositionlistschema, value: true }
            };
            //                var compositionsToExpand = { 'worklog_': true };

            params.options.compositionsToExpand = this.tabsService.buildCompositionsToExpand(compositionsToExpand, parentSchema,
                parentdata, compositiondetailschema.schemaId);
            return params;
        }

        expandAll({clonedData, wasExpandedBefore, detailData, compositiondata, compositiondetailschema, relationship, parentdata, compositionlistschema}) {
            if (wasExpandedBefore) {
                Object.keys(detailData).forEach(key => {
                    detailData[key].expanded = true;
                });
                return this.$q.reject();
            }

            var compositionListData = [];
            for (let i = 0; i < compositiondata.length; i++) {
                const data = compositiondata[i];
                const id = data[compositiondetailschema.idFieldName];
                compositionListData[id] = data;
            }
            const parameters = this.buildExpandAllParams(relationship, parentdata, compositionlistschema, compositiondetailschema);

            const urlToInvoke = removeEncoding(url("/api/generic/Composition/ExpandCompositions?" + $.param(parameters)));
            return this.$http.get(urlToInvoke).then(response => {
                const result = response.data;
                result.resultObject[relationship].forEach(value => {
                    const itemId = value[compositiondetailschema.idFieldName];
                    this.doToggle({ clonedData, detailData, relationship, parentdata, compositionlistschema }, value, compositionListData[itemId], true, itemId);
                });

            });
        }


        isSingleSelection(compositionlistSchema) {
            return "single" === this.schemaService.getProperty(compositionlistSchema, "list.selectionstyle");
        }

        isBatch(compositionschemadefinition) {
            return "batch" === compositionschemadefinition.rendererParameters["mode"];
        }

        /**
          * Toggles the current selected item value, and sets all the others to false
          * @param {} item 
          * @param {} rowIndex 
          * @returns {} 
          */
        handleSingleSelectionClick(compositionlistSchema, items, originalcompositiondata, item, rowIndex) {
            if (!this.isSingleSelection(compositionlistSchema)) {
                return;
            }
            const previousValue = item[CompositionConstants.Selected];

            //if all items besides the current selected one are already false, it means we are unselecting the current checkbox
            var isUnselecting = previousValue !== "false" && items.filter(a => a[CompositionConstants.Selected] === "false").length === items.length - 1;

            for (let i = 0; i < items.length; i++) {

                items[i][CompositionConstants.Selected] = "false";
                originalcompositiondata()[i][CompositionConstants.Selected] = "false";
            }

            item[CompositionConstants.Selected] = "true";
            //updating the original item, to make it possible to send custom action selection to server-side
            originalcompositiondata()[rowIndex][CompositionConstants.Selected] = "true";


            if (isUnselecting) {
                item[CompositionConstants.Selected] = "false";
                //updating the original item, to make it possible to send custom action selection to server-side
                originalcompositiondata()[rowIndex][CompositionConstants.Selected] = "false";

                items.forEach(i => i[CompositionConstants.Selected] = undefined);
            }

        }

        /**
         *  Method that gets called whenever a composition entry gets their detail data shown, for read-only purposes. This is not triggered upon a modal edition
         *
         */
        doToggle({clonedData, detailData, relationship, parentdata, compositionlistschema}, item, originalListItem, forcedState, informedId) {

            const parentschema = this.crudContextHolderService.currentSchema();

            const id = informedId ? informedId : item[compositionlistschema.idFieldName];

            if (clonedData[id] == undefined) {
                clonedData[id] = {
                    data: this.formatService.doContentStringConversion(jQuery.extend(true, {}, item))
                };
            }

            if (detailData[id] == undefined) {
                detailData[id] = {
                    expanded: false,
                    data: this.formatService.doContentStringConversion(item),
                };
                detailData[id].data[CompositionConstants.IsCreation] = id == null || id < 0;
            }
            const newState = forcedState != undefined ? forcedState : !detailData[id].expanded;
            detailData[id].expanded = newState;

            if (newState) {
                const parameters = {
                    compositionItemId: id,
                    compositionItemData: originalListItem,
                    parentData: parentdata,
                    parentSchema: parentschema
                };
                const compositionSchema = parentschema.cachedCompositions[relationship];
                this.eventService.onviewdetail(compositionSchema, parameters);
            }
        }




    }

    compositionListViewModel.$inject = ["$q", "$http", 'crud_inputcommons', 'fieldService', 'schemaService', 'tabsService', 'eventService', 'formatService', 'crudContextHolderService'];

    angular.module('sw_layout').service('compositionListViewModel', compositionListViewModel);



})(angular);
