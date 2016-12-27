(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .service('modalService', ["$rootScope","$q", "crudContextHolderService", function ($rootScope,$q, crudContextHolderService) {

            return {

                hide: function (modalId) {
                    //adding this call to solve a mistereous bug on minified environments where the modal-backdrop element would remain
                    $('.modal-backdrop').remove();
                    if ($rootScope.showingModal) {
                        $rootScope.$broadcast(JavascriptEventConstants.HideModal);
                    }
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
                ///     onloadfn: a function to be called when the modal loads, which would receive the modal scope as a parameter (function onload(modalscope))
                ///     savefn: a save fn to be called
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
                show: function (schemaorModalData, datamap, properties, savefn, cancelfn, parentdata, parentschema) {
                    if (schemaorModalData.schema) {
                        $rootScope.$broadcast(JavascriptEventConstants.ModalShown, schemaorModalData);
                        return;
                    }

                    properties = properties || {};
                    datamap = datamap || {};
                    var appResponseData = null;
                    savefn = savefn || properties.savefn;

                    if (!properties.cssclass) {
                        properties.cssclass = "crud-lookup-modal";
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
                        datamap: datamap,
                        appResponseData: appResponseData,
                        savefn: savefn,
                        cancelfn: cancelfn,
                        previousdata: parentdata,
                        previousschema: parentschema,
                        title: properties.title,
                        cssclass: properties.cssclass,
                        onloadfn: properties.onloadfn,
                        closeAfterSave: properties.closeAfterSave
                    };
                    crudContextHolderService.registerSaveFn(savefn);

                    $rootScope.$broadcast(JavascriptEventConstants.ModalShown, modaldata);
                },


                showPromise: function (schemaorModalData, datamap, properties, parentdata, parentschema) {
                    var deferred = $q.defer();

                    const savefn = (datamap) => {
                        deferred.resolve(datamap);
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

         


                panelid: "#modal"
            };

        }]);

})(angular);