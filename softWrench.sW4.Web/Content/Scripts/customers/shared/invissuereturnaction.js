function InvIssueReturnActionController($scope, contextService, alertService, modalService, restService, redirectService) {

    $scope.hasBeenReturned = function (matusetransitem) {
        var data = matusetransitem['fields'];
        if ((data['quantity'] + data['qtyrequested']) == 0) {
            return true;
        }

        return false;
    };

    $scope.return = function (matusetransitem) {
        var data = matusetransitem['fields'];
        var returnQty = Math.abs(data['quantity'] + data['qtyrequested']);
        var item = data['itemnum'];
        var storeloc = data['storeloc'];
        var binnum = data['binnum'];
        var message = "Return " + returnQty + " " + item + " to " + storeloc + "?";
        if (binnum != null) {
            message = message + " (Bin: " + binnum + ")";
        }
        alertService.confirm(null, null, function () {
            var jsonString = angular.toJson(data);
            var httpParameters = {
                application: "invissue",
                platform: "web",
                currentSchemaKey: "list.input.web"
            };
            restService.invokePost("data", "post", httpParameters, jsonString, function () {
                var restParameters = {
                    key: {
                        schemaId: "list",
                        mode: "none",
                        platform: "web"
                    },
                    SearchDTO: null
                };
                var urlToUse = url("/api/Data/invissue?" + $.param(restParameters));
                $http.get(urlToUse).success(function (data) {
                    redirectService.goToApplication("invissue", "list", null, data);
                });
            });
            modalService.hide();
        }, message , function () {
            modalService.hide();
        });
    };

    $scope.updateOpacity = function (matusetransitem) {
        if ($scope.hasBeenReturned(matusetransitem)) {
            return "low-opacity gray";
        }

        return "blue";
    };

    $scope.isReturnHidden = function (matusetransitem) {
        var data = matusetransitem['fields'];
        if (data['issuetype'] == 'RETURN') {
            return false;
        }
        return true;
    };

}

