softwrench.controller('CrudListController', function($scope,crudContextService) {

        $scope.list = function() {
            return crudContextService.itemlist();
        }

    }
);