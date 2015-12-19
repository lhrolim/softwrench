app.directive('compositionMasterDetails', function (contextService, formatService, schemaService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/composition_masterdetails.html'),
        scope: {
            //compositionschemadefinition: '=',
            //compositiondata: '=',
            //parentdata: '=',
            //relationship: '@',
            //title: '@',
            //cancelfn: '&',
            //previousschema: '=',
            //previousdata: '=',
            //parentschema: '=',
            ////the composition declaration tag, of the parent schema
            //metadatadeclaration: '=',
            //mode: '@',
            //ismodal: '@'
        },

        controller: function ($scope) {

            /*Directvie Methods*/
            //this.function = function () {

            //    return something;
            //}



            //init();
        }
    };
});
