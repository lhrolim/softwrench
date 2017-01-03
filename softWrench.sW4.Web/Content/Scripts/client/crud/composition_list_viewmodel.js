
(function (angular) {
    'use strict';

    class compositionListViewModel {



        constructor(crud_inputcommons, fieldService, schemaService, tabsService, eventService,formatService, crudContextHolderService) {
            this.crud_inputcommons = crud_inputcommons;
            this.fieldService = fieldService;
            this.schemaService = schemaService;
            this.tabsService = tabsService;
            this.eventService = eventService;
            this.formatService = formatService;
            this.crudContextHolderService = crudContextHolderService;
        }



        filterForColumn(schema, column) {
            if (!schema || !schema.schemaFilters || !schema.schemaFilters.filters) {
                return null;
            }
            return schema.schemaFilters.filters.find((filter) => {
                return filter.attribute === column.attribute;
            });
        }

        buildExpandAllParams(relationship,parentSchema, parentdata, compositionlistschema, compositiondetailschema) {
            const params = {
                key: {},
                options: {},
                application: parentSchema.applicationName
            };
            const key = {};
            params.detailRequest.key = key;
            params.detailRequest.id = this.fieldService.getId(parentdata, parentSchema);
            key.schemaId = parentSchema.schemaId;
            key.mode = parentSchema.mode;
            key.platform = platform();
            const compositionsToExpand = {};
            compositionsToExpand[relationship] = { schema: compositionlistschema, value: true };
            //                var compositionsToExpand = { 'worklog_': true };

            params.options.compositionsToExpand = tabsService.buildCompositionsToExpand(compositionsToExpand, parentSchema,
                parentdata, compositiondetailschema.schemaId);
            params.options.printMode = true;
            return params;
        }


        isSingleSelection (compositionlistSchema) {
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
        handleSingleSelectionClick (compositionlistSchema,items, originalcompositiondata,item, rowIndex) {
            if (!this.isSingleSelection(compositionlistSchema)) {
                return;
            }
            const previousValue = item[CompositionConstants.Selected];

            for (let i = 0; i < items.length; i++) {
                items[i][CompositionConstants.Selected] = "false";
                originalcompositiondata[i][CompositionConstants.Selected] = "false";
            }
            if (previousValue == undefined || "false" == previousValue) {
                item[CompositionConstants.Selected] = "true";
                //updating the original item, to make it possible to send custom action selection to server-side
                originalcompositiondata[rowIndex][CompositionConstants.Selected] = "true";
            }

        }

        /**
         *  Method that gets called whenever a composition entry gets their detail data shown, for read-only purposes. This is not triggered upon a modal edition
         *
         */
        doToggle ({clonedData, detailData,relationship,parentdata,compositionlistschema}, item, originalListItem, forcedState, informedId) {

            const parentschema = this.crudContextHolderService.currentSchema();

            const id = informedId ? informedId : item[compositionlistschema.idFieldName];

            if (clonedData[id] == undefined) {
                clonedData[id] = {
                    data : this.formatService.doContentStringConversion(jQuery.extend(true, {}, item))
                };
            }

            if (detailData[id] == undefined) {
                detailData[id] = {
                    expanded : false,
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

    compositionListViewModel.$inject = ['crud_inputcommons', 'fieldService', 'schemaService', 'tabsService', 'eventService', 'formatService', 'crudContextHolderService'];

    angular.module('sw_layout').service('compositionListViewModel', compositionListViewModel);



})(angular);
