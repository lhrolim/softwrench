softwrench.controller('CrudListController', function($scope,crudContextService) {

        $scope.title = function() {
            return crudContextService.currentTitle();
        }

        $scope.list = function() {
            return crudContextService.itemlist();
        }

    }
);