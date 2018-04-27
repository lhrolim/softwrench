﻿(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .service('modalService', ["$rootScope", "$q", "crudContextHolderService", "$timeout", "DeferredWithUpdate", function ($rootScope, $q, crudContextHolderService, $timeout, DeferredWithUpdate) {

            return {

                hide: function (ignoreConfirmClose) {
                    //adding this call to solve a mistereous bug on minified environments where the modal-backdrop element would remain
                    $('.modal-backdrop').remove();
                    if (crudContextHolderService.isShowingModal()) {
                        $rootScope.$broadcast(JavascriptEventConstants.HideModal, false, ignoreConfirmClose);
                    }
                },

                showWithModalData: function (modalData) {
                    $rootScope.$broadcast(JavascriptEventConstants.ModalShown, modalData);
                },

                /// <summary>
                /// method to be called for showing the modal a screen, 
                /// could receive as first parameter either the already filled modaldata, or a schema.
                /// </summary>
                /// <param name="schemaorModalData">the already filled modaldata, or a schema</param>
                /// <param name="datamap">the datamap of the current item being displayed on the modal, or an IApplicationResponse, where datamap would be the ResultObject; it could be null for a "creation" workflow.</param>
                /// <param name="properties">an object with the following:
                /// 
                ///     title: the title to display on the modal
                ///     cssclass: an extra class to add to the modal, making it possible to customize it via css later
                ///     removecrudmodalclass: a boolean indicating whether the crud-lookup-modal class should be removed
                ///     onloadfn: a function to be called when the modal loads, which would receive the modal scope as a parameter (function onload(modalscope))
                ///     useavailableheight: if true will force the scroll to pre-dimension with all available data
                ///     resizableElements: list of jquery selector elements to be resized with the main modal
                ///     savefn: a save fn to be called
                ///     resizable: boolean indicating whther the modal should be resizable. defaults to false
                ///     listResult: 
                /// 
                /// </param>

                /// <param name="savefn">the savefn to execute upon modal submit click. It should have the following signature:
                ///     save(modaldatamap) where:
                ///         modaldatamap is the datamap of the modal
                ///    deprecated, pass it as a properties instead
                /// </param>
                /// <param name="cancelfn">the cancel  function to execute upon modal cancel click. It should have the following signature:
                ///     cancel()  
                /// </param>
                /// <param name="parentdata">holds the parent datamap</param>
                /// <param name="parentschema">holds the parent schema</param>
                show: function (schemaorModalData, datamap = {}, properties = {}, savefn, cancelfn, parentdata, parentschema) {
                    if (schemaorModalData.schema) {
                        //this happens if the directive was compiled after the event was thrown, i.e, the first time the modal is being included on the screen
                        //later calls won´t come here
                        $rootScope.$broadcast(JavascriptEventConstants.ModalShown, schemaorModalData);
                        return;
                    }

                    var appResponseData = null;
                    savefn = savefn || properties.savefn;

                    if (!properties.cssclass) {
                        properties.cssclass = "crud-lookup-modal";
                    } else {
                        if (!properties.removecrudmodalclass) {
                            properties.cssclass += " crud-lookup-modal";
                        }

                    }

                    if (!properties.title) {
                        properties.title = schemaorModalData.title;
                    }


                    if (schemaorModalData.mode.equalIc("none")) {
                        schemaorModalData.mode = "input";
                    }

                    if (datamap.resultObject) {
                        appResponseData = datamap;
                        datamap = datamap.resultObject;
                    }
                    const modaldata = {
                        schema: schemaorModalData,
                        datamap,
                        appResponseData,
                        savefn,
                        cancelfn,
                        previousdata: parentdata,
                        previousschema: parentschema,
                        title: properties.title,
                        cssclass: properties.cssclass,
                        onloadfn: properties.onloadfn,
                        closeAfterSave: properties.closeAfterSave,
                        cancelOnClickOutside: properties.cancelOnClickOutside,
                        useavailableheight: properties.useavailableheight,
                        resizable: properties.resizable,
                        resizableElements: properties.resizableElements,
                        searchData: properties.searchData,
                        searchOperator: properties.searchOperator,
                        searchSort: properties.searchSort
                    };


                    $rootScope.$broadcast(JavascriptEventConstants.ModalShown, modaldata);

                },


                showPromise: function (schemaorModalData, datamap = {}, properties = {}, parentdata, parentschema) {
                    var deferred = DeferredWithUpdate.defer();

                    const savefn = (datamap, schema) => {
                        deferred.resolve(datamap);
                        //timeout to enforce the promise resolution to take place before the hide method execution
                        if (!schema.stereotype.equalsAny("CompositionDetail") && crudContextHolderService.isShowingModal() && properties.autoClose !== false) {
                            //not ideal, however for compositions we know for sure that the modal will auto close afterwards, so there´s no reason to close it here
                            // there´s a bug that the modal is closing in case of server side failure
                            //TODO: fix it
                            $timeout(() => { this.hide(); }, 0, false);    
                        }
                        
                    }

                    const cancelfn = (datamap) => {
                        deferred.reject(datamap);
                    }

                    this.show(schemaorModalData, datamap, properties, savefn, cancelfn, parentdata, parentschema);
                    //registering modal promise as well

                    return deferred.promise;
                },

                getSaveFn: function () {
                    if (crudContextHolderService.isShowingModal()) {
                        return crudContextHolderService.getSaveFn();
                    }
                },

                isShowingModal: function () {
                    return crudContextHolderService.isShowingModal();
                },

                //do not remove this constant
                panelid: "#modal"


            };

        }]);

})(angular);