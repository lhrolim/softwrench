
(function (angular) {
    'use strict';

    class helpService {
        constructor($window, historyService, alertService, restService, searchService, userService, $q) {
            this.historyService = historyService;
            this.$window = $window;
            this.alertService = alertService;
            this.restService = restService;
            this.searchService = searchService;
            this.userService = userService;
            this.$q = $q;
        }


        downloadForm({ id, label }, column) {
            const isAdmin = this.userService.isSysAdmin();
            const download = column.attribute === "downloadable";
            if (download || !isAdmin) {
                this.historyService.getRouteInfo().then(info => {
                    window.location = info.contextPath + `/Help/DownloadHelpEntry?id=${id}`;
                });
            } else if (column.attribute === "remove") {
                this.alertService.confirm("are you sure you want to delete this entry?").then(r => {
                    this.restService.post("HelpApi", "Delete", { id }).then(r => {
                        this.searchService.refreshGrid();
                    });
                });
            }
            else {
                return this.$q.when();
                //                    this.$window.open(info.contextPath + `/Help/Index?id=${id}&label=${label}`, "_blank");
            }
        }

    }

    helpService.$inject = ['$window', 'historyService', 'alertService', 'restService', 'searchService', 'userService', '$q'];

    angular.module('sw_layout').service('swhelpService', helpService);


})(angular);
