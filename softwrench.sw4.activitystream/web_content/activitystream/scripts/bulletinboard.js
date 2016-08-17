(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("bulletinBoard", ["contextService", function (contextService) {
        const directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Shared/activitystream/templates/bulletinboard.html"),
            scope: {
                panelid: "@"
            },

            controller: ["$scope", "sidePanelService", "restService", "$interval", "richTextService", "formatService", "configurationService", function ($scope, sidePanelService, restService, $interval, richTextService, formatService, configurationService) {

                $scope.bulletinboard = {
                    messages: [],
                    filterText: "",
                    enableFilter: false,
                    refreshRate: 5
                };

                const configuration = {
                    enabled: "/Global/BulletinBoard/Enabled",
                    refreshRate: "/Global/BulletinBoard/RefreshRate/Ui"
                }

                var currentIntervalPromise = null;

                

                
                $scope.setPaneHeight = function () {
                    return sidePanelService.calculateScrollPanelHeight($scope.panelid);
                };

                $scope.getItemsCountStyle = function() {
                    return { top: sidePanelService.getContext($scope.panelid).top - 85 + "px" }
                };

                $scope.toggleFilter = function() {
                    $scope.bulletinboard.enableFilter = !$scope.bulletinboard.enableFilter;
                    if (!$scope.bulletinboard.enableFilter) {
                        $scope.bulletinboard.filterText = "";
                    }
                };

                function hydrateMessage(message) {
                    message.formattedMessage = richTextService.getDecodedValue(message.message);
                    message.messagePreview = message.formattedMessage.length <= 200
                        ? message.formattedMessage
                        : `${message.formattedMessage.substring(0, 200)}...`;
                    message.formattedPostDate = formatService.formatDate(message.postDate);
                    message.formattedExpireDate = formatService.formatDate(message.expireDate);
                    message.meta = { more: false };
                    return message;
                }

                function parseRefreshRate(rate) {
                    const parsedRate = parseInt(rate);
                    return parsedRate <= 0 || isNaN(parsedRate) ? $scope.bulletinboard.refreshRate : parsedRate;
                }

                function fetchActiveMessages() {
                    return restService.get("BulletinBoard", "GetActiveMessages", null, { avoidspin: true })
                        .then(r => {
                            $scope.bulletinboard.messages = r.data.messages.map(hydrateMessage);
                            const refreshRate = parseRefreshRate(r.data.refreshRate);
                            const changedRate = refreshRate !== $scope.bulletinboard.refreshRate;
                            $scope.bulletinboard.refreshRate = refreshRate;
                            return changedRate;
                        });
                }

                function scheduleBulletinBoardPoll() {
                    if (!!currentIntervalPromise && angular.isFunction(currentIntervalPromise.then)) {
                        $interval.cancel(currentIntervalPromise);
                    }
                    const delay = $scope.bulletinboard.refreshRate * 1000 * 60;
                    currentIntervalPromise = $interval(() => $scope.refreshMessages(), delay, 0, false);
                }

                $scope.refreshMessages = function() {
                    return fetchActiveMessages().then(r => r ? scheduleBulletinBoardPoll() : null);
                };

                function init() {
                    // start hidden: we don't know yet if customer has enabled the bulletin board
                    sidePanelService.hide($scope.panelid);

                    // fetch bulletin board feature configs then start
                    configurationService.fetchConfigurations(Object.values(configuration))
                        .then(configs => {
                            $scope.bulletinboard.refreshRate = parseRefreshRate(configs[configuration.refreshRate]);

                            const enabled = configs[configuration.enabled];
                            if (![true, "true", "True", 1, "1"].includes(enabled)) return;

                            sidePanelService.show($scope.panelid);
                            fetchActiveMessages().then(scheduleBulletinBoardPoll);
                        });
                }

                init();

            }]
            
        };

        return directive;
    }]);

})(angular);