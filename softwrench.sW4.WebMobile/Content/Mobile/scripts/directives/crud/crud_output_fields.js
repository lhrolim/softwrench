softwrench.directive('sectionElementInput', function ($compile) {
    return {
        restrict: "E",
        replace: true,
        scope: {
            schema: '=',
            datamap: '=',
            isDirty: '=',
            displayables: '=',
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            extraparameters: '=',
            elementid: '@',
            orientation: '@',
            islabelless: '@',
            lookupAssociationsCode: '=',
            lookupAssociationsDescription: '=',

        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.displayables)) {
                element.append(
                "<crud-input-fields displayables='displayables'" +
                "schema='schema'" +
                "datamap='datamap'" +
                "is-dirty='isDirty'" +
                "displayables='displayables'" +
                "association-options='associationOptions'" +
                "association-schemas='associationSchemas'" +
                "blockedassociations='blockedassociations'" +
                "elementid='{{elementid}}'" +
                "orientation='{{orientation}}' insidelabellesssection='{{islabelless}}'" +
                "outerassociationcode='lookupAssociationsCode' outerassociationdescription='lookupAssociationsDescription' issection='true'" +
                "></crud-input-fields>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});
softwrench.directive('crudOutputFields', function ($log,fieldService, crudContextService) {

    return {
        restrict: 'E',
        replace: true,
        templateUrl: '/Content/Mobile/templates/directives/crud/crud_output_fields.html',
        scope: {
            displayables: '=',
            datamap:'=',
        },

        link: function (scope, element, attrs) {
            scope.name = 'crud_output_fields';

        },

        controller: function ($scope) {

            $scope.getDisplayables = function () {
                return $scope.displayables;
            }

            $scope.isFieldHidden = function (fieldMetadata) {
                return fieldService.isFieldHidden(crudContextService.currentDetailItem(), crudContextService.currentDetailSchema(), fieldMetadata);
            }


        }
    }
});