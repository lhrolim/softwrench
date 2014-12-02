function ReceiptActionsController($scope, alertService) {
    $scope.receipts = function(compositionItem) {
        $scope.$emit("sw.composition.edit", compositionItem);
    };
}