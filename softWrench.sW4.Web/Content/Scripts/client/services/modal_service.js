var app = angular.module('sw_layout');

app.factory('modalService', function ($rootScope, $timeout, i18NService) {

    return {

        hide: function () {
            if ($rootScope.showingModal) {
                $rootScope.$broadcast('sw.modal.hide');
            }
        },

        show: function (schemaorModalData, datamap,savefn, previousdata, previousschema) {
            /// <summary>
            /// method to be called for showing the modal a screen, 
            /// could receive as first parameter either the already filled modaldata, or a schema.
            /// </summary>
            /// <param name="schemaorModalData">the already filled modaldata, or a schema</param>
            /// <param name="datamap"></param>
            if (schemaorModalData.schema) {
                $rootScope.$broadcast("sw.modal.show", schemaorModalData);
                return;
            }

            if (schemaorModalData.mode.equalIc("none")) {
                schemaorModalData.mode = "input";
            }

            var modaldata = {
                schema: schemaorModalData,
                datamap: datamap,
                savefn: savefn,
                previousdata: previousdata,
                previousschema: previousschema
            };

            $rootScope.$broadcast("sw.modal.show", modaldata);
        },


    };

});


