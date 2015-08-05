(function (app) {
    "use strict";

    app.controller("AuditEntryListController",
        ["$scope", "offlineAuditService", "formatService", "$stateParams", "routeService", "$ionicHistory",
            function ($scope, offlineAuditService, formatService, $stateParams, routeService, $ionicHistory) {

                var application = $stateParams.application;

                $scope.data = {
                    entryList: []
                };

                $scope.paginationData = {
                    currentPage: 1,
                    pageSize: 10,
                    hasMoreAvailable: true
                };

                $scope.openDetail = function (entry) {
                    routeService.go(".entrydetail", { id: entry.id });
                };

                $scope.back = function () {
                    $ionicHistory.goBack();
                };

                var formatEntries = function (entries) {
                    return entries.map(function (entry) {
                        entry.formattedDate = formatService.formatDate(entry.createDate, "MM/dd/yyyy HH:mm");
                        return entry;
                    });
                };

                $scope.loadPagedList = function () {
                    var options = {
                        pagenumber: $scope.paginationData.currentPage,
                        pagesize: $scope.paginationData.pageSize
                    };
                    offlineAuditService.listEntries(application, options)
                        .then(function (entries) {
                            // update pagination data
                            $scope.paginationData.hasMoreAvailable = (entries && entries.length >= $scope.paginationData.pageSize);
                            $scope.paginationData.currentPage += 1;
                            // update list
                            $scope.data.entryList = $scope.data.entryList.concat(formatEntries(entries));
                        })
                        .finally(function () {
                            $scope.$broadcast("scroll.infiniteScrollComplete");
                        });
                };

                //TODO: add support for search filters

            }]);

})(softwrench);
