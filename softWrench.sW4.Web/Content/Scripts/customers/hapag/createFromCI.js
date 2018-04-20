function CreateFromCIController($scope, $http, i18NService, fieldService, redirectService) {

    "ngInject";


    $scope.createFromCi = function () {
        var datamap = $scope.datamap;
        var initialData = {};
        initialData['cinum'] = datamap["cinum"];

        var parameters = {
            //this means that we want to keep the same window (!=browser), but with no menu
            popupmode: 'nomenu'
        };
        redirectService.goToApplicationView('newchange', 'newchange', 'input', null, parameters, initialData);
    };

  


}