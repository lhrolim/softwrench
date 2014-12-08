function CompositionListDeleteActionController($scope) {

    $scope.deleteCompositionItem = function (clonedCompositionData, compositionitem) {
        for (var i = 0; i < clonedCompositionData.length; i++) {
            if (clonedCompositionData[i] == compositionitem) {
                clonedCompositionData.splice(i, 1);
                break;
            }
        }
    };

}

