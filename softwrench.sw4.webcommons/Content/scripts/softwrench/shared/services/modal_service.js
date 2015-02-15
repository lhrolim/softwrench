var app = angular.module('sw_layout');

app.factory('modalService', function ($rootScope, $timeout, i18NService) {

    return {

        hide: function () {
            if ($rootScope.showingModal) {
                $rootScope.$broadcast('sw.modal.hide');
            }
        },


        /// <summary>
        /// method to be called for showing the modal a screen, 
        /// could receive as first parameter either the already filled modaldata, or a schema.
        /// </summary>
        /// <param name="schemaorModalData">the already filled modaldata, or a schema</param>
        /// <param name="datamap">the datamap of the current item being displayed on the modal; it could be null for a "creation" workflow.</param>
        /// <param name="savefn">the savefn to execute upon modal submit click. It should have the following signature:
        ///     save(modaldatamap) where:
        ///         modaldatamap is the datamap of the modal
        /// </param>
        /// <param name="cancelfn">the cancel  function to execute upon modal cancel click. It should have the following signature:
        ///     cancel()  
        /// </param>
        /// <param name="parentdata">holds the parent datamap</param>
        /// <param name="parentschema">holds the parent schema</param>
        show: function (schemaorModalData, datamap, savefn,cancelfn, parentdata, parentschema) {
            
            if (schemaorModalData.schema) {
                $rootScope.$broadcast("sw.modal.show", schemaorModalData);
                return;
            }

            if (datamap == null) {
                datamap = {};
            }

            if (schemaorModalData.mode.equalIc("none")) {
                schemaorModalData.mode = "input";
            }

            var modaldata = {
                schema: schemaorModalData,
                datamap: datamap,
                savefn: savefn,
                cancelfn: cancelfn,
                previousdata: parentdata,
                previousschema: parentschema
            };

            $rootScope.$broadcast("sw.modal.show", modaldata);
        },


    };

});


