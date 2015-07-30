(function (angular, persistence) {
    /**
     * entities: exposed as provider propertiy so it can be configured by the modules that use them.
     * Usage in config:
     * angular.module("myApp", ["persistence.offline"]).config(["offlineEntitiesProvider", function(entitiesProvider){
     *     entitiesProvider.entities.MyTable = persistence.define("MyTable", { ... }); 
     * }]);
     * Usage in run, service, controller, etc:
     * app.controller("MyCtrl", ["$scope", "offlineEntities", function($scope, entities){
     *      $scope.newMyTable = function(){
     *          var record = new entities.MyTable();
     *          // ...
     *      };
     * }])
     */
    angular.module("persistence.offline").provider("offlineEntities", [function () {
        // service instance
        var entities = {};

        var provider = {
            // access to the service instance in config time
            entities: entities,
            // alias to persistence.define
            define: function (name, options) {
                entities[name] = persistence.define(name, options);
            },
            // service constructor
            $get: [function () {
                return entities;
            }]
        };
        return provider;
    }]);

})(angular, persistence);