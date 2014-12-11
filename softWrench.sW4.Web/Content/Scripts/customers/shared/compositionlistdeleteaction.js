function CompositionListDeleteActionController($scope) {

    $scope.deleteCompositionItem = function (clonedCompositionData, compositionItem, compositionItemId) {
        if (compositionItemId == null) {
            for (var i = 0; i < clonedCompositionData.length; i++) {
                if (clonedCompositionData[i] == compositionItem) {
                    clonedCompositionData.splice(i, 1);
                    break;
                }
            }
        } else {
            //TODO: Handle the case that the record exists in Maximo and needs to be deleted (For example, by applying a strikethrough on the row and inserting a flag on the itemdata so that we can handle it on the server side upon submission)
        }
    };

}

